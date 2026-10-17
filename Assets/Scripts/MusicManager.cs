using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public BTSAssetManager assetManager;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Ensure sources exist
        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        
        musicSource.loop = true;
    }

    public void PlayMenuMusic()
    {
        if (assetManager != null && assetManager.menuMusic != null)
        {
            CrossfadeMusic(assetManager.menuMusic);
        }
    }

    public void PlayGameplayMusic()
    {
        if (assetManager != null && assetManager.gameplayMusic != null)
        {
            CrossfadeMusic(assetManager.gameplayMusic);
        }
    }

    public void PlayVictoryMusic()
    {
        if (assetManager != null && assetManager.victoryMusic != null)
        {
            CrossfadeMusic(assetManager.victoryMusic, false);
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void PlayComboSound(int comboCount)
    {
        if (assetManager == null) return;
        
        if (comboCount >= 8 && assetManager.comboEncore != null)
            PlaySFX(assetManager.comboEncore);
        else if (comboCount >= 6 && assetManager.comboAmazing != null)
            PlaySFX(assetManager.comboAmazing);
        else if (comboCount >= 4 && assetManager.comboGreat != null)
            PlaySFX(assetManager.comboGreat);
        else if (comboCount >= 2 && assetManager.comboGood != null)
            PlaySFX(assetManager.comboGood);
    }

    private void CrossfadeMusic(AudioClip newClip, bool loop = true)
    {
        if (musicSource.clip == newClip) return;
        
        StopAllCoroutines();
        StartCoroutine(CrossfadeRoutine(newClip, loop));
    }

    private IEnumerator CrossfadeRoutine(AudioClip newClip, bool loop)
    {
        float fadeTime = 1f;
        float startVol = musicSource.volume;

        // Fade out
        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVol * Time.deltaTime / fadeTime;
            yield return null;
        }

        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.Play();

        // Fade in
        while (musicSource.volume < startVol)
        {
            musicSource.volume += startVol * Time.deltaTime / fadeTime;
            yield return null;
        }
        
        musicSource.volume = startVol;
    }
}
