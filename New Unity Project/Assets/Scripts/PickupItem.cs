using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public string playerTag = "Player"; // Тег игрока, который может подобрать предмет
    public GameObject pickupEffectPrefab; // Опциональный префаб эффекта подбора (например, искры, звук)

    // Эта функция вызывается, когда коллайдер этого объекта сталкивается с другим коллайдером
    void OnTriggerEnter(Collider other)
    {
        // Проверяем, является ли другой объект игроком
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Предмет подобран игроком!"); // Сообщение в консоль

            // Опционально: Воспроизвести эффект подбора
            if (pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            }

            // Здесь вы можете добавить логику, что произойдет с подобранным предметом:
            // Например:
            // 1. Добавить предмет в инвентарь игрока (потребуется система инвентаря)
            //    other.GetComponent<PlayerInventory>().AddItem(this.gameObject);
            // 2. Увеличить счетчик очков, здоровья и т.д.
            //    other.GetComponent<PlayerStats>().AddScore(100);

            // Удаляем объект из сцены после подбора
            Destroy(gameObject);
        }
    }
}