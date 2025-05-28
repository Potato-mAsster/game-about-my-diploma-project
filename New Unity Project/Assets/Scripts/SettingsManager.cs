using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // Добавляем для работы с событиями загрузки сцен (если нужно)

public class SettingsManager : MonoBehaviour
{
    // --- Синглтон паттерн (без DontDestroyOnLoad) ---
    // Этот менеджер будет существовать только в пределах одной сцены.
    // При переходе на новую сцену он будет уничтожен вместе со сценой
    // и создан заново, если присутствует в новой сцене.
    public static SettingsManager Instance { get; private set; }

    private void Awake()
    {
        // Проверяем, существует ли уже экземпляр этого менеджера В ЭТОЙ СЦЕНЕ
        // (если этот скрипт размещен на нескольких объектах в одной сцене)
        if (Instance == null)
        {
            // Если нет, то этот экземпляр становится единственным для этой сцены
            Instance = this;
            // DontDestroyOnLoad(gameObject); // ЭТА СТРОКА УДАЛЕНА
            // Debug.Log(gameObject.name + " помечен как DontDestroyOnLoad."); // И это сообщение тоже убираем
        }
        else
        {
            // Если экземпляр уже существует в этой сцене, уничтожаем дубликат
            Debug.LogWarning(gameObject.name + " попытка создания дубликата В ЭТОЙ СЦЕНЕ. Уничтожаю.");
            Destroy(gameObject);
        }
    }

    // ... остальной код вашего SettingsManager ...
    // (методы Start(), SetMusicVolume, LoadSettings и т.д.)
    // ...

    [Header("UI References")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public TMP_Dropdown resolutionDropdown;
    public GameObject settingsPanelUI; // Ссылка на саму панель настроек

    [Header("Audio Mixer")]
    public AudioMixer mainMixer;

    private const string MUSIC_VOLUME_KEY = "musicVolume";
    private const string SFX_VOLUME_KEY = "sfxVolume";
    private const string RESOLUTION_INDEX_KEY = "resolutionIndex";

    private const string MUSIC_MIXER_PARAM = "MusicVolume";
    private const string SFX_MIXER_PARAM = "SFXVolume";

    private Resolution[] resolutions;

    void Start()
    {
        // Теперь Start() будет вызываться при каждой загрузке сцены,
        // если объект с этим скриптом присутствует в новой сцене.
        // Поэтому здесь нужно будет заново привязывать UI элементы,
        // если они находятся на Canvas, который создается в каждой сцене.

        // Предполагаем, что UI элементы настроек находятся на панели settingsPanelUI,
        // которая должна быть назначена в инспекторе или найдена в Start().
        if (settingsPanelUI != null)
        {
            // Привязываем UI элементы, если они назначены или найдены
            // (если вы не перетащили их вручную, они будут null)
            if (musicVolumeSlider == null) musicVolumeSlider = settingsPanelUI.GetComponentInChildren<Slider>();
            // Используйте Find("SFXSlider") или GetComponentInChildren<Slider>() по порядку, если их несколько
            // Если вы назначаете вручную, эти Find не нужны.
            if (sfxVolumeSlider == null) sfxVolumeSlider = settingsPanelUI.transform.Find("SFXSlider")?.GetComponent<Slider>(); // Пример поиска по имени дочернего объекта
            if (resolutionDropdown == null) resolutionDropdown = settingsPanelUI.GetComponentInChildren<TMP_Dropdown>();

            if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            if (resolutionDropdown != null) resolutionDropdown.onValueChanged.AddListener(SetResolution);
        }
        else
        {
            Debug.LogWarning("[SettingsManager] Панель настроек (settingsPanelUI) не назначена в инспекторе. UI настройки не будут работать.");
        }

        SetupResolutionDropdown(); // Инициализируем дропдаун разрешения
        LoadSettings(); // Загружаем настройки

        // Убедимся, что панель настроек скрыта по умолчанию
        if (settingsPanelUI != null) settingsPanelUI.SetActive(false);
    }

    private void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null) return; // Проверка на null

        resolutions = Screen.resolutions;
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetMusicVolume(float volume)
    {
        if (mainMixer != null)
        {
            mainMixer.SetFloat(MUSIC_MIXER_PARAM, Mathf.Log10(volume) * 20);
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume); // Сохраняем сразу при изменении
            PlayerPrefs.Save();
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (mainMixer != null)
        {
            mainMixer.SetFloat(SFX_MIXER_PARAM, Mathf.Log10(volume) * 20);
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume); // Сохраняем сразу при изменении
            PlayerPrefs.Save();
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutionDropdown == null || resolutionIndex < 0 || resolutionIndex >= resolutions.Length) return;

        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt(RESOLUTION_INDEX_KEY, resolutionIndex); // Сохраняем сразу при изменении
        PlayerPrefs.Save();
    }

    // Методы SaveSettings() и LoadSettings() теперь не нужны для UI,
    // так как значения сохраняются сразу при изменении слайдера/дропдауна.
    // Но LoadSettings() все еще нужна для инициализации UI при старте.

    void LoadSettings()
    {
        float musicVolume = PlayerPrefs.HasKey(MUSIC_VOLUME_KEY) ? PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY) : 1f;
        float sfxVolume = PlayerPrefs.HasKey(SFX_VOLUME_KEY) ? PlayerPrefs.GetFloat(SFX_VOLUME_KEY) : 1f;
        int resolutionIndex = PlayerPrefs.HasKey(RESOLUTION_INDEX_KEY) ? PlayerPrefs.GetInt(RESOLUTION_INDEX_KEY) : 0;

        // Если UI элементы назначены, устанавливаем их значения
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolume;
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVolume;
        }

        if (resolutionDropdown != null)
        {
            if (resolutionDropdown.options.Count > resolutionIndex)
            {
                 resolutionDropdown.value = resolutionIndex;
            }
            else
            {
                resolutionDropdown.value = 0; // Сброс, если индекс недействителен
            }
            resolutionDropdown.RefreshShownValue();
        }
        
        // Применяем загруженные настройки к системе (микшеры, разрешение)
        // Эти вызовы Set...Volume/Resolution убедятся, что микшер/разрешение устанавливаются
        // даже если UI элементы не были найдены.
        SetMusicVolume(musicVolume); // Вызываем напрямую, чтобы применить к микшеру
        SetSFXVolume(sfxVolume);     // Вызываем напрямую, чтобы применить к микшеру
        if (resolutionDropdown != null)
        {
            SetResolution(resolutionDropdown.value); // Вызываем, чтобы применить разрешение
        } else {
            // Если дропдаун null, но разрешение должно быть применено
            if (resolutions != null && resolutions.Length > resolutionIndex) {
                 Screen.SetResolution(resolutions[resolutionIndex].width, resolutions[resolutionIndex].height, Screen.fullScreen);
            }
        }


        Debug.Log("[SettingsManager] Настройки загружены и применены.");
    }

    // Пример методов для открытия/закрытия панели настроек (вызывается из кнопки в UI)
    public void OpenSettingsPanel()
    {
        if (settingsPanelUI != null)
        {
            settingsPanelUI.SetActive(true);
            // Если настройки могут меняться извне (напр. из других скриптов),
            // можно вызывать LoadSettings() здесь для обновления UI.
            LoadSettings(); 
        }
        else
        {
            Debug.LogWarning("[SettingsManager] Попытка открыть панель настроек, но ссылка на нее не установлена.");
        }
    }

    public void CloseSettingsPanel()
    {
        if (settingsPanelUI != null)
        {
            // SaveSettings(); // Не нужно, так как сохраняем сразу при изменении
            settingsPanelUI.SetActive(false);
        }
    }
}