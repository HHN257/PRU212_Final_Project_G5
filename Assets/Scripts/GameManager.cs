using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int coin = 0;
    public TextMeshProUGUI coinText; // Assign in Inspector

    [Header("Death Penalty")]
    public float coinLossPercentage = 0.5f; // Lose 50% of coins on death
    public bool shouldDropCoins = true; // Whether to spawn coin objects when dying
    public GameObject coinPrefab; // Assign the coin prefab
    public float coinSpreadRadius = 2f; // How far coins spread when dropped
    public int maxDroppedCoins = 10; // Maximum number of physical coins to spawn

    private const string COIN_SAVE_KEY = "PlayerCoins";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCoins(); // Load saved coins when game starts
            SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to scene load events
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from scene load events when destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find and assign the coin text in the new scene
        coinText = GameObject.FindGameObjectWithTag("CoinText")?.GetComponent<TextMeshProUGUI>();
        UpdateUI(); // Update UI with current coin count
    }

    public void AddPoints(int amount)
    {
        coin += amount;
        UpdateUI();
        SaveCoins(); // Save coins whenever they change
    }

    public void HandlePlayerDeath(Vector3 deathPosition)
    {
        int coinsToLose = Mathf.FloorToInt(coin * coinLossPercentage);
        if (coinsToLose <= 0) return; // No coins to lose

        // Reduce coins
        coin -= coinsToLose;
        UpdateUI();
        SaveCoins();

        // Optionally spawn physical coins at death location
        if (shouldDropCoins && coinPrefab != null)
        {
            SpawnLostCoins(coinsToLose, deathPosition);
        }

        Debug.Log($"Player lost {coinsToLose} coins on death. Remaining coins: {coin}");
    }

    private void SpawnLostCoins(int coinAmount, Vector3 position)
    {
        // Determine how many physical coins to spawn (capped at maxDroppedCoins)
        int numberOfCoins = Mathf.Min(coinAmount, maxDroppedCoins);
        int valuePerCoin = Mathf.CeilToInt((float)coinAmount / numberOfCoins);

        for (int i = 0; i < numberOfCoins; i++)
        {
            // Calculate random position within spread radius
            Vector2 randomOffset = Random.insideUnitCircle * coinSpreadRadius;
            Vector3 spawnPos = position + new Vector3(randomOffset.x, randomOffset.y, 0);

            // Spawn coin
            GameObject coinObj = Instantiate(coinPrefab, spawnPos, Quaternion.identity);
            if (coinObj.TryGetComponent<Coin>(out var coinComponent))
            {
                coinComponent.pointValue = valuePerCoin;
            }
        }
    }

    void UpdateUI()
    {
        if (coinText != null)
            coinText.text = "Coins: " + coin.ToString();
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(COIN_SAVE_KEY, coin);
        PlayerPrefs.Save();
    }

    private void LoadCoins()
    {
        coin = PlayerPrefs.GetInt(COIN_SAVE_KEY, 0);
        UpdateUI();
    }

    // Optional: Method to reset coins (for testing or game over)
    public void ResetCoins()
    {
        coin = 0;
        SaveCoins();
        UpdateUI();
    }
}
