using UnityEngine;

public class CameraSoundPlayer : MonoBehaviour
{
    public AudioClip cameraMoveSound; // Звуковой клип
    private AudioSource audioSource; // Компонент AudioSource

    void Awake() // Используйте Awake, чтобы получить компонент раньше, чем Start
    {
        // Получаем компонент AudioSource. Если его нет, добавляем.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Эта функция будет вызвана из Animation Event
    public void PlayCameraSound()
    {
        if (cameraMoveSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(cameraMoveSound); // Воспроизвести звук
            Debug.Log("Воспроизведение звука камеры.");
        }
    }
}