using UnityEngine;

public class Coin : MonoBehaviour
{
    public int pointValue = 1;
    public float collectDelay = 0.1f; // Small delay before destroying
    public GameObject collectEffect; // Optional particle effect

    private bool isCollected = false;

    public void Collect()
    {
        if (isCollected) return; // Prevent double collection
        isCollected = true;

        // Add to score if GameManager exists
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPoints(pointValue);
        }
        else
        {
            Debug.LogWarning("GameManager not found! Coins will not be saved.");
        }

        // Spawn collect effect if assigned
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // Disable renderer and collider immediately
        if (TryGetComponent<SpriteRenderer>(out var renderer))
        {
            renderer.enabled = false;
        }
        if (TryGetComponent<Collider2D>(out var collider))
        {
            collider.enabled = false;
        }

        // Destroy after small delay (for effects to play)
        Destroy(gameObject, collectDelay);
    }
}
