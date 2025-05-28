using UnityEngine;
using TMPro; // Добавляем это пространство имен для работы с TextMeshPro.
             // Если у вас нет TextMeshPro, вам нужно его импортировать (Windows -> TextMeshPro -> Import TMP Essential Resources)
             // Или заменить на 'UnityEngine.UI' и 'Text' в коде, если вы используете стандартный UI Text.

public class EInteraction : MonoBehaviour
{
    // --- Существующие публичные поля для аниматоров и расстояния ---
    public Animator switchAnimator;
    public string switchAnimationName = "SwitchAnimation"; // Название анимации для рычага

    public Animator doorAnimator;
    public string doorOpenAnimationName = "DoorOpen"; // Если вы используете Animator.Play() для двери.
                                                      // Примечание: Если вы используете Animator.SetTrigger("OpenDoor"),
                                                      // как в вашем PlayDoorAnimation(), это поле не используется напрямую.

    public float interactionDistance = 2f; // Дистанция, на которой игрок может взаимодействовать с рычагом

    // --- Приватные поля для внутренних нужд скрипта ---
    private Transform playerTransform; // Ссылка на Transform игрока
    private bool switchActivated = false; // Флаг, отслеживающий, был ли рычаг уже активирован

    // --- НОВЫЕ поля для логики с листьями ---
    [Header("Настройки Листьев")] // Заголовок в инспекторе для этой секции
    [Tooltip("Количество листьев, которое необходимо собрать для активации рычага.")]
    public int requiredLeafCount = 3; // Сколько листьев нужно собрать для активации

    // --- НОВЫЕ поля для UI сообщений ---
    [Header("Настройки UI Сообщения")] // Заголовок в инспекторе для этой секции
    [Tooltip("Ссылка на GameObject, который является панелью или контейнером для текстового сообщения. Должен быть в Canvas.")]
    public GameObject messagePanel; // Панель, содержащая текст сообщения (мы будем ее показывать/скрывать)
    
    [Tooltip("Ссылка на компонент TextMeshProUGUI, который будет отображать текстовое сообщение.")]
    public TextMeshProUGUI messageText; // Компонент TextMeshProUGUI, в который будет записываться текст
    
    [Tooltip("Сообщение, отображаемое, если игрок пытается активировать рычаг без достаточного количества листьев.")]
    public string notEnoughLeavesMessage = "Найдите все листы!"; // Исправлено: не содержит динамической инициализации здесь
    
    [Tooltip("Сообщение-подсказка, отображаемое, когда игрок находится рядом с рычагом и может взаимодействовать.")]
    public string pressEToActivateMessage = "Нажмите 'E' для активации";
    
    [Tooltip("Длительность (в секундах), в течение которой будет отображаться сообщение 'Найдите все листы!' перед автоматическим скрытием.")]
    public float messageDisplayDuration = 5f; // Установлено на 5 секунд

    private Coroutine hideMessageCoroutine; // Переменная для хранения ссылки на запущенную корутину,
                                            // чтобы можно было ее остановить.

    void Start()
    {
        // Поиск объекта игрока по тегу "Player". Это стандартный способ.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            // Если игрок не найден, выводим ошибку и отключаем скрипт, чтобы избежать NullReferenceException.
            Debug.LogError("[EInteraction] Не найден игровой объект с тегом 'Player'. Убедитесь, что у вашего игрока есть тег 'Player'.");
            enabled = false; // Отключаем скрипт, если игрока нет
            return; // Выходим из метода Start
        }

        // --- НОВАЯ логика инициализации UI сообщения ---
        // Убедимся, что панель сообщения скрыта в начале игры.
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[EInteraction] Панель сообщения (messagePanel) не назначена в инспекторе скрипта. Сообщения не будут отображаться.");
        }
        // Проверяем, что текстовый компонент назначен.
        if (messageText == null)
        {
            Debug.LogWarning("[EInteraction] Текстовый компонент (messageText) не назначен в инспекторе скрипта. Сообщения не будут отображаться.");
        }

        // Примечание: Если вам нужно сбрасывать счетчик листьев при старте сцены/игры,
        // это можно сделать здесь: LeafCollector.leafCount = 0;
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distanceToSwitch = Vector3.Distance(playerTransform.position, transform.position);

        // --- Модифицированная логика отображения/скрытия подсказки ---
        if (distanceToSwitch <= interactionDistance)
        {
            if (LeafCollector.leafCount >= requiredLeafCount)
            {
                // Если все листья собраны, показываем подсказку "Нажмите 'E'".
                // Это сообщение не должно автоматически скрываться.
                ShowMessage(pressEToActivateMessage, false); 
            }
            else 
            {
                // Если листов не хватает, и игрок находится в зоне взаимодействия,
                // мы НЕ хотим постоянно скрывать/показывать сообщение "Найдите все листы!".
                // Это сообщение показывается ТОЛЬКО при попытке активации.
                // Поэтому, если НЕ показывается "Нажмите E" и панель активна,
                // то это может быть наше временное сообщение, и мы не должны его трогать здесь.
                // Единственное, что мы хотим сделать, это убедиться, что "Нажмите E" не висит, если его не должно быть.
                // Если сейчас висит не "Нажмите E", но мы в зоне, то оно должно быть результатом нажатия E,
                // и пусть оно само скроется по таймеру.
                
                // Убедимся, что если мы в радиусе, но листов нет,
                // и при этом НЕ висит сообщение "Найдите все листы!",
                // то никаких других сообщений тоже не висит.
                if (messagePanel != null && messagePanel.activeSelf && messageText.text != notEnoughLeavesMessage)
                {
                    HideMessage(); // Скрываем, если это не то сообщение, которое должно висеть по таймеру
                }
            }
        }
        else
        {
            // Игрок вышел из радиуса взаимодействия, скрываем все сообщения
            HideMessage();
        }

        // --- Логика обработки нажатия 'E' ---
        // ЭТОТ БЛОК БЫЛ ДУБЛИРОВАН, ТЕПЕРЬ ОН ЕДИНСТВЕННЫЙ И ПРАВИЛЬНЫЙ.
        if (Input.GetKeyDown(KeyCode.E) && distanceToSwitch <= interactionDistance)
        {
            // Проверяем, что рычаг еще не был активирован и аниматор рычага назначен
            if (!switchActivated && switchAnimator != null)
            {
                // --- НОВАЯ логика: проверка количества листьев ---
                if (LeafCollector.leafCount >= requiredLeafCount)
                {
                    // Активируем рычаг, если собрано достаточно листьев.
                    switchAnimator.Play(switchAnimationName); // Проигрываем анимацию рычага
                    switchActivated = true; // Устанавливаем флаг, что рычаг активирован
                    HideMessage(); // Скрываем любые UI подсказки после активации
                    Invoke("PlayDoorAnimation", 1f); // Вызываем метод PlayDoorAnimation через 1 секунду
                }
                else
                {
                    // Листьев не хватает, выводим сообщение игроку.
                    // 'true' означает, что сообщение будет автоматически скрыто через messageDisplayDuration.
                    ShowMessage(notEnoughLeavesMessage, true); 
                    Debug.Log($"[EInteraction] Не хватает листов для активации рычага! Собрано: {LeafCollector.leafCount}, Необходимо: {requiredLeafCount}.");
                }
            }
            // else if (switchActivated)
            // {
            //     // Опционально: если рычаг уже активирован, можно вывести другое сообщение или ничего не делать.
            //     // Debug.Log("[EInteraction] Рычаг уже был активирован ранее.");
            // }
        }
    }

    // --- Существующий метод для проигрывания анимации двери ---
    void PlayDoorAnimation()
    {
        if (doorAnimator != null)
        {
            // Важно: Вы используете SetTrigger("OpenDoor"), что предполагает наличие триггера "OpenDoor"
            // в вашем Animator Controller двери. Это корректный и часто используемый способ.
            doorAnimator.SetTrigger("OpenDoor");
            
            // Если бы вы использовали Animator.Play(), код был бы таким:
            // if (!string.IsNullOrEmpty(doorOpenAnimationName))
            // {
            //     doorAnimator.Play(doorOpenAnimationName);
            // }
        }
        else
        {
            Debug.LogWarning("[EInteraction] Аниматор двери не назначен или не указано название анимации открытия.");
        }
    }

    // --- НОВЫЕ вспомогательные методы для UI сообщений ---

    /// <summary>
    /// Показывает текстовое сообщение на UI.
    /// </summary>
    /// <param name="message">Текст сообщения, который будет отображен.</param>
    /// <param name="autoHide">Если true, сообщение будет автоматически скрыто через 'messageDisplayDuration' секунд.</param>
    void ShowMessage(string message, bool autoHide)
    {
        // Проверяем, назначены ли все необходимые UI элементы в инспекторе.
        if (messagePanel == null || messageText == null) return;

        // Если уже запущена корутина для скрытия предыдущего сообщения, останавливаем ее,
        // чтобы новое сообщение не исчезло преждевременно.
        if (hideMessageCoroutine != null)
        {
            StopCoroutine(hideMessageCoroutine);
            hideMessageCoroutine = null;
        }

        messageText.text = message; // Устанавливаем текст сообщения
        messagePanel.SetActive(true); // Активируем (показываем) панель сообщения

        if (autoHide)
        {
            // Запускаем корутину для автоматического скрытия сообщения через заданное время.
            hideMessageCoroutine = StartCoroutine(HideMessageAfterDelay(messageDisplayDuration));
        }
    }

    /// <summary>
    /// Скрывает текстовое сообщение на UI.
    /// </summary>
    void HideMessage()
    {
        // Проверяем, что панель сообщения назначена и активна, прежде чем пытаться ее скрыть.
        if (messagePanel != null && messagePanel.activeSelf) 
        {
            messagePanel.SetActive(false); // Деактивируем (скрываем) панель сообщения
            
            // Если корутина для скрытия была запущена, останавливаем ее,
            // так как сообщение уже скрыто вручную.
            if (hideMessageCoroutine != null)
            {
                StopCoroutine(hideMessageCoroutine);
                hideMessageCoroutine = null;
            }
        }
    }

    /// <summary>
    /// Корутина для скрытия сообщения после заданной задержки.
    /// </summary>
    /// <param name="delay">Время задержки в секундах.</param>
    System.Collections.IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Ждем указанное количество секунд
        HideMessage(); // Вызываем метод для скрытия сообщения
    }
}