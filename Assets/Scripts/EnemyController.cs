using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float patrolDistance = 5f;
    public float patrolSpeed = 2f;
    public float waitTime = 1f; // Time to wait at patrol points

    [Header("Attack Settings")]
    public float detectionRange = 3f;
    public float attackRange = 1.5f;
    public float attackSpeed = 4f;
    public float attackCooldown = 2f;
    public int attackDamage = 1;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayerMask;

    [Header("References")]
    public Transform player;

    // Private variables
    private Vector2 startPosition;
    private bool movingRight = true;
    private float waitTimer = 0f;
    //private bool isWaiting = false;
    private float lastAttackTime = 0f;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator; // Optional, if you have animations

    // States
    public enum EnemyState { Patrolling, Chasing, Attacking, Waiting }
    public EnemyState currentState = EnemyState.Patrolling;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // Optional

        startPosition = transform.position;

        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // State management
        switch (currentState)
        {
            case EnemyState.Patrolling:
                if (distanceToPlayer <= detectionRange)
                {
                    currentState = EnemyState.Chasing;
                }
                else
                {
                    Patrol();
                }
                break;

            case EnemyState.Chasing:
                if (distanceToPlayer > detectionRange * 1.5f) // Add some buffer to prevent flickering
                {
                    currentState = EnemyState.Patrolling;
                }
                else if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
                {
                    currentState = EnemyState.Attacking;
                }
                else
                {
                    ChasePlayer();
                }
                break;

            case EnemyState.Attacking:
                Attack();
                break;

            case EnemyState.Waiting:
                Wait();
                break;
        }

        // Update animator parameters based on your setup
        if (animator != null)
        {
            animator.SetBool("isWalking", rb.linearVelocity.x != 0 && currentState == EnemyState.Patrolling);
            animator.SetBool("IsAttacking", currentState == EnemyState.Attacking);
            animator.SetBool("isHurt", false); // This will be controlled by EnemyHealth script
            animator.SetBool("isDead", false); // This will be controlled by EnemyHealth script
        }
    }

    void Patrol()
    {
        // Check if we should turn around
        if (ShouldTurnAround())
        {
            movingRight = !movingRight; // Flip immediately
        }

        // Move in current direction
        float moveDirection = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(moveDirection * patrolSpeed, rb.linearVelocity.y);

        // Flip sprite
        spriteRenderer.flipX = !movingRight;

        // Set walking animation
        if (animator != null)
            animator.SetBool("isWalking", true);
    }

    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * attackSpeed, rb.linearVelocity.y);

        // Flip sprite based on movement direction
        //spriteRenderer.flipX = direction.x < 0;

        // Set walking animation for chasing
        if (animator != null)
        {
            animator.SetBool("isWalking", true);
        }
    }

    void Attack()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (animator != null)
        {
            animator.SetBool("IsAttacking", true);
            animator.SetBool("isWalking", false);
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            PlayerController playerController = player.GetComponent<PlayerController>();

            if (playerHealth != null && !playerHealth.IsInvincible())
            {
                if (playerController != null && playerController.IsBlocking)
                {
                    Debug.Log("Attack was blocked!");

                    // Apply knockback to enemy
                    float knockbackForce = 2f;
                    Vector2 knockbackDir = (transform.position - player.position).normalized;
                    rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
                }
                else
                {
                    playerHealth.TakeDamage(attackDamage, transform.position);
                }
            }
        }

        lastAttackTime = Time.time;
        currentState = EnemyState.Waiting;
        waitTimer = 0.5f;
    }



    void ResetAttackAnimation()
    {
        if (animator != null)
            animator.SetBool("IsAttacking", false);

        currentState = EnemyState.Chasing;
    }


    void Wait()
    {
        waitTimer -= Time.deltaTime;

        if (animator != null)
            animator.SetBool("isWalking", false);

        if (waitTimer <= 0f)
        {
            animator.SetBool("IsAttacking", false); // reset attack animation
            currentState = EnemyState.Chasing; // return to chasing after wait
        }
    }


    bool ShouldTurnAround()
    {
        // Check patrol range
        float distanceFromStart = Vector2.Distance(transform.position, startPosition);
        if (distanceFromStart >= patrolDistance)
            return true;

        // Cast ray ahead and down
        Vector2 rayOrigin = groundCheck.position + new Vector3(movingRight ? 0.2f : -0.2f, 0f);
        Vector2 rayDirection = Vector2.down;
        float rayLength = 0.5f;

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayLength, groundLayerMask);

        Debug.DrawRay(rayOrigin, rayDirection * rayLength, Color.magenta);

        return hit.collider == null;
    }



    // Visual debugging
    void OnDrawGizmosSelected()
    {
        // Draw patrol area
        Gizmos.color = Color.blue;
        Vector2 center = Application.isPlaying ? startPosition : (Vector2)transform.position;
        Gizmos.DrawLine(center + Vector2.left * patrolDistance, center + Vector2.right * patrolDistance);

        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw ground check
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}