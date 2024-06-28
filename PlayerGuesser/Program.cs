using System.Configuration;

namespace PlayerGuesser;

class Program
{
    static async Task Main(string[] args)
    {
        string connectionString = ConfigurationManager.AppSettings.Get("ConnectionString");

        var gameProcess = new GameProcess();

        await UserInput.ConfirmReady(connectionString);

        await gameProcess.OnStart(connectionString);

    }
}
