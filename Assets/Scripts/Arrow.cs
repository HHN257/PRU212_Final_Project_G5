using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 5f;
    public int damage = 1;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerHealth != null && !playerHealth.IsInvincible())
            {
                if (playerController != null && playerController.IsBlocking)
                {
                    // Phát âm thanh block khi block arrow
                    if (playerController.audioSource != null && playerController.blockClip != null)
                        playerController.audioSource.PlayOneShot(playerController.blockClip);
                    // Không gây damage, chỉ hủy arrow
                }
                else
                {
                    playerHealth.TakeDamage(damage, transform.position);
                }
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
