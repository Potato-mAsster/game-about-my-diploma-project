using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Добавляем для работы с TextMeshProUGUI

public class SceneChangerOnAnimationEnd : MonoBehaviour
{
    // Имя сцены, на которую нужно перейти ПОСЛЕ экрана загрузки
    [Tooltip("Имя целевой сцены, на которую нужно перейти ПОСЛЕ загрузочного экрана.")]
    public string nextLevelName = "MainMenu";

    // Название состояния анимации, которое должно завершиться
    [Tooltip("Точное имя состояния анимации в Animator Controller, после завершения которого произойдет переход.")]
    public string animationStateName; 

    // НОВОЕ ПОЛЕ: Ссылка на TextMeshProUGUI для отображения имени игрока
    [Header("UI для имени игрока")] // <-- НОВОЕ
    [Tooltip("Перетащите сюда текстовый компонент (TextMeshProUGUI) на сцене для отображения имени игрока.")] // <-- НОВОЕ
    public TextMeshProUGUI playerNameDisplay; // <-- НОВОЕ

    private Animator animator;
    private bool animationFinished = false; 

    // НОВОЕ: Ссылка на DatabaseManager
    private DatabaseManager dbManager; // <-- НОВОЕ

    void Start()
    {
        animator = GetComponent<Animator>(); 
        if (animator == null)
        {
            Debug.LogError("[SceneChangerOnAnimationEnd] Animator компонент не найден на этом объекте. Скрипт требует Animator. Отключаю скрипт.");
            enabled = false; 
            return;
        }

        // НОВОЕ: Получаем ссылку на DatabaseManager
        dbManager = DatabaseManager.Instance; // <-- НОВОЕ
        if (dbManager == null)
        {
            Debug.LogError("[SceneChangerOnAnimationEnd] DatabaseManager не найден! Невозможно отобразить имя игрока."); // <-- НОВОЕ
            // enabled = false; // Не отключаем полностью, вдруг анимация сама по себе нужна
        }
        else // Если DatabaseManager найден
        {
            // НОВОЕ: Обновляем текстовое поле именем игрока
            UpdatePlayerNameDisplay(); // <-- НОВЫЙ МЕТОД
        }


        // Убедитесь, что курсор виден (для UI сцен)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("[SceneChangerOnAnimationEnd] Курсор разблокирован и виден."); // <-- Изменил лог

        // Гарантируем, что время игры нормальное
        Time.timeScale = 1f;
    }

    // НОВЫЙ МЕТОД: Обновление текстового поля именем игрока
    void UpdatePlayerNameDisplay() // <-- НОВЫЙ МЕТОД
    {
        if (playerNameDisplay == null)
        {
            Debug.LogWarning("[SceneChangerOnAnimationEnd] Поле 'Player Name Display' (TextMeshProUGUI) не привязано в Инспекторе.");
            return;
        }

        if (dbManager != null && dbManager.CurrentPlayerId != -1)
        {
            // Получаем данные игрока по его ID
            DatabaseManager.PlayerData playerData = dbManager.GetPlayerById(dbManager.CurrentPlayerId); // Используем уже существующий GetPlayerById
            if (playerData != null)
            {
                playerNameDisplay.text = playerData.playerName + ",\nВы собрали все листы. \nДипломная работа готова!"; // Устанавливаем текст
                Debug.Log($"[SceneChangerOnAnimationEnd] Отображено имя игрока: {playerData.playerName}");
            }
            else
            {
                playerNameDisplay.text = "Игрок не найден";
                Debug.LogWarning("[SceneChangerOnAnimationEnd] Данные текущего игрока не найдены в БД.");
            }
        }
        else
        {
            playerNameDisplay.text = "Игрок не выбран";
            Debug.LogWarning("[SceneChangerOnAnimationEnd] DatabaseManager не готов или игрок не выбран (CurrentPlayerId == -1).");
        }
    }

    void Update()
    {
        if (animator == null || animationFinished)
        {
            return; 
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); 

        if (stateInfo.IsName(animationStateName) && stateInfo.normalizedTime >= 1.0f)
        {
            Debug.Log($"[SceneChangerOnAnimationEnd] Анимация '{animationStateName}' завершилась. Подготовка к загрузке сцены: {nextLevelName}");
            animationFinished = true; 

            if (string.IsNullOrEmpty(nextLevelName))
            {
                Debug.LogError("[SceneChangerOnAnimationEnd] Имя следующей сцены не указано! Невозможно загрузить.");
                return;
            }

            LoadingScreenManager.sceneToLoad = nextLevelName;
            // Cursor.visible = true; // Это уже делается в Start() и LoadMainMenu()
            SceneManager.LoadScene("LoadingScreen");
            
        }
    }

    void LoadMainMenu()
    {
        // Cursor.lockState = CursorLockMode.None; // Это уже делается в Start()
        // Cursor.visible = true; // Это уже делается в Start()
        Debug.Log("[SceneChangerOnAnimationEnd] Курсор разблокирован перед загрузкой LoadingScreen.");

        LoadingScreenManager.sceneToLoad = nextLevelName;
        SceneManager.LoadScene("LoadingScreen");
    }
}