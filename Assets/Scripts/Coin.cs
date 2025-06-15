using UnityEngine;

public class Coin : MonoBehaviour
{
    public int pointValue = 1;

    public void Collect()
    {
        // Add to score, play sound, etc.
        GameManager.Instance.AddPoints(pointValue);
        // Example: ScoreManager.instance.AddScore(value);
        Destroy(gameObject);
    }
}
