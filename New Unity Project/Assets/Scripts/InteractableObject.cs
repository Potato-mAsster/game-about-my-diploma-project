using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; 
using System.Collections; // Для корутин, если используются (для FadeScreen.instance.FadeOut)

public class InteractableObject : MonoBehaviour
{
    [Header("Настройки Взаимодействия")]
    [Tooltip("Текст, который будет отображаться, когда игрок находится рядом (например, 'Спать', 'Использовать').")]
    public string interactionText = "Спать"; 
    [Tooltip("Максимальная дистанция, на которой игрок может взаимодействовать с этим объектом.")]
    public float interactionDistance = 2f; 
    // Убираем nextSceneName, так как его будет определять БД!
    // public string nextSceneName = "Level1"; 
    
    [Header("Настройки Анимации Игрока")]
    [Tooltip("Название булевого параметра триггера анимации сна игрока (например, 'IsSleeping').")]
    public string animationSleepBool = "IsSleeping"; 

    [Header("Ссылки на UI")]
    [Tooltip("Перетащите сюда ваш текстовый UI-элемент (TextMeshProUGUI) из Canvas.")]
    public TextMeshProUGUI interactionUIText; 
    
    // Ссылки на контроллеры игрока и камеры
    private GameObject player; 
    private Animator playerAnimator; 
    private PlayerController playerController; 
    private CameraController cameraController; 

    private bool canInteract = false; 
    private bool isInteracting = false; 

    // Ссылка на DatabaseManager
    private DatabaseManager dbManager; // <-- ДОБАВЛЕНО

    void Start()
    {
        dbManager = DatabaseManager.Instance; // <-- ДОБАВЛЕНО
        if (dbManager == null)
        {
            Debug.LogError("[InteractableObject] DatabaseManager не найден! Убедитесь, что он инициализирован на стартовой сцене и DontDestroyOnLoad работает.");
            enabled = false; 
            return;
        }
        if (dbManager.CurrentPlayerId == -1) // <-- ДОБАВЛЕНО
        {
            Debug.LogError("[InteractableObject] Текущий игрок не определен в DatabaseManager. Невозможно сохранить прогресс.");
            enabled = false;
            return;
        }

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[InteractableObject] Не найден объект игрока с тегом 'Player'! Отключаем скрипт.");
            enabled = false; 
            return;
        }

        playerController = player.GetComponent<PlayerController>();
        cameraController = player.GetComponentInChildren<CameraController>();
        playerAnimator = player.GetComponent<Animator>(); 

        if (playerController == null) Debug.LogWarning("[InteractableObject] PlayerController не найден на игроке.");
        if (cameraController == null) Debug.LogWarning("[InteractableObject] CameraController не найден на игроке или его дочерних элементах.");
        if (playerAnimator == null) Debug.LogWarning("[InteractableObject] Animator не найден на игроке.");


        if (interactionUIText == null)
        {
            interactionUIText = GameObject.Find("InteractionText")?.GetComponent<TextMeshProUGUI>();
            if (interactionUIText == null)
            {
                Debug.LogError("[InteractableObject] UI Text объект с именем 'InteractionText' не найден или не назначен! Отключаем скрипт.");
                enabled = false;
                return;
            }
        }
        interactionUIText.gameObject.SetActive(false); 
    }

    void Update()
    {
        if (isInteracting) return;
        if (player == null) return; // Доп. проверка на случай, если игрок уничтожен
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= interactionDistance)
        {
            if (!interactionUIText.gameObject.activeSelf) 
            {
                interactionUIText.text = interactionText;
                interactionUIText.gameObject.SetActive(true);
            }
            canInteract = true;

            if (Input.GetKeyDown(KeyCode.E))
            {
                StartInteractionSequence();
            }
        }
        else
        {
            if (interactionUIText.gameObject.activeSelf) 
            {
                interactionUIText.gameObject.SetActive(false);
            }
            canInteract = false;
        }
    }

    void StartInteractionSequence()
    {
        Debug.Log("[InteractableObject] StartInteractionSequence() вызвана.");
        isInteracting = true; 

        if (interactionUIText != null)
        {
            interactionUIText.gameObject.SetActive(false);
        }

        if (playerController != null)
        {
            playerController.enabled = false; 
        }
        if (cameraController != null)
        {
            cameraController.enabled = false; 
        }
        
        if (playerAnimator != null && !string.IsNullOrEmpty(animationSleepBool))
        {
            playerAnimator.SetBool(animationSleepBool, true); 
        }

        if (FadeScreen.instance != null)
        {
            FadeScreen.instance.FadeOut(() =>
            {
                if (playerAnimator != null && !string.IsNullOrEmpty(animationSleepBool))
                {
                    playerAnimator.SetBool(animationSleepBool, false); 
                }

                ProcessLevelCompletionAndLoadNext(); // <-- ИЗМЕНЕНО: Новая функция
            });
        }
        else
        {
            Debug.LogError("[InteractableObject] Не найден экземпляр FadeScreen! Обрабатываем завершение уровня напрямую.");
            ProcessLevelCompletionAndLoadNext(); // <-- ИЗМЕНЕНО: Новая функция
        }
    }

    // НОВЫЙ МЕТОД: Обработка завершения уровня и загрузка следующего
    void ProcessLevelCompletionAndLoadNext() 
    {
        Time.timeScale = 1f;

        string currentSceneName = SceneManager.GetActiveScene().name;
        DatabaseManager.LevelData currentLevelData = dbManager.GetLevelDataBySceneName(currentSceneName); 

        if (currentLevelData == null)
        {
            Debug.LogError($"[InteractableObject] Информация о текущем уровне '{currentSceneName}' не найдена в БД. Возврат в главное меню.");
            LoadingScreenManager.sceneToLoad = "MainMenu"; 
            SceneManager.LoadScene("LoadingScreen");
            return;
        }

        Debug.Log($"Текущий уровень (из БД): ID={currentLevelData.id}, Name={currentLevelData.levelName}, Order={currentLevelData.order}");

        // --- ПОЛУЧАЕМ ЗАТРАЧЕННОЕ ВРЕМЯ ИЗ GameTimer ---
        float levelCompletionTime = -1f; 
        if (GameTimer.Instance != null)
        {
            levelCompletionTime = GameTimer.Instance.CurrentLevelElapsedTime; 
            GameTimer.Instance.StopLevelTimer(); 
            Debug.Log($"[InteractableObject] Время прохождения уровня получено из GameTimer: {levelCompletionTime} секунд.");
        }
        else
        {
            Debug.LogWarning("[InteractableObject] GameTimer не найден. Время прохождения уровня не будет записано.");
        }

        // --- ПОЛУЧАЕМ КОЛИЧЕСТВО СОБРАННЫХ ЛИСТЬЕВ ИЗ LeafCollector ---
        int currentScore = LeafCollector.leafCount; // <-- ИЗМЕНЕНО: ИСПОЛЬЗУЕМ leafCount
        Debug.Log($"[InteractableObject] Очки (собранные листья) на уровне: {currentScore}.");


        // 2. Сохраняем прогресс для ТЕКУЩЕГО уровня: помечаем его как завершенный и обновляем статистику
        dbManager.SetLevelCompleted(dbManager.CurrentPlayerId, currentLevelData.id, true, levelCompletionTime, currentScore); 
        // dbManager.IncrementLevelAttempts() вызывается в GameTimer.Awake

        // ... (остальная логика для нахождения следующего уровня и перехода) ...
        DatabaseManager.LevelData nextLevelData = dbManager.GetLevelDataByOrder(currentLevelData.order + 1);

        if (nextLevelData != null)
        {
            dbManager.SetLevelUnlocked(dbManager.CurrentPlayerId, nextLevelData.id, true);

            LeafCollector.leafCount = 0; 
            Debug.Log("Счетчик листьев сброшен до: " + LeafCollector.leafCount + " перед загрузкой новой сцены.");
            
            LoadingScreenManager.sceneToLoad = nextLevelData.sceneName; 
            Debug.Log($"Переход на сцену: {nextLevelData.sceneName} через LoadingScreen.");
            SceneManager.LoadScene("LoadingScreen");
        }
        else
        {
            Debug.Log("Это последний уровень в последовательности из БД. Игра завершена или возврат в меню.");
            LoadingScreenManager.sceneToLoad = "MainMenu"; 
            Debug.Log("Возврат в главное меню через LoadingScreen.");
            SceneManager.LoadScene("LoadingScreen");
        }
        isInteracting = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (transform.position != null && player != null) // Доп. проверка, чтобы избежать ошибок в редакторе
        {
             Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
}