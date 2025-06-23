using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("UI References")]
    public Image fillImage;
    public TMP_Text healthText;

    [Header("Colors")]
    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;
    public float lowHealthThreshold = 0.3f; // When health is below 30%, use low health color

    [Header("Animation")]
    public float colorTransitionSpeed = 2f;
    public float fillAmountSpeed = 5f; // Speed at which the fill amount changes

    private Color targetColor;
    private float maxHealth;
    private float targetFillAmount;

    void Start()
    {
        if (fillImage != null)
        {
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillAmount = 1f;
            targetFillAmount = 1f;
            targetColor = fullHealthColor;
            fillImage.color = fullHealthColor;
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

    public void SetHealth(float current, float max)
    {
        maxHealth = max;

        if (fillImage != null)
        {
            // Update target fill amount
            targetFillAmount = Mathf.Clamp01(current / max);

            // Update color based on health percentage
            float healthPercentage = current / max;
            targetColor = Color.Lerp(lowHealthColor, fullHealthColor, healthPercentage);
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }
    }
}