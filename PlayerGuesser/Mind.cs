using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerGuesser;

internal class Mind
{
    internal double LogBaseTwo(double x)
    {
        return Math.Log(x) / Math.Log(2);
    }
    internal async Task<Question> SelectBestQuestion(List<Question> questions, string connectionString, List<Player> filteredPlayers)
    {
        List<Question> TopFive = new List<Question>(5);
        ListShuffler.Shuffle(questions);

        for (int i = 0;i < questions.Count && TopFive.Count < 6; i++)
        {
            await CalculateEntropy(questions[i], connectionString, filteredPlayers);
            if (questions[i].RelevantQuestion())
            {
                TopFive.Add(questions[i]);
            }
        }//calculates initial entropy and adds to topfive OG

        TopFive.Sort((q1, q2) => q1.Entropy.CompareTo(q2.Entropy));//sorts topfive to have [0] as 

        if (questions.Count < 5)
        {
            foreach (Question question in questions)
            {
                var entropyScore = await CalculateEntropy(question, connectionString, filteredPlayers);

                if (entropyScore > TopFive[0].Entropy && question.RelevantQuestion())
                {
                    TopFive[0] = question;
                    TopFive.Sort((q1, q2) => q1.Entropy.CompareTo(q2.Entropy));

                }// replace the question with lowest entropy in TopFive and then sorts

            }//skips over first 5 elements and the iterates 
        }
        else
        {
            foreach (Question question in questions.Skip(TopFive.Count))
            {
                var entropyScore = await CalculateEntropy(question, connectionString, filteredPlayers);

                if (entropyScore > TopFive[0].Entropy && question.RelevantQuestion())
                {
                    TopFive[0] = question;
                    TopFive.Sort((q1, q2) => q1.Entropy.CompareTo(q2.Entropy));

                }// replace the question with lowest entropy in TopFive and then sorts

            }//skips over first 5 elements and the iterates 
        }//if questions is over 5 questions
        var rand = new Random();
        int chooser = rand.Next(TopFive.Count);//getting random of the top 5 questions

        return TopFive[chooser];

    }//this gets the best question from the given list of questions: This may not be for evert single question, but will probably go by stages so that 
    internal async Task<double> CalculateEntropy(Question question, string connectionString, List<Player> filteredPlayers)
    {
        await question.GetYesNoCount(connectionString, filteredPlayers);

        var total = question.YesCount + question.NoCount;
        if (total == 0) return 0;

        double YesProb = (double)question.YesCount / total;
        double NoProb = (double)question.NoCount / total;

        double entropy = ((-YesProb) * LogBaseTwo(YesProb)) - ((-NoProb) * LogBaseTwo(NoProb));
        entropy *= question.Weight;
        question.Entropy = entropy;

        return entropy;
    }//calculates entropy of each question


}
