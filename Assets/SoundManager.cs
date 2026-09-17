using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;   // Loa phát hiệu ứng ngắn (SFX)
    public AudioSource musicSource; // Loa phát nhạc nền (BGM)

    [Header("Audio Clips")]
    public AudioClip bgmClip;      // File nhạc nền
    public AudioClip clickClip;    // Tiếng bấm nút
    public AudioClip jumpClip;     // Tiếng nhảy
    public AudioClip gameOverClip; // Tiếng thua game

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Giữ SoundManager sống xuyên suốt khi load lại Scene (không bị reset nhạc)
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Tránh sinh ra bản sao khi reload Scene
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        PlayBGM(bgmClip);
    }

    // Hàm phát nhạc nền lặp vô tận
    public void PlayBGM(AudioClip clip)
    {
        if (clip != null && musicSource != null)
        {
            musicSource.clip = clip;
            musicSource.loop = true; // Tự động phát lặp lại
            musicSource.Play();
        }
    }

    // Hàm phát âm thanh hiệu ứng ngắn
    public void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // Tiện ích gọi nhanh
    public void PlayButtonClick() => PlaySound(clickClip);
    public void PlayJump() => PlaySound(jumpClip);
    public void PlayGameOver() => PlaySound(gameOverClip);
}