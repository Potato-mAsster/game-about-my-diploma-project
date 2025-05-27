using UnityEngine;
using UnityEngine.UI; // Для работы с UI элементами (текстом)
using UnityEngine.SceneManagement; // Для загрузки новых сцен
using TMPro;

public class InteractableObject : MonoBehaviour
{
    public string interactionText = "Спать"; // Текст, который будет отображаться
    public float interactionDistance = 2f; // Дистанция, на которой игрок может взаимодействовать
    public string nextSceneName = "Level1"; // Название сцены для перехода
    public string animationSleepTrigger = "Sleep"; // Название триггера анимации сна (если есть)

    private GameObject player; // Ссылка на объект игрока
    private TextMeshProUGUI interactionUIText; // Ссылка на текстовый элемент UI
    private Animator playerAnimator; // Ссылка на аниматор игрока (если есть)
    private bool canInteract = false; // Флаг, показывающий, может ли игрок взаимодействовать

    void Start()
    {
        // Находим игрока по тегу "Player". Убедись, что у твоего игрока есть такой тег.
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Не найден объект игрока с тегом 'Player'!");
            enabled = false; // Отключаем скрипт, чтобы избежать ошибок
            return;
        }

        // Находим UI Text с именем "InteractionText". Создадим его позже.
        interactionUIText = GameObject.Find("InteractionText")?.GetComponent<TextMeshProUGUI>();
        if (interactionUIText == null)
        {
            Debug.LogError("Не найден UI Text объект с именем 'InteractionText'!");
            enabled = false;
            return;
        }
        interactionUIText.gameObject.SetActive(false); // Скрываем текст при старте

        // Получаем компонент Animator у игрока, если он есть
        playerAnimator = player.GetComponent<Animator>();
    }

    void Update()
    {
        // Проверяем расстояние между игроком и этим объектом
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= interactionDistance)
        {
            // Если игрок достаточно близко, показываем текст и разрешаем взаимодействие
            interactionUIText.text = interactionText;
            interactionUIText.gameObject.SetActive(true);
            canInteract = true;

            // Проверяем нажатие клавиши "E"
            if (Input.GetKeyDown(KeyCode.E) && canInteract)
            {
                StartSleeping();
            }
        }
        else
        {
            // Если игрок далеко, скрываем текст и запрещаем взаимодействие
            interactionUIText.gameObject.SetActive(false);
            canInteract = false;
        }
    }

    void StartSleeping()
    {
        Debug.Log("StartSleeping() вызвана");

        // Скрываем текст взаимодействия
        if (interactionUIText != null)
        {
            interactionUIText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("interactionUIText is null!");
        }

        // Отключаем управление игроком
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Запускаем анимацию сна
        if (playerAnimator != null && !string.IsNullOrEmpty(animationSleepTrigger))
        {
            playerAnimator.SetBool(animationSleepTrigger, true);
        }

        // Запускаем затухание экрана и затем уничтожаем текст и загружаем следующую сцену
        if (FadeScreen.instance != null)
        {
            FadeScreen.instance.FadeOut(() =>
            {
                if (interactionUIText != null && interactionUIText.gameObject != null)
                {
                    Destroy(interactionUIText.gameObject);
                }
                LoadNextScene();
            });
        }
        else
        {
            Debug.LogError("Не найден экземпляр FadeScreen!");
            LoadNextScene();
        }
    }

    void LoadNextScene()
    {
        Debug.Log("Загружаем сцену: " + nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }

    // Этот метод будет вызываться Unity автоматически при отрисовке в редакторе
    private void OnDrawGizmosSelected()
    {
        // Рисуем окружность вокруг объекта, показывая радиус взаимодействия
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}