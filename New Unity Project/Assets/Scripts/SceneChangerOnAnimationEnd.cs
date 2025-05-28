using UnityEngine;
using UnityEngine.SceneManagement; // Необходимо для работы со сценами

public class SceneChangerOnAnimationEnd : MonoBehaviour
{
    // Имя сцены, на которую нужно перейти ПОСЛЕ экрана загрузки
    [Tooltip("Имя целевой сцены, на которую нужно перейти ПОСЛЕ загрузочного экрана.")]
    public string nextLevelName; 

    // Название состояния анимации, которое должно завершиться
    // Это должно быть точное имя состояния в вашем Animator Controller
    [Tooltip("Точное имя состояния анимации в Animator Controller, после завершения которого произойдет переход.")]
    public string animationStateName; 

    private Animator animator;
    private bool animationFinished = false; // Флаг для отслеживания завершения и предотвращения повторных загрузок

    void Start()
    {
        animator = GetComponent<Animator>(); // Получаем компонент Animator
        if (animator == null)
        {
            Debug.LogError("[SceneChangerOnAnimationEnd] Animator компонент не найден на этом объекте. " +
                           "Скрипт требует Animator. Отключаю скрипт.");
            enabled = false; // Отключаем скрипт, если Animator не найден
            return;
        }

        // Убедитесь, что курсор скрыт и заблокирован, если это не главное меню
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Гарантируем, что время игры нормальное
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (animator == null || animationFinished)
        {
            return; // Выходим, если Animator не найден или анимация уже завершилась
        }

        // Получаем информацию о текущем состоянии Animator-а
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // 0 - это слой Animator-а

        // Проверяем, находится ли Animator в указанном состоянии и завершила ли анимация воспроизведение
        // stateInfo.IsName(animationStateName) проверяет, активно ли это состояние
        // stateInfo.normalizedTime >= 1.0f означает, что анимация воспроизвелась хотя бы один раз полностью
        if (stateInfo.IsName(animationStateName) && stateInfo.normalizedTime >= 1.0f)
        {
            Debug.Log($"[SceneChangerOnAnimationEnd] Анимация '{animationStateName}' завершилась. Подготовка к загрузке сцены: {nextLevelName}");
            animationFinished = true; // Устанавливаем флаг, чтобы не загружать сцену повторно

            // --- КЛЮЧЕВОЕ ИЗМЕНЕНИЕ: ИСПОЛЬЗУЕМ LOADINGSCREENMANAGER ---
            if (string.IsNullOrEmpty(nextLevelName))
            {
                Debug.LogError("[SceneChangerOnAnimationEnd] Имя следующей сцены не указано! Невозможно загрузить.");
                return;
            }

            // 1. Устанавливаем целевую сцену для LoadingScreenManager
            LoadingScreenManager.sceneToLoad = nextLevelName; 

            // 2. Загружаем сцену LoadingScreen
            // Убедитесь, что имя "LoadingScreen" точно совпадает с именем вашей сцены загрузки в Build Settings.
            SceneManager.LoadScene("LoadingScreen"); 
        }
    }
}