using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PlayerGuesser;
public class Team
{
    public int id { get; set; }
    public string conference { get; set; }
    public string division { get; set; }
    public string city { get; set; }
    public string name { get; set; }
    public string full_name { get; set; }
    public string abbreviation { get; set; }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        Team team = (Team)obj;
        return id == team.id && name == team.name &&
            full_name == team.full_name
            && abbreviation == team.abbreviation;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(id, name, full_name, abbreviation); ;
    }
    public List<Team> GetTeams(List<Player> players)
    {
        HashSet<Team> teams = new HashSet<Team>();
        foreach (Player player in players)
        {
            teams.Add(player.team);
            if (teams.Count == 30)
            {
                return teams.ToList();
            }
        }
        if (teams.Count != 30)
        {
            throw new Exception("The number of teams is not correct.");
        }
        return teams.ToList();
    }

    public static async Task<List<Team>> GetTeamsAsync(string connectionString)
    {
        var teams = new List<Team>();

        using (var connection = new MySqlConnection(connectionString))
        {
            await connection.OpenAsync();

            var query = "SELECT * FROM teams";

            using (var command = new MySqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var team = new Team
                        {
                            id = reader.GetInt32(reader.GetOrdinal("id")),
                            conference = reader.GetString(reader.GetOrdinal("conference")),
                            division = reader.GetString(reader.GetOrdinal("division")),
                            city = reader.GetString(reader.GetOrdinal("city")),
                            name = reader.GetString(reader.GetOrdinal("name")),
                            full_name = reader.GetString(reader.GetOrdinal("fullName")),
                            abbreviation = reader.GetString(reader.GetOrdinal("abbreviation"))
                        };

                        teams.Add(team);
                    }//building and adding team
                }//using reader
            }//making sql command
        }//using mysql connection

        return teams;
    }//end of function

    public bool DoesMatch(List<PastTeam> pastTeams)
    {
        if (pastTeams == null)
            return false;

        return pastTeams.Any(pt => pt.strFormerTeam == full_name);
    }

    public static bool DoesMatch(string college, string collegeTeam)
    {
        if (college == null)
            return false;
        if (college == collegeTeam) return true;
        else return false;
    }

    public bool DoesMatch(List<PastTeam> pastTeams, List<Team> teams, string divOrConf)
    {
        if (divOrConf == null)
            return false;
        if (pastTeams == null)
            return false;

        if (divOrConf == "c")
        {

            return pastTeams.Any(pt => pt.strFormerTeam == full_name) || teams.Any(t => t.conference == conference && pastTeams.Any(pt => pt.strFormerTeam == t.full_name));
        }
        else
        {
            return pastTeams.Any(pt => pt.strFormerTeam == full_name) || teams.Any(t => t.division == division && pastTeams.Any(pt => pt.strFormerTeam == t.full_name));
        }
    }
    public static async Task<List<String>> GetCollegeTeamsAsync(string connectionString)
    {
        var teams = new List<String>();

        using (var connection = new MySqlConnection(connectionString))
        {
            await connection.OpenAsync();

            var query = "SELECT college FROM players";

            using (var command = new MySqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            teams.Add(reader.GetString(0));
                        }
                    }//building and adding team
                }//using reader
            }//making sql command
        }//using mysql connection
        return teams.ToList();
    }
}