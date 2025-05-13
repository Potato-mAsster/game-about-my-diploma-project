using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Добавляем для подписки на событие загрузки сцены
using System.Collections;

public class FadeScreen : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1f; // Длительность затухания в секундах
    private Color targetColor;
    private bool isFading = false; // Флаг, чтобы избежать повторных запусков затухания

    // Статический экземпляр скрипта, чтобы к нему можно было обращаться из других скриптов
    public static FadeScreen instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // Сделать объект FadeScreen persistent между сценами
            DontDestroyOnLoad(gameObject.transform.root.gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        // Подписываемся на событие загрузки новой сцены, чтобы запустить FadeIn
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // Отписываемся от события, чтобы избежать утечек памяти
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Запускаем появление экрана при загрузке новой сцены
        FadeIn();
    }

    // Запустить затухание до черного экрана и затем выполнить действие (например, загрузку сцены)
    public void FadeOut(System.Action onFadeComplete = null)
    {
        if (isFading) return;
        isFading = true;
        targetColor = new Color(0f, 0f, 0f, 1f); // Полностью черный, полностью непрозрачный
        StartCoroutine(Fade(targetColor, () =>
        {
            onFadeComplete?.Invoke();
            isFading = false;
        }));
    }

    // Запустить появление экрана из черного
    public void FadeIn(System.Action onFadeComplete = null)
    {
        if (isFading) return;
        isFading = true;
        targetColor = new Color(0f, 0f, 0f, 0f); // Полностью черный, полностью прозрачный
        StartCoroutine(Fade(targetColor, () =>
        {
            onFadeComplete?.Invoke();
            isFading = false;
        }));
    }

    IEnumerator Fade(Color endColor, System.Action onFadeComplete = null)
    {
        if (fadeImage == null)
        {
            Debug.LogError("Fade Image не назначена!");
            yield break;
        }

        Color startColor = fadeImage.color;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(time / fadeDuration);
            fadeImage.color = Color.Lerp(startColor, endColor, normalizedTime);
            yield return null;
        }

        fadeImage.color = endColor; // Убедиться, что цвет установлен окончательно
        onFadeComplete?.Invoke(); // Вызвать callback после завершения затухания
    }
}