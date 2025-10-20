using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Adds a satisfying squish animation to buttons when clicked.
/// Attach this script to any Button GameObject.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSquishAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animation Settings")]
    [Tooltip("Scale multiplier when button is pressed (e.g., 0.9 = shrink to 90%)")]
    [Range(0.5f, 1.0f)]
    public float pressedScale = 0.9f;
    
    [Tooltip("Scale multiplier when hovering over button (e.g., 1.1 = grow to 110%)")]
    [Range(1.0f, 1.3f)]
    public float hoverScale = 1.05f;
    
    [Tooltip("How fast the button animates (higher = faster)")]
    [Range(1f, 30f)]
    public float animationSpeed = 15f;
    
    [Tooltip("Add a slight bounce effect when releasing")]
    public bool bounceEffect = true;
    
    [Tooltip("Bounce overshoot amount (how much it bounces past normal size)")]
    [Range(1.0f, 1.2f)]
    public float bounceScale = 1.1f;
    
    [Header("Audio (Optional)")]
    [Tooltip("Sound to play when button is clicked")]
    public AudioClip clickSound;
    
    [Tooltip("Sound to play when hovering over button")]
    public AudioClip hoverSound;
    
    [Range(0f, 1f)]
    public float soundVolume = 0.5f;
    
    // Internal state
    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isPressed = false;
    private bool isHovering = false;
    private Button button;
    private AudioSource audioSource;
    private Coroutine animationCoroutine;
    
    void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;
        targetScale = originalScale;
        
        if (clickSound != null || hoverSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = soundVolume;
        }
    }
    
    void Update()
    {
        // Only animate if button is interactable
        if (!button.interactable)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * animationSpeed);
            return;
        }
        
        // Smoothly animate to target scale
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isPressed = true;
        targetScale = originalScale * pressedScale;
        
        // Play click sound
        PlaySound(clickSound);
        
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isPressed = false;
        
        if (bounceEffect)
        {
            // Bounce effect: overshoot then settle
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(BounceAnimation());
        }
        else
        {
            targetScale = isHovering ? originalScale * hoverScale : originalScale;
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isHovering = true;
        
        // Only hover if not already pressed
        if (!isPressed)
        {
            targetScale = originalScale * hoverScale;
        }
        
        // Play hover sound
        PlaySound(hoverSound);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isHovering = false;
        
        if (!isPressed)
        {
            targetScale = originalScale;
        }
    }
    
    private IEnumerator BounceAnimation()
    {
        // Bounce up (overshoot)
        float elapsed = 0f;
        float bounceDuration = 0.15f;
        Vector3 bounceTarget = originalScale * bounceScale;
        
        while (elapsed < bounceDuration)
        {
            targetScale = Vector3.Lerp(originalScale * pressedScale, bounceTarget, elapsed / bounceDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Settle back down
        elapsed = 0f;
        float settleDuration = 0.15f;
        Vector3 finalScale = isHovering ? originalScale * hoverScale : originalScale;
        
        while (elapsed < settleDuration)
        {
            targetScale = Vector3.Lerp(bounceTarget, finalScale, elapsed / settleDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        targetScale = finalScale;
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, soundVolume);
        }
    }
    
    // Public methods to trigger animation from code
    public void AnimatePress()
    {
        if (!button.interactable) return;
        StartCoroutine(ProgrammaticPress());
    }
    
    private IEnumerator ProgrammaticPress()
    {
        targetScale = originalScale * pressedScale;
        yield return new WaitForSeconds(0.1f);
        
        if (bounceEffect)
        {
            yield return StartCoroutine(BounceAnimation());
        }
        else
        {
            targetScale = originalScale;
        }
    }
    
    public void ResetScale()
    {
        transform.localScale = originalScale;
        targetScale = originalScale;
    }
    
    private void OnDisable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        transform.localScale = originalScale;
        targetScale = originalScale;
        isPressed = false;
        isHovering = false;
    }
}
