using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private int maxVisibleLines = 3;   // how many you want to keep
    private readonly Queue<string> lineBuffer = new();  // rolling window

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

        lineBuffer.Clear();  // clear the buffer for a fresh start

        ShowNextLine();
    }

    private void ShowNextLine()
    {
        if (index < lines.Length)
        {
            /* --- NEW: maintain a rolling buffer --- */
            lineBuffer.Enqueue(lines[index]);           // add newest line
            if (lineBuffer.Count > maxVisibleLines)     // too many? dump oldest
                lineBuffer.Dequeue();

            dialogueText.text = string.Join("\n", lineBuffer);
            index++;

            /* keep the auto-scroll if you still have a ScrollRect */
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
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