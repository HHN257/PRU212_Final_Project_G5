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

    [Header("Drops")]
    public GameObject coinPrefab;


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
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayerMask);

        foreach (Collider2D target in hitTargets)
        {
            // 1. Check for JumpBoost
            JumpBoost jumpBoost = target.GetComponent<JumpBoost>();
            if (jumpBoost != null)
            {
                jumpBoost.ActivateBoost(transform);
                continue;
            }

            // 2. Check for EnemyHealth
            EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage, transform.position);
                continue;
            }

            // 3. Destroy breakable blocks
            if (target.CompareTag("BreakableBlock"))
            {
                // Drop a coin after destroying the block
                if (coinPrefab != null)
                {
                    Vector3 dropPos = target.transform.position + new Vector3(Random.Range(-0.3f, 0.3f), 0.5f, 0);
                    int coinCount = 5; 

                    for (int i = 0; i < coinCount; i++)
                    {
                        Vector3 dropOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.2f, 0.8f), 0);
                        Instantiate(coinPrefab, target.transform.position + dropOffset, Quaternion.identity);
                    }

                }

                Destroy(target.gameObject);
                continue;
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