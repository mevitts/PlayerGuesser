﻿using System.Configuration;

namespace PlayerGuesser;

class Program
{
    static void Main(string[] args)
    {
        string connectionString = ConfigurationManager.AppSettings.Get("ConnectionString");

        DatabaseManager databaseManager = new DatabaseManager();
        databaseManager.CreateTables(connectionString);
        var gameProcess = new GameProcess();
        gameProcess.StartGame();
    }
}
