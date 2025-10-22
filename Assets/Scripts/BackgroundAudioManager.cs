using UnityEngine;
using System.Collections;

public class BackgroundAudioManager : MonoBehaviour
{
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float musicVolume = 0.3f;
    public float fadeInDuration = 2f;
    public AudioClip ambientLoop;
    [Range(0f, 1f)]
    public float ambientVolume = 0.2f;
    public bool playRandomCandyLands = false;
    public Vector2 candyLandInterval = new Vector2(30f, 60f);
    
    private AudioSource musicSource;
    private AudioSource ambientSource;
    private bool isFadingIn = false;
    
    void Awake()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0f;
        
        ambientSource = gameObject.AddComponent<AudioSource>();
        ambientSource.loop = true;
        ambientSource.playOnAwake = false;
        ambientSource.volume = ambientVolume;
    }
    
    void Start()
    {
        if (backgroundMusic != null)
        {
            PlayBackgroundMusic();
        }
        
        if (ambientLoop != null)
        {
            PlayAmbient();
        }
    }
    

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic == null || musicSource == null) return;
        
        musicSource.clip = backgroundMusic;
        musicSource.Play();
        
        if (fadeInDuration > 0)
        {
            StartCoroutine(FadeIn());
        }
        else
        {
            musicSource.volume = musicVolume;
        }
    }
    

    public void PlayAmbient()
    {
        if (ambientLoop == null || ambientSource == null) return;
        
        ambientSource.clip = ambientLoop;
        ambientSource.volume = ambientVolume;
        ambientSource.Play();
    }
    

    private IEnumerator FadeIn()
    {
        isFadingIn = true;
        float elapsed = 0f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, elapsed / fadeInDuration);
            yield return null;
        }
        
        musicSource.volume = musicVolume;
        isFadingIn = false;
    }
    

    public IEnumerator FadeOut(float duration = 1f)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        
        musicSource.volume = 0f;
        musicSource.Stop();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (!isFadingIn)
        {
            musicSource.volume = musicVolume;
        }
    }
    

    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        ambientSource.volume = ambientVolume;
    }

    public void Pause()
    {
        if (musicSource != null) musicSource.Pause();
        if (ambientSource != null) ambientSource.Pause();
    }
    

    public void Resume()
    {
        if (musicSource != null) musicSource.UnPause();
        if (ambientSource != null) ambientSource.UnPause();
    }
    

    public void Stop()
    {
        if (musicSource != null) musicSource.Stop();
        if (ambientSource != null) ambientSource.Stop();
    }
}
