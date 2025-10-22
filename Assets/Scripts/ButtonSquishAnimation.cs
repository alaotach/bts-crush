using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


[RequireComponent(typeof(Button))]
public class ButtonSquishAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Range(0.5f, 1.0f)]
    public float pressedScale = 0.9f;
    
    [Range(1.0f, 1.3f)]
    public float hoverScale = 1.05f;
    
    [Range(1f, 30f)]
    public float animationSpeed = 15f;
    
    public bool bounceEffect = true;
    
    [Range(1.0f, 1.2f)]
    public float bounceScale = 1.1f;
    
    public AudioClip clickSound;
    
    public AudioClip hoverSound;
    
    [Range(0f, 1f)]
    public float soundVolume = 0.5f;
    
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
        if (!button.interactable)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * animationSpeed);
            return;
        }
        
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isPressed = true;
        targetScale = originalScale * pressedScale;
        
        BTSSpecialCandyManager soundManager = FindObjectOfType<BTSSpecialCandyManager>();
        if (soundManager != null)
        {
            soundManager.PlayButtonPressSound();
        }
        else
        {
            PlaySound(clickSound);
        }
        
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
        
        if (!isPressed)
        {
            targetScale = originalScale * hoverScale;
        }
        
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
        float elapsed = 0f;
        float bounceDuration = 0.15f;
        Vector3 bounceTarget = originalScale * bounceScale;
        
        while (elapsed < bounceDuration)
        {
            targetScale = Vector3.Lerp(originalScale * pressedScale, bounceTarget, elapsed / bounceDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
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
