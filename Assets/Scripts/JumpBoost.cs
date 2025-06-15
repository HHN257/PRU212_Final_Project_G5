using UnityEngine;

public class JumpBoost : MonoBehaviour
{
    public float boostJumpForce = 10f;

    public void ActivateBoost(Transform attacker)
    {
        if (attacker.CompareTag("Player"))
        {
            Rigidbody2D rb = attacker.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, boostJumpForce);
            }

            // Destroy or deactivate
            Destroy(gameObject);
        }
    }
}
