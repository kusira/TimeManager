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
            if (bgmSlider != null)
            {
                // 初期値の設定 (スライダーの現在の値を使用)
                SetBGMVolume(bgmSlider.value);
                // リスナー登録
                bgmSlider.onValueChanged.AddListener(SetBGMVolume);
            }

            if (seSlider != null)
            {
                // 初期値の設定
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
        }
    }
}

