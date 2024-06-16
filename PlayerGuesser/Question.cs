using System;
using System.Collections.Generic;
using System.Linq;
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
}


