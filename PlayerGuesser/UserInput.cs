using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerGuesser;

    internal class UserInput
    {
        internal static PlayerFetcher playerFetcher = new PlayerFetcher();
    internal static Team team = new Team();

    internal static async void ConfirmReady(DatabaseManager databaseManager, string connectionString)
    {
        Console.WriteLine("@Welcome to the player guesser! Think of any current or former player from the NBA and I will try to guess it!" +
            "               \n\nPress any button to continue once you are ready.");
        Console.ReadKey();
        //List<Player> playersTask = playerFetcher.GetPlayers().Result;
        //var players = playersTask;
        //List<Team> teams = team.GetTeams(players);
        //databaseManager.AddToTables(connectionString, players, teams);
        

    }

    }

