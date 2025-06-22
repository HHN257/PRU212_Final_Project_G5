using UnityEngine;
using UnityEngine.UI;
using TMPro; // For text display
using System.Collections;

public class BossHealthBar : MonoBehaviour
{
    [Header("UI References")]
    public Image fillImage;
    public TMP_Text phaseText; // Optional, for showing Phase 1/2/3
    public TMP_Text healthText; // Optional, for showing health numbers

    [Header("Phase Colors")]
    public Color phase1Color = Color.green;
    public Color phase2Color = Color.yellow;
    public Color phase3Color = Color.red;

    [Header("Animation")]
    public float colorTransitionSpeed = 2f;

    private int currentPhase = 1;
    private float maxHealth = 100f;
    private Color targetColor;

    void Start()
    {
        // The container's visibility is now controlled by BossController
        if (fillImage != null)
        {
            fillImage.fillAmount = 1f;
            targetColor = phase1Color;
            fillImage.color = phase1Color;
        }

        // Set initial phase
        SetPhase(1);
        SetHealth(100f, 100f);
    }

    void Update()
    {
        // Smooth color transition
        if (fillImage != null && fillImage.color != targetColor)
        {
            fillImage.color = Color.Lerp(fillImage.color, targetColor, Time.deltaTime * colorTransitionSpeed);
        }
    }

    public void SetHealth(float current, float max)
    {
        maxHealth = max;

        if (fillImage != null)
        {
            fillImage.fillAmount = current / max;
        }
        else
        {
            Debug.LogWarning("BossHealthBar: fillImage is null!");
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }
    }

    public void SetPhase(int phase)
    {
        if (phase == currentPhase) return;

        currentPhase = phase;

        // Update phase text
        if (phaseText != null)
        {
            phaseText.text = "Phase " + phase;

            // Add some visual feedback for phase change
            StartCoroutine(PhaseChangeEffect());
        }
        else
        {
            Debug.LogWarning("BossHealthBar: phaseText is null!");
        }

        // Update health bar color based on phase
        if (fillImage != null)
        {
            switch (phase)
            {
                case 1:
                    targetColor = phase1Color;
                    break;
                case 2:
                    targetColor = phase2Color;
                    break;
                case 3:
                    targetColor = phase3Color;
                    break;
            }
        }
        else
        {
            Debug.LogWarning("BossHealthBar: fillImage is null during phase change!");
        }
    }

    private System.Collections.IEnumerator PhaseChangeEffect()
    {
        if (phaseText == null) yield break;

        // Flash effect for phase change
        Color originalColor = phaseText.color;
        phaseText.color = Color.white;
        phaseText.fontSize *= 1.2f;

        yield return new WaitForSeconds(0.3f);

        phaseText.color = originalColor;
        phaseText.fontSize /= 1.2f;
    }

    public int GetCurrentPhase()
    {
        return currentPhase;
    }
}
