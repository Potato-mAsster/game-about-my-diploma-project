using UnityEngine;

public class FloatingItem : MonoBehaviour
{
    public float floatAmplitude = 0.5f; // Амплитуда колебаний (насколько высоко/низко будет двигаться предмет)
    public float floatFrequency = 1f;    // Частота колебаний (как быстро он будет двигаться)

    private Vector3 startPos; // Начальная позиция предмета

    void Start()
    {
        startPos = transform.position; // Запоминаем начальную позицию объекта
    }

    void Update()
    {
        // Вычисляем новое положение по оси Y
        // Mathf.Sin(Time.time * floatFrequency) создает синусоидальную волну от -1 до 1
        // Умножаем на floatAmplitude, чтобы контролировать высоту колебаний
        // Добавляем к startPos.y, чтобы колебания происходили вокруг начальной позиции
        transform.position = new Vector3(startPos.x, startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude, startPos.z);
    }
}