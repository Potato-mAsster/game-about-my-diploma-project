using UnityEngine;
using UnityEngine.SceneManagement; // Обязательно для работы со сценами

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;      // Панель меню паузы
    public GameObject settingsMenuUI;   // Панель настроек (если есть)
    private bool isPaused = false;
    public PlayerController playerController; 
    public CameraController cameraController;
    public AudioSource musicAudioSource; 

    void Start()
    {
        pauseMenuUI.SetActive(false);
        // Убедимся, что время не заморожено, если это первая загрузка сцены
        // или если предыдущая сцена завершилась некорректно.
        Time.timeScale = 1f; 
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
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Замораживаем время игры
        isPaused = true;
        Cursor.lockState = CursorLockMode.None; // Разблокируем курсор
        Cursor.visible = true; // Делаем курсор видимым

        if (playerController != null)
            playerController.isInputEnabled = false; // Отключаем ввод игрока
        if (cameraController != null)
            cameraController.isInputEnabled = false; // Отключаем ввод камеры
        if (musicAudioSource != null)
            musicAudioSource.Pause(); // Приостанавливаем музыку
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Восстанавливаем время игры
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked; // Блокируем курсор в центре экрана
        Cursor.visible = false; // Скрываем курсор

        if (playerController != null)
            playerController.isInputEnabled = true; // Включаем ввод игрока
        if (cameraController != null)
            cameraController.isInputEnabled = true;  // Включаем ввод камеры
        if (musicAudioSource != null)
            musicAudioSource.UnPause(); // Возобновляем музыку
    }

public void Restart()
{
    LeafCollector.leafCount = 0; // Сбрасываем счетчик листьев.
    
    Time.timeScale = 1f; // Убедитесь, что время нормальное.

    // Устанавливаем текущую сцену как целевую для перезагрузки через экран загрузки
    LoadingScreenManager.sceneToLoad = SceneManager.GetActiveScene().name; // Используем имя текущей сцены
    // Загружаем сцену LoadingScreen
    SceneManager.LoadScene("LoadingScreen"); 
}

    public void QuitGame()
    {
        Time.timeScale = 1f;

        LoadingScreenManager.sceneToLoad = "MainMenu";
        SceneManager.LoadScene("LoadingScreen");
    }

    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false);
        if (settingsMenuUI != null)
            settingsMenuUI.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsMenuUI != null)
            settingsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }
}