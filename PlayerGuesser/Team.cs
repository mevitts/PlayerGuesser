using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
