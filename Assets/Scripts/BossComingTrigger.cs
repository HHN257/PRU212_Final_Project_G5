using UnityEngine;

public class BossComingTrigger : MonoBehaviour
{
    public GameObject bossComingText; // UI Text hoặc TextMeshProUGUI
    public float displayDuration = 5f;
    private bool hasTriggered = false;
    [Header("Music Change")]
    public AudioSource backgroundMusic;
    public AudioClip newMusic;
    public float fadeDuration = 1.5f; // Thời gian fade nhạc (giây)

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            if (bossComingText != null)
            {
                bossComingText.SetActive(true);
                Invoke(nameof(HideBossComingText), displayDuration);
            }
            // Đổi nhạc nền mượt mà (fade)
            if (backgroundMusic != null && newMusic != null)
            {
                StartCoroutine(FadeMusic(backgroundMusic, newMusic, fadeDuration));
            }
        }
    }

    private void HideBossComingText()
    {
        if (bossComingText != null)
            bossComingText.SetActive(false);
    }

    private System.Collections.IEnumerator FadeMusic(AudioSource audioSource, AudioClip newClip, float duration)
    {
        float startVolume = audioSource.volume;
        // Fade out
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null;
        }
        audioSource.volume = 0;
        audioSource.clip = newClip;
        audioSource.Play();
        // Fade in
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, startVolume, t / duration);
            yield return null;
        }
        audioSource.volume = startVolume;
    }
} 