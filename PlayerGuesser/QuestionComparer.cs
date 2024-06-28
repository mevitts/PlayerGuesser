using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerGuesser;


internal class QuestionComparer : IEqualityComparer<Question>
{
    public bool Equals(Question x, Question y)
    {
        if (x == null && y == null)
            return true;

        if (x == null || y == null)
            return false;

        return x.Text == y.Text &&
               x.Query == y.Query &&
               x.NegQuery == y.NegQuery &&
               x.Weight == y.Weight;
    }
    public int GetHashCode(Question obj)
    {
        if (obj == null)
            return 0;

        int hash = obj.Text.GetHashCode();
        hash = (hash * 397) ^ obj.Query.GetHashCode();
        hash = (hash * 397) ^ obj.NegQuery.GetHashCode();
        hash = (hash * 397) ^ obj.Weight.GetHashCode();

        return hash;
    }

}
