using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;             // The background panel
    [SerializeField] private TextMeshProUGUI dialogueText;         // The text box inside
    [SerializeField] private ScrollRect scrollRect;                // Scroll View component

    [Header("Dialogue Lines")]
    [TextArea(2, 5)]
    public string[] lines;                                         // Fill in the Inspector

    private int index = 0;             // Current line index
    private bool dialogueActive = false;
    private bool dialogueEnded = false;

    public static bool DialogueOpen { get; private set; }

    void Start()
    {
        StartDialogue();
    }

    void Update()
    {
        // Space to continue dialogue
        if (dialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            ShowNextLine();
        }

        // Esc to close panel after dialogue ends
        if (dialogueEnded && Input.GetKeyDown(KeyCode.Escape))
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void StartDialogue()
    {
        DialogueOpen = true;
        index = 0;
        dialogueText.text = string.Empty;
        dialoguePanel.SetActive(true);
        dialogueActive = true;
        dialogueEnded = false;

        ShowNextLine();
    }

    private void ShowNextLine()
    {
        if (index < lines.Length)
        {
            // Append line, add newline if needed
            if (!string.IsNullOrEmpty(dialogueText.text))
                dialogueText.text += "\n";

            dialogueText.text += lines[index];
            index++;

            // Auto-scroll to bottom
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f; // 0 = bottom
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        dialogueActive = false;
        dialogueEnded = true;
        DialogueOpen = false;

        // Option A: auto-close immediately (uncomment this if you want it)
        dialoguePanel.SetActive(false);
    }
}
