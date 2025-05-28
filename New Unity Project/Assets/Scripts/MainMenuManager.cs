using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor; // Необходимо для EditorApplication.isPlaying = false;
using System.IO;
using System.Collections.Generic;
using TMPro; // Если вы используете TextMeshPro
using UnityEngine.UI; // Если используете стандартный UI

public class MainMenuManager : MonoBehaviour
{
    // Ваша основная игровая сцена, которая будет загружаться после выбора игрока
    public string gameSceneName = "Level0"; // Убедитесь, что это имя вашей первой игровой сцены

    // Сцена, которая загружается после "Новой игры" (интро, катсцена и т.д.)
    public string openingSceneName = "Opening"; // Убедитесь, что это имя вашей интро-сцены

    [Header("Панель загрузки игры")]
    public GameObject loadGamePanel;
    public Transform playerListParent;
    public GameObject playerButtonPrefab;

    [Header("Панель ввода имени")]
    public GameObject playerNameInputPanel;
    public TMP_InputField playerNameInputField_TMP; // Для TextMeshPro
    public InputField playerNameInputField_UI;    // Для стандартного UI
    public Button confirmNameButton;

    private string dbPath;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Гарантируем, что время игры нормальное при старте меню
        Time.timeScale = 1f;

        dbPath = Path.Combine(Application.persistentDataPath, "HypersomniaDB.db");
        // Убедитесь, что база данных открыта
        // Здесь предполагается, что SimpleSQLite.dbConnection - это статический член,
        // который проверяет, установлено ли соединение.
        // Возможно, вам нужен более надежный способ управления подключением к БД.
        if (SimpleSQLite.dbConnection.Equals(System.IntPtr.Zero)) // Проверяем, что соединение не установлено
        {
            Debug.Log($"[MainMenuManager] Попытка открыть базу данных по пути: {dbPath}");
            SimpleSQLite.Open(dbPath); // Открываем соединение
            Debug.Log("[MainMenuManager] База данных поднята!");
            SimpleSQLite.Close(); // Закрываем соединение после проверки/инициализации
        }
        else
        {
            Debug.Log("[MainMenuManager] База данных уже открыта или не требуется переоткрытие.");
        }
    }

    // Метод, который вызывается кнопкой "Загрузить игру"
    public void LoadGame()
    {
        Debug.Log("[MainMenuManager] Функция LoadGame вызвана!");
        if (loadGamePanel != null)
        {
            loadGamePanel.SetActive(true);
            LoadPlayerList(); // Загружаем список игроков для выбора
        }
        else
        {
            Debug.LogError("[MainMenuManager] Панель загрузки игры не назначена.");
        }
    }

    // Метод, который вызывается кнопкой "Новая игра"
    public void StartNewGame() // Переименовал для ясности, если есть также LoadGame
    {
        Debug.Log("[MainMenuManager] Начало новой игры...");
        
        // --- НОВАЯ ЛОГИКА: ИСПОЛЬЗУЕМ ЭКРАН ЗАГРУЗКИ ---
        LoadingScreenManager.sceneToLoad = openingSceneName; // Устанавливаем целевую сцену
        SceneManager.LoadScene("LoadingScreen"); // Загружаем экран загрузки

        // Закомментированный код для создания нового игрока.
        // Если вы хотите, чтобы ввод имени был ДО экрана загрузки:
        // Вам нужно будет перенести логику LoadScene(openingSceneName) в SetPlayerNameAndLoadGame().
        // Если ввод имени должен быть ПОСЛЕ экрана загрузки:
        // Тогда логика создания игрока и открытия панели ввода имени должна быть в скрипте OpeningSceneManager.
        
        // // 1. Создаем новую запись игрока в таблице Players
        // string insertNewPlayerQuery = "INSERT INTO Players (creationDate) VALUES (strftime('%s','now'))";
        // SimpleSQLite.ExecuteQuery(insertNewPlayerQuery);

        // // 2. Получаем ID только что созданного игрока
        // List<string[]> playerIdResult = SimpleSQLite.ExecuteQuery("SELECT last_insert_rowid()");
        // if (playerIdResult.Count > 0 && playerIdResult[0].Length > 0 && int.TryParse(playerIdResult[0][0], out int newPlayerId))
        // {
        //     Debug.Log($"[MainMenuManager] Создан новый игрок с ID: {newPlayerId}");
        //     // 3. Открываем панель ввода имени
        //     Debug.Log($"[MainMenuManager] Открытие панели ввода имени: {playerNameInputPanel}");
        //     if (playerNameInputPanel != null)
        //     {
        //         CanvasGroup cg = playerNameInputPanel.GetComponent<CanvasGroup>();
        //         if (cg != null)
        //         {
        //             cg.alpha = 1;
        //             cg.interactable = true;
        //             cg.blocksRaycasts = true;
        //         }
        //         else
        //         {
        //             Debug.LogError("[MainMenuManager] Компонент CanvasGroup не найден на PlayerNameInputPanel.");
        //         }
        //         PlayerPrefs.SetInt("NewPlayerId", newPlayerId); // Сохраняем ID, чтобы использовать в SetPlayerNameAndLoadGame
        //     }
        // }
        // else
        // {
        //     Debug.LogError("[MainMenuManager] Не удалось получить ID нового игрока.");
        // }
    }

    // Метод, который вызывается кнопкой "Подтвердить имя"
    public void SetPlayerNameAndLoadGame()
    {
        string playerName = "";
        if (playerNameInputField_TMP != null) playerName = playerNameInputField_TMP.text;
        else if (playerNameInputField_UI != null) playerName = playerNameInputField_UI.text;

        if (!string.IsNullOrEmpty(playerName))
        {
            int newPlayerId = PlayerPrefs.GetInt("NewPlayerId", -1);

            if (newPlayerId != -1)
            {
                // 4. Обновляем имя игрока в таблице Players
                string updatePlayerNameQuery = $"UPDATE Players SET playerName = '{playerName}', lastPlayedDate = strftime('%s','now') WHERE id = {newPlayerId}";
                SimpleSQLite.ExecuteQuery(updatePlayerNameQuery);
                Debug.Log($"[MainMenuManager] Имя игрока с ID {newPlayerId} установлено на '{playerName}'.");

                // 5. Инициализируем прогресс для нового игрока
                List<string[]> firstLevelResult = SimpleSQLite.ExecuteQuery("SELECT id FROM Levels ORDER BY `order` ASC LIMIT 1");
                if (firstLevelResult.Count > 0 && firstLevelResult[0].Length > 0)
                {
                    string firstLevelId = firstLevelResult[0][0];
                    string initializeProgressQuery = $"INSERT OR IGNORE INTO PlayerProgress (playerId, levelId, isUnlocked) VALUES ({newPlayerId}, '{firstLevelId}', 1)";
                    SimpleSQLite.ExecuteQuery(initializeProgressQuery);
                    Debug.Log($"[MainMenuManager] Первый уровень ({firstLevelId}) разблокирован для игрока {newPlayerId}.");
                }
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                
                // --- НОВАЯ ЛОГИКА: ИСПОЛЬЗУЕМ ЭКРАН ЗАГРУЗКИ ---
                // Загружаем основную игровую сцену после ввода имени (если вы выбрали этот путь)
                LoadingScreenManager.sceneToLoad = gameSceneName;
                SceneManager.LoadScene("LoadingScreen");
            }
            else
            {
                Debug.LogError("[MainMenuManager] ID нового игрока не найден. Невозможно установить имя.");
            }

            // Скрываем панель ввода имени
            if (playerNameInputPanel != null) playerNameInputPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[MainMenuManager] Пожалуйста, введите имя игрока.");
        }
    }
    
    // Метод для загрузки списка сохраненных игр (игроков)
    void LoadPlayerList()
    {
        // Обязательно откройте соединение с БД перед выполнением запроса
        SimpleSQLite.Open(dbPath); 
        List<string[]> players = SimpleSQLite.ExecuteQuery("SELECT id, playerName FROM Players");
        SimpleSQLite.Close(); // Закройте соединение после использования

        foreach (Transform child in playerListParent)
        {
            Destroy(child.gameObject); // Удаляем старые кнопки, если они есть
        }

        foreach (string[] playerInfo in players)
        {
            if (playerInfo.Length == 2 && int.TryParse(playerInfo[0], out int playerId))
            {
                string playerName = playerInfo[1];
                GameObject playerButtonGO = Instantiate(playerButtonPrefab, playerListParent);
                
                // Проверяем наличие TextMeshProUGUI или стандартного Text
                TMPro.TextMeshProUGUI buttonTextTMP = playerButtonGO.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                UnityEngine.UI.Text buttonTextUI = playerButtonGO.GetComponentInChildren<UnityEngine.UI.Text>();

                if (buttonTextTMP != null)
                {
                    buttonTextTMP.text = string.IsNullOrEmpty(playerName) ? "Игрок без имени" : playerName;
                }
                else if (buttonTextUI != null)
                {
                    buttonTextUI.text = string.IsNullOrEmpty(playerName) ? "Игрок без имени" : playerName;
                }

                UnityEngine.UI.Button playerButton = playerButtonGO.GetComponent<UnityEngine.UI.Button>();
                if (playerButton != null)
                {
                    // Добавляем слушатель к кнопке, чтобы при нажатии загружалась выбранная игра
                    playerButton.onClick.AddListener(() => LoadSelectedGame(playerId));
                }
                else
                {
                    Debug.LogError("[MainMenuManager] Префаб кнопки игрока не имеет компонента Button. Невозможно назначить слушатель.");
                }
            }
            else
            {
                Debug.LogError("[MainMenuManager] Ошибка при получении информации об игроке из БД (неверный формат данных).");
            }
        }
    }

    // Метод, который вызывается при выборе игрока из списка загрузки
    public void LoadSelectedGame(int playerId)
    {
        Debug.Log($"[MainMenuManager] Загрузка игры для игрока с ID: {playerId}");
        
        // Открываем соединение с БД для выполнения запросов
        SimpleSQLite.Open(dbPath);

        // 1. Обновляем дату последнего входа
        string updateLastPlayedQuery = $"UPDATE Players SET lastPlayedDate = strftime('%s','now') WHERE id = {playerId}";
        SimpleSQLite.ExecuteQuery(updateLastPlayedQuery);

        // 2. Загружаем прогресс игрока (для использования в GameManager или других местах)
        List<string[]> progress = SimpleSQLite.ExecuteQuery($"SELECT levelId, isUnlocked, isCompleted FROM PlayerProgress WHERE playerId = {playerId}");
        // Здесь вы можете передать этот прогресс в свой GameManager/SaveLoadManager.
        // Например: GameManager.Instance.LoadPlayerProgress(progress);
        foreach (string[] row in progress)
        {
            Debug.Log($"[MainMenuManager] Прогресс: Уровень={row[0]}, Разблокирован={row[1]}, Пройден={row[2]}");
            // В реальной игре здесь вы бы сохраняли этот прогресс куда-то (например, в PlayerData Singleton)
        }

        // 3. Загружаем настройки игрока (для использования в GameManager/AudioManager)
        List<string[]> settings = SimpleSQLite.ExecuteQuery($"SELECT soundVolume, musicVolume, resolutionWidth, resolutionHeight, isFullscreen, language, controlMapping, graphicsQuality FROM UserSettings WHERE playerId = {playerId}");
        if (settings.Count > 0 && settings[0].Length > 0)
        {
            Debug.Log($"[MainMenuManager] Настройки игрока: Звук={settings[0][0]}, Музыка={settings[0][1]}, Разрешение={settings[0][2]}x{settings[0][3]}, Fullscreen={settings[0][4]}, Язык={settings[0][5]}, Управление={settings[0][6]}, Качество={settings[0][7]}");
            // Здесь вы можете применить настройки к вашей игре через AudioManager, SettingsManager и т.д.
        }

        SimpleSQLite.Close(); // Закрываем соединение после выполнения всех запросов

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // 5. Скрываем панель загрузки игры (если она была открыта)
        if (loadGamePanel != null) loadGamePanel.SetActive(false);

        // --- НОВАЯ ЛОГИКА: ИСПОЛЬЗУЕМ ЭКРАН ЗАГРУЗКИ ---
        LoadingScreenManager.sceneToLoad = gameSceneName; // Устанавливаем целевую игровую сцену
        SceneManager.LoadScene("LoadingScreen"); // Загружаем экран загрузки
    }

    public void OpenSettings()
    {
        Debug.Log("[MainMenuManager] Открытие настроек...");
        // Здесь может быть логика открытия панели настроек
    }

    public void OpenAbout()
    {
        Debug.Log("[MainMenuManager] Информация об авторе...");
        // Здесь может быть логика открытия панели "Об авторе"
    }

    public void ExitGame()
    {
        Debug.Log("[MainMenuManager] Выход из игры...");
        SimpleSQLite.Close(); // Закрываем соединение с БД перед выходом
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}