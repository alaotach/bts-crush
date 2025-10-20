using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Advanced button animation with multiple effect options.
/// Includes idle animations, squish, rotation, color change, and particle effects.
/// </summary>
public enum IdleAnimationType
{
    None,
    Pulse,          // Scale in and out
    Bounce,         // Bob up and down
    Rotate,         // Rotate back and forth
    Float,          // Smooth up/down movement
    Wave            // Sine wave rotation
}

[RequireComponent(typeof(Button))]
public class ButtonSquishAnimationAdvanced : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Idle Animation")]
    public bool enableIdleAnimation = true;
    
    [Tooltip("Type of idle animation")]
    public IdleAnimationType idleType = IdleAnimationType.Pulse;
    
    [Range(0.5f, 3f)]
    public float idleSpeed = 1f;
    
    [Range(1.0f, 1.2f)]
    public float idlePulseScale = 1.1f;
    
    [Range(-10f, 10f)]
    public float idleRotationAngle = 5f;
    
    [Header("Scale Animation")]
    [Range(0.5f, 1.0f)]
    public float pressedScale = 0.9f;
    
    [Range(1.0f, 1.3f)]
    public float hoverScale = 1.05f;
    
    [Range(1f, 30f)]
    public float animationSpeed = 15f;
    
    public bool bounceEffect = true;
    
    [Range(1.0f, 1.2f)]
    public float bounceScale = 1.1f;
    
    [Header("Rotation Animation")]
    public bool enableRotation = false;
    
    [Tooltip("Degrees to rotate on press")]
    [Range(-15f, 15f)]
    public float rotationAngle = 5f;
    
    [Header("Color Animation")]
    public bool enableColorChange = false;
    
    [Tooltip("Color when pressed")]
    public Color pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    
    [Tooltip("Color when hovering")]
    public Color hoverColor = new Color(1.1f, 1.1f, 1.1f, 1f);
    
    [Header("Particle Effects")]
    public bool enableParticles = false;
    
    [Tooltip("Particle system to play on click")]
    public ParticleSystem clickParticles;
    
    [Header("Audio")]
    public AudioClip clickSound;
    public AudioClip hoverSound;
    
    [Range(0f, 1f)]
    public float soundVolume = 0.5f;
    
    [Header("Haptic Feedback (Mobile)")]
    public bool enableHaptics = true;
    
    // Internal state
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private Color originalColor;
    private Color targetColor;
    
    private bool isPressed = false;
    private bool isHovering = false;
    
    private Button button;
    private Image buttonImage;
    private AudioSource audioSource;
    private Coroutine animationCoroutine;
    
    // Idle animation state
    private float idleAnimationTime = 0f;
    private Vector3 idleScaleOffset;
    private Vector3 idlePositionOffset;
    private float idleRotationOffset;
    
    void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        
        originalScale = transform.localScale;
        targetScale = originalScale;
        
        originalRotation = transform.localRotation;
        targetRotation = originalRotation;
        
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
            targetColor = originalColor;
        }
        
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
            transform.localRotation = Quaternion.Lerp(transform.localRotation, originalRotation, Time.deltaTime * animationSpeed);
            
            if (buttonImage != null && enableColorChange)
            {
                buttonImage.color = Color.Lerp(buttonImage.color, originalColor, Time.deltaTime * animationSpeed);
            }
            return;
        }
        
        if (enableIdleAnimation && !isPressed)
        {
            idleAnimationTime += Time.deltaTime * idleSpeed;
            CalculateIdleAnimation();
        }
        else
        {
            idleScaleOffset = Vector3.zero;
            idlePositionOffset = Vector3.zero;
            idleRotationOffset = 0f;
        }
        
        Vector3 finalScale = targetScale + idleScaleOffset;
        transform.localScale = Vector3.Lerp(transform.localScale, finalScale, Time.deltaTime * animationSpeed);
        
        if (enableRotation || enableIdleAnimation)
        {
            Quaternion finalRotation = targetRotation * Quaternion.Euler(0, 0, idleRotationOffset);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, finalRotation, Time.deltaTime * animationSpeed);
        }
        
        if (enableIdleAnimation && idleType == IdleAnimationType.Float || idleType == IdleAnimationType.Bounce)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, idlePositionOffset, Time.deltaTime * animationSpeed);
        }
        
        if (buttonImage != null && enableColorChange)
        {
            buttonImage.color = Color.Lerp(buttonImage.color, targetColor, Time.deltaTime * animationSpeed);
        }
    }
    
    private void CalculateIdleAnimation()
    {
        switch (idleType)
        {
            case IdleAnimationType.Pulse:
                float pulseAmount = Mathf.Sin(idleAnimationTime * Mathf.PI) * 0.1f * (idlePulseScale - 1f);
                idleScaleOffset = Vector3.one * pulseAmount;
                break;
                
            case IdleAnimationType.Bounce:
                // Bob up and down
                float bounceHeight = Mathf.Abs(Mathf.Sin(idleAnimationTime * Mathf.PI)) * 10f;
                idlePositionOffset = new Vector3(0, bounceHeight, 0);
                break;
                
            case IdleAnimationType.Rotate:
                idleRotationOffset = Mathf.Sin(idleAnimationTime * Mathf.PI) * idleRotationAngle;
                break;
                
            case IdleAnimationType.Float:
                // Smooth floating motion
                float floatHeight = Mathf.Sin(idleAnimationTime * Mathf.PI * 2f) * 8f;
                idlePositionOffset = new Vector3(0, floatHeight, 0);
                break;
                
            case IdleAnimationType.Wave:
                // Sine wave rotation with scale
                idleRotationOffset = Mathf.Sin(idleAnimationTime * Mathf.PI * 2f) * idleRotationAngle;
                float waveScale = Mathf.Sin(idleAnimationTime * Mathf.PI * 2f + Mathf.PI * 0.5f) * 0.05f;
                idleScaleOffset = Vector3.one * waveScale;
                break;
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isPressed = true;
        
        // Scale
        targetScale = originalScale * pressedScale;
        
        // Rotation
        if (enableRotation)
        {
            targetRotation = originalRotation * Quaternion.Euler(0, 0, rotationAngle);
        }
        
        // Color
        if (enableColorChange && buttonImage != null)
        {
            targetColor = pressedColor;
        }
        
        // Audio
        PlaySound(clickSound);
        
        // Haptics
        if (enableHaptics)
        {
            TriggerHapticFeedback();
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
        
        // Rotation
        if (enableRotation)
        {
            targetRotation = originalRotation;
        }
        
        // Particles
        if (enableParticles && clickParticles != null)
        {
            clickParticles.Play();
        }
        
        // Bounce animation
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
            
            if (enableColorChange && buttonImage != null)
            {
                targetColor = isHovering ? hoverColor : originalColor;
            }
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isHovering = true;
        
        if (!isPressed)
        {
            targetScale = originalScale * hoverScale;
            
            if (enableColorChange && buttonImage != null)
            {
                targetColor = hoverColor;
            }
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
            
            if (enableColorChange && buttonImage != null)
            {
                targetColor = originalColor;
            }
        }
    }
    
    private IEnumerator BounceAnimation()
    {
        // Bounce up
        float elapsed = 0f;
        float bounceDuration = 0.15f;
        Vector3 bounceTarget = originalScale * bounceScale;
        Color colorStart = enableColorChange && buttonImage != null ? buttonImage.color : originalColor;
        
        while (elapsed < bounceDuration)
        {
            float t = elapsed / bounceDuration;
            targetScale = Vector3.Lerp(originalScale * pressedScale, bounceTarget, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Settle down
        elapsed = 0f;
        float settleDuration = 0.15f;
        Vector3 finalScale = isHovering ? originalScale * hoverScale : originalScale;
        Color finalColor = isHovering && enableColorChange ? hoverColor : originalColor;
        
        while (elapsed < settleDuration)
        {
            float t = elapsed / settleDuration;
            targetScale = Vector3.Lerp(bounceTarget, finalScale, t);
            
            if (enableColorChange && buttonImage != null)
            {
                targetColor = Color.Lerp(colorStart, finalColor, t);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        targetScale = finalScale;
        targetColor = finalColor;
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, soundVolume);
        }
    }
    
    private void TriggerHapticFeedback()
    {
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }
    
    public void ResetAll()
    {
        transform.localScale = originalScale;
        transform.localRotation = originalRotation;
        transform.localPosition = Vector3.zero;
        targetScale = originalScale;
        targetRotation = originalRotation;
        idleAnimationTime = 0f;
        
        if (buttonImage != null)
        {
            buttonImage.color = originalColor;
            targetColor = originalColor;
        }
    }
    
    public void SetIdleAnimationType(IdleAnimationType type)
    {
        idleType = type;
        idleAnimationTime = 0f;
    }
    
    private void OnDisable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        ResetAll();
        isPressed = false;
        isHovering = false;
    }
}
