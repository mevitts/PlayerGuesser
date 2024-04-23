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
                        team_name VARCHAR(255),
                        FOREIGN KEY (team_name) REFERENCES teams(name)";

                tableCmd.ExecuteNonQuery();
            }
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
            //honors
            using (var tableCmd = connection.CreateCommand())
            {
                tableCmd.CommandText =
                    @$"CREATE TABLE IF NOT EXISTS honors (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        title VARCHAR(255),
                        year_received INT
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
            //playersHonors
            using (var tableCmd = connection.CreateCommand())
            {
                tableCmd.CommandText =
                    @$"CREATE TABLE IF NOT EXISTS players_honors (
                        player_id INT,
                        honor_id INT,
                        year_received INT,
                        PRIMARY KEY (player_id, honor_id),
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
                        player_id INT,
                        past_team_id INT,
                        start_year INT,
                        end_year INT,
                        PRIMARY KEY (player_id, past_team_id),
                        FOREIGN KEY (player_id) REFERENCES players(id),
                        FOREIGN KEY (past_team_id) REFERENCES past_teams(id)
                        )";

                tableCmd.ExecuteNonQuery();
            }

            connection.Close();
        }
    }
    internal void AddToTables(string connectionString)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            //players
            using (var tableCmd = connection.CreateCommand())
            {
                tableCmd.CommandText = @$"INSERT INTO players (id, first_name, last_name, position, height, weight, jersey_number, college, country, draft_year, draft_round, draft_number, team_full_name)
                    VALUES (@playerID, @firstName, @lastName, @playerPos, @height, @playerWeight, @jersey, @college, @country, @draftYear, @draftRound, @draftNumber, @teamFullName)";
                tableCmd.ExecuteNonQuery();
            }
            //teams
            using (var tableCmd = connection.CreateCommand())
            {
                tableCmd.CommandText = @$"INSERT INTO teams (id, conference, division, city, name, fullName, abbreviation)
                    VALUES (@teamId, @conference, @division, @city, @name, @fullName, @abbreviation)";
                tableCmd.ExecuteNonQuery();
            }
            //honors
            using (var tableCmd = connection.CreateCommand())
            {
                tableCmd.CommandText = @$"INSERT INTO honors (title, year_received)
                    VALUES (@title, @yearReceived)";
                tableCmd.ExecuteNonQuery();
            }
            //oastTTeams
            using (var tableCmd = connection.CreateCommand())
            {
                tableCmd.CommandText = @$"INSERT INTO past_teams (team_name, start_year, end_year)
                    VALUES (@teamName, @startYear, @endYear)";
                tableCmd.ExecuteNonQuery();
            }
            //playersHonors
            using (var tableCmd = connection.CreateCommand())
            {
                tableCmd.CommandText = @$"INSERT INTO players_honors (player_id, title_id, year_received)
                    VALUES (@playerId, @titleId, @yearReceived)";
                tableCmd.ExecuteNonQuery();
            }
            //playersPastTeams
            using (var tableCmd = connection.CreateCommand())
            {
                tableCmd.CommandText = @$"INSERT INTO players_pastteams (player_id, past_team_id, start_year, end_year)
                    VALUES (@playerId, @pastTeamId, @startYear, @endYear)";
                tableCmd.ExecuteNonQuery();
            }

            connection.Close();
        }
    }
}

