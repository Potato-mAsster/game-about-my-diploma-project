using UnityEngine;
using UnityEngine.SceneManagement; // Обязательно для работы со SceneManager
using TMPro; // Если вы используете TextMeshPro, добавьте эту строку
using UnityEngine.UI; // Если вы используете обычный Text

public class ClearTextOnSceneLoad : MonoBehaviour
{
    // Ссылка на ваш текстовый компонент.
    // Используйте TextMeshProUGUI, если вы используете TextMeshPro.
    // Используйте Text, если вы используете обычный UI.Text.
    [SerializeField] private TextMeshProUGUI targetTextComponentTMP;
    // [SerializeField] private Text targetTextComponentLegacy; // Если вы используете обычный Text

    [Header("Настройки очистки")]
    [Tooltip("Очистить текст (установить пустую строку)")]
    [SerializeField] private bool clearTextContent = true;
    [Tooltip("Скрыть GameObject с текстом")]
    [SerializeField] private bool disableGameObject = false;


    private void Awake()
    {
        // Подписываемся на событие загрузки сцены.
        // Этот метод будет вызываться КАЖДЫЙ РАЗ, когда загружается новая сцена.
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log(gameObject.name + ": Подписан на событие загрузки сцены для очистки текста.");

        // Проверка, что компонент текста назначен в инспекторе.
        if (targetTextComponentTMP == null /*&& targetTextComponentLegacy == null*/) // раскомментировать для Text
        {
            Debug.LogWarning("ClearTextOnSceneLoad: Не назначен текстовый компонент в инспекторе на объекте " + gameObject.name + ".");
        }
    }

    private void OnDestroy()
    {
        // Очень важно отписаться от события, когда объект уничтожается,
        // чтобы избежать утечек памяти и ошибок NullReferenceException.
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Debug.Log(gameObject.name + ": Отписан от события загрузки сцены.");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Этот метод вызывается после того, как новая сцена полностью загружена.
        Debug.Log(gameObject.name + ": Сцена '" + scene.name + "' загружена. Попытка очистить текст.");

        // Если вы используете TextMeshPro
        if (targetTextComponentTMP != null)
        {
            if (clearTextContent)
            {
                targetTextComponentTMP.text = ""; // Очищаем содержимое текста
                Debug.Log(gameObject.name + ": Содержимое TextMeshPro очищено.");
            }
            if (disableGameObject)
            {
                targetTextComponentTMP.gameObject.SetActive(false); // Скрываем объект
                Debug.Log(gameObject.name + ": GameObject TextMeshPro скрыт.");
            }
        }
        /*
        // Если вы используете обычный UI.Text (раскомментируйте, если это ваш случай)
        else if (targetTextComponentLegacy != null)
        {
            if (clearTextContent)
            {
                targetTextComponentLegacy.text = ""; // Очищаем содержимое текста
                Debug.Log(gameObject.name + ": Содержимое Legacy Text очищено.");
            }
            if (disableGameObject)
            {
                targetTextComponentLegacy.gameObject.SetActive(false); // Скрываем объект
                Debug.Log(gameObject.name + ": GameObject Legacy Text скрыт.");
            }
        }
        */
        else
        {
            Debug.LogWarning(gameObject.name + ": Нет назначенного текстового компонента для очистки.");
        }
    }

    // Если вы хотите, чтобы текст был очищен или скрыт при старте этой сцены,
    // (например, если этот скрипт находится на объекте, который не DontDestroyOnLoad)
    // можете использовать Start() или OnEnable()
    void Start()
    {
        // При необходимости, можно сбросить текст сразу при старте сцены,
        // если скрипт прикреплен к объекту, который появляется в каждой новой сцене.
        // if (clearTextContent && targetTextComponentTMP != null) targetTextComponentTMP.text = "";
        // if (disableGameObject && targetTextComponentTMP != null) targetTextComponentTMP.gameObject.SetActive(false);
    }
}