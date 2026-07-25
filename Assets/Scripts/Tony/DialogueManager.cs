using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Plays a scripted back-and-forth conversation between any number of
/// SpeechBubble owners (Player, NPC, etc), one line at a time, in the exact
/// order listed in the Inspector. Add as many lines as you want.
///
/// SETUP:
/// 1. Add this script to any GameObject in the scene (e.g. an empty
///    "DialogueManager").
/// 2. In "Lines", add one entry per line:
///    - Speaker = the SpeechBubble component that should say it
///    - Text = the line itself
///    - Duration = how long it stays up (0 = use that speaker's default)
/// 3. Order the list top-to-bottom exactly how the conversation should
///    play — alternate Speaker between entries to go back and forth.
/// 4. Check "Play On Start" for it to auto-play when the scene loads, or
///    leave unchecked and call StartDialogue() from elsewhere (an
///    interaction key press, trigger zone, button, etc).
/// </summary>
public class DialogueManager : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        [Tooltip("Which SpeechBubble says this line (Player, NPC, etc).")]
        public SpeechBubble speaker;

        [TextArea(1, 3)]
        public string text;

        [Tooltip("Seconds this line stays visible. 0 = use the speaker's own default duration.")]
        public float duration = 0f;
    }

    [Header("Conversation")]
    public List<DialogueLine> lines = new List<DialogueLine>();

    [Header("Playback")]
    public bool playOnStart = true;

    [Tooltip("Extra pause after each line, on top of its own duration, for a natural conversational beat.")]
    public float gapBetweenLines = 0.3f;

    [Header("Events")]
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueComplete;

    private bool isPlaying;

    private void Start()
    {
        if (playOnStart)
            StartDialogue();
    }

    /// <summary>Begins playing the conversation from the first line.</summary>
    public void StartDialogue()
    {
        if (isPlaying) return;
        StartCoroutine(PlayDialogue());
    }

    private IEnumerator PlayDialogue()
    {
        isPlaying = true;
        onDialogueStart?.Invoke();

        foreach (DialogueLine line in lines)
        {
            if (line.speaker == null)
            {
                Debug.LogWarning($"DialogueManager: a line has no Speaker assigned — skipping '{line.text}'.");
                continue;
            }

            float duration = line.duration > 0f ? line.duration : line.speaker.displayDuration;
            line.speaker.Show(line.text, duration);

            yield return new WaitForSeconds(duration + gapBetweenLines);
        }

        isPlaying = false;
        onDialogueComplete?.Invoke();
    }
}