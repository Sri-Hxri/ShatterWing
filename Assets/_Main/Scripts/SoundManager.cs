using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("SFX Clips")]
    public AudioClip playerShoot;
    public AudioClip upgradePopup;
    public AudioClip DashSound;
    public AudioClip PlayerHitSound;
    public AudioClip AsteroidDestroySound;

    [Header("Music")]
    public AudioClip backgroundMusic;

    [Header("Pitch Randomization")]
    public Vector2 shootPitchRange = new Vector2(0.95f, 1.05f);

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        PlayMusic();
    }

    // ================= MUSIC =================

    public void PlayMusic()
    {
        if (musicSource == null || backgroundMusic == null)
            return;

        if (musicSource.isPlaying)
            return;

        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    // ================= SFX =================

    public void PlayPlayerShoot()
    {
        PlaySFX(playerShoot, Random.Range(shootPitchRange.x, shootPitchRange.y));
    }

public void PlayDashSound()
    {
        PlaySFX(DashSound);
    }

    public void PlayAsteroidDestroySound()
    {
        PlaySFX(AsteroidDestroySound);
    }

    public void PlayHitSound()
    {
        PlaySFX(PlayerHitSound);
    }

    public void PlayUpgradePopup()
    {
        PlaySFX(upgradePopup);
    }

    public void PlayUpgradeConfirm()
    {
        if (!sfxSource || !upgradePopup) return;
        sfxSource.pitch = 1.15f;
        sfxSource.PlayOneShot(upgradePopup, 0.35f);
    }

    void PlaySFX(AudioClip clip, float pitch = 1f)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip);
    }
}
