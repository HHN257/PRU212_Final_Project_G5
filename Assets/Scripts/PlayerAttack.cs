using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int attackDamage = 1;
    public float attackRange = 1.5f;
    public LayerMask enemyLayerMask = -1;

    [Header("Attack Points")]
    public Transform attackPoint; // Position where attack originates from

    private Animator animator;
    private bool isAttacking = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Create attack point if not assigned
        if (attackPoint == null)
        {
            GameObject attackPointObj = new GameObject("AttackPoint");
            attackPointObj.transform.SetParent(transform);
            attackPointObj.transform.localPosition = new Vector3(0.5f, 0, 0);
            attackPoint = attackPointObj.transform;
        }
    }

    void Update()
    {
        // This is called from your existing PlayerController when J is pressed
        // We'll detect attacks through animation events instead
    }

    // Call this method from Animation Events in your attack animations
    public void OnAttackHit()
    {
        PerformAttack();
    }

    void PerformAttack()
    {
        // Get all enemies in attack range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayerMask);

        foreach (Collider2D enemy in hitEnemies)
        {
            // Check if it's an enemy
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage, transform.position);
            }
        }
    }

    // Alternative method if you don't want to use animation events
    public void TriggerAttack()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            Invoke("PerformAttack", 0.2f); // Delay to match animation timing
            Invoke("ResetAttack", 0.5f);   // Reset attack state
        }
    }

    void ResetAttack()
    {
        isAttacking = false;
    }

    // Visual debugging
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}