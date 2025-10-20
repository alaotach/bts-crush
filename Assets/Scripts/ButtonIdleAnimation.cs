using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Simple button with constant idle animation.
/// Choose from pulse, bounce, rotate, float, or wave effects.
/// </summary>
public enum ButtonIdleType
{
    Pulse,          // Scale in and out (breathing effect)
    Bounce,         // Bob up and down
    Rotate,         // Rotate back and forth
    Float,          // Smooth floating motion
    Wave,           // Combined rotation and scale
    Shake           // Subtle shake left and right
}

[RequireComponent(typeof(Button))]
public class ButtonIdleAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Idle Animation Settings")]
    [Tooltip("Type of continuous animation")]
    public ButtonIdleType animationType = ButtonIdleType.Pulse;
    
    [Range(0.5f, 5f)]
    [Tooltip("Speed of the idle animation")]
    public float animationSpeed = 1f;
    
    [Range(0f, 1f)]
    [Tooltip("Intensity of the effect (0 = subtle, 1 = strong)")]
    public float intensity = 0.5f;
    
    [Header("Press Effect")]
    [Range(0.7f, 1.0f)]
    [Tooltip("Scale when button is pressed")]
    public float pressedScale = 0.9f;
    
    [Tooltip("Stop idle animation while pressed")]
    public bool pauseIdleWhenPressed = true;
    
    // Internal state
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float animationTime = 0f;
    private bool isPressed = false;
    
    private Button button;
    
    void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }
    
    void Update()
    {
        if (!button.interactable)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * 10f);
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * 10f);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, originalRotation, Time.deltaTime * 10f);
            return;
        }
        
        if (isPressed)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale * pressedScale, Time.deltaTime * 15f);
            
            if (pauseIdleWhenPressed)
            {
                return;
            }
        }
        
        animationTime += Time.deltaTime * animationSpeed;
        
        ApplyIdleAnimation();
    }
    
    private void ApplyIdleAnimation()
    {
        switch (animationType)
        {
            case ButtonIdleType.Pulse:
                ApplyPulse();
                break;
                
            case ButtonIdleType.Bounce:
                ApplyBounce();
                break;
                
            case ButtonIdleType.Rotate:
                ApplyRotate();
                break;
                
            case ButtonIdleType.Float:
                ApplyFloat();
                break;
                
            case ButtonIdleType.Wave:
                ApplyWave();
                break;
                
            case ButtonIdleType.Shake:
                ApplyShake();
                break;
        }
    }
    
    private void ApplyPulse()
    {
        // Breathing effect - scale in and out
        float pulseValue = 1f + Mathf.Sin(animationTime * Mathf.PI) * 0.15f * intensity;
        Vector3 targetScale = originalScale * pulseValue;
        
        if (!isPressed)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 10f);
        }
    }
    
    private void ApplyBounce()
    {
        // Bob up and down
        float bounceHeight = Mathf.Abs(Mathf.Sin(animationTime * Mathf.PI)) * 15f * intensity;
        Vector3 targetPosition = originalPosition + new Vector3(0, bounceHeight, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 10f);
        
        // Slight squash and stretch
        float squash = 1f - (bounceHeight / (15f * intensity)) * 0.05f;
        if (!isPressed)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, 
                new Vector3(originalScale.x * (2f - squash), originalScale.y * squash, originalScale.z), 
                Time.deltaTime * 10f);
        }
    }
    
    private void ApplyRotate()
    {
        float rotationAngle = Mathf.Sin(animationTime * Mathf.PI) * 10f * intensity;
        Quaternion targetRotation = originalRotation * Quaternion.Euler(0, 0, rotationAngle);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * 10f);
    }
    
    private void ApplyFloat()
    {
        // Smooth sine wave floating
        float floatHeight = Mathf.Sin(animationTime * Mathf.PI * 2f) * 12f * intensity;
        Vector3 targetPosition = originalPosition + new Vector3(0, floatHeight, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 8f);
    }
    
    private void ApplyWave()
    {
        // Combined rotation and scale for wave effect
        float rotationAngle = Mathf.Sin(animationTime * Mathf.PI * 2f) * 8f * intensity;
        Quaternion targetRotation = originalRotation * Quaternion.Euler(0, 0, rotationAngle);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * 10f);
        
        float scaleValue = 1f + Mathf.Sin(animationTime * Mathf.PI * 2f + Mathf.PI * 0.5f) * 0.1f * intensity;
        Vector3 targetScale = originalScale * scaleValue;
        
        if (!isPressed)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 10f);
        }
    }
    
    private void ApplyShake()
    {
        // Rapid subtle shake left and right
        float shakeAmount = Mathf.Sin(animationTime * Mathf.PI * 4f) * 3f * intensity;
        Vector3 targetPosition = originalPosition + new Vector3(shakeAmount, 0, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 12f);
        
        float rotationShake = Mathf.Sin(animationTime * Mathf.PI * 4f) * 2f * intensity;
        Quaternion targetRotation = originalRotation * Quaternion.Euler(0, 0, rotationShake);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * 12f);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (button.interactable)
        {
            isPressed = true;
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }
    
    private void OnDisable()
    {
        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        isPressed = false;
    }
}
