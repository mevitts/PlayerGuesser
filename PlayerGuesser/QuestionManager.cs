using MySql.Data.MySqlClient;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
namespace PlayerGuesser;
internal class QuestionManager
{
    public static async Task<List<Question>> FinalQuestionGenerator(string connectionString)
    {
        var questions = new HashSet<Question>(new QuestionComparer());

        questions.UnionWith(await QuestionsByTeams(connectionString));
        questions.UnionWith(await QuestionsByAttribute(connectionString));
        questions.UnionWith(await QuestionByDraft(connectionString));
        questions.UnionWith(await QuestionByHonor(connectionString));

        return questions.ToList();
    }
    public static async Task<List<Question>> QuestionsByTeams(string connectionString)
    {
        var questions = new List<Question>();

        List<Team> teams = await Team.GetTeamsAsync(connectionString);
        List<string> colleges = await Team.GetCollegeTeamsAsync(connectionString);

        foreach (Team team in teams)
        {
            #region Current Team
            questions.Add(new Question
            {
                Text = $"Does your player play for the {team.name}?",
                Predicate = p => p.team.full_name == team.full_name,
                Query = $"SELECT * FROM players WHERE team_name = @team_name",
                NegQuery = $"SELECT * FROM players WHERE team_name != @team_name",
                Weight = 3,
                Parameters = new Dictionary<string, object> { { "@team_name", team.full_name } }
            });//team name

            questions.Add(new Question()
            {
                Text = $"Does your player play in the {team.conference} conference?",
                Predicate = p => p.team.conference == team.conference,
                Query = $"SELECT * FROM players WHERE team_name IN (SELECT fullName FROM teams WHERE conference = @conference)",
                NegQuery = $"SELECT * FROM players WHERE team_name NOT IN (SELECT fullName FROM teams WHERE conference = @conference)",
                Weight = 1,
                Parameters = new Dictionary<string, object> { { "@conference", team.conference } }

            });//team conference

            questions.Add(new Question
            {
                Text = $"Does your player play in the {team.division} division?",
                Predicate = p => p.team.division == team.division,
                Query = $"SELECT * FROM players WHERE team_name IN (SELECT fullName FROM teams WHERE division = @division)",
                NegQuery = $"SELECT * FROM players WHERE team_name NOT IN (SELECT fullName FROM teams WHERE division = @division)",
                Weight = 2,
                Parameters = new Dictionary<string, object> { { "@division", team.division } }
            });//team divisionquestions

            questions.Add(new Question
            {
                Text = $"Does your player play for {team.city}?",
                Predicate = p => p.team.city == team.city,
                Query = $"SELECT * FROM players WHERE team_name IN (SELECT fullName FROM teams WHERE city = @city)",
                NegQuery = $"SELECT * FROM players WHERE team_name NOT IN (SELECT fullName FROM teams WHERE city = @city)",
                Weight = 3,
                Parameters = new Dictionary<string, object> { { "@city", team.city } }
            });//team city
            #endregion Current Team

            #region Past Teams

            questions.Add(new Question
            {
                Text = $"Has your player ever played for the {team.name}?",
                Predicate = p => team.DoesMatch(p.PastTeams),
                Query = $"SELECT * FROM players WHERE id IN (SELECT player_id FROM players_pastteams WHERE past_team_id = @past_team_id)",
                NegQuery = $"SELECT * FROM players p LEFT JOIN players_pastteams pt ON p.id = pt.player_id AND pt.past_team_id = @past_team_id WHERE pt.past_team_id IS NULL",
                Weight = 3,
                Parameters = new Dictionary<string, object> { { "past_team_id", team.name } }
            });//team name

            questions.Add(new Question
            {
                Text = $"Has your player ever played in the {team.conference} conference?",
                Predicate = p => team.DoesMatch(p.PastTeams, teams, "c"),
                Query = $"SELECT * FROM players WHERE id IN (SELECT player_id FROM players_pastteams WHERE past_team_id IN (SELECT fullName FROM teams WHERE conference = @conference))",
                NegQuery = $"SELECT * FROM players WHERE id NOT IN (SELECT player_id FROM players_pastteams WHERE past_team_id IN (SELECT fullName FROM teams WHERE conference = @conference) GROUP BY player_id HAVING COUNT(*) > 0)",
                Weight = 2,
                Parameters = new Dictionary<string, object> { { "conference", team.conference } }
            });//team name

            questions.Add(new Question
            {
                Text = $"Has your player ever played in the {team.division} division?",
                Predicate = p => team.DoesMatch(p.PastTeams, teams, "d"),
                Query = $"SELECT * FROM players WHERE id IN (SELECT player_id FROM players_pastteams WHERE past_team_id IN (SELECT fullName FROM teams WHERE division = @division))",
                NegQuery = $"SELECT * FROM players WHERE id NOT IN (SELECT player_id FROM players_pastteams WHERE past_team_id IN (SELECT fullName FROM teams WHERE division = @division) GROUP BY player_id HAVING COUNT(*) > 0)",
                Weight = 2,
                Parameters = new Dictionary<string, object> { { "division", team.division } }
            });//team name

            #endregion Past Teams
        }//NBA teams

        #region Non NBA
        foreach (var team in colleges)
        {
            questions.Add(new Question
            {
                Text = $"Did your player play college at {team}?",
                Predicate = p => Team.DoesMatch(p.college, team),
                Query = $"SELECT * FROM players WHERE college = @college",
                NegQuery = $"SELECT * FROM players WHERE college != @college",
                Weight = 3,
                Parameters = new Dictionary<string, object> { { "college", team } }
            });//team name
        }//college teams
        #endregion Non NBA

        return questions;
    }
    public static async Task<List<Question>> QuestionsByAttribute(string connectionString)
    {
        var questions = new List<Question>();

        List<String> alphabet = new List<string>();
        for (char letter = 'A'; letter <= 'Z'; letter++)
        {
            alphabet.Add(letter.ToString());
        }//creating alphabet list

        string[] positions = { "Center", "Forward", "Guard" };
        foreach (string pos in positions)
        {
            string abbrev = pos.Substring(0, 1);
            questions.Add(new Question
            {
                Text = $"Is your player a {pos.ToLower()}?",
                Predicate = p => p.position == abbrev,
                Query = $"SELECT * FROM players WHERE position = @position",
                NegQuery = $"SELECT * FROM players WHERE position != @position",
                Weight = 1,
                Parameters = new Dictionary<string, object> { { "position", abbrev } }
            });
        }//position

        questions.Add(new Question
        {
            Text = $"Is your player from the US?",
            Predicate = p => p.country == "USA",
            Query = $"SELECT * FROM players WHERE country = @country",
            NegQuery = $"SELECT * FROM players WHERE country != @country",
            Weight = 2,
            Parameters = new Dictionary<string, object> { { "country", "USA" } }
        });//country

        foreach (string letter in alphabet)
        {
            questions.Add(new Question
            {
                Text = $"Does your player's first name start with {letter}?",
                Predicate = p => p.first_name.StartsWith(letter),
                Query = $"SELECT * FROM players WHERE first_name LIKE @letter",
                NegQuery = $"SELECT * FROM players WHERE first_name NOT LIKE @letter",
                Weight = 3,
                Parameters = new Dictionary<string, object> { { "letter", $"{letter}%" } }
            });//first name
            questions.Add(new Question
            {
                Text = $"Does your player's last name start with {letter}?",
                Predicate = p => p.last_name.StartsWith(letter),
                Query = $"SELECT * FROM players WHERE last_name LIKE @letter",
                NegQuery = $"SELECT * FROM players WHERE last_name NOT LIKE @letter",
                Weight = 3,
                Parameters = new Dictionary<string, object> { { "letter", $"{letter}%" } }
            });//last name
        }//name starting

        return questions;
    }
    public static async Task<List<Question>> QuestionByHonor(string connectionString)
    {
        var questions = new List<Question>();
        #region Championships
        var championships = new List<String>
            ([
                "NBA",
                "NCAA Basketball",
                "FIBA World Cup",
                "Olympics Gold",
                "Lega Basket",
                "EuroLeague",
                "ABA League",
                "Spanish Supercopa de Espana de Baloncesto",
                "Spanish Copa del Rey de Baloncesto"
            ]);
        foreach (string chip in championships)
        {
            questions.Add(new Question
            {
                Text = $"Has your player won a/an {chip} (championship)?",
                Predicate = p => p.Honors != null && p.Honors.Any(h => h.strHonour == chip),
                Query = "SELECT * FROM players WHERE id IN (SELECT player_id FROM players_honors WHERE honor_title = @honorTitle)",
                NegQuery = "SELECT * FROM players p LEFT JOIN players_honors ph ON p.id = ph.player_id AND ph.honor_title = @honorTitle WHERE ph.honor_title IS NULL",
                Weight = 2,
                Parameters = new Dictionary<string, object> { { "honorTitle", chip } }
            });//one chip

            questions.Add(new Question
            {
                Text = $"Has your player won multiple {chip} (championships)?",
                Predicate = p => p.Honors != null && p.Honors.Count(h => h.strHonour == chip) > 1,
                Query = $"SELECT * FROM players WHERE id IN (SELECT player_id FROM players_honors WHERE honor_title = @honorTitle GROUP BY player_id HAVING COUNT(*) > 1)",
                NegQuery = $"SELECT * FROM players WHERE id NOT IN (SELECT player_id FROM players_honors WHERE honor_title = @honorTitle GROUP BY player_id HAVING COUNT(*) > 1)",
                Weight = 3,
                Parameters = new Dictionary<string, object> { { "honorTitle", chip } }
            });//multiple

        }
        #endregion Championships

        #region Single Honors
        var singleTimeHonors = new List<String>
            ([
                "NBA Rookie of the year",
                "Basketball Hall of Fame",
                "NBA Sixth Man of the Year",
                "Gatorade Player of the Year",
                "NBA Most Improved Player"
            ]);

        foreach (string honor in singleTimeHonors)
        {
            questions.Add(new Question
            {
                Text = $"Did your player achieve {honor})?",
                Predicate = p => p.Honors != null && p.Honors.Any(h => h.strHonour == honor),
                Query = $"SELECT * FROM players WHERE id IN (SELECT player_id FROM players_honors WHERE honor_title = @honorTitle)",
                NegQuery = $"SELECT * FROM players p LEFT JOIN players_honors ph ON p.id = ph.player_id AND ph.honor_title = @honorTitle WHERE ph.honor_title IS NULL",
                Weight = 3,
                Parameters = new Dictionary<string, object> { { "honorTitle", honor } }
            });//one chip
        }
        #endregion Single Honors

        #region Multiple Honors
        var multipleAwards = new List<String>
        {                "NBA All Star",
                "NBA Three-Point Contest",
                "NBA scoring champion",
                "NBA Sixth Man of the Year",
                "NBA Slam Dunk Contest",
                "NBA MVP",
                "All NBA First Team",
                "NBA rebounding leader",
                "NBA assists leader"
        };

        foreach (string honor in multipleAwards)
        {
            questions.Add(new Question
            {
                Text = $"Has your player been/won {honor}?",
                Predicate = p => p.Honors != null && p.Honors.Any(h => h.strHonour == honor),
                Query = "SELECT * FROM players WHERE id IN (SELECT player_id FROM players_honors WHERE honor_title = @honorTitle)",
                NegQuery = "SELECT * FROM players p LEFT JOIN players_honors ph ON p.id = ph.player_id AND ph.honor_title = @honorTitle WHERE ph.honor_title IS NULL",
                Weight = 2,
                Parameters = new Dictionary<string, object> { { "honorTitle", honor } }
            });//one honor

            questions.Add(new Question
            {
                Text = $"Has your player been/won {honor} more than once)?",
                Predicate = p => p.Honors != null && p.Honors.Count(h => h.strHonour == honor) > 1,
                Query = $"SELECT * FROM players WHERE id IN (SELECT player_id FROM players_honors WHERE honor_title = @honorTitle GROUP BY player_id HAVING COUNT(*) > 1)",
                NegQuery = $"SELECT * FROM players WHERE id NOT IN (SELECT player_id FROM players_honors WHERE honor_title = @honorTitle GROUP BY player_id HAVING COUNT(*) > 1)",
                Weight = 3,
                Parameters = new Dictionary<string, object> { { "honorTitle", honor } }
            });//multiple honors
        }
        #endregion Multiple Honors

        return questions;
    }
    public static async Task<List<Question>> QuestionByDraft(string connectionString)
    {
        var questions = new List<Question>();

        #region Draft Years
        questions.Add(new Question
        {
            Text = $"Was your player drafted in the 60s?",
            Predicate = p => p.draft_year >= 1960 && p.draft_year <= 1969,
            Query = $"SELECT * FROM players WHERE draft_year BETWEEN 1960 AND 1969",
            NegQuery = $"SELECT * FROM players WHERE draft_year NOT BETWEEN 1960 AND 1969",
            Weight = 2,
            Parameters = null
        });

        questions.Add(new Question
        {
            Text = $"Was your player drafted in the 70s?",
            Predicate = p => p.draft_year >= 1970 && p.draft_year <= 1979,
            Query = $"SELECT * FROM players WHERE draft_year BETWEEN 1970 AND 1979",
            NegQuery = $"SELECT * FROM players WHERE draft_year NOT BETWEEN 1970 AND 1979",
            Weight = 2,
            Parameters = null
        });

        questions.Add(new Question
        {
            Text = $"Was your player drafted in the 80s?",
            Predicate = p => p.draft_year >= 1980 && p.draft_year <= 1989,
            Query = $"SELECT * FROM players WHERE draft_year BETWEEN 1980 AND 1989",
            NegQuery = $"SELECT * FROM players WHERE draft_year NOT BETWEEN 1980 AND 1989",
            Weight = 2,
            Parameters = null
        });

        questions.Add(new Question
        {
            Text = $"Was your player drafted in the 90s?",
            Predicate = p => p.draft_year >= 1990 && p.draft_year <= 1999,
            Query = $"SELECT * FROM players WHERE draft_year BETWEEN 1990 AND 1999",
            NegQuery = $"SELECT * FROM players WHERE draft_year NOT BETWEEN 1990 AND 1999",
            Weight = 2,
            Parameters = null
        });

        questions.Add(new Question
        {
            Text = $"Was your player drafted in the 2000s (before 2010)?",
            Predicate = p => p.draft_year >= 2000 && p.draft_year <= 2010,
            Query = $"SELECT * FROM players WHERE draft_year BETWEEN 2000 AND 2009",
            NegQuery = $"SELECT * FROM players WHERE draft_year NOT BETWEEN 2000 AND 2009",
            Weight = 2,
            Parameters = null
        });

        questions.Add(new Question
        {
            Text = $"Was your player drafted in the 2010s?",
            Predicate = p => p.draft_year >= 2010 && p.draft_year <= 2019,
            Query = $"SELECT * FROM players WHERE draft_year BETWEEN 2010 AND 2019",
            NegQuery = $"SELECT * FROM players WHERE draft_year NOT BETWEEN 2010 AND 2019",
            Weight = 2,
            Parameters = null
        });

        questions.Add(new Question
        {
            Text = $"Was your player drafted in the last 5 years?",
            Predicate = p => p.draft_year >= 2019,
            Query = $"SELECT * FROM players WHERE draft_year >= 2019",
            NegQuery = $"SELECT * FROM players WHERE draft_year < 2019",
            Weight = 2,
            Parameters = null
        });

        questions.Add(new Question
        {
            Text = $"Was your player a rookie or second year this year?",
            Predicate = p => p.draft_year >= 2022,
            Query = $"SELECT * FROM players WHERE draft_year >= 2022",
            NegQuery = $"SELECT * FROM players WHERE draft_year < 2022",
            Weight = 2,
            Parameters = null
        });

        questions.Add(new Question
        {
            Text = $"Was your player drafted before 1990?",
            Predicate = p => p.draft_year <= 1990,
            Query = $"SELECT * FROM players WHERE draft_year < 1990",
            NegQuery = $"SELECT * FROM players WHERE draft_year >= 1990",
            Weight = 1,
            Parameters = null
        });
        #endregion Draft Years

        #region Draft Position

        questions.Add(new Question
        {
            Text = $"Was your player a lottery pick?",
            Predicate = p => p.draft_number <= 14,
            Query = $"SELECT * FROM players WHERE draft_number <= 14",
            NegQuery = $"SELECT * FROM players WHERE draft_number > 14",
            Weight = 2,
            Parameters = null
        });

        questions.Add(new Question
        {
            Text = $"Was your player drafted in the first round?",
            Predicate = p => p.draft_round == 1,
            Query = $"SELECT * FROM players WHERE draft_round = 1",
            NegQuery = $"SELECT * FROM players WHERE draft_round != 1",
            Weight = 1,
            Parameters = null
        });

        questions.Add(new Question
        {
            Text = $"Was your player drafted in the second round?",
            Predicate = p => p.draft_round == 2,
            Query = $"SELECT * FROM players WHERE draft_round = 2",
            NegQuery = $"SELECT * FROM players WHERE draft_round != 2",
            Weight = 1,
            Parameters = null
        });

        questions.Add(new Question
        {
            Text = $"Was your player a top 5 pick?",
            Predicate = p => p.draft_number <= 5,
            Query = $"SELECT * FROM players WHERE draft_number <= 5",
            NegQuery = $"SELECT * FROM players WHERE draft_number > 5",
            Weight = 3,
            Parameters = null
        });
        #endregion Draft Position

        return questions;
    }
    public async Task<List<Question>> FinalQuestion(string connectionString, List<Player> finalPlayers)
    {
        var questions = new List<Question>();

        foreach (var player in finalPlayers)
        {
            questions.Add(new Question
            {
                Text = $"Is your player {player.first_name} {player.last_name}?",
                Predicate = p => p.id == player.id,
                Query = $"SELECT * FROM players WHERE id = @id",
                NegQuery = $"SELECT * FROM players WHERE id != @id",
                Weight = 4,
                Parameters = new Dictionary<string, object> { { "id", player.id } }
            });
        }
        return questions;
    }
    public async Task GetYesNoCount(Question question, string connectionString)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            await connection.OpenAsync();

            using (var command = new MySqlCommand(question.Query, connection))
            {
                foreach (var param in question.Parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }//goes through each parameter for the question

                question.YesCount = (int)await command.ExecuteScalarAsync();
            }//counts the yescount using the query
        }//connects to db
        using (var connection = new MySqlConnection(connectionString))
        {
            await connection.OpenAsync();

            using (var command = new MySqlCommand(question.NegQuery, connection))
            {
                foreach (var param in question.Parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }//goes through each parameter for the question

                question.YesCount = (int)await command.ExecuteScalarAsync();
            }
        }
    }//gets Yes and No count of each question
}

