using UnityEngine;
using TMPro; // Для TextMeshPro
using UnityEngine.SceneManagement; // Для SceneManager (переход в меню/перезапуск)
using UnityEngine.UI; // Для UnityEngine.UI.Button (если используете)

public class GameTimer : MonoBehaviour
{
    // НОВОЕ: Синглтон для GameTimer (чтобы другие скрипты могли его получить)
    public static GameTimer Instance { get; private set; } 

    [Header("Настройки Таймера")]
    [Tooltip("Текстовый элемент для отображения времени (TextMeshProUGUI).")]
    public TextMeshProUGUI timerText;

    [Tooltip("Общее время для обратного отсчета в секундах.")]
    public float totalTime = 110f; 

    [Tooltip("Имя первой игровой сцены (обычно Level0), куда нужно будет перезапустить игру.")]
    public string firstGameSceneName = "Level0"; // Возможно, это всегда текущая сцена для перезапуска

    [Header("UI Окно 'Время вышло'")]
    [Tooltip("Панель GameObject, которая будет показана, когда время закончится.")]
    public GameObject gameOverPanel; 
    [Tooltip("Текст, который будет отображаться в панели 'Время вышло' (например, 'Время вышло!').")]
    public TextMeshProUGUI gameOverMessageText;
    [Tooltip("Кнопка 'Начать заново' в панели 'Время вышло'.")]
    public Button restartButton; 
    [Tooltip("Кнопка 'Выйти в меню' в панели 'Время вышло'.")]
    public Button mainMenuButton; 

    private float timeRemaining;
    private bool isTimerRunning = true;
    private bool gameOver = false; 
    private float levelStartTime; 
    private float elapsedTime = 0f; 

    // НОВОЕ: Ссылка на DatabaseManager
    private DatabaseManager dbManager; 
    private DatabaseManager.LevelData currentLevelData; 

    // НОВЫЕ: Объявления контроллеров игрока и камеры <-- ДОБАВЛЕНО/ВОССТАНОВЛЕНО
    private PlayerController playerController; 
    private CameraController cameraController; 

    // НОВОЕ: Публичное свойство для получения затраченного времени
    public float CurrentLevelElapsedTime 
    {
        get { return elapsedTime; }
    }

    void Awake()
    {
        // Инициализация синглтона
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        timeRemaining = totalTime;

        dbManager = DatabaseManager.Instance; 
        if (dbManager == null)
        {
            Debug.LogError("[GameTimer] DatabaseManager не найден! Невозможно обновить попытки или получить данные уровня.");
            this.enabled = false;
            return;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        currentLevelData = dbManager.GetLevelDataBySceneName(currentSceneName); 
        if (currentLevelData == null)
        {
            Debug.LogError($"[GameTimer] Информация о текущем уровне '{currentSceneName}' не найдена в БД! Невозможно обновить попытки.");
            this.enabled = false;
            return;
        }

        if (dbManager.CurrentPlayerId != -1) 
        {
            dbManager.IncrementLevelAttempts(dbManager.CurrentPlayerId, currentLevelData.id); 
            Debug.Log($"[GameTimer] Количество попыток для уровня {currentLevelData.levelName} увеличено.");
        }
        else
        {
            Debug.LogWarning("[GameTimer] Текущий игрок не определен, попытка не записана.");
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("[GameTimer] Панель 'Время вышло' (gameOverPanel) не назначена! Игра не сможет сообщить о конце времени.");
        }

        // Находим контроллеры игрока и камеры <-- ЭТИ СТРОКИ СНОВА СМОГУТ РАБОТАТЬ
        playerController = FindObjectOfType<PlayerController>();
        cameraController = FindObjectOfType<CameraController>();

        // Привязываем кнопки
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        else
        {
            Debug.LogWarning("[GameTimer] Кнопка 'Начать заново' не назначена.");
        }
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
        else
        {
            Debug.LogWarning("[GameTimer] Кнопка 'Выйти в меню' не назначена.");
        }

        StartLevelTimer();
    }

    void Update()
    {
        if (isTimerRunning && !gameOver)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                elapsedTime += Time.deltaTime; 
                UpdateTimerDisplay();
            }
            else
            {
                timeRemaining = 0; 
                elapsedTime = totalTime; 
                UpdateTimerDisplay(); 
                GameOver(); 
            }
        }
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = string.Format("Время: {0:00}:{1:00}", minutes, seconds);
    }

    public void StartLevelTimer() 
    {
        elapsedTime = 0f; 
        timeRemaining = totalTime; 
        isTimerRunning = true;
        gameOver = false;
        UpdateTimerDisplay();
        Debug.Log("[GameTimer] Таймер уровня запущен.");
    }

    public void StopLevelTimer() 
    {
        isTimerRunning = false;
        Debug.Log("[GameTimer] Таймер уровня остановлен.");
    }

    void GameOver()
    {
        gameOver = true;
        isTimerRunning = false;

        Debug.Log("[GameTimer] Время вышло! Игра окончена.");

        Time.timeScale = 0f; 

        // Эти строки теперь могут использовать playerController и cameraController
        if (playerController != null) playerController.enabled = false;
        if (cameraController != null) cameraController.enabled = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        if (gameOverMessageText != null)
        {
            gameOverMessageText.text = "Время вышло!";
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        Debug.Log("[GameTimer] Нажата кнопка 'Начать заново'.");
        
        LeafCollector.leafCount = 0; 
        
        Time.timeScale = 1f; 

        LoadingScreenManager.sceneToLoad = SceneManager.GetActiveScene().name; 
        SceneManager.LoadScene("LoadingScreen");
    }

    public void GoToMainMenu()
    {
        Debug.Log("[GameTimer] Нажата кнопка 'Выйти в меню'.");
        
        LeafCollector.leafCount = 0; 
        
        Time.timeScale = 1f; 
        
        LoadingScreenManager.sceneToLoad = "MainMenu"; 
        SceneManager.LoadScene("LoadingScreen");
    }
}