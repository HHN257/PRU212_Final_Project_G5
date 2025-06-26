using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger : MonoBehaviour
{
    [TextArea(2, 5)]
    public string[] dialogueLines;          // Sentences shown when triggered

    [Tooltip("Trigger only once, then disable itself.")]
    public bool oneShot = true;

    bool hasFired = false;

    void Reset()                            // when the script is first added
    {
        // Ensure the collider is a trigger
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;    // only react to player

        if (oneShot && hasFired) return;            // skip repeats if one-shot
        hasFired = true;

        // Feed lines to the DialogueManager
        DialogueManager mgr = Object.FindFirstObjectByType<DialogueManager>();

        if (mgr != null)
        {
            mgr.lines = dialogueLines;   // overwrite the array
            mgr.enabled = true;          // in case you disabled Start()

            // Call StartDialogue() via reflection-safe way
            mgr.SendMessage("StartDialogue", SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            Debug.LogWarning("No DialogueManager found in scene!");
        }
    }
}
