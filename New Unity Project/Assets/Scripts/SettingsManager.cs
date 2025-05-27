using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    // --- Синглтон паттерн (очень рекомендуется) ---
    // Это гарантирует, что у вас всегда будет только ОДИН экземпляр этого менеджера
    public static SettingsManager Instance { get; private set; }

    private void Awake()
    {
        // Проверяем, существует ли уже экземпляр этого менеджера
        if (Instance == null)
        {
            // Если нет, то этот экземпляр становится единственным
            Instance = this;
            // И мы говорим Unity НЕ уничтожать этот GameObject при загрузке новой сцены
            DontDestroyOnLoad(gameObject);
            Debug.Log(gameObject.name + " помечен как DontDestroyOnLoad.");
        }
        else
        {
            // Если экземпляр уже существует, значит, мы загрузили новую сцену,
            // и там уже есть старый менеджер. Новый нам не нужен, уничтожаем его.
            Debug.LogWarning(gameObject.name + " попытка создания дубликата. Уничтожаю.");
            Destroy(gameObject);
        }
    }

    // ... остальной код вашего SettingsManager ...
    // (методы Start(), SetMusicVolume, LoadSettings и т.д.)
    // Все они остаются без изменений
    // ...

    [Header("UI References")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public TMP_Dropdown resolutionDropdown;

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
        // Здесь может потребоваться дополнительная проверка, если UI элементы
        // находятся в сцене, которая *уничтожается* при переходе.
        // Если SettingsManager является DontDestroyOnLoad, а его UI элементы - нет,
        // то ссылки на UI элементы будут null при загрузке новой сцены.
        // В этом случае, вам нужно будет найти UI элементы в новой сцене
        // после ее загрузки (например, в методе, который вызывается после SceneManager.sceneLoaded)
        // или передавать им ссылки.

        // Для настроек UI обычно лучше, чтобы сам SettingsManager был DontDestroyOnLoad,
        // а UI элементы настроек были в одной сцене (например, Главное меню),
        // и вы показывали/скрывали эту сцену или просто UI панель.
        // Если же UI элементы тоже должны быть DontDestroyOnLoad, то их также нужно помечать.

        // Инициализируем дропдаун разрешения
        SetupResolutionDropdown(); // Это будет работать, если UI элементы тоже DontDestroyOnLoad
                                  // ИЛИ если этот Start() вызывается только в первой сцене

        LoadSettings();

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }
        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        }
    }

    private void SetupResolutionDropdown()
    {
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

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (mainMixer != null)
        {
            mainMixer.SetFloat(MUSIC_MIXER_PARAM, Mathf.Log10(volume) * 20);
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (mainMixer != null)
        {
            mainMixer.SetFloat(SFX_MIXER_PARAM, Mathf.Log10(volume) * 20);
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutionIndex >= 0 && resolutionIndex < resolutions.Length)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolumeSlider != null ? musicVolumeSlider.value : 1f);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolumeSlider != null ? sfxVolumeSlider.value : 1f);
        PlayerPrefs.SetInt(RESOLUTION_INDEX_KEY, resolutionDropdown != null ? resolutionDropdown.value : 0);

        PlayerPrefs.Save();
        Debug.Log("Настройки сохранены.");
    }

    void LoadSettings()
    {
        float musicVolume = PlayerPrefs.HasKey(MUSIC_VOLUME_KEY) ? PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY) : 1f;
        float sfxVolume = PlayerPrefs.HasKey(SFX_VOLUME_KEY) ? PlayerPrefs.GetFloat(SFX_VOLUME_KEY) : 1f;
        int resolutionIndex = PlayerPrefs.HasKey(RESOLUTION_INDEX_KEY) ? PlayerPrefs.GetInt(RESOLUTION_INDEX_KEY) : 0;

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolume;
            SetMusicVolume(musicVolume);
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVolume;
            SetSFXVolume(sfxVolume);
        }

        if (resolutionDropdown != null)
        {
            if (resolutionDropdown.options.Count > resolutionIndex)
            {
                 resolutionDropdown.value = resolutionIndex;
            }
            else
            {
                resolutionDropdown.value = 0;
            }
            resolutionDropdown.RefreshShownValue();
            SetResolution(resolutionDropdown.value);
        }

        Debug.Log("Настройки загружены.");
    }
}