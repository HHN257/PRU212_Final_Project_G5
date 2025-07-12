using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUICreator : MonoBehaviour
{
    [Header("UI Creation")]
    public bool createShopUI = false;
    public string canvasName = "ShopCanvas";
    public string modalName = "ShopModal";
    
    [Header("UI Settings")]
    public Vector2 modalSize = new Vector2(400, 300);
    public Color backgroundColor = new Color(0, 0, 0, 0.8f);
    public Color contentBackgroundColor = Color.white;
    
    void Update()
    {
        if (createShopUI)
        {
            createShopUI = false;
            CreateShopUI();
        }
    }
    
    [ContextMenu("Create Shop UI")]
    public void CreateShopUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject(canvasName);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create Shop Modal Panel
        GameObject modalPanel = new GameObject(modalName);
        modalPanel.transform.SetParent(canvasObj.transform, false);
        
        RectTransform modalRect = modalPanel.AddComponent<RectTransform>();
        modalRect.anchorMin = Vector2.zero;
        modalRect.anchorMax = Vector2.one;
        modalRect.sizeDelta = Vector2.zero;
        modalRect.anchoredPosition = Vector2.zero;
        
        Image modalImage = modalPanel.AddComponent<Image>();
        modalImage.color = backgroundColor;
        
        // Create Shop Content
        GameObject contentObj = new GameObject("ShopContent");
        contentObj.transform.SetParent(modalPanel.transform, false);
        
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = modalSize;
        contentRect.anchoredPosition = Vector2.zero;
        
        Image contentImage = contentObj.AddComponent<Image>();
        contentImage.color = contentBackgroundColor;
        
        // Create Title
        CreateTextElement(contentObj, "Title", "SHOP", 24, new Vector2(0, 120), new Vector2(300, 40));
        
        // Create Coin Display
        CreateTextElement(contentObj, "CoinDisplay", "Coins: 0", 18, new Vector2(0, 80), new Vector2(300, 30));
        
        // Create Health Section
        GameObject healthSection = CreateSection(contentObj, "HealthSection", new Vector2(0, 20));
        CreateTextElement(healthSection, "HealthLabel", "Health", 16, new Vector2(-100, 0), new Vector2(80, 25));
        CreateTextElement(healthSection, "HealthLevel", "Level 1", 16, new Vector2(0, 0), new Vector2(80, 25));
        CreateTextElement(healthSection, "HealthCost", "100 Coins", 16, new Vector2(100, 0), new Vector2(100, 25));
        CreateButton(healthSection, "UpgradeHealthButton", "Upgrade", new Vector2(0, -30), new Vector2(120, 30));
        
        // Create Attack Section
        GameObject attackSection = CreateSection(contentObj, "AttackSection", new Vector2(0, -40));
        CreateTextElement(attackSection, "AttackLabel", "Attack", 16, new Vector2(-100, 0), new Vector2(80, 25));
        CreateTextElement(attackSection, "AttackLevel", "Level 1", 16, new Vector2(0, 0), new Vector2(80, 25));
        CreateTextElement(attackSection, "AttackCost", "150 Coins", 16, new Vector2(100, 0), new Vector2(100, 25));
        CreateButton(attackSection, "UpgradeAttackButton", "Upgrade", new Vector2(0, -30), new Vector2(120, 30));
        
        // Create Close Button
        CreateButton(contentObj, "CloseButton", "X", new Vector2(0, -100), new Vector2(60, 30));
        
        // Initially disable the modal
        modalPanel.SetActive(false);
        
        Debug.Log("Shop UI created successfully! Assign the references in ShopManager.");
    }
    
    GameObject CreateTextElement(GameObject parent, string name, string text, int fontSize, Vector2 position, Vector2 size)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        
        TextMeshProUGUI textMesh = textObj.AddComponent<TextMeshProUGUI>();
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = Color.black;
        
        return textObj;
    }
    
    GameObject CreateSection(GameObject parent, string name, Vector2 position)
    {
        GameObject section = new GameObject(name);
        section.transform.SetParent(parent.transform, false);
        
        RectTransform rect = section.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(300, 60);
        rect.anchoredPosition = position;
        
        return section;
    }
    
    GameObject CreateButton(GameObject parent, string name, string text, Vector2 position, Vector2 size)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent.transform, false);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 1f);
        
        Button button = buttonObj.AddComponent<Button>();
        
        // Create button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI textMesh = textObj.AddComponent<TextMeshProUGUI>();
        textMesh.text = text;
        textMesh.fontSize = 14;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = Color.white;
        
        return buttonObj;
    }
} 