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

    // --- НОВЫЙ БЛОК: Вставка стартовых данных для Levels (если их еще нет) ---
    Debug.Log("Попытка вставить начальные данные в таблицу Levels...");

    // Используем INSERT OR IGNORE, чтобы не вставлять записи, если они уже существуют (по id или order UNIQUE)
    string[] insertLevelSqls = new string[]
    {
        "INSERT OR IGNORE INTO Levels (id, levelName, sceneName, \"order\", description) VALUES (1, 'Первый Уровень', 'Level0', 1, 'Это самый первый уровень игры.');",
        "INSERT OR IGNORE INTO Levels (id, levelName, sceneName, \"order\", description) VALUES (2, 'Второй Уровень', 'Level1', 2, 'Продолжение приключения.');",
        "INSERT OR IGNORE INTO Levels (id, levelName, sceneName, \"order\", description) VALUES (3, 'Третий Уровень', 'Level2', 3, 'Еще более сложные испытания.');",
        "INSERT OR IGNORE INTO Levels (id, levelName, sceneName, \"order\", description) VALUES (4, 'Четвертый Уровень', 'Level3', 4, 'Приближаемся к финалу.');",
        "INSERT OR IGNORE INTO Levels (id, levelName, sceneName, \"order\", description) VALUES (5, 'Пятый Уровень', 'Level4', 5, 'Финальное сражение!');"
        // Добавьте сюда другие уровни, если они у вас есть
    };

    foreach (string sql in insertLevelSqls)
    {
        using (IDbCommand dbcmd = dbConnection.CreateCommand())
        {
            dbcmd.CommandText = sql;
            // ExecuteNonQuery() возвращает количество затронутых строк. 0, если IGNORE сработал.
            int rowsAffected = dbcmd.ExecuteNonQuery(); 
            if (rowsAffected > 0)
            {
                Debug.Log($"Уровень вставлен: {sql.Substring(0, Mathf.Min(sql.Length, 50))}...");
            }
            else
            {
                //Debug.Log($"Уровень уже существует или не вставлен: {sql.Substring(0, Mathf.Min(sql.Length, 50))}...");
            }
        }
    }
    Debug.Log("Завершено добавление/проверка начальных данных в Levels.");
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

    /// <summary>
    /// Получает информацию об уровне по имени сцены.
    /// </summary>
    public LevelData GetLevelDataBySceneName(string sceneName) // <-- НОВЫЙ МЕТОД
    {
        if (dbConnection == null || dbConnection.State != ConnectionState.Open)
        {
            Debug.LogError("Соединение с БД не открыто. Невозможно получить данные уровня.");
            return null;
        }

        LevelData level = null;
        string sql = "SELECT id, levelName, sceneName, \"order\", description FROM Levels WHERE sceneName = @sceneName;";
        using (IDbCommand command = dbConnection.CreateCommand())
        {
            command.CommandText = sql;
            AddParameter(command, "@sceneName", sceneName);
            using (IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    level = new LevelData
                    {
                        id = reader.GetInt32(0),
                        levelName = reader.GetString(1),
                        sceneName = reader.GetString(2),
                        order = reader.GetInt32(3),
                        description = reader.GetString(4)
                    };
                }
            }
        }
        return level;
    }

    /// <summary>
    /// Получает информацию об уровне по его порядку.
    /// </summary>
    public LevelData GetLevelDataByOrder(int order) // <-- НОВЫЙ МЕТОД
    {
        if (dbConnection == null || dbConnection.State != ConnectionState.Open)
        {
            Debug.LogError("Соединение с БД не открыто. Невозможно получить данные уровня.");
            return null;
        }

        LevelData level = null;
        string sql = "SELECT id, levelName, sceneName, \"order\", description FROM Levels WHERE \"order\" = @order;";
        using (IDbCommand command = dbConnection.CreateCommand())
        {
            command.CommandText = sql;
            AddParameter(command, "@order", order);
            using (IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    level = new LevelData
                    {
                        id = reader.GetInt32(0),
                        levelName = reader.GetString(1),
                        sceneName = reader.GetString(2),
                        order = reader.GetInt32(3),
                        description = reader.GetString(4)
                    };
                }
            }
        }
        return level;
    }

    /// <summary>
    /// Устанавливает статус завершения уровня для игрока и обновляет статистику уровня (очки, время).
    /// Попытки управляются отдельно через IncrementLevelAttempts.
    /// </summary>
    public void SetLevelCompleted(int playerId, int levelId, bool completed, float newBestTime, int newScore)
    {
        if (dbConnection == null || dbConnection.State != ConnectionState.Open)
        {
            Debug.LogError("Соединение с БД не открыто. Невозможно обновить прогресс уровня.");
            return;
        }

        // 1. Получаем текущие данные прогресса для этого уровня.
        // Если запись уже есть, мы ее обновим; если нет, создадим новую.
        PlayerProgressData currentProgress = GetPlayerProgressForLevel(playerId, levelId);

        // Инициализируем значения для обновления
        float finalBestTime = currentProgress != null ? currentProgress.bestTime : 0.0f;
        int finalScore = currentProgress != null ? currentProgress.score : 0;
        int totalAttempts = currentProgress != null ? currentProgress.attempts : 0; // Попытки не меняются здесь
        bool isUnlockedStatus = currentProgress != null ? currentProgress.isUnlocked : false; // Сохраняем статус разблокировки


        if (newBestTime > 0) // Если новое время валидно
        {
            if (finalBestTime == 0.0f || newBestTime < finalBestTime) // Если старого времени не было (0) или новое время лучше
            {
                finalBestTime = newBestTime;
            }
        }
        
        if (newScore > finalScore) // Если новый счет лучше старого
        {
            finalScore = newScore;
        }

        // Определяем, нужно ли вставлять новую запись или обновлять существующую
        if (currentProgress == null)
        {
            // ЗАПИСИ НЕТ: Вставляем новую запись (например, если игрок почему-то не был инициализирован)
            // Примечание: CreateNewPlayer должен был создать запись, так что этот сценарий маловероятен.
            string insertSql = @"
                INSERT INTO PlayerProgress 
                (playerId, levelId, isUnlocked, isCompleted, bestTime, score, attempts, lastPlayedTime) 
                VALUES (@playerId, @levelId, @isUnlocked, @isCompleted, @finalBestTime, @finalScore, @totalAttempts, strftime('%s', 'now'));";

            using (IDbCommand command = dbConnection.CreateCommand())
            {
                command.CommandText = insertSql;
                AddParameter(command, "@playerId", playerId);
                AddParameter(command, "@levelId", levelId);
                AddParameter(command, "@isUnlocked", isUnlockedStatus ? 1 : 0); // По умолчанию разблокирован, если нет старой записи
                AddParameter(command, "@isCompleted", completed ? 1 : 0);
                AddParameter(command, "@finalBestTime", finalBestTime);
                AddParameter(command, "@finalScore", finalScore);
                AddParameter(command, "@totalAttempts", totalAttempts); // Пока 0, будет обновляться IncrementLevelAttempts
                command.ExecuteNonQuery();
                Debug.Log($"Прогресс игрока {playerId} для уровня {levelId} ВСТАВЛЕН: Завершено={completed}, Лучшее время={finalBestTime}, Счет={finalScore}, Попытки={totalAttempts}.");
            }
        }
        else
        {
            // ЗАПИСЬ СУЩЕСТВУЕТ: Обновляем ее
            string updateSql = @"
                UPDATE PlayerProgress 
                SET isCompleted = @isCompleted, 
                    bestTime = @finalBestTime, 
                    score = @finalScore, 
                    lastPlayedTime = strftime('%s', 'now') 
                WHERE playerId = @playerId AND levelId = @levelId;";

            using (IDbCommand command = dbConnection.CreateCommand())
            {
                command.CommandText = updateSql;
                AddParameter(command, "@isCompleted", completed ? 1 : 0);
                AddParameter(command, "@finalBestTime", finalBestTime);
                AddParameter(command, "@finalScore", finalScore);
                AddParameter(command, "@playerId", playerId);
                AddParameter(command, "@levelId", levelId);
                command.ExecuteNonQuery();
                Debug.Log($"Прогресс игрока {playerId} для уровня {levelId} ОБНОВЛЕН: Завершено={completed}, Лучшее время={finalBestTime}, Счет={finalScore}.");
            }
        }
    }

    /// <summary>
    /// Увеличивает количество попыток для прохождения уровня для данного игрока.
    /// Этот метод нужно вызывать, например, при каждой новой попытке или смерти.
    /// </summary>
    public void IncrementLevelAttempts(int playerId, int levelId) 
    {
        if (dbConnection == null || dbConnection.State != ConnectionState.Open)
        {
            Debug.LogError("Соединение с БД не открыто. Невозможно увеличить попытки.");
            return;
        }

        PlayerProgressData currentProgress = GetPlayerProgressForLevel(playerId, levelId);
        int newAttempts = (currentProgress != null ? currentProgress.attempts : 0) + 1;

        if (currentProgress == null)
        {
            // ЗАПИСИ НЕТ: Вставляем новую запись с увеличенными попытками
            string insertSql = @"
                INSERT INTO PlayerProgress 
                (playerId, levelId, isUnlocked, isCompleted, bestTime, score, attempts, lastPlayedTime) 
                VALUES (@playerId, @levelId, 0, 0, 0.0, 0, @newAttempts, strftime('%s', 'now'));";

            using (IDbCommand command = dbConnection.CreateCommand())
            {
                command.CommandText = insertSql;
                AddParameter(command, "@playerId", playerId);
                AddParameter(command, "@levelId", levelId);
                AddParameter(command, "@newAttempts", newAttempts);
                command.ExecuteNonQuery();
                Debug.Log($"Попытки игрока {playerId} для уровня {levelId} ВСТАВЛЕНЫ: {newAttempts}.");
            }
        }
        else
        {
            // ЗАПИСЬ СУЩЕСТВУЕТ: Обновляем только попытки и lastPlayedTime
            string updateSql = @"
                UPDATE PlayerProgress 
                SET attempts = @newAttempts, 
                    lastPlayedTime = strftime('%s', 'now') 
                WHERE playerId = @playerId AND levelId = @levelId;";

            using (IDbCommand command = dbConnection.CreateCommand())
            {
                command.CommandText = updateSql;
                AddParameter(command, "@newAttempts", newAttempts);
                AddParameter(command, "@playerId", playerId);
                AddParameter(command, "@levelId", levelId);
                command.ExecuteNonQuery();
                Debug.Log($"Попытки игрока {playerId} для уровня {levelId} ОБНОВЛЕНЫ до {newAttempts}.");
            }
        }
    }

    /// <summary>
    /// Получает текущую запись прогресса для конкретного игрока и уровня.
    /// Это вспомогательный метод для SetLevelCompleted и IncrementLevelAttempts.
    /// </summary>
    public PlayerProgressData GetPlayerProgressForLevel(int playerId, int levelId) 
    {
        // ... (код метода без изменений) ...
        if (dbConnection == null || dbConnection.State != ConnectionState.Open)
        {
            Debug.LogError("Соединение с БД не открыто. Невозможно получить текущий прогресс уровня.");
            return null;
        }

        PlayerProgressData progressData = null;
        string sql = "SELECT id, playerId, levelId, isUnlocked, isCompleted, bestTime, score, attempts, lastPlayedTime FROM PlayerProgress WHERE playerId = @playerId AND levelId = @levelId;";
        
        using (IDbCommand command = dbConnection.CreateCommand())
        {
            command.CommandText = sql;
            AddParameter(command, "@playerId", playerId);
            AddParameter(command, "@levelId", levelId);

            using (IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    progressData = new PlayerProgressData
                    {
                        id = reader.GetInt32(0),
                        playerId = reader.GetInt32(1),
                        levelId = reader.GetInt32(2),
                        isUnlocked = reader.GetInt32(3) == 1,
                        isCompleted = reader.GetInt32(4) == 1,
                        bestTime = reader.GetFloat(5),
                        score = reader.GetInt32(6),
                        attempts = reader.GetInt32(7),
                        lastPlayedTime = reader.GetInt64(8)
                    };
                }
            }
        }
        return progressData;
    }
    

    /// <summary>
    /// Устанавливает статус разблокировки уровня для игрока.
    /// </summary>
    public void SetLevelUnlocked(int playerId, int levelId, bool unlocked) // <-- НОВЫЙ МЕТОД
    {
        if (dbConnection == null || dbConnection.State != ConnectionState.Open)
        {
            Debug.LogError("Соединение с БД не открыто. Невозможно обновить прогресс уровня.");
            return;
        }

        string sql = @"
            INSERT OR REPLACE INTO PlayerProgress 
            (id, playerId, levelId, isUnlocked, isCompleted, bestTime, score, attempts, lastPlayedTime) 
            VALUES (
                (SELECT id FROM PlayerProgress WHERE playerId = @playerId AND levelId = @levelId),
                @playerId, 
                @levelId, 
                @isUnlocked, 
                (SELECT isCompleted FROM PlayerProgress WHERE playerId = @playerId AND levelId = @levelId), -- сохраняем текущий статус завершения
                (SELECT bestTime FROM PlayerProgress WHERE playerId = @playerId AND levelId = @levelId), 
                (SELECT score FROM PlayerProgress WHERE playerId = @playerId AND levelId = @levelId), 
                (SELECT attempts FROM PlayerProgress WHERE playerId = @playerId AND levelId = @levelId), 
                strftime('%s', 'now')
            );"; // Обновляем lastPlayedTime

        using (IDbCommand command = dbConnection.CreateCommand())
        {
            command.CommandText = sql;
            AddParameter(command, "@playerId", playerId);
            AddParameter(command, "@levelId", levelId);
            AddParameter(command, "@isUnlocked", unlocked ? 1 : 0);
            command.ExecuteNonQuery();
            Debug.Log($"Прогресс игрока {playerId} для уровня {levelId}: isUnlocked = {unlocked}.");
        }
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

    /// <summary>
    /// Получает информацию об игроке по его ID.
    /// </summary>
    public PlayerData GetPlayerById(int playerId) // <-- ВОЗВРАЩАЕМ ЭТОТ МЕТОД
    {
        if (dbConnection == null || dbConnection.State != ConnectionState.Open)
        {
            Debug.LogError("Соединение с БД не открыто. Невозможно получить данные игрока по ID.");
            return null;
        }

        PlayerData player = null;
        
        // В этом методе мы не будем считать isGameCompleted, т.к. вы просили убрать эту логику.
        // Если понадобится isGameCompleted, ее можно будет добавить обратно здесь.

        string sql = "SELECT id, playerName, creationDate, lastPlayedDate FROM Players WHERE id = @playerId;";
        using (IDbCommand command = dbConnection.CreateCommand())
        {
            command.CommandText = sql;
            AddParameter(command, "@playerId", playerId);
            using (IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    player = new PlayerData
                    {
                        id = reader.GetInt32(0),
                        playerName = reader.GetString(1),
                        creationDate = reader.GetInt64(2),
                        lastPlayedDate = reader.GetInt64(3)
                        // isGameCompleted = false; // Нет смысла устанавливать, если не рассчитывается
                    };
                }
            }
        }
        return player;
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