using UnityEngine;
using TMPro;

public class UpgradeEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    public GameObject upgradeEffectPrefab;
    public Transform effectSpawnPoint;
    public float effectDuration = 2f;
    
    [Header("Text Effect")]
    public GameObject textEffectPrefab;
    public string healthUpgradeText = "+1 HP";
    public string attackUpgradeText = "+1 ATK";
    public Color healthTextColor = Color.green;
    public Color attackTextColor = Color.red;
    
    [Header("Sound")]
    public AudioClip upgradeSound;
    public AudioSource audioSource;
    
    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    
    public void PlayHealthUpgradeEffect()
    {
        PlayUpgradeEffect(healthUpgradeText, healthTextColor);
    }
    
    public void PlayAttackUpgradeEffect()
    {
        PlayUpgradeEffect(attackUpgradeText, attackTextColor);
    }
    
    void PlayUpgradeEffect(string text, Color color)
    {
        // Spawn particle effect
        if (upgradeEffectPrefab != null)
        {
            Vector3 spawnPos = effectSpawnPoint != null ? effectSpawnPoint.position : transform.position;
            GameObject effect = Instantiate(upgradeEffectPrefab, spawnPos, Quaternion.identity);
            Destroy(effect, effectDuration);
        }
        
        // Spawn text effect
        if (textEffectPrefab != null)
        {
            Vector3 textPos = effectSpawnPoint != null ? effectSpawnPoint.position : transform.position;
            textPos += Vector3.up * 2f; // Offset above the player
            
            GameObject textObj = Instantiate(textEffectPrefab, textPos, Quaternion.identity);
            TextMeshPro textMesh = textObj.GetComponent<TextMeshPro>();
            
            if (textMesh != null)
            {
                textMesh.text = text;
                textMesh.color = color;
            }
            
            // Animate the text
            StartCoroutine(AnimateText(textObj));
        }
        
        // Play sound
        if (audioSource != null && upgradeSound != null)
        {
            audioSource.PlayOneShot(upgradeSound);
        }
    }
    
    System.Collections.IEnumerator AnimateText(GameObject textObj)
    {
        Vector3 startPos = textObj.transform.position;
        Vector3 endPos = startPos + Vector3.up * 3f;
        float elapsed = 0f;
        
        while (elapsed < effectDuration)
        {
            float t = elapsed / effectDuration;
            textObj.transform.position = Vector3.Lerp(startPos, endPos, t);
            
            // Fade out
            TextMeshPro textMesh = textObj.GetComponent<TextMeshPro>();
            if (textMesh != null)
            {
                Color color = textMesh.color;
                color.a = 1f - t;
                textMesh.color = color;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        Destroy(textObj);
    }
} 