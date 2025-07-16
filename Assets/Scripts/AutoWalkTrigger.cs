using UnityEngine;

public class AutoWalkTrigger : MonoBehaviour
{
    public AudioSource backgroundMusic; // AudioSource nhạc nền
    public AudioSource sfxSource; // AudioSource để phát hiệu ứng (không phải AudioSource của Player)
    public AudioClip triggerClip; // Âm thanh hiệu ứng khi qua vùng
    public GameObject nextLevelText; // UI Text hoặc Panel "Next Level"
    public GameObject killAllEnemyText; // UI Text hoặc Panel "Kill all enemy to join next level"
    private bool canShowKillText = true; // Cờ kiểm soát việc hiện lại text

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Kiểm tra còn enemy hoặc boss không
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");
            if (enemies.Length > 0 || bosses.Length > 0)
            {
                // Nếu còn enemy hoặc boss thì hiện text cảnh báo, chỉ hiện nếu được phép
                if (killAllEnemyText != null && canShowKillText)
                {
                    killAllEnemyText.SetActive(true);
                    canShowKillText = false;
                    CancelInvoke(nameof(HideKillAllEnemyText));
                    Invoke(nameof(HideKillAllEnemyText), 5f); // Ẩn sau 5 giây
                }
                return;
            }

            // Tắt nhạc nền
            if (backgroundMusic != null)
                backgroundMusic.Stop();

            // Phát âm thanh hiệu ứng
            if (sfxSource != null && triggerClip != null)
                sfxSource.PlayOneShot(triggerClip);

            // Hiện chữ Next Level
            if (nextLevelText != null)
                nextLevelText.SetActive(true);

            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TriggerAutoWalk();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Khi player rời khỏi trigger, cho phép hiện lại text lần sau
            canShowKillText = true;
        }
    }

    private void HideKillAllEnemyText()
    {
        if (killAllEnemyText != null)
            killAllEnemyText.SetActive(false);
    }
}
