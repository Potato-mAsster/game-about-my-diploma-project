using UnityEngine;
using System.Data; 
using System.IO;   
using System.Collections.Generic; // Обязательно для List
using System.Data.SQLite; 

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; } 
    private IDbConnection dbConnection; 
    
    private string databaseFileName = "HypersomniaDB.db"; 
    /// <summary>
    /// Устанавливает ID текущего активного игрока.
    /// Этот метод должен быть вызван из других скриптов, когда игрок выбран/создан.
    /// </summary>
    public void SetCurrentPlayerId(int playerId) // <-- НОВЫЙ МЕТОД
    {
        CurrentPlayerId = playerId;
        Debug.Log($"DatabaseManager: Текущий активный игрок установлен на ID: {CurrentPlayerId}");
    }
    public int CurrentPlayerId { get; private set; } = -1; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeDatabase(); 
    }

    void OnDestroy()
    {
        CloseDb();
    }

    private void InitializeDatabase()
    {
        string dbPath = "";
        
        dbPath = "Data Source=" + Path.Combine(Application.dataPath, "StreamingAssets", databaseFileName);

        OpenDb(dbPath);
        CreateOrUpdateTables(); 
    }

    private void OpenDb(string path)
    {
        try
        {
            dbConnection = new SQLiteConnection(path); 
            
            dbConnection.Open();
            Debug.Log("Database opened: " + path);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка открытия соединения с БД: " + e.Message);
        }
    }

    private void CloseDb()
    {
        if (dbConnection != null && dbConnection.State == ConnectionState.Open)
        {
            dbConnection.Close();
            dbConnection.Dispose();
            dbConnection = null;
            Debug.Log("Database closed.");
        }
    }

    private void CreateOrUpdateTables()
    {
        // добавить лвлы (импорты)
        if (dbConnection == null || dbConnection.State != ConnectionState.Open)
        {
            Debug.LogError("Невозможно создать таблицы: соединение с БД не открыто.");
            return;
        }

        string[] createTableSqls = new string[]
        {
            @"CREATE TABLE IF NOT EXISTS Players (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                playerName TEXT NOT NULL UNIQUE,
                creationDate INTEGER,
                lastPlayedDate INTEGER
            );",
            @"CREATE TABLE IF NOT EXISTS Levels (
                id INTEGER PRIMARY KEY,
                levelName TEXT NOT NULL,
                sceneName TEXT NOT NULL,
                ""order"" INTEGER UNIQUE,
                description TEXT
            );",
            @"CREATE TABLE IF NOT EXISTS PlayerProgress (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                playerId INTEGER NOT NULL,
                levelId INTEGER NOT NULL,
                isUnlocked INTEGER DEFAULT 0,
                isCompleted INTEGER DEFAULT 0,
                bestTime REAL DEFAULT 0.0,
                score INTEGER DEFAULT 0,
                attempts INTEGER DEFAULT 0,
                lastPlayedTime INTEGER DEFAULT 0,
                FOREIGN KEY (playerId) REFERENCES Players(id) ON DELETE CASCADE,
                FOREIGN KEY (levelId) REFERENCES Levels(id) ON DELETE CASCADE,
                UNIQUE (playerId, levelId)
            );",
            @"CREATE TABLE IF NOT EXISTS UserSettings (
                playerId INTEGER PRIMARY KEY,
                soundVolume REAL DEFAULT 1.0,
                musicVolume REAL DEFAULT 1.0,
                resolutionWidth INTEGER DEFAULT 1920,
                FOREIGN KEY (playerId) REFERENCES Players(id) ON DELETE CASCADE
            );"
        };

        foreach (string sql in createTableSqls)
        {
            using (IDbCommand dbcmd = dbConnection.CreateCommand())
            {
                dbcmd.CommandText = sql;
                dbcmd.ExecuteNonQuery();
                Debug.Log($"Таблица создана или существует: {sql.Substring(0, Mathf.Min(sql.Length, 50))}...");
            }
        }
    }

    private void AddParameter(IDbCommand command, string name, object value)
    {
        command.Parameters.Add(new SQLiteParameter(name, value)); 
    }

    // --- Методы для работы с данными ---

    public int CreateNewPlayer(string playerName)
    {
        if (dbConnection == null || dbConnection.State != ConnectionState.Open)
        {
            Debug.LogError("Соединение с БД не открыто.");
            return -1;
        }

        if (DoesPlayerNameExist(playerName))
        {
            Debug.LogWarning($"Игрок с именем '{playerName}' уже существует.");
            return -2;
        }

        int newPlayerId = -1;
        IDbTransaction transaction = null;

        try
        {
            transaction = dbConnection.BeginTransaction();

            // 1. Вставка в Players
            string insertPlayerSql = "INSERT INTO Players (playerName, creationDate, lastPlayedDate) VALUES (@playerName, strftime('%s', 'now'), strftime('%s', 'now'));";
            using (IDbCommand command = dbConnection.CreateCommand())
            {
                command.CommandText = insertPlayerSql;
                command.Transaction = transaction;
                AddParameter(command, "@playerName", playerName);
                command.ExecuteNonQuery();
            }

            // 2. Получение ID нового игрока
            using (IDbCommand command = dbConnection.CreateCommand())
            {
                command.CommandText = "SELECT last_insert_rowid();";
                command.Transaction = transaction;
                newPlayerId = System.Convert.ToInt32(command.ExecuteScalar());
            }

            // 3. Инициализация UserSettings
            using (IDbCommand command = dbConnection.CreateCommand())
            {
                command.CommandText = "INSERT INTO UserSettings (playerId, soundVolume, musicVolume, resolutionWidth) VALUES (@playerId, 1.0, 1.0, 1920);";
                command.Transaction = transaction;
                AddParameter(command, "@playerId", newPlayerId);
                command.ExecuteNonQuery();
            }

            // 4. Получение ВСЕХ ID уровней и инициализация PlayerProgress для КАЖДОГО уровня
            int firstLevelId = -1; 
            List<int> allLevelIds = new List<int>();

            string selectAllLevelsSql = "SELECT id, \"order\" FROM Levels ORDER BY \"order\" ASC;";
            using (IDbCommand command = dbConnection.CreateCommand())
            {
                command.CommandText = selectAllLevelsSql;
                command.Transaction = transaction; 
                using (IDataReader reader = command.ExecuteReader()) 
                {
                    while (reader.Read())
                    {
                        int currentLevelId = reader.GetInt32(0); 
                        int currentLevelOrder = reader.GetInt32(1); 

                        allLevelIds.Add(currentLevelId);

                        if (currentLevelOrder == 1)
                        {
                            firstLevelId = currentLevelId;
                        }
                    }
                }
            }

            if (firstLevelId == -1)
            {
                Debug.LogError("Не удалось найти первый уровень (order = 1) в таблице Levels. Убедитесь, что таблица Levels заполнена и содержит уровень с order = 1.");
                transaction.Rollback();
                return -1;
            }

            string insertPlayerProgressSql = "INSERT INTO PlayerProgress (playerId, levelId, isUnlocked, isCompleted, bestTime, score, attempts, lastPlayedTime) VALUES (@playerId, @levelId, @isUnlocked, 0, 0.0, 0, 0, 0);";
            foreach (int levelId in allLevelIds)
            {
                bool isLevelUnlocked = (levelId == firstLevelId); // Разблокирован только первый уровень

                using (IDbCommand command = dbConnection.CreateCommand())
                {
                    command.CommandText = insertPlayerProgressSql;
                    command.Transaction = transaction;
                    AddParameter(command, "@playerId", newPlayerId);
                    AddParameter(command, "@levelId", levelId);
                    AddParameter(command, "@isUnlocked", isLevelUnlocked ? 1 : 0);
                    command.ExecuteNonQuery();
                }
            }
            
            transaction.Commit();
            CurrentPlayerId = newPlayerId;
            return newPlayerId;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка при создании нового игрока: " + e.Message);
            if (transaction != null) { transaction.Rollback(); } 
            return -1;
        }
    }

    public bool DoesPlayerNameExist(string playerName)
    {
        if (dbConnection == null || dbConnection.State != ConnectionState.Open) return false;

        string sql = "SELECT COUNT(id) FROM Players WHERE playerName = @playerName;";
        using (IDbCommand command = dbConnection.CreateCommand())
        {
            command.CommandText = sql;
            AddParameter(command, "@playerName", playerName);
            return System.Convert.ToInt32(command.ExecuteScalar()) > 0;
        }
    }

    public string GetSceneNameForLevel(int levelId)
    {
        if (dbConnection == null || dbConnection.State != ConnectionState.Open) return null;

        string sceneName = null;
        string sql = "SELECT sceneName FROM Levels WHERE id = @levelId;";
        using (IDbCommand command = dbConnection.CreateCommand())
        {
            command.CommandText = sql;
            AddParameter(command, "@levelId", levelId);
            object result = command.ExecuteScalar();
            if (result != null)
            {
                sceneName = result.ToString();
            }
        }
        return sceneName;
    }

    // НОВЫЙ МЕТОД: Получение списка всех игроков
    public List<PlayerData> GetAllPlayers() // <-- ДОБАВЛЕНО
    {
        List<PlayerData> players = new List<PlayerData>();
        if (dbConnection == null || dbConnection.State != ConnectionState.Open)
        {
            Debug.LogError("Соединение с БД не открыто. Невозможно получить список игроков.");
            return players;
        }

        string sql = "SELECT id, playerName, creationDate, lastPlayedDate FROM Players ORDER BY lastPlayedDate DESC;"; // Сортируем по дате последней игры
        using (IDbCommand command = dbConnection.CreateCommand())
        {
            command.CommandText = sql;
            using (IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    players.Add(new PlayerData
                    {
                        id = reader.GetInt32(0),
                        playerName = reader.GetString(1),
                        creationDate = reader.GetInt64(2), 
                        lastPlayedDate = reader.GetInt64(3)
                    });
                }
            }
        }
        return players;
    }

    // НОВЫЙ МЕТОД: Получение данных о последнем незавершенном разблокированном уровне игрока
    public PlayerProgressData GetPlayerLastUnlockedLevel(int playerId) // <-- ДОБАВЛЕНО
    {
        if (dbConnection == null || dbConnection.State != ConnectionState.Open)
        {
            Debug.LogError("Соединение с БД не открыто. Невозможно получить прогресс игрока.");
            return null;
        }

        PlayerProgressData progressData = null;

        // Ищем первый разблокированный и НЕ завершенный уровень.
        // Если все завершены, то берем последний завершенный уровень (с самым большим order).
        string sql = @"
            SELECT pp.id, pp.playerId, pp.levelId, pp.isUnlocked, pp.isCompleted, pp.bestTime, pp.score, pp.attempts, pp.lastPlayedTime
            FROM PlayerProgress pp
            JOIN Levels l ON pp.levelId = l.id
            WHERE pp.playerId = @playerId 
            ORDER BY 
                CASE 
                    WHEN pp.isCompleted = 0 AND pp.isUnlocked = 1 THEN 0 -- Приоритет 1: разблокирован, но не завершен
                    ELSE 1                                            -- Приоритет 2: остальные (завершенные или заблокированные)
                END,
                l.""order"" ASC; -- Сортируем по порядку уровня, чтобы получить самый ранний из приоритетных
        ";
        // Важно: Этот запрос вернет *первый* уровень, который соответствует условиям сортировки.
        // Если вы хотите, чтобы игрок продолжил именно с *последнего* сыгранного уровня (даже если он его завершил),
        // логика запроса может быть другой (например, "ORDER BY lastPlayedTime DESC LIMIT 1").
        // Для простоты, я оставил логику "продолжить с первого незавершенного разблокированного".
        
        using (IDbCommand command = dbConnection.CreateCommand())
        {
            command.CommandText = sql;
            AddParameter(command, "@playerId", playerId);

            using (IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read()) // Читаем только первую строку, т.к. она будет нашим целевым уровнем
                {
                    progressData = new PlayerProgressData
                    {
                        id = reader.GetInt32(0),
                        playerId = reader.GetInt32(1),
                        levelId = reader.GetInt32(2),
                        isUnlocked = reader.GetInt32(3) == 1, // Преобразуем INTEGER в bool
                        isCompleted = reader.GetInt32(4) == 1, // Преобразуем INTEGER в bool
                        bestTime = reader.GetFloat(5),
                        score = reader.GetInt32(6),
                        attempts = reader.GetInt32(7),
                        lastPlayedTime = reader.GetInt64(8)
                    };
                }
            }
        }
        // Если никаких разблокированных/завершенных уровней нет (т.е. прогресс игрока пуст),
        // можно вернуть null и обработать это в MainMenuController (например, загрузить первый уровень).
        return progressData;
    }


    // Классы для представления данных (можете сделать их в отдельных файлах)
    public class PlayerData
    {
        public int id;
        public string playerName;
        public long creationDate;
        public long lastPlayedDate;
    }

    public class LevelData
    {
        public int id;
        public string levelName;
        public string sceneName;
        public int order;
        public string description;
    }

    public class PlayerProgressData
    {
        public int id;
        public int playerId;
        public int levelId;
        public bool isUnlocked;
        public bool isCompleted;
        public float bestTime;
        public int score;
        public int attempts;
        public long lastPlayedTime;
    }

    public class UserSettingsData
    {
        public int playerId;
        public float soundVolume;
        public float musicVolume;
        public int resolutionWidth;
    }
}