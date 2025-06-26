using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Damage Settings")]
    public float invincibilityDuration = 1f;
    public float knockbackForce = 5f;

    [Header("UI References")]
    public GameObject healthBarContainer; // The parent object containing the health bar UI
    public PlayerHealthBar healthBar;

    [Header("Effects")]
    public Color damageColor = Color.red;
    public float flashDuration = 0.1f;

    // Private variables
    private Animator animator;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb;
    private PlayerController playerController;
    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        // Initialize health bar
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }
    }

    void Update()
    {
        // Handle invincibility timer
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                if (spriteRenderer != null)
                    spriteRenderer.color = originalColor;
            }
        }
    }

    public void TakeDamage(int damage, Vector2 damageSource)
    {
        if (playerController != null && playerController.IsBlocking)
        {
            Debug.Log("Attack Blocked!");
            return;
        }

        if (isInvincible || isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        // Flash effect
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashEffect());
        }

        // Apply knockback
        ApplyKnockback(damageSource);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityFrames());
            // Trigger hit animation
            if (animator != null)
            {
                animator.SetTrigger("Hit");
            }
        }
    }

    // Overload for backward compatibility
    public void TakeDamage(int damage)
    {
        TakeDamage(damage, transform.position + Vector3.left);
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Update health bar
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }
    }

    void ApplyKnockback(Vector2 enemyPosition)
    {
        if (rb != null)
        {
            // Calculate knockback direction away from enemy
            Vector2 knockbackDirection = (transform.position - (Vector3)enemyPosition).normalized;
            Vector2 knockback = new Vector2(knockbackDirection.x * knockbackForce, 0.5f * knockbackForce);
            rb.linearVelocity = new Vector2(knockback.x, rb.linearVelocity.y + knockback.y);
        }
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

    System.Collections.IEnumerator FlashEffect()
    {
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            spriteRenderer.color = Color.Lerp(originalColor, damageColor, elapsed / flashDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < flashDuration)
        {
            spriteRenderer.color = Color.Lerp(damageColor, originalColor, elapsed / flashDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = originalColor;
    }

    public void SetTemporaryInvincibility(float duration)
    {
        StartCoroutine(InvincibilityFrames(duration));
    }

    private IEnumerator InvincibilityFrames(float duration = -1)
    {
        if (duration < 0) duration = invincibilityDuration;

        isInvincible = true;

        // Optional: Flash effect
        if (spriteRenderer != null)
        {
            float flashInterval = 0.1f;
            Color originalColor = spriteRenderer.color;

            for (float t = 0; t < duration; t += flashInterval)
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
                yield return new WaitForSeconds(flashInterval * 0.5f);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(flashInterval * 0.5f);
            }

            spriteRenderer.color = originalColor;
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }

        isInvincible = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Trap"))
        {
            Debug.Log("Player touched a trap!");
            currentHealth = 0;
            if (healthBar != null)
            {
                healthBar.SetHealth(currentHealth, maxHealth);
            }
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // Handle coin loss through GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.HandlePlayerDeath(transform.position);
        }

        Debug.Log("Player died!");

        // Play death animation
        if (animator != null)
        {
            animator.SetBool("noBlood", false);
            animator.SetTrigger("Death");
        }

        // Disable movement or other controls
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Hide health bar
        if (healthBarContainer != null)
        {
            healthBarContainer.SetActive(false);
        }

        // Restart scene after delay
        StartCoroutine(RestartScene());
    }

    IEnumerator RestartScene()
    {
        yield return new WaitForSeconds(2f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Respawn();
    }

    public void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        // Re-enable components
        if (TryGetComponent<PlayerController>(out var controller))
        {
            controller.enabled = true;
        }
        if (TryGetComponent<Collider2D>(out var collider))
        {
            collider.enabled = true;
        }

        // Reset animation
        if (animator != null)
        {
            animator.SetTrigger("Respawn");
        }
    }
}