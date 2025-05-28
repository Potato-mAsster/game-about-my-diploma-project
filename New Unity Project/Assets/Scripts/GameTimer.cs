using UnityEngine;
using TMPro; // Для TextMeshPro
using UnityEngine.SceneManagement; // Для SceneManager (переход в меню/перезапуск)

public class GameTimer : MonoBehaviour
{
    [Header("Настройки Таймера")]
    [Tooltip("Текстовый элемент для отображения времени (TextMeshProUGUI).")]
    public TextMeshProUGUI timerText;

    [Tooltip("Общее время для обратного отсчета в секундах.")]
    public float totalTime = 110f; // Например, 2 минуты (120 секунд)

    [Tooltip("Имя первой игровой сцены (обычно Level0), куда нужно будет перезапустить игру.")]
    public string firstGameSceneName = "Level0"; 

    [Header("UI Окно 'Время вышло'")]
    [Tooltip("Панель GameObject, которая будет показана, когда время закончится.")]
    public GameObject gameOverPanel; 
    [Tooltip("Текст, который будет отображаться в панели 'Время вышло' (например, 'Время вышло!').")]
    public TextMeshProUGUI gameOverMessageText;
    [Tooltip("Кнопка 'Начать заново' в панели 'Время вышло'.")]
    public UnityEngine.UI.Button restartButton; // Используем UnityEngine.UI.Button
    [Tooltip("Кнопка 'Выйти в меню' в панели 'Время вышло'.")]
    public UnityEngine.UI.Button mainMenuButton; // Используем UnityEngine.UI.Button

    private float timeRemaining;
    private bool isTimerRunning = true;
    private bool gameOver = false; // Флаг, чтобы событие "время вышло" сработало один раз

    // Ссылки на контроллеры игрока и камеры, чтобы их можно было отключить
    private PlayerController playerController; 
    private CameraController cameraController;

    void Awake()
    {
        // Инициализация времени при старте сцены
        timeRemaining = totalTime;

        // Убедимся, что панель "Время вышло" скрыта в начале игры
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("[GameTimer] Панель 'Время вышло' (gameOverPanel) не назначена! Игра не сможет сообщить о конце времени.");
        }

        // Находим контроллеры игрока и камеры
        playerController = FindObjectOfType<PlayerController>();
        cameraController = FindObjectOfType<CameraController>();

        // Привязываем кнопки (если они назначены)
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
    }

    void Update()
    {
        if (isTimerRunning && !gameOver)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerDisplay();
            }
            else
            {
                timeRemaining = 0; // Время закончилось, убедимся, что не уходит в минус
                UpdateTimerDisplay(); // Обновим, чтобы показать 00:00
                GameOver(); // Вызываем событие "Игра окончена"
            }
        }
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = string.Format("Время: {0:00}:{1:00}", minutes, seconds);
    }

    void GameOver()
    {
        gameOver = true; // Устанавливаем флаг, чтобы это событие не сработало повторно
        isTimerRunning = false; // Останавливаем таймер

        Debug.Log("[GameTimer] Время вышло! Игра окончена.");

        // Останавливаем время игры
        Time.timeScale = 0f; 

        // Отключаем управление игроком и камерой
        if (playerController != null) playerController.enabled = false;
        if (cameraController != null) cameraController.enabled = false;

        // Показываем панель "Время вышло"
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        if (gameOverMessageText != null)
        {
            gameOverMessageText.text = "Время вышло!"; // Или любое другое сообщение
        }

        // Разблокируем курсор для взаимодействия с UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Методы для кнопок UI
    public void RestartGame()
    {
        Debug.Log("[GameTimer] Нажата кнопка 'Начать заново'.");
        
        // Сбросим статичные счетчики (например, листья)
        LeafCollector.leafCount = 0; 
        // Если у вас есть другие статические переменные, которые нужно сбросить, добавьте их здесь.

        // Убедимся, что Time.timeScale = 1f перед перезагрузкой
        Time.timeScale = 1f; 

        // Загружаем сцену через экран загрузки
        // Assuming firstGameSceneName is the current level or level 0
        LoadingScreenManager.sceneToLoad = firstGameSceneName; // Загружаем первую игровую сцену
        SceneManager.LoadScene("LoadingScreen");
    }

    public void GoToMainMenu()
    {
        Debug.Log("[GameTimer] Нажата кнопка 'Выйти в меню'.");
        
        // Сбросим статичные счетчики
        LeafCollector.leafCount = 0; 
        // Если есть другие статические переменные, которые нужно сбросить, добавьте их здесь.

        // Убедимся, что Time.timeScale = 1f перед перезагрузкой
        Time.timeScale = 1f; 
        
        // Загружаем сцену главного меню через экран загрузки
        LoadingScreenManager.sceneToLoad = "MainMenu"; // Замените на имя вашей сцены главного меню
        SceneManager.LoadScene("LoadingScreen");
    }
}