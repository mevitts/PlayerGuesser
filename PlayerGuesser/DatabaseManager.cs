using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml.Linq;
using MySql.Data;
using MySql.Data.MySqlClient;
using ZstdSharp.Unsafe;

namespace PlayerGuesser;

internal class DatabaseManager
{
    internal void CreateTables(string connectionString)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            //teams
            using (var tableCmd = connection.CreateCommand())
            {
                //primary key makes it unique and not null
                tableCmd.CommandText =
                    @$"CREATE TABLE IF NOT EXISTS teams (
                        id INT PRIMARY KEY,
                        conference VARCHAR(10),
                        division VARCHAR(255),
                        city VARCHAR(255),
                        name VARCHAR(255),
                        fullName VARCHAR(255),
                        abbreviation VARCHAR(5))";

                tableCmd.ExecuteNonQuery();
            }
            using (var indexCmd = connection.CreateCommand())
            {
                bool indexExists = IndexExists(connection, "teams", "team_name_index");

                if (!indexExists)
                {
                    indexCmd.CommandText = "ALTER TABLE teams ADD INDEX team_name_index (name)";
                    indexCmd.ExecuteNonQuery();
                }
            }
            //honors
            using (var tableCmd = connection.CreateCommand())
            {
                tableCmd.CommandText =
                    @$"CREATE TABLE IF NOT EXISTS honors (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        title VARCHAR(255),
                        year_received VARCHAR(255),
                        team_name VARCHAR(255)
                        )";

                tableCmd.ExecuteNonQuery();
            }
            //past teams
            using (var tableCmd = connection.CreateCommand())
            {
                tableCmd.CommandText =
                    @$"CREATE TABLE IF NOT EXISTS past_teams (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        team_name VARCHAR(255),
                        start_year INT,
                        end_year INT
                        )";

                tableCmd.ExecuteNonQuery();
            }
            //players
            using (var tableCmd = connection.CreateCommand())
            {
                //primary key makes it unique and not null
                tableCmd.CommandText =
                    @$"CREATE TABLE IF NOT EXISTS players (
                        id INT PRIMARY KEY,
                        first_name VARCHAR(255),
                        last_name VARCHAR(255),
                        position VARCHAR(50),
                        height VARCHAR(10),
                        weight VARCHAR(10),
                        jersey_number VARCHAR(10),
                        college VARCHAR(255),
                        country VARCHAR(255),
                        draft_year INT,
                        draft_round INT,
                        draft_number INT,
                        team_name VARCHAR(255)
                     )";

                tableCmd.ExecuteNonQuery();
            }
            //playersHonors
            using (var tableCmd = connection.CreateCommand())
            {
                tableCmd.CommandText =
                    @$"CREATE TABLE IF NOT EXISTS players_honors (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        player_id INT,
                        honor_id INT,
                        honor_title VARCHAR(255),
                        year_received VARCHAR(255),
                        team VARCHAR(255),
                        FOREIGN KEY (player_id) REFERENCES players(id),
                        FOREIGN KEY (honor_id) REFERENCES honors(id)
                        )";

                tableCmd.ExecuteNonQuery();
            }
            //playersPastTeams
            using (var tableCmd = connection.CreateCommand())
            {
                tableCmd.CommandText =
                    @$"CREATE TABLE IF NOT EXISTS players_pastteams (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        player_id INT,
                        past_team_id VARCHAR(255),
                        start_year INT,
                        end_year INT,
                        FOREIGN KEY (player_id) REFERENCES players(id)
                        )";

                tableCmd.ExecuteNonQuery();
            }

            connection.Close();
        }
    }
    internal void AddToTables(string connectionString, List<Player> players, List<Team> teams)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            //players
            Console.WriteLine("Working on step1");
            using (var tableCmd = connection.CreateCommand())
            {
                tableCmd.CommandText = @$"INSERT INTO players (id, first_name, last_name, position, height, weight, jersey_number, college, country, draft_year, draft_round, draft_number, team_name)
                    VALUES (@playerID, @firstName, @lastName, @playerPos, @height, @playerWeight, @jersey, @college, @country, @draftYear, @draftRound, @draftNumber, @teamFullName)";
                foreach (var player in players)
                {
                    tableCmd.Parameters.Clear();
                    tableCmd.Parameters.AddWithValue("@playerID", player.id);
                    tableCmd.Parameters.AddWithValue("@firstName", player.first_name);
                    tableCmd.Parameters.AddWithValue("@lastName", player.last_name);
                    tableCmd.Parameters.AddWithValue("@playerPos", player.position);
                    tableCmd.Parameters.AddWithValue("@height", player.height);
                    tableCmd.Parameters.AddWithValue("@playerWeight", player.weight);
                    tableCmd.Parameters.AddWithValue("@jersey", player.jersey_number);
                    tableCmd.Parameters.AddWithValue("@college", player.college);
                    tableCmd.Parameters.AddWithValue("@country", player.country);
                    tableCmd.Parameters.AddWithValue("@draftYear", player.draft_year);
                    tableCmd.Parameters.AddWithValue("@draftRound", player.draft_round);
                    tableCmd.Parameters.AddWithValue("@draftNumber", player.draft_number);
                    tableCmd.Parameters.AddWithValue("@teamFullName", player.team.full_name);
                    tableCmd.ExecuteNonQuery();
                }
            }
            //teams
            Console.WriteLine("Working on step2");
            using (var tableCmd = connection.CreateCommand())
            {
                tableCmd.CommandText = @$"INSERT INTO teams (id, conference, division, city, name, fullName, abbreviation)
                    VALUES (@teamId, @conference, @division, @city, @name, @fullName, @abbreviation)";
                foreach (var team in teams)
                {
                    tableCmd.Parameters.Clear();
                    tableCmd.Parameters.AddWithValue("@teamId", team.id);
                    tableCmd.Parameters.AddWithValue("@conference", team.conference);
                    tableCmd.Parameters.AddWithValue("@division", team.division);
                    tableCmd.Parameters.AddWithValue("@city", team.city);
                    tableCmd.Parameters.AddWithValue("@name", team.name);
                    tableCmd.Parameters.AddWithValue("@fullName", team.full_name);
                    tableCmd.Parameters.AddWithValue("@abbreviation", team.abbreviation);
                    tableCmd.ExecuteNonQuery();
                }
            }
            //honors
            Console.WriteLine("Working on step3");
            using (var tableCmd = connection.CreateCommand())
            {
                tableCmd.CommandText = @$"INSERT INTO honors (title, year_received, team_name)
                    VALUES (@title, @year_received, @team)";
                foreach (var player in players)
                {
                    if (player.Honors != null && player.Honors.Any()) // Check if the player has any honors
                    {
                        foreach (Honor honor in player.Honors)
                        {
                            tableCmd.CommandText = @$"INSERT INTO honors (title, year_received, team_name)
                    VALUES (@title, @year_Received, @team_name)";
                            tableCmd.Parameters.Clear();
                            tableCmd.Parameters.AddWithValue("@title", honor.strHonour);
                            tableCmd.Parameters.AddWithValue("@year_received", honor.strSeason);
                            tableCmd.Parameters.AddWithValue("@team_name", honor.strTeam);
                            tableCmd.ExecuteNonQuery();


                            long honorId = tableCmd.LastInsertedId;
                            //playersHonors
                            using (var playerHonorCmd = connection.CreateCommand())
                            {
                                playerHonorCmd.CommandText = @$"INSERT INTO players_honors (player_id, honor_id, honor_title, year_received, team)
                    VALUES (@player_id, @honorID, @honor_title, @year_received, @team)";
                                playerHonorCmd.Parameters.Clear();
                                playerHonorCmd.Parameters.AddWithValue("@player_id", player.id);
                                playerHonorCmd.Parameters.AddWithValue("@honorId", honorId);
                                playerHonorCmd.Parameters.AddWithValue("@honor_title", honor.strHonour);
                                playerHonorCmd.Parameters.AddWithValue("@year_received", honor.strSeason);
                                playerHonorCmd.Parameters.AddWithValue("@team", honor.strTeam);
                                playerHonorCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

            }
            //pastTTeams
            Console.WriteLine("Working on step4");
            using (var tableCmd = connection.CreateCommand())
            {
                tableCmd.CommandText = @$"INSERT INTO past_teams (team_name, start_year, end_year)
                    VALUES (@teamName, @startYear, @endYear)";
                foreach (var player in players)
                {
                    if (player.PastTeams != null && player.PastTeams.Any())
                    {
                        foreach (PastTeam team in player.PastTeams)
                        {
                            tableCmd.Parameters.Clear();
                            tableCmd.Parameters.AddWithValue("@teamName", team.strFormerTeam);
                            tableCmd.Parameters.AddWithValue("@startYear", int.Parse(team.strJoined));
                            tableCmd.Parameters.AddWithValue("@endYear", int.Parse(team.strDeparted));
                            tableCmd.ExecuteNonQuery();

                            //playersPastTeams
                            using (var playerPastCmd = connection.CreateCommand())
                            {
                                playerPastCmd.CommandText = @$"INSERT INTO players_pastteams (player_id, past_team_id, start_year, end_year)
                    VALUES (@player_id, @pastTeamId, @startYear, @endYear)";
                                playerPastCmd.Parameters.Clear();
                                playerPastCmd.Parameters.AddWithValue("@player_id", player.id);
                                playerPastCmd.Parameters.AddWithValue("@pastTeamId", team.strFormerTeam);
                                playerPastCmd.Parameters.AddWithValue("@startYear", int.Parse(team.strJoined));
                                playerPastCmd.Parameters.AddWithValue("@endYear", int.Parse(team.strDeparted));
                                playerPastCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            connection.Close();
        }
    }
    static bool IndexExists(MySqlConnection connection, string tableName, string indexName)
    {
        using (MySqlCommand cmd = connection.CreateCommand())
        {
            cmd.CommandText = "SELECT COUNT(*) FROM information_schema.statistics " +
                          //setting parameters here, then adds them later
                          "WHERE table_schema = @dbName AND table_name = @tableName AND index_name = @indexName";
            cmd.Parameters.AddWithValue("@dbName", connection.Database);
            cmd.Parameters.AddWithValue("@tableName", tableName);
            cmd.Parameters.AddWithValue("@indexName", indexName);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }
    }

    internal async Task AddRemainingInfo(MySqlConnection connection)
    {
        using (MySqlCommand cmd = connection.CreateCommand()) 
        {
            cmd.CommandText = "SELECT first_name, last_name FROM players";

            var reader = cmd.ExecuteReader();
            PlayerFetcher playerFetcher = new PlayerFetcher();
            while (reader.Read())
            {
                var fullName = $"{reader["first_name"].ToString()}_{reader["last_name"].ToString()}";

                var pastTeamList = await playerFetcher.GetPlayerPastTeams(fullName);

            }
        }
    }
}

