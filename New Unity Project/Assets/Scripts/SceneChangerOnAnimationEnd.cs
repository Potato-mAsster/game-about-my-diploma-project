using UnityEngine;
using UnityEngine.SceneManagement; // Необходимо для работы со сценами

public class SceneChangerOnAnimationEnd : MonoBehaviour
{
    // Имя сцены, на которую нужно перейти
    public string nextLevelName; 

    // Название состояния анимации, которое должно завершиться
    // Это должно быть точное имя состояния в вашем Animator Controller
    public string animationStateName; 

    private Animator animator;
    private bool animationFinished = false; // Флаг для отслеживания завершения

    void Start()
    {
        animator = GetComponent<Animator>(); // Получаем компонент Animator
        if (animator == null)
        {
            Debug.LogError("Animator компонент не найден на этом объекте. " +
                           "Скрипт 'SceneChangerOnAnimationEnd' требует Animator.");
            enabled = false; // Отключаем скрипт, если Animator не найден
        }
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
            Debug.Log($"Анимация '{animationStateName}' завершилась. Переход на следующий уровень: {nextLevelName}");
            animationFinished = true; // Устанавливаем флаг, чтобы не загружать сцену повторно
            SceneManager.LoadScene(nextLevelName); // Загружаем следующую сцену
        }
    }
}