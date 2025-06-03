using UnityEngine;
using UnityEngine.SceneManagement; // Для работы со сценами

public class CursorManager : MonoBehaviour
{
    // Переменная для хранения ссылки на единственный экземпляр этого менеджера
    public static CursorManager Instance { get; private set; }

    void Awake()
    {
        // Реализация паттерна Singleton: убеждаемся, что существует только один экземпляр
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Если уже есть экземпляр, уничтожаем этот
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Не уничтожать объект при загрузке новых сцен

        // Подписываемся на событие загрузки сцены
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // Отписываемся от события, чтобы избежать утечек памяти, когда объект уничтожается
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[CursorManager] Сцена загружена: {scene.name}");

        // Пример: если это сцена меню, показываем курсор.
        // Замените "MenuScene" на фактическое имя вашей сцены меню.
        if (scene.name == "MenuScene" || scene.name == "YourAnotherMenuScene") // Добавьте все сцены меню
        {
            ShowCursor();
        }
        // Пример: если это игровая сцена, скрываем и блокируем курсор.
        // Замените "GameScene" на фактическое имя вашей игровой сцены.
        else if (scene.name == "GameScene" || scene.name == "Level1" || scene.name == "Level2") // Добавьте все игровые сцены
        {
            HideCursor();
        }
        else // Для всех остальных сцен (например, загрузочный экран, интро и т.д.)
        {
            // Можно выбрать поведение по умолчанию, например, всегда показывать
            ShowCursor(); 
        }
    }

    // Метод для показа курсора
    public void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Debug.Log("[CursorManager] Курсор показан и разблокирован.");
    }

    // Метод для скрытия курсора
    public void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked; // Или CursorLockMode.Confined, если нужно ограничить окном
        Debug.Log("[CursorManager] Курсор скрыт и заблокирован.");
    }
}