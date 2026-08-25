using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Roguelite.SaveSystem;

public class SoundMixerManager : MonoBehaviour
{
    private const float MinLinear = 0.0001f;
    private const float DefaultLinear = 1f;
    private const string MasterPrefKey = "Audio.MasterVolume";
    private const string MusicPrefKey = "Audio.MusicVolume";
    private const string SfxPrefKey = "Audio.SFXVolume";

    [SerializeField] private AudioMixer audioMixer;

    private void Awake()
    {
        ApplySavedVolumesToMixer();
    }

    private void Start()
    {
        SyncSlidersToSavedVolumes();
        ApplySavedVolumesToMixer();
    }

    public void SetMasterVolume(float level)
    {
        SaveAndApply(MasterPrefKey, "MasterVolume", level);
    }

    public void SetSFXVolume(float level)
    {
        SaveAndApply(SfxPrefKey, "SFXVolume", level);
    }

    public void SetMusicVolume(float level)
    {
        SaveAndApply(MusicPrefKey, "MusicVolume", level);
    }

    private void ApplySavedVolumesToMixer()
    {
        ApplyMixerExposed("MasterVolume", LoadLinear(MasterPrefKey));
        ApplyMixerExposed("MusicVolume", LoadLinear(MusicPrefKey));
        ApplyMixerExposed("SFXVolume", LoadLinear(SfxPrefKey));
    }

    private void SyncSlidersToSavedVolumes()
    {
        Slider[] sliders = FindObjectsByType<Slider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < sliders.Length; i++)
        {
            Slider slider = sliders[i];
            if (slider == null)
            {
                continue;
            }

            string sliderName = NormalizeName(slider.gameObject.name);
            if (sliderName == "mastervolume")
            {
                BindSlider(slider, LoadLinear(MasterPrefKey), SetMasterVolume);
            }
            else if (sliderName == "music" || sliderName == "musicvolume")
            {
                BindSlider(slider, LoadLinear(MusicPrefKey), SetMusicVolume);
            }
            else if (sliderName == "sfx" || sliderName == "sfxvolume")
            {
                BindSlider(slider, LoadLinear(SfxPrefKey), SetSFXVolume);
            }
        }
    }

    private static void BindSlider(Slider slider, float linearValue, UnityEngine.Events.UnityAction<float> setter)
    {
        slider.onValueChanged.RemoveListener(setter);
        slider.SetValueWithoutNotify(Mathf.Clamp(linearValue, slider.minValue, slider.maxValue));
        slider.onValueChanged.AddListener(setter);
    }

    private void SaveAndApply(string prefKey, string mixerParameter, float linearLevel)
    {
        float clamped = Mathf.Clamp(linearLevel, MinLinear, 1f);
        PlayerPrefs.SetFloat(prefKey, clamped);

        if (SaveManager.Instance != null && SaveManager.Instance.CurrentSettingData != null)
        {
            var setting = SaveManager.Instance.CurrentSettingData;
            if (mixerParameter == "MasterVolume") setting.masterVolume = clamped;
            else if (mixerParameter == "MusicVolume") setting.bgmVolume = clamped;
            else if (mixerParameter == "SFXVolume") setting.sfxVolume = clamped;

            SaveManager.Instance.SaveSettingData();
        }

        ApplyMixerExposed(mixerParameter, clamped);
    }

    private void ApplyMixerExposed(string mixerParameter, float linearLevel)
    {
        if (audioMixer == null)
        {
            return;
        }

        float clamped = Mathf.Clamp(linearLevel, MinLinear, 1f);
        audioMixer.SetFloat(mixerParameter, Mathf.Log10(clamped) * 20f);
    }

    private static float LoadLinear(string prefKey)
    {
        if (SaveManager.Instance != null && SaveManager.Instance.CurrentSettingData != null)
        {
            var setting = SaveManager.Instance.CurrentSettingData;
            if (prefKey == MasterPrefKey) return Mathf.Clamp(setting.masterVolume, MinLinear, 1f);
            if (prefKey == MusicPrefKey) return Mathf.Clamp(setting.bgmVolume, MinLinear, 1f);
            if (prefKey == SfxPrefKey) return Mathf.Clamp(setting.sfxVolume, MinLinear, 1f);
        }

        return Mathf.Clamp(PlayerPrefs.GetFloat(prefKey, DefaultLinear), MinLinear, 1f);
    }

    private static string NormalizeName(string name)
    {
        return name.Replace(" ", string.Empty).ToLowerInvariant();
    }
}
