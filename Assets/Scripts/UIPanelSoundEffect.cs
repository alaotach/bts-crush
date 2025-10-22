using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class UIPanelSoundEffect : MonoBehaviour
{
    [Header("Sound Settings")]
    [Tooltip("Play swoosh sound when panel opens")]
    public bool playSwooshIn = true;
    
    [Tooltip("Play swoosh sound when panel closes")]
    public bool playSwooshOut = true;
    
    [Tooltip("Custom sound to play on open (optional)")]
    public AudioClip customOpenSound;
    
    [Tooltip("Custom sound to play on close (optional)")]
    public AudioClip customCloseSound;
    
    [Header("Animation Settings")]
    [Tooltip("Animate panel scale when opening")]
    public bool animateScale = true;
    
    [Tooltip("Animation duration")]
    public float animationDuration = 0.3f;
    
    [Tooltip("Start scale for pop-in effect")]
    public float startScale = 0.8f;
    
    [Tooltip("Use bounce effect")]
    public bool useBounce = true;
    
    private Vector3 originalScale;
    private CanvasGroup canvasGroup;
    private bool isAnimating = false;
    private BTSSpecialCandyManager soundManager;
    
    void Awake()
    {
        originalScale = transform.localScale;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        soundManager = FindObjectOfType<BTSSpecialCandyManager>();
    }
    
    void OnEnable()
    {
        if (!isAnimating)
        {
            PlayOpenEffects();
        }
    }
    
    void OnDisable()
    {
        transform.localScale = originalScale;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }
    
    public void PlayOpenEffects()
    {
        if (soundManager == null) return;
        
        // Play sound
        if (customOpenSound != null)
        {
            soundManager.PlayCustomSound(customOpenSound);
        }
        else if (playSwooshIn)
        {
            soundManager.PlayPanelOpenSound();
        }
        
        // Play animation
        if (animateScale)
        {
            StartCoroutine(AnimateOpen());
        }
    }
    
    public void PlayCloseEffects()
    {
        if (soundManager == null) return;
        
        // Play sound
        if (customCloseSound != null)
        {
            soundManager.PlayCustomSound(customCloseSound);
        }
        else if (playSwooshOut)
        {
            soundManager.PlayPanelCloseSound(); // Use panel close sound
        }
        
        // Play animation
        if (animateScale)
        {
            StartCoroutine(AnimateClose());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private IEnumerator AnimateOpen()
    {
        isAnimating = true;
        transform.localScale = originalScale * startScale;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animationDuration;
            float scale;
            if (useBounce)
            {
                scale = Mathf.Lerp(startScale, 1f, EaseOutBack(t));
            }
            else
            {
                scale = Mathf.Lerp(startScale, 1f, EaseOutQuad(t));
            }
            
            transform.localScale = originalScale * scale;
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            }
            
            yield return null;
        }
        transform.localScale = originalScale;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        
        isAnimating = false;
    }
 
    private IEnumerator AnimateClose()
    {
        isAnimating = true;
        
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animationDuration;
            
            float scale = Mathf.Lerp(1f, startScale, EaseInQuad(t));
            transform.localScale = originalScale * scale;
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            }
            
            yield return null;
        }
        
        isAnimating = false;
        gameObject.SetActive(false);
    }
    
    private float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }
    
    private float EaseInQuad(float t)
    {
        return t * t;
    }
    
    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
    
    public void ClosePanel()
    {
        PlayCloseEffects();
    }
}
