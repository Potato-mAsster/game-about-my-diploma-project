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

            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                // *** ВОТ ЧТО МЫ ДОБАВЛЯЕМ/МЕНЯЕМ ***

                // 1. Сбрасываем счетчик листьев ПЕРЕД загрузкой следующей сцены
                LeafCollector.leafCount = 0;
                Debug.Log("Счетчик листьев сброшен до: " + LeafCollector.leafCount + " перед загрузкой новой сцены.");

                // 2. Загружаем следующую сцену
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.Log("Это последний уровень в настройках сборки. Возврат к началу или переход к экрану победы.");
                // Опционально: можно загрузить первую сцену (например, главное меню)
                // SceneManager.LoadScene(0);
            }
        }
    }
}