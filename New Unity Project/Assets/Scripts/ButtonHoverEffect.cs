using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Color hoverColor = Color.yellow; // Цвет подсветки при наведении
    private Color originalColor;
    private Image buttonImage;
    private Text buttonText; // Для подсветки текста, если необходимо

    void Start()
    {
        buttonImage = GetComponent<Image>();
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }
        buttonText = GetComponentInChildren<Text>(); // Поиск TextMeshPro
        if (buttonText == null)
        {
            TMPro.TextMeshProUGUI tmpText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
            {
                buttonText = tmpText.GetComponent<Text>(); // Получаем Text компонент из TMP
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonImage != null)
        {
            buttonImage.color = hoverColor;
        }
        if (buttonText != null)
        {
            buttonText.color = hoverColor; // Подсвечиваем текст
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonImage != null)
        {
            buttonImage.color = originalColor;
        }
        if (buttonText != null)
        {
            buttonText.color = originalColor; // Возвращаем исходный цвет текста
        }
    }
}