using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        StartCoroutine(ResetAndLoad());
    }

    private IEnumerator ResetAndLoad()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGameProgress();
        }
        yield return null; // Wait one frame to ensure PlayerPrefs are flushed
        SceneManager.LoadSceneAsync(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
