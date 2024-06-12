using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Mysqlx.Cursor;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers;

namespace PlayerGuesser
{
    internal class PlayerFetcher
    {
        private readonly TokenBucket bucket = new TokenBucket();
        private readonly TokenBucket sharedBucket = new TokenBucket();
        string API_KEY = "7db6e40f-a4da-4c9d-8cd1-f86f5803b6ac";
        public async Task<List<Player>> GetPlayers()
        {
            try
            {
                int? cursor = 0;
                bucket.CreateBucket(30, 30);
                sharedBucket.CreateBucket(100, 100);

                List<Player> players = new List<Player>();

                while (cursor != -1)
                {
                    await bucket.HandleRequest(bucket);

                    var client = new RestClient("https://api.balldontlie.io/v1/");
                    var request = new RestRequest($"players?cursor={cursor}");
                    request.AddHeader("Authorization", $"{API_KEY}");

                    var response = await client.ExecuteAsync<Players>(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        //if status is ok store in string
                        string rawResponse = response.Content;
                        var serialize = JsonConvert.DeserializeObject<Players>(rawResponse);
                        int? nextCursor = serialize.MetaData.next_cursor;
                        List<Player> temp = serialize.PlayersList;
                        foreach (Player player in temp)
                        {
                            String name = player.first_name + "_" + player.last_name;

                            await sharedBucket.HandleRequest(sharedBucket);
                            int newID = await GetPlayerNewID(player.id, name);
                            player.Honors = await GetPlayerHonors(newID);
                            player.PastTeams = await GetPlayerPastTeams(newID);

                        }
                        Console.WriteLine($"Working on page {cursor}");
                        players.AddRange(temp);
                        //if cursor is not null, ok. If it is, then it is -1

                        if (nextCursor == 0)
                        {
                            cursor = -1;
                        }
                        else
                        {
                            cursor = nextCursor;
                        }
                    }

                    else
                    {
                        Console.WriteLine("Error: API req failed");
                        return new List<Player>();
                    }
                }
                return players;
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
        //uses id 
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
