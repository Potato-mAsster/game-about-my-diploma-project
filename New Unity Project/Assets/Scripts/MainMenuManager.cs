using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor; // Необходимо для EditorApplication.isPlaying = false;
using System.IO;
using System.Collections.Generic;
using TMPro; // Если вы используете TextMeshPro
using UnityEngine.UI; // Если используете стандартный UI

public class MainMenuManager : MonoBehaviour
{
    public string gameSceneName = "Game";

    [Header("Панель загрузки игры")]
    public GameObject loadGamePanel;
    public Transform playerListParent;
    public GameObject playerButtonPrefab;

    [Header("Панель ввода имени")]
    public GameObject playerNameInputPanel;
    public TMP_InputField playerNameInputField_TMP; // Для TextMeshPro
    public InputField playerNameInputField_UI;   // Для стандартного UI
    public Button confirmNameButton;

    private string dbPath;

    void Start()
    {
        dbPath = Path.Combine(Application.persistentDataPath, "HypersomniaDB.db");
        // Убедитесь, что база данных открыта
        if (SimpleSQLite.dbConnection.Equals(System.IntPtr.Zero))
        {
            SimpleSQLite.Open(dbPath);
            Debug.Log("База поднята!");
            SimpleSQLite.Close();
        }
    }

    public void LoadGame()
    {
        Debug.Log("Функция LoadGame вызвана!");
        if (loadGamePanel != null)
        {
            loadGamePanel.SetActive(true);/////////
            LoadPlayerList();
        }
        else
        {
            Debug.LogError("Панель загрузки игры не назначена.");
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Level1");
        // Debug.Log("Начало новой игры...");
        // // 1. Создаем новую запись игрока в таблице Players
        // string insertNewPlayerQuery = "INSERT INTO Players (creationDate) VALUES (strftime('%s','now'))";
        // SimpleSQLite.ExecuteQuery(insertNewPlayerQuery);

        // // 2. Получаем ID только что созданного игрока
        // List<string[]> playerIdResult = SimpleSQLite.ExecuteQuery("SELECT last_insert_rowid()");
        // if (playerIdResult.Count > 0 && playerIdResult[0].Length > 0 && int.TryParse(playerIdResult[0][0], out int newPlayerId))
        // {
        //     Debug.Log($"Создан новый игрок с ID: {newPlayerId}");
        //     // 3. Открываем панель ввода имени
        //     Debug.Log(playerNameInputPanel);
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
        //             Debug.LogError("Компонент CanvasGroup не найден на PlayerNameInputPanel.");
        //         }
        //         PlayerPrefs.SetInt("NewPlayerId", newPlayerId);
        //     }
        // }
        // else
        // {
        //     Debug.LogError("Не удалось получить ID нового игрока.");
        // }
    }

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
                Debug.Log($"Имя игрока с ID {newPlayerId} установлено на '{playerName}'.");

                // 5. Инициализируем прогресс для нового игрока
                List<string[]> firstLevelResult = SimpleSQLite.ExecuteQuery("SELECT id FROM Levels ORDER BY `order` ASC LIMIT 1");
                if (firstLevelResult.Count > 0 && firstLevelResult[0].Length > 0)
                {
                    string firstLevelId = firstLevelResult[0][0];
                    string initializeProgressQuery = $"INSERT OR IGNORE INTO PlayerProgress (playerId, levelId, isUnlocked) VALUES ({newPlayerId}, '{firstLevelId}', 1)";
                    SimpleSQLite.ExecuteQuery(initializeProgressQuery);
                    Debug.Log($"Первый уровень ({firstLevelId}) разблокирован для игрока {newPlayerId}.");
                }

                // 6. Загружаем игровую сцену
                SceneManager.LoadScene(gameSceneName);
            }
            else
            {
                Debug.LogError("ID нового игрока не найден.");
            }

            // Скрываем панель ввода имени
            if (playerNameInputPanel != null) playerNameInputPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Пожалуйста, введите имя игрока.");
        }
    }
    
    void LoadPlayerList()
    {
        List<string[]> players = SimpleSQLite.ExecuteQuery("SELECT id, playerName FROM Players");

        foreach (Transform child in playerListParent)
        {
            Destroy(child.gameObject);
        }

        foreach (string[] playerInfo in players)
        {
            if (playerInfo.Length == 2 && int.TryParse(playerInfo[0], out int playerId))
            {
                string playerName = playerInfo[1];
                GameObject playerButtonGO = Instantiate(playerButtonPrefab, playerListParent);
                TMPro.TextMeshProUGUI buttonTextTMP = playerButtonGO.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                UnityEngine.UI.Text buttonTextUI = playerButtonGO.GetComponentInChildren<UnityEngine.UI.Text>();

                if (buttonTextTMP != null)
                {
                    buttonTextTMP.text = playerName != null ? playerName : "Игрок без имени";
                }
                else if (buttonTextUI != null)
                {
                    buttonTextUI.text = playerName != null ? playerName : "Игрок без имени";
                }

                UnityEngine.UI.Button playerButton = playerButtonGO.GetComponent<UnityEngine.UI.Button>();
                if (playerButton != null)
                {
                    playerButton.onClick.AddListener(() => LoadSelectedGame(playerId));
                }
                else
                {
                    Debug.LogError("Префаб кнопки игрока не имеет компонента Button.");
                }
            }
            else
            {
                Debug.LogError("Ошибка при получении информации об игроке из БД.");
            }
        }
    }

    public void LoadSelectedGame(int playerId)
    {
        Debug.Log($"Загрузка игры для игрока с ID: {playerId}");
        // 1. Обновляем дату последнего входа
        string updateLastPlayedQuery = $"UPDATE Players SET lastPlayedDate = strftime('%s','now') WHERE id = {playerId}";
        SimpleSQLite.ExecuteQuery(updateLastPlayedQuery);

        // 2. Загружаем прогресс игрока
        List<string[]> progress = SimpleSQLite.ExecuteQuery($"SELECT levelId, isUnlocked, isCompleted FROM PlayerProgress WHERE playerId = {playerId}");
        foreach (string[] row in progress)
        {
            Debug.Log($"Уровень: {row[0]}, Разблокирован: {row[1]}, Пройден: {row[2]}");
            // Здесь вы можете загрузить состояние уровней в вашей игре
        }

        // 3. Загружаем настройки игрока
        List<string[]> settings = SimpleSQLite.ExecuteQuery($"SELECT soundVolume, musicVolume, resolutionWidth, resolutionHeight, isFullscreen, language, controlMapping, graphicsQuality FROM UserSettings WHERE playerId = {playerId}");
        if (settings.Count > 0 && settings[0].Length > 0)
        {
            Debug.Log($"Настройки игрока: Звук={settings[0][0]}, Музыка={settings[0][1]}, Разрешение={settings[0][2]}x{settings[0][3]}, Fullscreen={settings[0][4]}, Язык={settings[0][5]}, Управление={settings[0][6]}, Качество={settings[0][7]}");
            // Здесь вы можете применить настройки к вашей игре
        }

        // 4. Загружаем игровую сцену
        SceneManager.LoadScene(gameSceneName);

        // 5. Скрываем панель загрузки
        if (loadGamePanel != null) loadGamePanel.SetActive(false);
    }

    public void OpenSettings()
    {
        Debug.Log("Открытие настроек...");
        // Здесь может быть логика открытия панели настроек
    }

    public void OpenAbout()
    {
        Debug.Log("Информация об авторе...");
        // Здесь может быть логика открытия панели "Об авторе"
    }

    public void ExitGame()
    {
        Debug.Log("Выход из игры...");
        SimpleSQLite.Close();
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}