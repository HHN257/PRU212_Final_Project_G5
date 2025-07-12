using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PromptUICreator : MonoBehaviour
{
    [Header("Prompt Creation")]
    public bool createPromptUI = false;
    public string promptCanvasName = "PromptCanvas";
    public string promptPanelName = "ShopPrompt";
    
    [Header("Prompt Settings")]
    public Vector2 promptSize = new Vector2(300, 60);
    public Color promptBackgroundColor = new Color(0, 0, 0, 0.8f);
    public Color promptTextColor = Color.white;
    public string promptText = "Press E to open Shop";
    
    void Update()
    {
        if (createPromptUI)
        {
            createPromptUI = false;
            CreatePromptUI();
        }
    }
    
    [ContextMenu("Create Prompt UI")]
    public void CreatePromptUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject(promptCanvasName);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create Prompt Panel
        GameObject promptPanel = new GameObject(promptPanelName);
        promptPanel.transform.SetParent(canvasObj.transform, false);
        
        RectTransform promptRect = promptPanel.AddComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0.2f);
        promptRect.anchorMax = new Vector2(0.5f, 0.2f);
        promptRect.sizeDelta = promptSize;
        promptRect.anchoredPosition = Vector2.zero;
        
        Image promptImage = promptPanel.AddComponent<Image>();
        promptImage.color = promptBackgroundColor;
        
        // Add CanvasGroup for fade effects
        CanvasGroup canvasGroup = promptPanel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        
        // Create Prompt Text
        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(promptPanel.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI textMesh = textObj.AddComponent<TextMeshProUGUI>();
        textMesh.text = promptText;
        textMesh.fontSize = 18;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = promptTextColor;
        
        // Add ShopPrompt component
        ShopPrompt shopPrompt = promptPanel.AddComponent<ShopPrompt>();
        shopPrompt.promptPanel = promptPanel;
        shopPrompt.promptText = textMesh;
        shopPrompt.promptMessage = promptText;
        
        // Initially disable the prompt
        promptPanel.SetActive(false);
        
        Debug.Log("Shop Prompt UI created successfully! Assign the ShopPrompt reference in ShopTrigger.");
    }
} 