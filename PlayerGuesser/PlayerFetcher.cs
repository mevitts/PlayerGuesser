using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace PlayerGuesser
{
    internal class PlayerFetcher
    {
        string API_KEY = "7db6e40f-a4da-4c9d-8cd1-f86f5803b6ac";
        public async Task<List<Player>> GetPlayers()
        {
            try
            {
                var client = new RestClient("https://api.balldontlie.io/v1/");
                var request = new RestRequest($"players");
                request.AddHeader("Authorization", $"{API_KEY}");

                var response = await client.ExecuteAsync<Players>(request);

                List<Player> players = new();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //if status is ok store in string
                    string rawResponse = response.Content;
                    var serialize = JsonConvert.DeserializeObject<Players>(rawResponse);

                    players = serialize.PlayersList;

                    foreach (Player player in players)
                    {
                        String name = player.first_name + "_" + player.last_name;
                        int newID = await GetPlayerNewID(player.id, name);
                        player.Honors = await GetPlayerHonors(newID);
                        player.PastTeams = await GetPlayerPastTeams(newID);
                    }
                    return players;
                }
                else
                {
                    Console.WriteLine("Error: API req failed");
                    return new List<Player>() ;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new List<Player>();
            }
        }

        public async Task<int> GetPlayerNewID(int id, String name)
        {
            var client = new RestClient("https://thesportsdb.com/api/v1/json/3/");
            var request = new RestRequest($"searchplayers.php?p={name}");

            var response = await client.ExecuteAsync(request);
            //need to add assurance that not a duplicate
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var rawResponse = response.Content;
                dynamic responseObject = JsonConvert.DeserializeObject(rawResponse);
                if (responseObject != null && responseObject.player != null)
                {
                    string idPlayer = responseObject.player[0]?.idPlayer;

                    if (int.TryParse(idPlayer, out int newID))
                    {
                        return newID;
                    }
                    else
                    {
                        Console.WriteLine("Unable to parse player ID");
                        return -1;
                    }
                }
                else
                {
                    return -1;
                }
            }
            return -1;
        }
        public async Task<List<PastTeam>> GetPlayerPastTeams(int id)
        {
            var client = new RestClient("https://thesportsdb.com/api/v1/json/3/");
            var request = new RestRequest($"lookupformerteams.php?id={id}");
           
            var response = await client.ExecuteAsync<PastTeamRoot>(request);

            List<PastTeam> pastTeams = new();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string rawResponse = response.Content;
                var serialize = JsonConvert.DeserializeObject<PastTeamRoot>(rawResponse);

                //serialize is the rawResponse converted into a PastTeamRoot object, which is why it is serialize.PastTeamList
                pastTeams = serialize.PastTeamList;
                return pastTeams;
            }
            return pastTeams;
        }

        public async Task<List<Honor>> GetPlayerHonors(int id)
        {
            var client = new RestClient("https://thesportsdb.com/api/v1/json/3/");
            var request = new RestRequest($"lookuphonours.php?id={id}");

            var response = await client.ExecuteAsync<Honors>(request);

            List<Honor> honours = new();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string rawResponse = response.Content;
                var serialize = JsonConvert.DeserializeObject<Honors>(rawResponse);

                honours = serialize.HonorsList;
                return honours;
            }
            return honours;
        }
    }
}
