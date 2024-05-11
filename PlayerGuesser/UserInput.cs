using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerGuesser;

    internal class UserInput
    {
        internal static PlayerFetcher playerFetcher = new PlayerFetcher();

    internal static void ConfirmReady()
    {
        Console.WriteLine("@Welcome to the player guesser! Think of any current or former player from the NBA and I will try to guess it!" +
            "               \n\nPress any button to continue once you are ready.");
        Console.ReadKey();
        List<Player> players = playerFetcher.GetPlayers().Result;
        foreach (Player p in players)
        {
            /*var honorList = "";
            if (p.Honors != null)
            {
                foreach (Honor h in p.Honors)
                {
                    honorList += $"{h.strHonour} ,";
                }
            }*/
            var pastTeamList = "";
            if (p.PastTeams != null)
            {
                foreach (PastTeam pt in p.PastTeams)
                {
                    pastTeamList += $"{pt.strFormerTeam} ";
                }
            }
            Console.WriteLine($"{p.first_name} {p.last_name}, {pastTeamList}");
        }
    }

    }

