using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor; // Необходимо для EditorApplication.isPlaying = false;

public class MainMenuManager : MonoBehaviour
{
    public string gameSceneName = "Game"; // Название сцены с игрой

    public void LoadGame()
    {
        // Здесь может быть логика загрузки сохранения
        Debug.Log("Загрузка игры...");
        SceneManager.LoadScene(gameSceneName);
    }

    public void StartGame()
    {
        Debug.Log("Начало новой игры...");
        SceneManager.LoadScene(gameSceneName);
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
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}