using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class DatabaseTest : MonoBehaviour
{
    void Start()
    {
        string dbPath = Path.Combine(Application.persistentDataPath, "HypersomniaDB.db");
        if (SimpleSQLite.Open(dbPath))
        {
            Debug.Log("База данных успешно открыта.");

            // SQL-запросы для создания таблиц
            string createLevelsTableQuery = @"
            CREATE TABLE IF NOT EXISTS Levels (
                id INTEGER PRIMARY KEY,
                levelName TEXT NOT NULL,
                sceneName TEXT NOT NULL,
                ""order"" INTEGER,
                description TEXT
            );";

            string createPlayerProgressTableQuery = @"
            CREATE TABLE IF NOT EXISTS PlayerProgress (
                playerId INTEGER,
                levelId INTEGER,
                isUnlocked INTEGER DEFAULT 0,
                isCompleted INTEGER DEFAULT 0,
                bestTime REAL,
                highScore INTEGER,
                attempts INTEGER DEFAULT 0,
                lastPlayedTime INTEGER,
                PRIMARY KEY(playerId,levelId),
                FOREIGN KEY(levelId) REFERENCES Levels(id),
                FOREIGN KEY(playerId) REFERENCES Players(id)
            );";

            string createPlayersTableQuery = @"
            CREATE TABLE IF NOT EXISTS Players (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                playerName TEXT,
                creationDate INTEGER DEFAULT (strftime('%s', 'now')),
                lastPlayedDate INTEGER
            );";

            string createUserSettingsTableQuery = @"
            CREATE TABLE IF NOT EXISTS UserSettings (
                playerId INTEGER PRIMARY KEY,
                musicVolume REAL DEFAULT 1.0,
                resolutionWidth INTEGER DEFAULT 1920,
                resolutionHeight INTEGER DEFAULT 1080,
                isFullscreen INTEGER DEFAULT 1,
                language TEXT DEFAULT 'en',
                controlMapping TEXT,
                FOREIGN KEY(playerId) REFERENCES Players(id)
            );";

            // Выполнение SQL-запросов
            SimpleSQLite.ExecuteQuery(createLevelsTableQuery);
            Debug.Log("Таблица Levels создана (или уже существовала).");

            SimpleSQLite.ExecuteQuery(createPlayersTableQuery);
            Debug.Log("Таблица Players создана (или уже существовала).");

            SimpleSQLite.ExecuteQuery(createPlayerProgressTableQuery);
            Debug.Log("Таблица PlayerProgress создана (или уже существовала).");

            SimpleSQLite.ExecuteQuery(createUserSettingsTableQuery);
            Debug.Log("Таблица UserSettings создана (или уже существовала).");

            SimpleSQLite.Close();
            Debug.Log("База данных закрыта.");
        }
        else
        {
            Debug.LogError("Не удалось открыть базу данных.");
        }
    }
}