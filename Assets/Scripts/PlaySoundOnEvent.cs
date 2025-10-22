using UnityEngine;
using UnityEngine.UI;


public class PlaySoundOnEvent : MonoBehaviour
{
    public AudioClip soundClip;
    
    [Range(0f, 1f)]
    public float volume = 1f;
    
    public bool playOnEnable = false;
    public bool playOnDisable = false;
    
    public bool playOnStart = false;
    
    public bool playOnButtonClick = true;
    
    private Button button;
    private BTSSpecialCandyManager soundManager;
    
    void Awake()
    {
        button = GetComponent<Button>();
        soundManager = FindObjectOfType<BTSSpecialCandyManager>();
        
        if (button != null && playOnButtonClick)
        {
            button.onClick.AddListener(PlaySound);
        }
    }
    
    void Start()
    {
        if (playOnStart)
        {
            PlaySound();
        }
    }
    
    void OnEnable()
    {
        if (playOnEnable)
        {
            PlaySound();
        }
    }
    
    void OnDisable()
    {
        if (playOnDisable)
        {
            PlaySound();
        }
    }
    
    public void PlaySound()
    {
        if (soundClip == null) return;
        
        if (soundManager != null)
        {
            soundManager.PlayCustomSound(soundClip); // Use public PlayCustomSound method
        }
        else
        {
            Debug.LogWarning("BTSSpecialCandyManager not found! Cannot play sound.");
        }
    }
    
    public void PlayButtonClick()
    {
        if (soundManager != null)
            soundManager.PlayButtonPressSound();
    }
    
    public void PlayMatch()
    {
        if (soundManager != null)
            soundManager.PlayMatchSound(3);
    }
    
    public void PlaySwap()
    {
        if (soundManager != null)
            soundManager.PlaySwapSound();
    }
}
