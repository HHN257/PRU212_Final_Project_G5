using UnityEngine;
using System.Collections.Generic;

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
    //private float waitTimer = 0f;
    //private bool isWaiting = false;
    private float lastAttackTime = 0f;
    private float lastTurnTime = 0f;
    public float turnCooldown = 0.5f; // Thời gian chờ giữa các lần quay đầu

    // Thời gian delay giữa các lần phát âm thanh block cho mỗi player
    private static Dictionary<GameObject, float> lastBlockSoundTime = new Dictionary<GameObject, float>();
    private const float blockSoundDelay = 2f;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator; // Optional, if you have animations

    // States
    public enum EnemyState { Patrolling, Chasing, Attacking, ExecutingAttack }
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
                if (distanceToPlayer <= attackRange)
                {
                    currentState = EnemyState.Attacking;
                }
                else if (distanceToPlayer > detectionRange)
                {
                    currentState = EnemyState.Patrolling;
                }
                else
                {
                    ChasePlayer();
                }
                break;
            case EnemyState.Attacking:
                Attack();
                break;
            case EnemyState.ExecutingAttack:
                // Stay still during attack animation
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                break;
        }

        // Animator updates
        if (animator != null)
        {
            animator.SetBool("isWalking", (currentState == EnemyState.Patrolling && rb.linearVelocity.x != 0) || currentState == EnemyState.Chasing);
            animator.SetBool("IsAttacking", currentState == EnemyState.Attacking || currentState == EnemyState.ExecutingAttack);
        }
    }


    void Patrol()
    {
        // Nếu phát hiện player trong detectionRange, chuyển sang chase
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRange)
        {
            // Quay đầu về phía player
            if ((player.position.x < transform.position.x && movingRight) || (player.position.x > transform.position.x && !movingRight))
            {
                movingRight = !movingRight;
                lastTurnTime = Time.time;
            }
            currentState = EnemyState.Chasing;
            return;
        }
        // Check if we should turn around
        if (ShouldTurnAround() && Time.time - lastTurnTime > turnCooldown)
        {
            movingRight = !movingRight; // Flip immediately
            lastTurnTime = Time.time;
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
        spriteRenderer.flipX = direction.x < 0;
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
                // Instead of immediately changing state or setting cooldown here,
                // we transition to a state where the animation plays out.
                currentState = EnemyState.ExecutingAttack;
            }
        }
    }

    void ResetAttackAnimation()
    {
        if (animator != null)
            animator.SetBool("IsAttacking", false);

        lastAttackTime = Time.time;
        currentState = EnemyState.Patrolling;
    }

    bool ShouldTurnAround()
    {
        // Check patrol boundary
        float distanceFromStart = Mathf.Abs(transform.position.x - startPosition.x);
        if (distanceFromStart >= patrolDistance)
            return true;

        // Only check for ground ahead when grounded
        Vector2 rayOrigin = groundCheck.position + new Vector3(movingRight ? 0.2f : -0.2f, 0f);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 0.5f, groundLayerMask);
        Debug.DrawRay(rayOrigin, Vector2.down * 0.5f, Color.magenta);

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

    public void DealDamageToPlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRange) return;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        PlayerController playerController = player.GetComponent<PlayerController>();

        if (playerHealth != null && !playerHealth.IsInvincible())
        {
            if (playerController != null && playerController.IsBlocking)
            {
                Debug.Log("Attack was blocked!");
                // Phát âm thanh block khiên với delay 2 giây
                float now = Time.time;
                float lastTime = 0f;
                lastBlockSoundTime.TryGetValue(player.gameObject, out lastTime);
                if (now - lastTime >= blockSoundDelay)
                {
                    if (playerController.audioSource != null && playerController.blockClip != null)
                    {
                        playerController.audioSource.PlayOneShot(playerController.blockClip);
                    }
                    lastBlockSoundTime[player.gameObject] = now;
                }
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

}