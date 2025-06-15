using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    public GameObject breakEffect; // Optional particle effect prefab
    public bool requiresAttack = true; // If true, only attack breaks it

    public void Break()
    {
        if (breakEffect != null)
        {
            Instantiate(breakEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!requiresAttack && collision.CompareTag("Player"))
        {
            Break();
        }
    }
}
