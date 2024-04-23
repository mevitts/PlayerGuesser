using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace PlayerGuesser
{
    internal class PlayerFetcher
    {
        string API_KEY = "-7db6e40f-a4da-4c9d-8cd1-f86f5803b6ac ";
        public async Task<List<Player>> getPlayers()
        {
            var client = new RestClient("https://api.balldontlie.io/v1/");
            var request = new RestRequest($"players");
            request.AddHeader("Authorization:", $" {API_KEY}");

            var response = await client.ExecuteAsync<Players>(request);

            List<Player> players = new();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //if status is ok store in string
                string rawResponse = response.Content;
                var serialize = JsonConvert.DeserializeObject<Players>(rawResponse);

                players = serialize.PlayersList;

                foreach ( Player player in players )
                {
                    getPlayerHonors(player.id);
                }
                foreach ( Player player in players )
                {
                    getPlayerPastTeams(player.id);
                }
                return players;
            }
            return players;
        }

        private void getPlayerPastTeams(int id)
        {
            throw new NotImplementedException();
        }

        private void getPlayerHonors(int id)
        {
            throw new NotImplementedException();
        }
    }
}
