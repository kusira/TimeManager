using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

namespace Components.Game.Canvas.Scripts
{
    public class VolumeManager : MonoBehaviour
    {
        [Header("Audio Mixer")]
        [Tooltip("AudioMixerをアサイン (Exposed Parameters: 'BGM', 'SE' が必要)")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string bgmPrefKey = "VolumeManager_BGM";
        [SerializeField] private string sePrefKey = "VolumeManager_SE";

        [Header("BGM Settings")]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private TMP_Text bgmValueText;

        [Header("SE Settings")]
        [SerializeField] private Slider seSlider;
        [SerializeField] private TMP_Text seValueText;

        // AudioMixerのExposed Parameter名
        private const string BGM_PARAM = "BGM";
        private const string SE_PARAM = "SE";

        private void Start()
        {
            float savedBgm = PlayerPrefs.HasKey(bgmPrefKey) ? PlayerPrefs.GetFloat(bgmPrefKey) : (bgmSlider != null ? bgmSlider.value : 1f);
            float savedSe = PlayerPrefs.HasKey(sePrefKey) ? PlayerPrefs.GetFloat(sePrefKey) : (seSlider != null ? seSlider.value : 1f);

            if (bgmSlider != null)
            {
                bgmSlider.value = savedBgm;
                SetBGMVolume(bgmSlider.value);
                // リスナー登録
                bgmSlider.onValueChanged.AddListener(SetBGMVolume);
            }

            if (seSlider != null)
            {
                seSlider.value = savedSe;
                SetSEVolume(seSlider.value);
                // リスナー登録
                seSlider.onValueChanged.AddListener(SetSEVolume);
            }
        }

        public void SetBGMVolume(float value)
        {
            // UI更新 (0-100)
            if (bgmValueText != null)
            {
                bgmValueText.text = (value * 100f).ToString("F0");
            }

            // AudioMixer更新 (Decibel変換)
            // スライダー0のときは -80dB (無音) にする
            float db = value <= 0 ? -80f : Mathf.Log10(value) * 20f;
            
            if (audioMixer != null)
            {
                audioMixer.SetFloat(BGM_PARAM, db);
            }

            PlayerPrefs.SetFloat(bgmPrefKey, value);
            PlayerPrefs.Save();
        }

        public void SetSEVolume(float value)
        {
            // UI更新 (0-100)
            if (seValueText != null)
            {
                seValueText.text = (value * 100f).ToString("F0");
            }

            // AudioMixer更新 (Decibel変換)
            float db = value <= 0 ? -80f : Mathf.Log10(value) * 20f;

            if (audioMixer != null)
            {
                audioMixer.SetFloat(SE_PARAM, db);
            }

            PlayerPrefs.SetFloat(sePrefKey, value);
            PlayerPrefs.Save();
        }
    }
}

