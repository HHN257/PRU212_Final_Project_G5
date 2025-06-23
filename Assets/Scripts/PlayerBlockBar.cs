using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerBlockBar : MonoBehaviour
{
    [Header("UI References")]
    public Image fillImage;
    public TMP_Text blockText; // Optional

    [Header("Colors")]
    public Color fullBlockColor = Color.blue;
    public Color lowBlockColor = Color.red;
    public float lowBlockThreshold = 0.3f;

    [Header("Block Settings")]
    public float minBlockValue = 0f; // Minimum value the block can reach

    [Header("Animation")]
    public float colorTransitionSpeed = 2f;
    public float fillAmountSpeed = 5f; // Speed at which the fill amount changes

    private Color targetColor;
    private float maxBlock;
    private float targetFillAmount;
    private float currentBlock;

    public delegate void OnBlockDepletedHandler();
    public event OnBlockDepletedHandler OnBlockDepleted;

    void Start()
    {
        if (fillImage != null)
        {
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillAmount = 1f;
            targetFillAmount = 1f;
            targetColor = fullBlockColor;
            fillImage.color = fullBlockColor;
        }
    }

    void Update()
    {
        if (fillImage != null)
        {
            // Smooth color transition
            if (fillImage.color != targetColor)
            {
                fillImage.color = Color.Lerp(fillImage.color, targetColor, Time.deltaTime * colorTransitionSpeed);
            }

            // Smooth fill amount transition
            if (fillImage.fillAmount != targetFillAmount)
            {
                fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFillAmount, Time.deltaTime * fillAmountSpeed);
            }
        }
    }

    public void SetBlock(float current, float max)
    {
        maxBlock = max;
        currentBlock = Mathf.Max(current, minBlockValue); // Ensure block doesn't go below minimum

        if (fillImage != null)
        {
            // Update target fill amount
            targetFillAmount = Mathf.Clamp01(currentBlock / max);

            // Update color based on block percentage
            float blockPercentage = currentBlock / max;
            targetColor = Color.Lerp(lowBlockColor, fullBlockColor, blockPercentage);
        }

        if (blockText != null)
        {
            blockText.text = $"{Mathf.CeilToInt(currentBlock)} / {Mathf.CeilToInt(max)}";
        }

        // Trigger event when block is depleted
        if (current <= minBlockValue)
        {
            OnBlockDepleted?.Invoke();
        }
    }

    public float GetCurrentBlock()
    {
        return currentBlock;
    }
}