using Newtonsoft.Json;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerGuesser
{
    public class Players
    {
        [JsonProperty("data")]
        public List<Player> PlayersList { get; set; }

        [JsonProperty("meta")]
        public Meta MetaData { get; set; }
    }
    public class Player
    {
        public int id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string position { get; set; }
        public string height { get; set; }
        public string weight { get; set; }
        public string jersey_number { get; set; }
        public string college { get; set; }
        public string country { get; set; }
        public int? draft_year { get; set; }
        public int? draft_round { get; set; }
        public int? draft_number { get; set; }
        public Team team { get; set; }
        public List<PastTeam>? PastTeams { get; set; }
        public List<Honor>? Honors { get; set; }
    }
    public class PastTeam
    {
        public string strFormerTeam { get; set; }
        public string strJoined { get; set; }
        public string strDeparted { get; set; }
    }
    public class PastTeamRoot
    {
        [JsonProperty("formerteams")]
        public List<PastTeam> PastTeamList { get; set; }
    }
    public class Honor
    {
        public string strHonour { get; set; }
        public string strSeason { get; set; }
        public string strTeam {  get; set; }
    }
    public class Honors
    {
        [JsonProperty("honours")]
        public List<Honor> HonorsList { get; set; }
    }
    public class Meta
    {
        public int next_cursor { get; set; }
    }
}
