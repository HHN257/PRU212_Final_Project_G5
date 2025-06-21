using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    private Animator animator;
    public Transform player;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Boss Stats")]
    public float health = 100f;
    public float moveSpeed = 2f;
    private int phase = 1;
    public float detectionRadius = 15f;

    [Header("Phase 1 Settings")]
    public float idealShootingDistance = 10f;
    public float kiteBackDistance = 7f;

    [Header("Attack Settings")]
    public int meleeAttackDamage = 1;
    public float meleeAttackRange = 2.5f;
    public Transform meleeAttackPoint;
    public LayerMask playerLayer;

    [Header("Effects")]
    public Color damageColor = Color.red;
    public float flashDuration = 0.1f;

    private float attackCooldown = 2f;
    private float shootCooldown = 2f;
    private float attackTimer;
    private float shootTimer;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        if (player == null || health <= 0)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
            return;
        }

        // Phase update
        if (health <= 70 && phase == 1)
        {
            phase = 2;
            animator.SetInteger("Phase", phase);
        }
        else if (health <= 30 && phase == 2)
        {
            phase = 3;
            animator.SetInteger("Phase", phase);
        }

        float distance = Vector2.Distance(transform.position, player.position);

        // --- Main AI Logic: Only engage if player is within detection radius ---
        if (distance <= detectionRadius)
        {
            Vector2 direction = (player.position - transform.position).normalized;

            // --- Flipping Logic ---
            if (Mathf.Abs(direction.x) > 0.01f)
            {
                spriteRenderer.flipX = direction.x < 0;

                // Flip melee attack point
                if (meleeAttackPoint != null)
                {
                    Vector3 localPos = meleeAttackPoint.localPosition;
                    localPos.x = Mathf.Abs(localPos.x) * (spriteRenderer.flipX ? -1 : 1);
                    meleeAttackPoint.localPosition = localPos;
                }

                // Flip fire point
                if (firePoint != null)
                {
                    Vector3 localPos = firePoint.localPosition;
                    localPos.x = Mathf.Abs(localPos.x) * (spriteRenderer.flipX ? -1 : 1);
                    firePoint.localPosition = localPos;
                    firePoint.rotation = spriteRenderer.flipX ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
                }
            }

            // --- Movement ---
            if (phase == 1)
            {
                if (distance > idealShootingDistance)
                {
                    // Too far, move closer
                    transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
                    animator.SetBool("IsWalking", true);
                }
                else if (distance < kiteBackDistance)
                {
                    // Too close, move away
                    transform.position -= (Vector3)direction * moveSpeed * Time.deltaTime;
                    animator.SetBool("IsWalking", true);
                }
                else
                {
                    // Just right distance for shooting, so stop.
                    animator.SetBool("IsWalking", false);
                }
                animator.SetBool("IsRunning", false);
            }
            else // For Phases 2 and 3
            {
                // The boss will stop moving only when it's very close to the player.
                float stopDistance = meleeAttackRange / 2f;
                if (distance > stopDistance)
                {
                    transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
                    animator.SetBool("IsWalking", true);
                    animator.SetBool("IsRunning", phase >= 2);
                }
                else
                {
                    animator.SetBool("IsWalking", false);
                    animator.SetBool("IsRunning", false);
                }
            }

            // --- Timers ---
            attackTimer += Time.deltaTime;
            shootTimer += Time.deltaTime;


            // --- Attack Logic ---
            if (phase == 1)
            {
                // In Phase 1, the boss only shoots.
                if (shootTimer >= shootCooldown)
                {
                    shootTimer = 0;
                    Debug.Log("Boss is attempting to shoot! (Phase 1)");
                    animator.SetTrigger("IsShooting");
                }
            }
            else // For Phases 2 and 3
            {
                if (distance <= meleeAttackRange)
                {
                    // Player is in melee range, prioritize melee attacks.
                    if (attackTimer >= attackCooldown)
                    {
                        attackTimer = 0;
                        int attackType = Random.Range(1, phase + 1);
                        switch (attackType)
                        {
                            case 1: animator.SetTrigger("IsAttacking1"); break;
                            case 2: if (phase >= 2) animator.SetTrigger("IsAttacking2"); break;
                            case 3: if (phase >= 3) animator.SetTrigger("IsAttacking3"); break;
                        }
                    }
                }
                else if (shootTimer >= shootCooldown) // Player is outside melee range, use ranged attacks.
                {
                    shootTimer = 0;
                    Debug.Log("Boss is attempting to shoot!");
                    animator.SetTrigger("IsShooting");
                }
            }
        }
        else
        {
            // --- Player is out of range, idle ---
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
        }
    }

    public void TakeDamage(float amount)
    {
        if (health <= 0) return;

        health -= amount;

        if (spriteRenderer != null)
        {
            StartCoroutine(FlashEffect());
        }

        if (health <= 0)
        {
            animator.SetTrigger("IsDead");
            // Disable movement/collision
            GetComponent<Collider2D>().enabled = false;
            this.enabled = false;
        }
    }

    // Called from Animation Event
    public void FireProjectile()
    {
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }

    IEnumerator FlashEffect()
    {
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    // This method should be called from an animation event on attack animations
    public void DealMeleeDamage()
    {
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(meleeAttackPoint.position, meleeAttackRange, playerLayer);

        foreach (Collider2D playerCollider in hitPlayers)
        {
            PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null && !playerHealth.IsInvincible())
            {
                playerHealth.TakeDamage(meleeAttackDamage, transform.position);
            }
        }
    }

    // Add this for visual debugging of the melee attack range
    void OnDrawGizmosSelected()
    {
        // Melee Attack Range
        if (meleeAttackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(meleeAttackPoint.position, meleeAttackRange);
        }

        // Detection Radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Fire Point
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.3f);
        }
    }
}
