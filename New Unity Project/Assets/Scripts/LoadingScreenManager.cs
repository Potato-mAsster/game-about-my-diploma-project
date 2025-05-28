using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // Для TextMeshPro
using System.Collections; // Для IEnumerator

public class LoadingScreenManager : MonoBehaviour
{
    // Статическая переменная для хранения имени сцены, которую нужно загрузить.
    // Это позволяет другим скриптам "сказать" менеджеру загрузки, куда идти.
    public static string sceneToLoad; 

    [Header("UI Элементы")]
    [Tooltip("Ссылка на TextMeshProUGUI для отображения текста 'Загрузка...'")]
    public TextMeshProUGUI loadingText; 
    [Tooltip("Ссылка на UI Slider для отображения прогресса загрузки.")]
    public Slider progressBar;

    [Tooltip("Скорость заполнения прогресс-бара для визуального эффекта (полезно для быстрых загрузок).")]
    public float fakeProgressSpeed = 0.5f; // Скорость "фиктивного" прогресса, если загрузка очень быстрая

    private AsyncOperation asyncOperation; // Объект для асинхронной загрузки сцены

    void Start()
    {
        // Проверяем, что имя сцены для загрузки установлено
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("[LoadingScreenManager] Имя сцены для загрузки не установлено! Возвращаемся в главное меню.");
            // Если сцена не указана, можно вернуться в главное меню или загрузить стартовую сцену.
            SceneManager.LoadScene("MainMenu"); // Замените на имя вашей главной сцены
            return;
        }

        // Убедимся, что время идет нормально (могло быть заморожено предыдущей сценой)
        Time.timeScale = 1f;

        // Начинаем асинхронную загрузку сцены
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        // Начинаем асинхронную операцию
        asyncOperation = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncOperation.allowSceneActivation = false; // Не активировать сцену сразу после загрузки

        float progress = 0; // Текущий прогресс для отображения на UI
        float targetProgress = 0; // Целевой прогресс (от асинхронной операции)

        while (!asyncOperation.isDone)
        {
            // Обновляем целевой прогресс (Unity выдает от 0 до 0.9, где 0.9 = сцена загружена, но не активирована)
            targetProgress = asyncOperation.progress;

            // Плавно интерполируем отображаемый прогресс, чтобы он выглядел более плавно
            progress = Mathf.MoveTowards(progress, targetProgress, Time.deltaTime * fakeProgressSpeed);
            
            // Если прогресс достиг 0.9, это значит, что сцена загружена, но еще не активирована
            if (progress >= 0.9f)
            {
                progress = 1.0f; // Устанавливаем 100% для UI
            }

            // Обновляем UI прогресс-бар и текст
            if (progressBar != null)
            {
                progressBar.value = progress;
            }
            if (loadingText != null)
            {
                loadingText.text = "Загрузка... " + (int)(progress * 100) + "%";
            }

            // Если сцена полностью загружена (до 0.9) и прогресс-бар дошел до конца,
            // можно подождать нажатия кнопки или активировать сцену автоматически.
            if (asyncOperation.progress >= 0.9f && progress >= 1.0f)
            {
                // Опционально: можно добавить задержку или сообщение "Нажмите любую кнопку..."
                // yield return new WaitForSeconds(0.5f); // Короткая задержка перед активацией

                // Если нужно, чтобы игрок нажал кнопку:
                // if (loadingText != null) loadingText.text = "Нажмите любую кнопку...";
                // while (!Input.anyKeyDown)
                // {
                //     yield return null;
                // }

                asyncOperation.allowSceneActivation = true; // Активируем загруженную сцену
            }

            yield return null; // Ждем следующего кадра
        }
        
        // Эта часть кода выполнится после того, как asyncOperation.isDone станет true
        // (то есть после SceneActivation). Здесь можно ничего не делать, так как сцена уже загружена.
    }
}