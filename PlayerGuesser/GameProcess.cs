using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerGuesser;
internal class GameProcess
{
    private readonly Mind mind;

    public GameProcess()
    {
        mind = new Mind();
    }
    internal void StartGame(DatabaseManager databaseManager, string connectionString)
    {
        UserInput.ConfirmReady(databaseManager, connectionString);
    }
}

