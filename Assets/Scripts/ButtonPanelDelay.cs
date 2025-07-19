using UnityEngine;
using System.Collections;

public class ButtonPanelDelay : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clickClip;
    public GameObject panelToShow;
    public GameObject panelToHide;
    public float delay = 0.3f; // Hoặc clickClip.length nếu muốn delay đúng bằng thời gian âm thanh

    public void PlaySoundAndSwitchPanel()
    {
        StartCoroutine(PlayAndSwitch());
    }

    private IEnumerator PlayAndSwitch()
    {
        if (audioSource != null && clickClip != null)
            audioSource.PlayOneShot(clickClip);

        yield return new WaitForSeconds(delay);

        if (panelToShow != null)
            panelToShow.SetActive(true);
        if (panelToHide != null)
            panelToHide.SetActive(false);
    }
} 