using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseLvlZero : MonoBehaviour
{
    public GameObject pauseMenuUI;      // Панель меню паузы
    public GameObject settingsMenuUI;   // Панель настроек (если есть)
    private bool isPaused = false;

    // Ссылки на контроллеры игрока и камеры
    public PlayerController playerController; 
    public CameraController cameraController;
    public AudioSource musicAudioSource; 

    // Метод Awake вызывается раньше Start, что более надежно для инициализации
    void Awake()
    {
        // Убедимся, что меню паузы всегда скрыто при загрузке сцены
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        if (settingsMenuUI != null)
        {
            settingsMenuUI.SetActive(false); // Также скрываем меню настроек
        }
        
        // Гарантируем, что время игры всегда нормальное при старте новой сцены
        // Это КЛЮЧЕВОЙ момент для решения проблемы с "застывшей" сценой.
        Time.timeScale = 1f; 
        isPaused = false; // Убедимся, что флаг паузы сброшен

        // Также восстанавливаем состояние курсора на случай, если предыдущая сцена
        // завершилась в состоянии паузы.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Start()
    {
        // В Start() уже можно искать ссылки на контроллеры, если они не назначены вручную в инспекторе.
        // Если playerController и cameraController назначаются вручную, эти FindObjectOfType не нужны.
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController == null) Debug.LogWarning("[PauseMenu] PlayerController не найден в сцене.");
        }
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
            if (cameraController == null) Debug.LogWarning("[PauseMenu] CameraController не найден в сцене.");
        }
        if (musicAudioSource == null)
        {
            // Если музыка не назначена, попробуем найти AudioSource с тегом "Music" или просто в сцене.
            GameObject musicObject = GameObject.FindGameObjectWithTag("Music"); // Предполагая, что у вашей музыки есть тег "Music"
            if (musicObject != null)
            {
                musicAudioSource = musicObject.GetComponent<AudioSource>();
            }
            if (musicAudioSource == null) Debug.LogWarning("[PauseMenu] Music AudioSource не найден в сцене.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Замораживаем время игры
        isPaused = true;
        Cursor.lockState = CursorLockMode.None; // Разблокируем курсор
        Cursor.visible = true; // Делаем курсор видимым

        if (playerController != null) playerController.isInputEnabled = false; // Отключаем ввод игрока
        if (cameraController != null) cameraController.isInputEnabled = false; // Отключаем ввод камеры
        if (musicAudioSource != null) musicAudioSource.Pause(); // Приостанавливаем музыку
    }

    public void Resume()
    {
        if (settingsMenuUI != null) settingsMenuUI.SetActive(false); // Убедимся, что настройки скрыты
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Восстанавливаем время игры
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked; // Блокируем курсор в центре экрана
        Cursor.visible = false; // Скрываем курсор

        if (playerController != null) playerController.isInputEnabled = true; // Включаем ввод игрока
        if (cameraController != null) cameraController.isInputEnabled = true;  // Включаем ввод камеры
        if (musicAudioSource != null) musicAudioSource.UnPause(); // Возобновляем музыку
    }

    public void Restart()
    {
        // Сбрасываем все статические переменные, которые должны быть сброшены
        LeafCollector.leafCount = 0; // Сбрасываем счетчик листьев
        // Добавьте сюда другие статические переменные, если они есть и требуют сброса:
        // YourOtherStaticClass.yourStaticVariable = defaultValue;

        Time.timeScale = 1f; // Гарантируем, что время игры нормально перед загрузкой сцены
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Перезагружаем текущую сцену
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

        LoadingScreenManager.sceneToLoad = "MainMenu";
        SceneManager.LoadScene("LoadingScreen");
    }

    public void OpenSettings()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (settingsMenuUI != null) settingsMenuUI.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsMenuUI != null) settingsMenuUI.SetActive(false);
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
    }
}