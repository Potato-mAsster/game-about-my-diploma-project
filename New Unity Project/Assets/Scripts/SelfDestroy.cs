using UnityEngine;
using UnityEngine.Audio; // Обязательно для работы с AudioMixer

public class SelfDestroy : MonoBehaviour
{
    [Tooltip("Время в секундах, через которое объект будет уничтожен.")]
    public float destroyTime = 2f; // Время, через которое объект будет уничтожен

    [Header("Настройки Audio Mixer")]
    [Tooltip("Ссылка на ваш Audio Mixer (перетащите сюда ваш MainMixer из Project Window).")]
    public AudioMixer audioMixer; // Ссылка на ваш Audio Mixer
    
    [Tooltip("Имя группы Audio Mixer, к которой относится этот звук (например, 'SFX' или 'Music').")]
    public string audioMixerGroupName = "SFX"; // Имя группы Audio Mixer, к которой относится этот звук

    void Start()
    {

        // Получаем компонент AudioSource на этом GameObject
        AudioSource audioSource = GetComponent<AudioSource>();
    Debug.Log("SelfDestroy скрипт активирован на объекте: " + gameObject.name, this);

        // Проверяем, существует ли AudioSource, есть ли у него клип и не проигрывается ли он уже
        if (audioSource != null && audioSource.clip != null && !audioSource.isPlaying)
        {
            // Если Audio Mixer назначен
            if (audioMixer != null)
            {
                // Ищем соответствующую группу в Audio Mixer по имени
                AudioMixerGroup[] groups = audioMixer.FindMatchingGroups(audioMixerGroupName);
                
                if (groups.Length > 0)
                {
                    // Назначаем AudioSource на найденную группу микшера
                    audioSource.outputAudioMixerGroup = groups[0];
                }
                else
                {
                    // Выводим предупреждение, если группа не найдена
                    Debug.LogWarning($"[SelfDestroy] Группа Audio Mixer '{audioMixerGroupName}' не найдена в микшере '{audioMixer.name}'! Звук будет воспроизводиться без микшера.");
                }
            }
            else
            {
                // Выводим предупреждение, если Audio Mixer не назначен в инспекторе
                Debug.LogWarning("[SelfDestroy] Ссылка на Audio Mixer не установлена в скрипте. Звук будет воспроизводиться без микшера.");
            }

            // Проигрываем звук
            audioSource.Play();
        }

        // Уничтожить объект через destroyTime секунд
        Destroy(gameObject, destroyTime);
    }
}