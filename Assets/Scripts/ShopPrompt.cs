using UnityEngine;
using TMPro;

public class ShopPrompt : MonoBehaviour
{
    [Header("Prompt Settings")]
    public GameObject promptPanel;
    public TextMeshProUGUI promptText;
    public string promptMessage = "Press E to open Shop";
    
    [Header("Animation")]
    public float fadeInSpeed = 2f;
    public float fadeOutSpeed = 3f;
    
    private CanvasGroup canvasGroup;
    private bool isVisible = false;
    
    void Start()
    {
        if (promptPanel != null)
        {
            canvasGroup = promptPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = promptPanel.AddComponent<CanvasGroup>();
            }
            
            promptPanel.SetActive(false);
        }
        
        if (promptText != null)
        {
            promptText.text = promptMessage;
        }
    }
    
    public void ShowPrompt()
    {
        if (promptPanel != null && !isVisible)
        {
            isVisible = true;
            promptPanel.SetActive(true);
            StartCoroutine(FadeIn());
        }
    }
    
    public void HidePrompt()
    {
        if (promptPanel != null && isVisible)
        {
            isVisible = false;
            StartCoroutine(FadeOut());
        }
    }
    
    System.Collections.IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;
        
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime * fadeInSpeed;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    System.Collections.IEnumerator FadeOut()
    {
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeOutSpeed;
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        promptPanel.SetActive(false);
    }
} 