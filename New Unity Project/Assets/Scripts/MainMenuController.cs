using UnityEngine;
using UnityEngine.UI; // Для работы с UI элементами (Button, InputField, Text)
using UnityEngine.SceneManagement; // Для загрузки сцен
using System.Collections; // Для корутин
using System.Collections.Generic; // Для List (обязательно!)
// Если используете TextMeshPro, добавьте:
// using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;    // Главная панель меню (Загрузить, Начать, Настройки, Выход)
    public GameObject newGamePanel;     // Паненель для ввода имени нового игрока
    public GameObject loadGamePanel;    // НОВАЯ ПАНЕЛЬ: Панель для загрузки игры <-- ДОБАВЛЕНО

    [Header("Main Menu Buttons")]
    public Button loadGameButton;       // Кнопка "ЗАГРУЗИТЬ"
    public Button startGameButton;      // Кнопка "НАЧАТЬ"
    public Button settingsButton;       // Кнопка "НАСТРОЙКИ"
    public Button aboutButton;          // Кнопка "ОБ АВТОРЕ"
    public Button exitButton;           // Кнопка "ВЫХОД"

    [Header("New Game UI Elements")]
    public InputField playerNameInput;  // Поле для ввода имени игрока
    public Button confirmNewGameButton; // Кнопка "Подтвердить" на панели новой игры
    public Button backFromNewGameButton; // Кнопка "Назад" на панели новой игры
    public Text errorMessageText;       // Текст для отображения ошибок (например, "Введите имя!")

    // НОВЫЙ РАЗДЕЛ: Элементы UI для панели загрузки игры
    [Header("Load Game UI Elements")] 
    public Transform playerListContentParent; // Родительский объект для элементов списка игроков (Content в ScrollView)
    public GameObject playerListItemPrefab;   // Префаб элемента списка игрока (например, Panel с Text и Button)
    public Button backFromLoadGameButton;     // Кнопка "Назад" на панели загрузки
    // Опционально: Text для сообщения, если нет сохраненных игр
    // public Text noSavedGamesMessageText; 


    // Ссылка на DatabaseManager
    private DatabaseManager dbManager;

    void Awake()
    {
        // Проверяем все существующие привязки + новые для загрузки игры
        if (mainMenuPanel == null || newGamePanel == null ||
            loadGameButton == null || startGameButton == null || settingsButton == null || aboutButton == null || exitButton == null ||
            playerNameInput == null || confirmNewGameButton == null || backFromNewGameButton == null || errorMessageText == null ||
            loadGamePanel == null || playerListContentParent == null || playerListItemPrefab == null || backFromLoadGameButton == null) // <-- ДОБАВЛЕНО
        {
            Debug.LogError("MainMenuController: Некоторые UI-элементы не привязаны в Инспекторе! Проверьте все поля.");
            return;
        }

        dbManager = DatabaseManager.Instance; // Получаем синглтон DatabaseManager

        // Устанавливаем начальное состояние панелей
        mainMenuPanel.SetActive(true);
        newGamePanel.SetActive(false);
        loadGamePanel.SetActive(false); // НОВАЯ СТРОКА: скрываем панель загрузки <-- ДОБАВЛЕНО
        errorMessageText.gameObject.SetActive(false);
        // if (noSavedGamesMessageText != null) noSavedGamesMessageText.gameObject.SetActive(false); // Если опциональный текст

        AddButtonListeners();
    }

    private void AddButtonListeners()
    {
        // Кнопки главного меню (существующие)
        loadGameButton.onClick.AddListener(OnLoadGameClicked); 
        startGameButton.onClick.AddListener(OnStartGameClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        aboutButton.onClick.AddListener(OnAboutClicked);
        exitButton.onClick.AddListener(OnExitClicked);

        // Кнопки панели "Новая игра" (существующие)
        confirmNewGameButton.onClick.AddListener(OnConfirmNewGameClicked);
        backFromNewGameButton.onClick.AddListener(OnBackFromNewGameClicked);

        // НОВЫЙ СЛУШАТЕЛЬ: Кнопка "Назад" на панели загрузки игры
        backFromLoadGameButton.onClick.AddListener(OnBackFromLoadGameClicked); // <-- ДОБАВЛЕНО
    }

    // --- ОБРАБОТЧИКИ КНОПОК ГЛАВНОГО МЕНЮ ---

    public void OnLoadGameClicked()
    {
        Debug.Log("Кнопка 'ЗАГРУЗИТЬ' нажата.");
        //mainMenuPanel.SetActive(false); // Скрываем основное меню
        newGamePanel.SetActive(false); // Убеждаемся, что панель новой игры скрыта
        loadGamePanel.SetActive(true);  // НОВАЯ СТРОКА: Показываем панель загрузки игры <-- ДОБАВЛЕНО
        PopulatePlayerList();           // НОВАЯ ФУНКЦИЯ: заполняем список игроков <-- ДОБАВЛЕНО
    }

    public void OnStartGameClicked()
    {
        Debug.Log("Кнопка 'НАЧАТЬ' нажата.");
        //mainMenuPanel.SetActive(false); // Скрываем основное меню
        newGamePanel.SetActive(true);   // Показываем панель ввода имени
        playerNameInput.text = "";      // Очищаем поле ввода
        errorMessageText.gameObject.SetActive(false); // Скрываем старые ошибки
    }

    public void OnSettingsClicked()
    {
        Debug.Log("Кнопка 'НАСТРОЙКИ' нажата (заглушка).");
        // TODO: Реализовать логику настроек.
    }

    public void OnAboutClicked()
    {
        Debug.Log("Кнопка 'ОБ АВТОРЕ' нажата (заглушка).");
        // TODO: Реализовать логику "Об авторе".
    }

    public void OnExitClicked()
    {
        Debug.Log("Кнопка 'ВЫХОД' нажата.");
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // --- ОБРАБОТЧИКИ КНОПОК ПАНЕЛИ "НОВАЯ ИГРА" ---

    public void OnConfirmNewGameClicked()
    {
        string name = playerNameInput.text.Trim();

        if (string.IsNullOrEmpty(name))
        {
            errorMessageText.text = "Пожалуйста, введите имя!";
            errorMessageText.gameObject.SetActive(true);
            return;
        }

        errorMessageText.gameObject.SetActive(false);

        StartCoroutine(CreateAndLoadGameRoutine(name));
    }

    public void OnBackFromNewGameClicked()
    {
        Debug.Log("Кнопка 'Назад' на панели новой игры нажата.");
        newGamePanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        errorMessageText.gameObject.SetActive(false);
    }

    // --- НОВЫЕ ОБРАБОТЧИКИ КНОПОК ПАНЕЛИ "ЗАГРУЗИТЬ ИГРУ" ---

    public void OnBackFromLoadGameClicked() // <-- ДОБАВЛЕНО
    {
        Debug.Log("Кнопка 'Назад' на панели загрузки нажата.");
        loadGamePanel.SetActive(false); // Скрываем панель загрузки
        mainMenuPanel.SetActive(true);  // Показываем основное меню
        ClearPlayerList();              // Очищаем список игроков <-- ДОБАВЛЕНО
    }

    public void OnSelectPlayerClicked(int playerId) // <-- ДОБАВЛЕНО
    {
        Debug.Log($"Выбран игрок с ID: {playerId}. Загрузка прогресса...");
        
        // ИСПРАВЛЕНИЕ ЗДЕСЬ: используем новый метод SetCurrentPlayerId
        dbManager.SetCurrentPlayerId(playerId); // <-- ИЗМЕНЕНО

        // Получаем прогресс этого игрока (какой уровень ему нужно загрузить)
        DatabaseManager.PlayerProgressData playerProgress = dbManager.GetPlayerLastUnlockedLevel(playerId);

        if (playerProgress != null)
        {
            Debug.Log($"Игрок {dbManager.CurrentPlayerId} продолжит на уровне {playerProgress.levelId} (Разблокирован: {playerProgress.isUnlocked}, Завершен: {playerProgress.isCompleted})");
            string sceneName = dbManager.GetSceneNameForLevel(playerProgress.levelId);
            if (!string.IsNullOrEmpty(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogError($"Сцена для уровня ID {playerProgress.levelId} не найдена в БД! Проверьте таблицу Levels.");
                errorMessageText.text = "Ошибка загрузки уровня! Обратитесь к разработчику.";
                errorMessageText.gameObject.SetActive(true);
                OnBackFromLoadGameClicked(); 
            }
        }
        else
        {
            Debug.LogWarning($"Прогресс для игрока {playerId} не найден. Этого не должно произойти для существующего игрока.");
            // Если прогресса нет (очень маловероятно для существующего игрока),
            // можно загрузить первый уровень по умолчанию или показать ошибку.
            string firstLevelSceneName = dbManager.GetSceneNameForLevel(1); 
            if (!string.IsNullOrEmpty(firstLevelSceneName))
            {
                Debug.Log($"Загрузка первого уровня для игрока {playerId}, т.к. прогресс не найден.");
                SceneManager.LoadScene(firstLevelSceneName);
            }
            else
            {
                Debug.LogError("Не удалось найти первый уровень в БД. Невозможно начать игру.");
                errorMessageText.text = "Ошибка: невозможно начать игру без прогресса.";
                errorMessageText.gameObject.SetActive(true);
                OnBackFromLoadGameClicked(); 
            }
        }
    }


    // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ---

    private IEnumerator CreateAndLoadGameRoutine(string playerName)
    {
        // Опционально: показать UI индикатор загрузки
        // loadingIndicator.SetActive(true); 

        Debug.Log($"Попытка создать нового игрока: {playerName}...");

        int newPlayerId = dbManager.CreateNewPlayer(playerName);

        yield return null; // Небольшая задержка, чтобы UI обновился

        if (newPlayerId > 0) // Успешно создан новый игрок
        {
            Debug.Log($"Игрок '{playerName}' (ID: {newPlayerId}) успешно создан. Загружаем первый уровень...");

            string firstLevelSceneName = dbManager.GetSceneNameForLevel(1);
            if (!string.IsNullOrEmpty(firstLevelSceneName))
            {
                SceneManager.LoadScene(firstLevelSceneName);
            }
            else
            {
                errorMessageText.text = "Ошибка: не удалось найти первый уровень в базе данных!";
                errorMessageText.gameObject.SetActive(true);
                Debug.LogError("MainMenuController: Имя сцены первого уровня не найдено в БД! Проверьте таблицу Levels и DatabaseManager.");
                newGamePanel.SetActive(false);
                mainMenuPanel.SetActive(true);
            }
        }
        else if (newPlayerId == -2) // Имя игрока уже занято
        {
            errorMessageText.text = "Игрок с таким именем уже существует. Пожалуйста, выберите другое.";
            errorMessageText.gameObject.SetActive(true);
        }
        else // Общая ошибка при создании игрока
        {
            errorMessageText.text = "Произошла ошибка при создании игрока. Попробуйте еще раз.";
            errorMessageText.gameObject.SetActive(true);
            Debug.LogError("MainMenuController: Ошибка при создании игрока в БД (CreateNewPlayer вернул -1).");
        }

        // Опционально: скрыть UI индикатор загрузки
        // loadingIndicator.SetActive(false);
    }

    private void PopulatePlayerList() 
    {
        ClearPlayerList(); 

        List<DatabaseManager.PlayerData> players = dbManager.GetAllPlayers();
        

        if (players != null && players.Count > 0)
        {
            foreach (var player in players)
            {
                Debug.Log(player);
                // Создаем экземпляр префаба как дочерний элемент playerListContentParent
                GameObject item = Instantiate(playerListItemPrefab, playerListContentParent);

                // ***ВАЖНО: КАК НАЙТИ КОМПОНЕНТЫ ВНУТРИ ПРЕФАБА***
                // Если Text и Button НАХОДЯТСЯ ПРЯМО НА КОРНЕВОМ PlayerListItem (т.е., они не вложены глубоко):
                Text playerNameText = item.GetComponent<Text>(); // Если обычный UI.Text
                                                                 // Или: TextMeshProUGUI playerNameText = item.GetComponent<TextMeshProUGUI>(); // Если TextMeshPro

                Button selectPlayerButton = item.GetComponent<Button>();

                // Если Text и/или Button ЯВЛЯЮТСЯ ДОЧЕРНИМИ ОБЪЕКТАМИ PlayerListItem:
                // Например, если PlayerNameText - это дочерний элемент PlayerListItem
                // Text playerNameText = item.GetComponentInChildren<Text>(); 
                // Button selectPlayerButton = item.GetComponentInChildren<Button>(); 


                if (playerNameText != null)
                {
                    playerNameText.text = player.playerName;
                }
                else
                {
                    // Это предупреждение сработает, если скрипт не найдет Text.
                    Debug.LogWarning("Player list item prefab is missing a Text component on its root or children!");
                }

                if (selectPlayerButton != null)
                {
                    // Важно: захватываем player.id для лямбда-выражения (это корректно)
                    int playerIdToLoad = player.id;
                    selectPlayerButton.onClick.AddListener(() => OnSelectPlayerClicked(playerIdToLoad));
                }
                else
                {
                    // Это предупреждение сработает, если скрипт не найдет Button.
                    Debug.LogWarning("Player list item prefab is missing a Button component on its root or children!");
                }
            }
        }
        else
        {
            Debug.Log("Нет сохраненных игроков для загрузки.");
            // (Опционально: показать сообщение "Нет сохраненных игр")
        }
    }

    private void ClearPlayerList() // <-- ДОБАВЛЕНО
    {
        foreach (Transform child in playerListContentParent)
        {
            Destroy(child.gameObject);
        }
    }
}