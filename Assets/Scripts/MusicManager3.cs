using UnityEngine;

public class MusicManager3 : MonoBehaviour
{
    public AudioClip sceneMusic; // Assign in inspector
    public AudioClip winningMusic;
    public AudioClip bossMusic;
    private AudioSource musicSource;

    void Awake()
    {
        // Ensure only one MusicManager exists
        if (FindObjectsOfType<MusicManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = sceneMusic;
        musicSource.loop = true;
        musicSource.playOnAwake = true;
    }

    void Start()
    {
        if (musicSource != null && sceneMusic != null)
        {
            musicSource.Play();
        }
    }

    public void PlayWinningMusic()
    {
        if (winningMusic == null) return;
        AudioSource source = GetComponent<AudioSource>();
        source.clip = winningMusic;
        source.loop = false;
        source.Play();
    }

    public void PlayBossMusic()
    {
        if (bossMusic == null) return;
        musicSource.clip = bossMusic;
        musicSource.loop = true;
        musicSource.Play();
    }
}
