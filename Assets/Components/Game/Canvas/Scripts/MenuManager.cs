using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Components.Game.Canvas.Scripts
{
    public class MenuManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button menuButton;
        [SerializeField] private GameObject blackGround; 
        [SerializeField] private Transform menuPanel;
        [SerializeField] private Button closeButton;

        [Header("Settings")]
        [SerializeField] private float animationDuration = 0.3f;
        [Tooltip("パネルのアニメーション移動距離")]
        [SerializeField] private float slideDistance = 50f;

        private CanvasGroup backgroundCanvasGroup;
        private CanvasGroup panelCanvasGroup;
        private bool isMenuOpen = false;
        private float previousTimeScale = 1.0f;
        
        private Vector3 panelOriginalPos;

        private void Awake()
        {
            if (blackGround != null)
            {
                backgroundCanvasGroup = blackGround.GetComponent<CanvasGroup>();
                if (backgroundCanvasGroup == null)
                {
                    backgroundCanvasGroup = blackGround.AddComponent<CanvasGroup>();
                }
            }

            if (menuPanel != null)
            {
                panelCanvasGroup = menuPanel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                {
                    panelCanvasGroup = menuPanel.gameObject.AddComponent<CanvasGroup>();
                }
                panelOriginalPos = menuPanel.localPosition;
            }

            // Initial state: BlackGround inactive
            if (blackGround != null) blackGround.SetActive(false);
            isMenuOpen = false;
        }

        private void Start()
        {
            if (menuButton != null)
            {
                menuButton.onClick.AddListener(OpenMenu);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseMenu);
            }
        }

        private void OpenMenu()
        {
            if (isMenuOpen) return;
            isMenuOpen = true;

            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            if (blackGround != null)
            {
                blackGround.SetActive(true);
                StartCoroutine(AnimateMenu(true));
            }
        }

        private void CloseMenu()
        {
            if (!isMenuOpen) return;
            isMenuOpen = false;

            Time.timeScale = previousTimeScale;

            if (blackGround != null)
            {
                StartCoroutine(AnimateMenu(false));
            }
        }

        private IEnumerator AnimateMenu(bool opening)
        {
            float timer = 0f;
            float startAlpha = opening ? 0f : 1f;
            float endAlpha = opening ? 1f : 0f;

            // Position Logic:
            // Opening: From (Original - Offset) to Original
            // Closing: From Original to (Original - Offset)
            Vector3 startPos = opening ? panelOriginalPos - new Vector3(0, slideDistance, 0) : panelOriginalPos;
            Vector3 endPos = opening ? panelOriginalPos : panelOriginalPos - new Vector3(0, slideDistance, 0);

            if (backgroundCanvasGroup != null) backgroundCanvasGroup.alpha = startAlpha;
            
            // Panel CanvasGroup Alpha handling (optional, but good for fade in)
            if (panelCanvasGroup != null) panelCanvasGroup.alpha = startAlpha;

            if (menuPanel != null)
            {
                menuPanel.localPosition = startPos;
                // Also handle Scale if desired, but user asked for "fade in/out from below position"
                // Usually just position + alpha is enough. Let's keep scale fixed at 1?
                // Previously scale was 0->1. Let's reset scale to 1 just in case.
                menuPanel.localScale = Vector3.one;
            }

            while (timer < animationDuration)
            {
                timer += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(timer / animationDuration);
                
                float easedT = 1f - (1f - t) * (1f - t); // Ease Out Quad

                if (backgroundCanvasGroup != null)
                {
                    backgroundCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, easedT);
                }
                
                if (panelCanvasGroup != null)
                {
                    panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, easedT);
                }

                if (menuPanel != null)
                {
                    menuPanel.localPosition = Vector3.Lerp(startPos, endPos, easedT);
                }

                yield return null;
            }

            if (backgroundCanvasGroup != null) backgroundCanvasGroup.alpha = endAlpha;
            if (panelCanvasGroup != null) panelCanvasGroup.alpha = endAlpha;
            if (menuPanel != null) menuPanel.localPosition = endPos;

            if (!opening && blackGround != null)
            {
                blackGround.SetActive(false);
            }
        }
    }
}
