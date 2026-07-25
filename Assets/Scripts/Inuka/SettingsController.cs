using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        musicSlider.value = AudioManager.Instance.BgmVolume;
        sfxSlider.value = AudioManager.Instance.SfxVolume;

        musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetBgmVolume);
        sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSfxVolume);
    }

    private void OnDestroy()
    {
        if (AudioManager.Instance == null) return;

        musicSlider.onValueChanged.RemoveListener(AudioManager.Instance.SetBgmVolume);
        sfxSlider.onValueChanged.RemoveListener(AudioManager.Instance.SetSfxVolume);
    }
}