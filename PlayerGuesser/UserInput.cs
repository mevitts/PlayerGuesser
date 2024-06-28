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

    internal static async Task ConfirmReady(string connectionString)
    {
        Console.WriteLine("@Welcome to the player guesser! Think of any current or former player from the NBA and I will try to guess it!" +
            "               \n\nPress any button to continue once you are ready.");
        Console.ReadKey();
    }

    internal static Task<bool> AskQuestion(Question question)
    {
        Console.WriteLine($"\n{question.Text}. Press 'y' key for yes and 'n' for no.");

        var response = Console.ReadKey();

        while (response.Key != ConsoleKey.Y && response.Key != ConsoleKey.N) 
        {
            Console.WriteLine("\nInvalid input, please press y or n key");
            response = Console.ReadKey();
        }//validates response

        if (response.Key == ConsoleKey.Y)
        { 
            return Task.FromResult(true);

        }//if press yes

        else     
        {
            return Task.FromResult(false);

        }//if press no
        
    }//returns boolean based on user response

}

