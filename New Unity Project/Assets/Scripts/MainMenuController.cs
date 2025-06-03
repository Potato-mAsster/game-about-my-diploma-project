using UnityEngine;
using UnityEngine.UI; // Для работы с UI элементами (Button, InputField, Text)
using UnityEngine.SceneManagement; // Для загрузки сцен
using System.Collections; // Для корутин
using System.Collections.Generic;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;    // Главная панель меню (Загрузить, Начать, Настройки, Выход)
    public GameObject newGamePanel;     // Паненель для ввода имени нового игрока
    public GameObject loadGamePanel;    // НОВАЯ ПАНЕЛЬ: Панель для загрузки игры
    public GameObject infoPanel;       // Панель "Об авторе" / Инфо-панель <-- ДОБАВЛЕНО
    public GameObject settingsPanel;   // Панель настроек <-- ДОБАВЛЕНО


    [Header("Main Menu Buttons")]
    public Button loadGameButton;       // Кнопка "ЗАГРУЗИТЬ"
    public Button startGameButton;      // Кнопка "НАЧАТЬ"
    public Button settingsButton;       // Кнопка "НАСТРОЙКИ"
    public Button aboutButton;          // Кнопка "ОБ АВТОРЕ"
    public Button exitButton;           // Кнопка "ВЫХОД"

    [Header("New Game UI Elements")]
    public TMP_InputField playerNameInput;  // Поле для ввода имени игрока
    public Button confirmNewGameButton; // Кнопка "Подтвердить" на панели новой игры
    public Button backFromNewGameButton; // Кнопка "Назад" на панели новой игры
    public TMP_Text errorMessageText;       // Текст для отображения ошибок (например, "Введите имя!")

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
        // Проверяем все существующие привязки + новые для загрузки игры и новых панелей
        if (mainMenuPanel == null || newGamePanel == null || loadGamePanel == null || 
            loadGameButton == null || startGameButton == null || settingsButton == null || aboutButton == null || exitButton == null ||
            playerNameInput == null || confirmNewGameButton == null || backFromNewGameButton == null || errorMessageText == null ||
            playerListContentParent == null || playerListItemPrefab == null || backFromLoadGameButton == null ||
            infoPanel == null || settingsPanel == null) // <-- ДОБАВЛЕНО: Проверка новых панелей
        {
            Debug.LogError("MainMenuController: Некоторые UI-элементы не привязаны в Инспекторе! Проверьте все поля.");
            // Отключаем скрипт, чтобы избежать дальнейших ошибок, если что-то не привязано
            this.enabled = false; 
            return;
        }

        dbManager = DatabaseManager.Instance; // Получаем синглтон DatabaseManager
        if (dbManager == null) // Дополнительная проверка на всякий случай
        {
            Debug.LogError("MainMenuController: DatabaseManager не найден! Убедитесь, что он инициализирован.");
            this.enabled = false;
            return;
        }

        // При старте MainMenu, убедимся, что курсор виден
        // Cursor.lockState = CursorLockMode.None; // Разблокировать курсор
        // Cursor.visible = true; // Сделать курсор видимым
        Debug.Log("[MainMenuController] Курсор разблокирован и виден.");

        // Устанавливаем начальное состояние панелей: главное меню активно, остальные скрыты
        mainMenuPanel.SetActive(true);
        newGamePanel.SetActive(false);
        loadGamePanel.SetActive(false);
        errorMessageText.gameObject.SetActive(false);
        infoPanel.SetActive(false); // НОВАЯ СТРОКА: скрываем инфо-панель <-- ДОБАВЛЕНО
        settingsPanel.SetActive(false); // НОВАЯ СТРОКА: скрываем панель настроек <-- ДОБАВЛЕНО

        AddButtonListeners();
    }

    private void AddButtonListeners()
    {
        // Кнопки главного меню
        loadGameButton.onClick.AddListener(OnLoadGameClicked);
        startGameButton.onClick.AddListener(OnStartGameClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked); // Эта кнопка теперь будет вызывать ToggleSettingsPanel()
        aboutButton.onClick.AddListener(OnAboutClicked);       // Эта кнопка теперь будет вызывать ToggleInfoPanel()
        exitButton.onClick.AddListener(OnExitClicked);

        // Кнопки панели "Новая игра"
        confirmNewGameButton.onClick.AddListener(OnConfirmNewGameClicked);
        backFromNewGameButton.onClick.AddListener(OnBackFromNewGameClicked);

        // Кнопка "Назад" на панели загрузки игры
        backFromLoadGameButton.onClick.AddListener(OnBackFromLoadGameClicked); 
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

    public void OnSettingsClicked() // <-- ИЗМЕНЕНО: теперь вызывает ToggleSettingsPanel
    {
        Debug.Log("Кнопка 'НАСТРОЙКИ' нажата.");
        ToggleSettingsPanel(true); // Показываем панель настроек
        // Можно скрыть главное меню, если панель настроек не накладывается сверху
        // mainMenuPanel.SetActive(false); 
    }

    public void OnAboutClicked() // <-- ИЗМЕНЕНО: теперь вызывает ToggleInfoPanel
    {
        Debug.Log("Кнопка 'ОБ АВТОРЕ' нажата.");
        ToggleInfoPanel(true); // Показываем инфо-панель
        // Можно скрыть главное меню, если инфо-панель не накладывается сверху
        // mainMenuPanel.SetActive(false); 
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
        
        Debug.Log($"MainMenuController: PopulatePlayerList вызван. Получено игроков: {players.Count}"); // Проверка: сколько игроков найдено

        if (players != null && players.Count > 0)
        {
            foreach (var player in players)
            {
                Debug.Log($"MainMenuController: Создаем элемент для игрока '{player.playerName}'."); // Проверка: для какого игрока
                
                // Создаем экземпляр префаба
                GameObject item = Instantiate(playerListItemPrefab, playerListContentParent);

                // *** ЭТО САМЫЙ КРИТИЧНЫЙ БЛОК: ПОИСК КОМПОНЕНТОВ ВНУТРИ СОЗДАННОГО ЭЛЕМЕНТА ***
                // Поскольку PlayerNameText и SelectPlayerButton являются ДОЧЕРНИМИ элементами PlayerListItem
                // И мы используем TextMeshPro
                
                TextMeshProUGUI playerNameText = item.GetComponentInChildren<TextMeshProUGUI>(); 
                Button selectPlayerButton = item.GetComponentInChildren<Button>(); 

                if (playerNameText != null)
                {
                    playerNameText.text = player.playerName;
                    Debug.Log($"MainMenuController: Имя '{player.playerName}' назначено TextMeshPro компоненту.");
                }
                else
                {
                    // Это предупреждение вылезет, если скрипт не найдет TextMeshProUGUI компонент внутри префаба
                    Debug.LogWarning($"MainMenuController: Player list item prefab for '{player.playerName}' is missing a TextMeshProUGUI component in its children!");
                }

                if (selectPlayerButton != null)
                {
                    int playerIdToLoad = player.id; 
                    selectPlayerButton.onClick.AddListener(() => OnSelectPlayerClicked(playerIdToLoad));
                    Debug.Log($"MainMenuController: Кнопка для игрока '{player.playerName}' привязана.");
                }
                else
                {
                    // Это предупреждение вылезет, если скрипт не найдет Button компонент внутри префаба
                    Debug.LogWarning($"MainMenuController: Player list item prefab for '{player.playerName}' is missing a Button component in its children!");
                }
            }
        }
        else
        {
            Debug.Log("Нет сохраненных игроков для загрузки (после DatabaseManager.GetAllPlayers()).");
            // (Опционально: показать сообщение "Нет сохраненных игр")
        }
    }

    // НОВЫЕ МЕТОДЫ: Управление видимостью панелей "Об авторе" и "Настройки" <-- ДОБАВЛЕНО
    public void ToggleInfoPanel(bool show) // Параметр show для явного показа/скрытия
    {
        // Убедимся, что другие панели меню скрыты, если эта панель накладывается сверху
        //mainMenuPanel.SetActive(!show); // Скрыть главное меню, если показываем инфо-панель
        loadGamePanel.SetActive(false);
        newGamePanel.SetActive(false);
        settingsPanel.SetActive(false); // Убедимся, что панель настроек скрыта
        errorMessageText.gameObject.SetActive(false);


        infoPanel.SetActive(show); // Устанавливаем активность инфо-панели
        Debug.Log($"Info Panel {(show ? "показана" : "скрыта")}.");
    }

    public void ToggleSettingsPanel(bool show) // Параметр show для явного показа/скрытия
    {
        // Убедимся, что другие панели меню скрыты, если эта панель накладывается сверху
        //mainMenuPanel.SetActive(!show); // Скрыть главное меню, если показываем панель настроек
        loadGamePanel.SetActive(false);
        newGamePanel.SetActive(false);
        infoPanel.SetActive(false); // Убедимся, что инфо-панель скрыта
        errorMessageText.gameObject.SetActive(false);


        settingsPanel.SetActive(show); // Устанавливаем активность панели настроек
        Debug.Log($"Settings Panel {(show ? "показана" : "скрыта")}.");
    }

    private void ClearPlayerList() // <-- ДОБАВЛЕНО
    {
        foreach (Transform child in playerListContentParent)
        {
            Destroy(child.gameObject);
        }
    }
}