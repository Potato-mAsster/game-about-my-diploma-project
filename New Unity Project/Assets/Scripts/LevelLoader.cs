using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    private DatabaseManager dbManager;

    void Start()
    {
        dbManager = DatabaseManager.Instance;
        if (dbManager == null)
        {
            Debug.LogError("[LevelLoader] DatabaseManager не найден! Отключаем скрипт.");
            this.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (dbManager == null || dbManager.CurrentPlayerId == -1)
        {
            Debug.LogWarning("[LevelLoader] Текущий игрок не определен или DatabaseManager не инициализирован. Прогресс не будет сохранен.");
            return;
        }

        if (other.CompareTag("Player"))
        {
            Debug.Log("Игрок вошел в триггер перехода на следующий уровень.");

            string currentSceneName = SceneManager.GetActiveScene().name;
            DatabaseManager.LevelData currentLevelData = dbManager.GetLevelDataBySceneName(currentSceneName);

            if (currentLevelData == null)
            {
                Debug.LogError($"[LevelLoader] Информация о текущем уровне '{currentSceneName}' не найдена в БД. Возврат в главное меню.");
                LoadingScreenManager.sceneToLoad = "MainMenu";
                SceneManager.LoadScene("LoadingScreen");
                return;
            }

            Debug.Log($"Текущий уровень (из БД): ID={currentLevelData.id}, Name={currentLevelData.levelName}, Order={currentLevelData.order}");

            // --- ПОЛУЧАЕМ ЗАТРАЧЕННОЕ ВРЕМЯ ИЗ GameTimer ---
            float levelCompletionTime = -1f; // Инициализация значением по умолчанию
            if (GameTimer.Instance != null)
            {
                levelCompletionTime = GameTimer.Instance.CurrentLevelElapsedTime; 
                GameTimer.Instance.StopLevelTimer(); 
                Debug.Log($"[LevelLoader] Время прохождения уровня получено из GameTimer: {levelCompletionTime} секунд.");
            }
            else
            {
                Debug.LogWarning("[LevelLoader] GameTimer не найден. Время прохождения уровня не будет записано.");
            }

            // --- ПОЛУЧАЕМ КОЛИЧЕСТВО СОБРАННЫХ ЛИСТЬЕВ ИЗ LeafCollector ---
            int currentScore = LeafCollector.leafCount; // <-- ИЗМЕНЕНО: ИСПОЛЬЗУЕМ leafCount
            Debug.Log($"[LevelLoader] Очки (собранные листья) на уровне: {currentScore}.");


            // 2. Сохраняем прогресс для ТЕКУЩЕГО уровня: помечаем его как завершенный и обновляем статистику
            dbManager.SetLevelCompleted(dbManager.CurrentPlayerId, currentLevelData.id, true, levelCompletionTime, currentScore); 
            // dbManager.IncrementLevelAttempts() вызывается в GameTimer.Awake

            // 3. Находим СЛЕДУЮЩИЙ уровень в БД (по порядку)
            DatabaseManager.LevelData nextLevelData = dbManager.GetLevelDataByOrder(currentLevelData.order + 1);

            if (nextLevelData != null)
            {
                Debug.Log($"Найден следующий уровень в БД: ID={nextLevelData.id}, Name={nextLevelData.levelName}, Order={nextLevelData.order}");

                // 4. Разблокируем СЛЕДУЮЩИЙ уровень для текущего игрока
                dbManager.SetLevelUnlocked(dbManager.CurrentPlayerId, nextLevelData.id, true);

                LeafCollector.leafCount = 0; // Сброс листьев для следующего уровня (это уже было)
                Debug.Log("Счетчик листьев сброшен до: " + LeafCollector.leafCount + " перед загрузкой новой сцены.");
                
                LoadingScreenManager.sceneToLoad = nextLevelData.sceneName; 
                Debug.Log($"Переход на сцену: {nextLevelData.sceneName} через LoadingScreen.");
                SceneManager.LoadScene("LoadingScreen");
            }
            else
            {
                // Если следующего уровня в БД нет, загружаем EndingScene
                Debug.Log("Это последний уровень в последовательности из БД. Переход к EndingScene.");
                LoadingScreenManager.sceneToLoad = "Ending"; // Замените на точное имя вашей сцены концовки
                SceneManager.LoadScene("LoadingScreen");
            }
        }
    }
}