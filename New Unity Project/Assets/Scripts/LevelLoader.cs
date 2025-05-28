using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Переход на следующий уровень...");

            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            Debug.Log("Следующая сцена по индексу: " + nextSceneIndex);

            // Проверяем, что следующая сцена существует в настройках сборки
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                // *** ВОТ ЧТО МЫ ДОБАВЛЯЕМ/МЕНЯЕМ ***

                // 1. Сбрасываем счетчик листьев ПЕРЕД загрузкой следующей сцены
                LeafCollector.leafCount = 0;
                Debug.Log("Счетчик листьев сброшен до: " + LeafCollector.leafCount + " перед загрузкой новой сцены.");

                // 2. Устанавливаем целевую сцену для LoadingScreenManager
                // SceneUtility.GetScenePathByBuildIndex(nextSceneIndex) вернет полный путь к сцене,
                // из которого нам нужно извлечь только имя. Это надежнее, чем использовать nextSceneIndex напрямую,
                // если LoadingScreenManager.sceneToLoad ожидает имя сцены.
                
                string nextSceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(nextSceneIndex));

                if (string.IsNullOrEmpty(nextSceneName))
                {
                    Debug.LogError($"[LevelLoader] Не удалось получить имя сцены для индекса {nextSceneIndex}. Проверьте Build Settings.");
                    return; // Прерываем операцию, если имя сцены не получено
                }

                // Устанавливаем целевую сцену в статической переменной LoadingScreenManager
                LoadingScreenManager.sceneToLoad = nextSceneName; 
                
                // 3. Загружаем сцену LoadingScreen
                // Убедитесь, что "LoadingScreen" добавлена в Build Settings и ее имя точно совпадает.
                SceneManager.LoadScene("LoadingScreen"); 
            }
            else
            {
                Debug.Log("Это последний уровень в настройках сборки. Возврат к началу или переход к экрану победы.");
                // Опционально: можно загрузить первую сцену (например, главное меню) через экран загрузки.
                // LoadingScreenManager.sceneToLoad = "MainMenu"; // Замените на имя вашей первой сцены
                // SceneManager.LoadScene("LoadingScreen");
                
                // Если это конец игры, можно загрузить сцену "GameComplete" или "Credits"
                LoadingScreenManager.sceneToLoad = "MainMenu"; // Пример: возвращаемся в MainMenu
                SceneManager.LoadScene("LoadingScreen");
            }
        }
    }
}