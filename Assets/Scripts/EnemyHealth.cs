using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Damage Settings")]
    public float knockbackForce = 5f;
    public float stunDuration = 0.5f;

    [Header("Effects")]
    public Color damageColor = Color.red;
    public float flashDuration = 0.1f;

    // Private variables
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb;
    private EnemyController enemyController;
    private Animator animator;
    private bool isStunned = false;
    private float stunTimer = 0f;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        enemyController = GetComponent<EnemyController>();
        animator = GetComponent<Animator>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void Update()
    {
        // Handle stun timer
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                isStunned = false;
                if (enemyController != null)
                    enemyController.enabled = true;
            }
        }
    }

    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Trigger hurt animation
        if (animator != null)
        {
            animator.SetBool("isHurt", true);
            animator.SetBool("isWalking", false);
            animator.SetBool("IsAttacking", false);
        }

        // Stun the enemy briefly
        isStunned = true;
        stunTimer = stunDuration;
        if (enemyController != null)
            enemyController.enabled = false;

        // Visual feedback
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashEffect());
        }

        // Knockback effect
        ApplyKnockback(attackerPosition);

        // Reset hurt animation after a short delay
        Invoke("ResetHurtAnimation", 0.3f);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void ResetHurtAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isHurt", false);
        }
    }

    void ApplyKnockback(Vector2 attackerPosition)
    {
        if (rb != null)
        {
            // Calculate knockback direction away from attacker
            Vector2 knockbackDirection = (transform.position - (Vector3)attackerPosition).normalized;
            Vector2 knockback = new Vector2(knockbackDirection.x * knockbackForce, 0.5f * knockbackForce);
            rb.linearVelocity = new Vector2(knockback.x, rb.linearVelocity.y + knockback.y);
        }
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

    void Die()
    {
        Debug.Log(gameObject.name + " died!");

        // Trigger death animation
        if (animator != null)
        {
            animator.SetBool("isDead", true);
            animator.SetBool("isWalking", false);
            animator.SetBool("IsAttacking", false);
            animator.SetBool("isHurt", false);
        }

        // Disable enemy controller
        if (enemyController != null)
            enemyController.enabled = false;

        // Add death effects here:
        // - Play death animation
        // - Drop items/coins
        // - Add score
        // - Particle effects

        // Destroy the enemy after death animation (adjust timing as needed)
        Destroy(gameObject, 1f);
    }

    public bool IsStunned()
    {
        return isStunned;
    }
}