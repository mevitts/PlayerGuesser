using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerGuesser;
internal class GameProcess
{
    private readonly Mind mind;
    private readonly QuestionManager questionMgr;
    private bool guessed;
    private int questionCount;
    public GameProcess()
    {
        mind = new Mind();
        questionMgr = new QuestionManager();
        guessed = false;
    }

    internal async Task OnStart(string connectionString)
    {
        bool cont = true;
        while (cont)
        {
            questionCount = 0;
            bool correct;
            int playerCount;

            var contextQuestions = await QuestionManager.FinalQuestionGenerator(connectionString);
            var askedQuestions = new List<Question>();
            Question questionToAsk;

            List<Player> filteredPlayers = await QueryDB(connectionString); //gets players list with all original players

            while (guessed == false)
            {
                playerCount = filteredPlayers.Count;
                if (playerCount == 0)
                {
                    Console.WriteLine("No players left");
                    break;
                }

                contextQuestions = await QuestionManager.FinalQuestionGenerator(connectionString);
                contextQuestions = QuestionsWithContext(questionCount, playerCount, askedQuestions, contextQuestions);

                if (contextQuestions.Count == 1)
                {
                    contextQuestions.RemoveAt(0);
                    contextQuestions = await questionMgr.FinalQuestion(connectionString, filteredPlayers);

                    questionToAsk = await mind.SelectBestQuestion(contextQuestions, connectionString, filteredPlayers);

                    correct = await UserInput.AskQuestion(questionToAsk);
                    askedQuestions.Add(questionToAsk);

                    if (correct == true)
                    {
                        guessed = true; break;
                    }//if plaer guessed correctly

                    else
                    {
                        filteredPlayers = await QueryDB(connectionString, questionToAsk, filteredPlayers, correct);
                    }
                }//prepared to ask final question

                else
                {
                    questionToAsk = await mind.SelectBestQuestion(contextQuestions, connectionString, filteredPlayers);

                    //QuestionsWithContext method will handle if the Final question will be asked so there is nothing different
                    //if it sees the final question needs to be asked, it will return a list of Questions with just one question, which will be selected 

                    correct = await UserInput.AskQuestion(questionToAsk);
                    askedQuestions.Add(questionToAsk);

                    filteredPlayers = await QueryDB(connectionString, questionToAsk, filteredPlayers, correct);
                }//every other question process

                questionCount++;

                if (questionCount > 200)
                {
                    Console.WriteLine("You have beat me! Who was your player?");

                }//losing

            }//end of guessing stage

            #region PostGame
            Console.Clear();
            Console.WriteLine("I won! Would you like to play again?");

            string playAgain = Console.ReadLine();

            while (playAgain == null || playAgain.ToLower() != "y" || playAgain.ToLower() != "n")
            {
                Console.WriteLine("Invalid input. Press 'y' or 'n'.");
                playAgain = Console.ReadLine();

            }//validating

            if (playAgain.ToLower() == "n")
            {
                Console.WriteLine("Thanks for playing.");
                cont = false;
            }//if no play again
            #endregion PostGame
        }//while keep wanting to play
    }
    internal async Task<List<Player>> QueryDB(string connectionString)
    {
        var players = new List<Player>();

        using (var connection = new MySqlConnection(connectionString))
        {
            await connection.OpenAsync();

            string query = $"SELECT * FROM players";

            using (var command = new MySqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Player p = new Player
                        {
                            id = reader.GetInt32("id"),
                            first_name = reader.IsDBNull(reader.GetOrdinal("first_name")) ? null : reader.GetString("first_name"),
                            last_name = reader.IsDBNull(reader.GetOrdinal("last_name")) ? null : reader.GetString("last_name"),
                            position = reader.IsDBNull(reader.GetOrdinal("position")) ? null : reader.GetString("position"),
                            height = reader.IsDBNull(reader.GetOrdinal("height")) ? null : reader.GetString("height"),
                            weight = reader.IsDBNull(reader.GetOrdinal("weight")) ? null : reader.GetString("weight"),
                            jersey_number = reader.IsDBNull(reader.GetOrdinal("jersey_number")) ? null : reader.GetString("jersey_number"),
                            college = reader.IsDBNull(reader.GetOrdinal("college")) ? null : reader.GetString("college"),
                            country = reader.IsDBNull(reader.GetOrdinal("country")) ? null : reader.GetString("country"),
                            draft_year = reader.IsDBNull(reader.GetOrdinal("draft_year")) ? (int?)null : reader.GetInt32("draft_year"),
                            draft_round = reader.IsDBNull(reader.GetOrdinal("draft_round")) ? (int?)null : reader.GetInt32("draft_round"),
                            draft_number = reader.IsDBNull(reader.GetOrdinal("draft_number")) ? (int?)null : reader.GetInt32("draft_number"),
                            team = new Team
                            {
                                full_name = reader.IsDBNull(reader.GetOrdinal("team_name")) ? null : reader.GetString("team_name")
                            }
                        };//creates each player 

                        players.Add(p);

                    }//while reading from DB players table columns

                }//using the reader

            }//creates commands

        }//connects to db
        return players;

    }//end of querying DB. Only used for initial players
    internal async Task<List<Player>> QueryDB(string connectionString, Question questionToAsk, List<Player> filteredPlayers, bool correct)
    {
        var players = new List<Player>();
        string query;

        using (var connection = new MySqlConnection(connectionString))
        {
            await connection.OpenAsync();
            //counts the number of players that match the query of the question and also matches an id in the filtered players list
            if (correct)
            {
                query = $"SELECT * FROM players WHERE {questionToAsk.Query.Substring(28)} AND id IN ({string.Join(",", filteredPlayers.Select(p => p.id))})";
            }

            else
            {
                var shorter = questionToAsk.NegQuery.Substring(0, 28);
                if (shorter == "SELECT * FROM players p LEFT")
                {
                    query = $"SELECT * FROM players {questionToAsk.NegQuery.Substring(22)} AND p.id IN ({string.Join(",", filteredPlayers.Select(p => p.id))})";
                }
                else
                {
                    query = $"SELECT * FROM players WHERE {questionToAsk.NegQuery.Substring(28)} AND id IN ({string.Join(",", filteredPlayers.Select(p => p.id))})";
                }
            }

            using (var command = new MySqlCommand(query, connection))
            {
                if (questionToAsk.Parameters != null)
                {
                    var firstParam = questionToAsk.Parameters.FirstOrDefault();

                    if (firstParam.Value != null)
                    {
                        command.Parameters.AddWithValue("@" + firstParam.Key, firstParam.Value);
                    }
                }

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Player p = new Player
                        {
                            id = reader.GetInt32("id"),
                            first_name = reader.GetString("first_name"),
                            last_name = reader.GetString("last_name"),
                            position = reader.IsDBNull(reader.GetOrdinal("position")) ? null : reader.GetString(reader.GetOrdinal("position")),
                            height = reader.IsDBNull(reader.GetOrdinal("height")) ? null : reader.GetString(reader.GetOrdinal("height")),
                            weight = reader.IsDBNull(reader.GetOrdinal("weight")) ? null : reader.GetString(reader.GetOrdinal("weight")),
                            jersey_number = reader.IsDBNull(reader.GetOrdinal("jersey_number")) ? null : reader.GetString(reader.GetOrdinal("jersey_number")),
                            college = reader.IsDBNull(reader.GetOrdinal("college")) ? null : reader.GetString(reader.GetOrdinal("college")),
                            country = reader.IsDBNull(reader.GetOrdinal("country")) ? null : reader.GetString(reader.GetOrdinal("country")),
                            draft_year = reader.IsDBNull(reader.GetOrdinal("draft_year")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("draft_year")),
                            draft_round = reader.IsDBNull(reader.GetOrdinal("draft_round")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("draft_round")),
                            draft_number = reader.IsDBNull(reader.GetOrdinal("draft_number")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("draft_number")),
                            team = new Team
                            {
                                full_name = reader.IsDBNull(reader.GetOrdinal("team_name")) ? null : reader.GetString(reader.GetOrdinal("team_name"))
                            }
                        };//creates each player 

                        players.Add(p);

                    }//while reading from DB players table columns

                }//using the reader

            }//creates commands

        }//connects to db
        return players;

    }//end of querying DB for rest of questions
    private List<Question> QuestionsWithContext(int questionCount, int playerCount, List<Question> askedQuestions, List<Question> contextQuestions)
    {
        var returnQuestions = new List<Question>();
        // question count will try to keep it basic and very easy until around 4 ish questions. Middle will start considering around 7 
        // but each phase will break if a playercount threshold is broken
        
        //final count 
        if (playerCount <= 3)
        {
            returnQuestions.Add(askedQuestions[0]);
        }//will add just one question and return it as a placeholder to create last question. Preferred over returning nothing in case error also returns empty list. 

        // beginning count 
        else if (questionCount >= 0 && questionCount < 2)
        {
            returnQuestions = contextQuestions.Where(q => q.Weight == 1 && !askedQuestions.Any(a => a.Text == q.Text)).ToList(); // question weight is 1 and not an asked question
        }

        //middle
        else if (questionCount >= 2 && questionCount < 5 || playerCount < 400 && playerCount >= 200)
        {
            returnQuestions = contextQuestions.Where(q => q.Weight == 2 || q.Weight == 1 && !askedQuestions.Any(a => a.Text == q.Text)).ToList(); // question weight is 2 and not an asked question
        }

        //specific
        else if (playerCount < 200 && playerCount > 3 || questionCount >= 5)
        {
            returnQuestions = contextQuestions.Where(q => q.Weight == 3 || q.Weight == 2 || q.Weight == 1 && !askedQuestions.Any(a => a.Text == q.Text)).ToList(); // question weight is 3 and not an asked question
        }

        
        return returnQuestions;
    }// end of contextQuestions
}


