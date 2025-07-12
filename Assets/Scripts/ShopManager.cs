using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("Shop UI References")]
    public GameObject shopModalPanel;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI healthLevelText;
    public TextMeshProUGUI attackLevelText;
    public TextMeshProUGUI healthCostText;
    public TextMeshProUGUI attackCostText;
    public Button upgradeHealthButton;
    public Button upgradeAttackButton;
    public Button closeButton;
    
    [Header("Upgrade Settings")]
    public int baseHealthUpgradeCost = 100;
    public int baseAttackUpgradeCost = 150;
    public float healthUpgradeCostMultiplier = 1.5f;
    public float attackUpgradeCostMultiplier = 1.8f;
    public int healthUpgradeAmount = 1;
    public int attackUpgradeAmount = 1;
    
    [Header("Player References")]
    public PlayerHealth playerHealth;
    public PlayerAttack playerAttack;
    public UpgradeEffect upgradeEffect;
    
    private int healthLevel = 1;
    private int attackLevel = 1;
    private int currentHealthCost;
    private int currentAttackCost;
    
    private const string HEALTH_LEVEL_KEY = "PlayerHealthLevel";
    private const string ATTACK_LEVEL_KEY = "PlayerAttackLevel";
    
    void Start()
    {
        LoadUpgradeLevels();
        CalculateUpgradeCosts();
        SetupUI();
        UpdateUI();
        
        if (shopModalPanel != null)
        {
            shopModalPanel.SetActive(false);
        }
    }
    
    void SetupUI()
    {
        if (upgradeHealthButton != null)
        {
            upgradeHealthButton.onClick.AddListener(UpgradeHealth);
        }
        
        if (upgradeAttackButton != null)
        {
            upgradeAttackButton.onClick.AddListener(UpgradeAttack);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop);
        }
    }
    
    public void OpenShop()
    {
        if (shopModalPanel != null)
        {
            shopModalPanel.SetActive(true);
            UpdateUI();
        }
        
        // Disable player movement while shop is open
        if (playerHealth != null)
        {
            PlayerController playerController = playerHealth.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
        }
    }
    
    public void CloseShop()
    {
        if (shopModalPanel != null)
        {
            shopModalPanel.SetActive(false);
        }
        
        // Re-enable player movement
        if (playerHealth != null)
        {
            PlayerController playerController = playerHealth.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }
    }
    
    void UpdateUI()
    {
        // Update coin display
        if (coinText != null && GameManager.Instance != null)
        {
            coinText.text = $"Coins: {GameManager.Instance.coin}";
        }
        
        // Update level displays
        if (healthLevelText != null)
        {
            healthLevelText.text = $"Level {healthLevel}";
        }
        
        if (attackLevelText != null)
        {
            attackLevelText.text = $"Level {attackLevel}";
        }
        
        // Update cost displays
        if (healthCostText != null)
        {
            healthCostText.text = $"{currentHealthCost} Coins";
        }
        
        if (attackCostText != null)
        {
            attackCostText.text = $"{currentAttackCost} Coins";
        }
        
        // Update button interactability
        if (upgradeHealthButton != null)
        {
            upgradeHealthButton.interactable = CanAffordUpgrade(currentHealthCost);
        }
        
        if (upgradeAttackButton != null)
        {
            upgradeAttackButton.interactable = CanAffordUpgrade(currentAttackCost);
        }
    }
    
    void UpgradeHealth()
    {
        if (!CanAffordUpgrade(currentHealthCost)) return;
        
        // Deduct coins
        GameManager.Instance.AddPoints(-currentHealthCost);
        
        // Upgrade health
        healthLevel++;
        playerHealth.maxHealth += healthUpgradeAmount;
        playerHealth.currentHealth += healthUpgradeAmount; // Also heal the player
        
        // Update health bar
        if (playerHealth.healthBar != null)
        {
            playerHealth.healthBar.SetHealth(playerHealth.currentHealth, playerHealth.maxHealth);
        }
        
        // Play upgrade effect
        if (upgradeEffect != null)
        {
            upgradeEffect.PlayHealthUpgradeEffect();
        }
        
        // Save and recalculate
        SaveUpgradeLevels();
        CalculateUpgradeCosts();
        UpdateUI();
        
        Debug.Log($"Health upgraded to level {healthLevel}! New max health: {playerHealth.maxHealth}");
    }
    
    void UpgradeAttack()
    {
        if (!CanAffordUpgrade(currentAttackCost)) return;
        
        // Deduct coins
        GameManager.Instance.AddPoints(-currentAttackCost);
        
        // Upgrade attack
        attackLevel++;
        playerAttack.attackDamage += attackUpgradeAmount;
        
        // Play upgrade effect
        if (upgradeEffect != null)
        {
            upgradeEffect.PlayAttackUpgradeEffect();
        }
        
        // Save and recalculate
        SaveUpgradeLevels();
        CalculateUpgradeCosts();
        UpdateUI();
        
        Debug.Log($"Attack upgraded to level {attackLevel}! New attack damage: {playerAttack.attackDamage}");
    }
    
    bool CanAffordUpgrade(int cost)
    {
        return GameManager.Instance != null && GameManager.Instance.coin >= cost;
    }
    
    void CalculateUpgradeCosts()
    {
        currentHealthCost = Mathf.RoundToInt(baseHealthUpgradeCost * Mathf.Pow(healthUpgradeCostMultiplier, healthLevel - 1));
        currentAttackCost = Mathf.RoundToInt(baseAttackUpgradeCost * Mathf.Pow(attackUpgradeCostMultiplier, attackLevel - 1));
    }
    
    void LoadUpgradeLevels()
    {
        healthLevel = PlayerPrefs.GetInt(HEALTH_LEVEL_KEY, 1);
        attackLevel = PlayerPrefs.GetInt(ATTACK_LEVEL_KEY, 1);
        
        // Apply loaded levels to player stats
        if (playerHealth != null)
        {
            playerHealth.maxHealth = 5 + (healthLevel - 1) * healthUpgradeAmount;
        }
        
        if (playerAttack != null)
        {
            playerAttack.attackDamage = 1 + (attackLevel - 1) * attackUpgradeAmount;
        }
    }
    
    void SaveUpgradeLevels()
    {
        PlayerPrefs.SetInt(HEALTH_LEVEL_KEY, healthLevel);
        PlayerPrefs.SetInt(ATTACK_LEVEL_KEY, attackLevel);
        PlayerPrefs.Save();
    }
    
    // Public methods for external access
    public int GetHealthLevel() => healthLevel;
    public int GetAttackLevel() => attackLevel;
    public int GetHealthUpgradeCost() => currentHealthCost;
    public int GetAttackUpgradeCost() => currentAttackCost;
} 