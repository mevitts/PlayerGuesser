using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PlayerGuesser;

public class Question
{
    public string Text { get; set; }
    public Func<Player, bool> Predicate { get; set; }
    public string Query { get; set; }
    public string NegQuery { get; set; }
    public int Weight { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public int YesCount { get; set; }
    public int NoCount { get; set; }
    public double Entropy { get; set; }

    public async Task GetYesNoCount(string connectionString, List<Player> filteredPlayers)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            await connection.OpenAsync();
            if (!filteredPlayers.Any())
            {
                throw new InvalidOperationException("The filteredPlayers list is empty.");
            }
            //counts the number of players that match the query of the question and also matches an id in the filtered players list
            string query = $"SELECT COUNT(*) FROM players WHERE {Query.Substring(28)} AND id IN ({string.Join(",", filteredPlayers.Select(p => p.id))})";

            using (var command = new MySqlCommand(query, connection))
            {
                if (this.Parameters != null)
                {
                    var firstParam = this.Parameters.FirstOrDefault();

                    if (firstParam.Value != null)
                    {
                        command.Parameters.AddWithValue("@" + firstParam.Key, firstParam.Value);
                    }
                }
                var result = await command.ExecuteScalarAsync();

                YesCount = Convert.ToInt32(result);
            }//counts the yescount using the query
        }//connects to db

        using (var connection = new MySqlConnection(connectionString))
        {
            await connection.OpenAsync();
            string negQuery;
            if (this.NegQuery.Substring(0, 28) == "SELECT * FROM players p LEFT")
            {
                negQuery = $"SELECT COUNT(*) FROM players {NegQuery.Substring(22)} AND p.id IN ({string.Join(",", filteredPlayers.Select(p => p.id))})";

            }//changes if it has this format of question that makes id ambiguous
            else
            {
                negQuery = $"SELECT * FROM players WHERE {NegQuery.Substring(28)} AND id IN ({string.Join(",", filteredPlayers.Select(p => p.id))})";
            }//standard negQuery

            //need to find a way to not have that id be ambiguous for the 2 differnet types of negqueries
            using (var command = new MySqlCommand(negQuery, connection))
            {
                if (this.Parameters != null)
                {
                    var firstParam = this.Parameters.FirstOrDefault();
                    if (firstParam.Value != null)
                    {
                        command.Parameters.AddWithValue("@" + firstParam.Key, firstParam.Value);
                    }
                }
                var result = await command.ExecuteScalarAsync();

                NoCount = Convert.ToInt32(result);
            }
        }
    }//gets Yes and No count of each question

    internal bool RelevantQuestion()
    {
        if (Double.IsNaN(Entropy) && YesCount!= 1) return false;
        else return true;
    }
}


