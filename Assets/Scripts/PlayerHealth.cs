using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Damage Settings")]
    public float invincibilityTime = 1f;
    public float knockbackForce = 5f;

    [Header("UI References")]
    public Slider healthBar; // Optional health bar
    public Text healthText;  // Optional health text

    [Header("Effects")]
    public Color damageColor = Color.red;
    public float flashDuration = 0.1f;

    // Private variables
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb;
    private PlayerController playerController;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        UpdateHealthUI();
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

    public void TakeDamage(int damage, Vector2 enemyPosition)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Start invincibility
        isInvincible = true;
        invincibilityTimer = invincibilityTime;

        // Visual feedback
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashEffect());
        }

        // Knockback effect based on enemy position
        ApplyKnockback(enemyPosition);

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
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
        UpdateHealthUI();
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

    void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"Health: {currentHealth}/{maxHealth}";
        }
    }

    public void SetTemporaryInvincibility(float duration)
    {
        StopCoroutine("TemporaryInvincibility");
        StartCoroutine(TemporaryInvincibility(duration));
    }

    private IEnumerator TemporaryInvincibility(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Trap"))
        {
            Debug.Log("Player touched a trap!");
            currentHealth = 0;
            UpdateHealthUI();
            Die();
        }
    }


    void Die()
    {
        Debug.Log("Player died!");
        // Add your death logic here:
        // - Restart level
        // - Show game over screen
        // - Play death animation
        // - etc.

        // Example: Restart the current scene
        // UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}