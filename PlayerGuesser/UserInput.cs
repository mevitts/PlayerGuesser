using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerGuesser;

    internal class UserInput
    {
        PlayerFetcher playerFetcher = new PlayerFetcher();

    internal static void ConfirmReady()
    {
        Console.WriteLine("@Welcome to the player guesser! Think of any current or former player from the NBA and I will try to guess it!" +
            "               \n\nPress any button to continue once you are ready.");
        Console.ReadKey();
    }
    }

