using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    public float destroyTime = 2f; // Время, через которое объект будет уничтожен

    void Start()
    {
        // Проигрываем звук, если есть AudioSource
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioSource.clip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }

        // Уничтожить объект через destroyTime секунд
        Destroy(gameObject, destroyTime);
    }
}