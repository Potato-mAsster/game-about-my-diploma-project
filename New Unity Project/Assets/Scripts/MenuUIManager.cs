using UnityEngine;
using UnityEngine.UI; // Обязательно для работы с UI компонентами, такими как Button

public class MenuUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject infoPanel; // Ссылка на вашу информационную панель
    public GameObject settingsPanel;
    // Метод, который будет вызываться при нажатии на кнопку
    public void ToggleInfoPanel()
    {
        // Переключаем состояние активности панели:
        // Если панель активна (true), делаем ее неактивной (false)
        // Если панель неактивна (false), делаем ее активной (true)
        infoPanel.SetActive(!infoPanel.activeSelf);

        // Также можно явно показать или скрыть:
        // infoPanel.SetActive(true);  // Показать панель
        // infoPanel.SetActive(false); // Скрыть панель
    }
public void ToggleSettingsPanel()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }
    // Если вы хотите, чтобы панель была скрыта при запуске игры
    void Start()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false); // Убедимся, что панель скрыта при старте
        }
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false); // Убедимся, что панель скрыта при старте
        }
    }
}