using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("UI")]
    public GameObject bossHealthBarContainer; // Assign the parent GameObject of the health bar (e.g., BossHP_Background)
    public GameObject phaseTextGameObject; // Assign your Phase Text UI GameObject here
    public BossHealthBar healthBar;
    private Animator animator;
    public Transform player;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Boss Stats")]
    public float health = 100f;
    public float maxHealth = 100f;
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

    [Header("Phase 3 Combo Settings")]
    public float attack1Delay = 1.2f; // Adjust to match animation length
    public float attack2Delay = 1.0f; // Adjust to match animation length
    public float attack3Delay = 1.5f; // Adjust to match animation length

    [Header("Effects")]
    public Color damageColor = Color.red;
    public float flashDuration = 0.1f;

    private float attackCooldown = 2f;
    private float shootCooldown = 2f;
    private float attackTimer;
    private float shootTimer;

    private bool isAttacking = false; // To control attack state

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

        // Initially hide the health bar and phase text
        if (bossHealthBarContainer != null)
        {
            bossHealthBarContainer.SetActive(false);
        }
        if (phaseTextGameObject != null)
        {
            phaseTextGameObject.SetActive(false);
        }

        // Initialize health bar
        if (healthBar != null)
        {
            healthBar.SetHealth(health, maxHealth);
            healthBar.SetPhase(phase);
            Debug.Log($"BossController: Health bar initialized with health={health}, maxHealth={maxHealth}, phase={phase}");
        }
        else
        {
            Debug.LogWarning("BossController: healthBar is null! Make sure to assign it in the inspector.");
        }

        // Set initial animator phase
        animator.SetInteger("Phase", phase);
    }

    void Update()
    {
        if (player == null || health <= 0 || isAttacking)
        {
            // If we are in an attack combo, halt all other logic
            if (isAttacking)
            {
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsRunning", false);
            }
            return;
        }

        // Phase update logic moved to TakeDamage method for better consistency
        CheckPhaseTransition();

        float distance = Vector2.Distance(transform.position, player.position);

        // --- Main AI Logic: Only engage if player is within detection radius ---
        if (distance <= detectionRadius)
        {
            // Show health bar and phase text when boss engages
            if (bossHealthBarContainer != null && !bossHealthBarContainer.activeInHierarchy)
            {
                bossHealthBarContainer.SetActive(true);
            }
            if (phaseTextGameObject != null && !phaseTextGameObject.activeInHierarchy)
            {
                phaseTextGameObject.SetActive(true);
            }

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

            // --- Timers ---
            attackTimer += Time.deltaTime;
            shootTimer += Time.deltaTime;

            // --- Movement & Attack Logic ---
            if (phase == 1)
            {
                // In Phase 1, the boss kites and shoots.
                if (distance > idealShootingDistance)
                {
                    transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
                    animator.SetBool("IsWalking", true);
                }
                else if (distance < kiteBackDistance)
                {
                    transform.position -= (Vector3)direction * moveSpeed * Time.deltaTime;
                    animator.SetBool("IsWalking", true);
                }
                else
                {
                    animator.SetBool("IsWalking", false);
                }
                animator.SetBool("IsRunning", false);

                if (shootTimer >= shootCooldown)
                {
                    shootTimer = 0;
                    animator.SetTrigger("IsShooting");
                }
            }
            else // For Phases 2 and 3
            {
                if (distance <= meleeAttackRange)
                {
                    // --- Stop and Melee Attack ---
                    animator.SetBool("IsWalking", false);
                    animator.SetBool("IsRunning", false);

                    if (attackTimer >= attackCooldown)
                    {
                        attackTimer = 0;
                        if (phase == 2)
                        {
                            // Phase 2: random between attack 1 and 2
                            int attackType = Random.Range(1, 3); // Gets 1 or 2
                            animator.SetTrigger("IsAttacking" + attackType);
                        }
                        else if (phase == 3)
                        {
                            // Phase 3: perform the 3-hit combo
                            StartCoroutine(Phase3AttackCombo());
                        }
                    }
                }
                else
                {
                    // --- Move and Shoot ---
                    transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
                    animator.SetBool("IsWalking", true);
                    animator.SetBool("IsRunning", true); // Always run when moving in later phases

                    if (shootTimer >= shootCooldown)
                    {
                        shootTimer = 0;
                        animator.SetTrigger("IsShooting");
                    }
                }
            }
        }
        else
        {
            // --- Player is out of range, idle ---
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
        }

        if (health <= 0)
        {
            Die();
        }
    }

    private void CheckPhaseTransition()
    {
        int newPhase = phase;

        if (health <= 30 && phase < 3)
        {
            newPhase = 3;
        }
        else if (health <= 70 && phase < 2)
        {
            newPhase = 2;
        }

        if (newPhase != phase)
        {
            phase = newPhase;
            animator.SetInteger("Phase", phase);

            if (healthBar != null)
            {
                healthBar.SetPhase(phase);
            }

            // Phase transition effects
            StartCoroutine(PhaseTransitionEffect());
        }
    }

    private IEnumerator PhaseTransitionEffect()
    {
        // Flash effect for phase transition
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = originalColor;
        }

        // You can add more phase transition effects here
        Debug.Log($"Boss entered Phase {phase}!");
    }

    IEnumerator Phase3AttackCombo()
    {
        isAttacking = true;

        // Attack 1
        animator.SetTrigger("IsAttacking1");
        yield return new WaitForSeconds(attack1Delay);

        // Attack 2
        if (health > 0) // Check if boss is still alive
        {
            animator.SetTrigger("IsAttacking2");
            yield return new WaitForSeconds(attack2Delay);
        }

        // Attack 3
        if (health > 0) // Check if boss is still alive
        {
            animator.SetTrigger("IsAttacking3");
            yield return new WaitForSeconds(attack3Delay);
        }

        isAttacking = false;
    }

    public void TakeDamage(float amount)
    {
        if (health <= 0) return;

        float oldHealth = health;
        health -= amount;

        Debug.Log($"Boss took {amount} damage: {oldHealth} -> {health}");

        if (spriteRenderer != null)
            StartCoroutine(FlashEffect());

        if (healthBar != null)
        {
            healthBar.SetHealth(health, maxHealth);
        }
        else
        {
            Debug.LogWarning("BossController: healthBar is null in TakeDamage!");
        }

        // Phase transitions are now handled in CheckPhaseTransition()
        CheckPhaseTransition();

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        animator.SetTrigger("IsDead");
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        // Hide health bar and phase text on death
        if (bossHealthBarContainer != null)
        {
            bossHealthBarContainer.SetActive(false);
        }
        if (phaseTextGameObject != null)
        {
            phaseTextGameObject.SetActive(false);
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
