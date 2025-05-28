using UnityEngine; // ОБЯЗАТЕЛЬНО
using UnityEngine.UI; // ОБЯЗАТЕЛЬНО, если используете стандартный UnityEngine.UI.Text (хотя у вас TMPro)
using UnityEngine.SceneManagement; // ОБЯЗАТЕЛЬНО
using TMPro; // ОБЯЗАТЕЛЬНО

public class InteractableObject : MonoBehaviour
{
    [Header("Настройки Взаимодействия")]
    [Tooltip("Текст, который будет отображаться, когда игрок находится рядом (например, 'Спать', 'Использовать').")]
    public string interactionText = "Спать"; 
    [Tooltip("Максимальная дистанция, на которой игрок может взаимодействовать с этим объектом.")]
    public float interactionDistance = 2f; 
    [Tooltip("Название сцены, которая будет загружена после успешного взаимодействия.")]
    public string nextSceneName = "Level1"; 
    
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

    void Start()
    {
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

                LoadNextScene();
            });
        }
        else
        {
            Debug.LogError("[InteractableObject] Не найден экземпляр FadeScreen! Загружаем сцену напрямую.");
            LoadNextScene();
        }
    }

    void LoadNextScene()
    {
        Debug.Log("[InteractableObject] Загружаем сцену через экран загрузки: " + nextSceneName);
        Time.timeScale = 1f; 

        // Устанавливаем целевую сцену для LoadingScreenManager
        LoadingScreenManager.sceneToLoad = nextSceneName; 
        // Загружаем сцену LoadingScreen
        SceneManager.LoadScene("LoadingScreen"); 
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}