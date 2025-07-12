using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShopTrigger : MonoBehaviour
{
    [Header("Shop Settings")]
    [TextArea(2, 3)]
    public string shopGreeting = "Welcome to the Shop! Press E to browse upgrades.";
    
    [Header("UI References")]
    public GameObject shopModalPrefab; // Assign the shop modal prefab in inspector
    public ShopPrompt shopPrompt; // Reference to the shop prompt component
    
    private bool playerInRange = false;
    private bool shopOpen = false;
    private ShopManager shopManager;
    
    void Start()
    {
        // Ensure the collider is a trigger
        GetComponent<Collider2D>().isTrigger = true;
        
        // Find or create ShopManager
        shopManager = FindObjectOfType<ShopManager>();
        if (shopManager == null)
        {
            Debug.LogWarning("No ShopManager found in scene! Creating one...");
            GameObject shopManagerObj = new GameObject("ShopManager");
            shopManager = shopManagerObj.AddComponent<ShopManager>();
        }
        
        // Find ShopPrompt if not assigned
        if (shopPrompt == null)
        {
            shopPrompt = FindObjectOfType<ShopPrompt>();
        }
    }
    
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!shopOpen)
            {
                OpenShop();
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log(shopGreeting);
            
            // Show prompt
            if (shopPrompt != null)
            {
                shopPrompt.ShowPrompt();
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            // Hide prompt
            if (shopPrompt != null)
            {
                shopPrompt.HidePrompt();
            }
            
            if (shopOpen)
            {
                CloseShop();
            }
        }
    }
    
    void OpenShop()
    {
        shopOpen = true;
        shopManager.OpenShop();
        
        // Hide prompt when shop opens
        if (shopPrompt != null)
        {
            shopPrompt.HidePrompt();
        }
    }
    
    void CloseShop()
    {
        shopOpen = false;
        shopManager.CloseShop();
    }
} 