using UnityEngine;

public class LeafCollector : MonoBehaviour
{
    public static int leafCount = 0;

    void Awake()
    {
        // Сброс leafCount до 0 при загрузке сцены
        leafCount = 0; 
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            leafCount++;
            Destroy(gameObject);
        }
    }
}