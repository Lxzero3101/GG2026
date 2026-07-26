using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Visual-novel style dialogue: shows one line at a time, advances to the
/// next on mouse click. Add as many lines as you want, in any order.
/// After the last line, waits a set delay then loads the next scene.
///
/// SETUP:
/// 1. Add this script to any GameObject in the scene (e.g. an empty
///    "DialogueManager").
/// 2. In "Lines", add one entry per line:
///    - Speaker = the SpeechBubble component that should say it
///    - Text = the line itself
/// 3. Order the list top-to-bottom exactly how the conversation should
///    play — alternate Speaker between entries to go back and forth.
/// 4. Check "Play On Start" for the first line to appear automatically
///    when the scene loads, or leave unchecked and call StartDialogue()
///    from elsewhere (an interaction key press, trigger zone, etc).
/// 5. Click anywhere (left mouse button) to advance to the next line.
/// 6. Set "Scene To Load" (must be added to Build Settings) and "Delay
///    Before Scene Load" for what happens after the last line.
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
    }

    [Header("Conversation")]
    public List<DialogueLine> lines = new List<DialogueLine>();

    [Header("Playback")]
    public bool playOnStart = true;

    [Header("Scene Transition")]
    [Tooltip("If true, automatically loads a scene after the last line is dismissed.")]
    public bool loadSceneOnComplete = true;

    [Tooltip("Exact name of the scene to load (must be added to Build Settings).")]
    public string sceneToLoad = "NextScene";

    [Tooltip("Seconds to wait after the last line before loading the scene.")]
    public float delayBeforeSceneLoad = 2f;

    [Header("Events")]
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueComplete;

    private int currentIndex = -1;
    private bool isPlaying;

    private void Start()
    {
        if (playOnStart)
            StartDialogue();
    }

    private void Update()
    {
        if (!isPlaying) return;

        if (AdvanceClicked())
            AdvanceLine();
    }

    private bool AdvanceClicked()
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        return mouse != null && mouse.leftButton.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetMouseButtonDown(0);
#else
        return false;
#endif
    }

    /// <summary>Begins the conversation from the first line.</summary>
    public void StartDialogue()
    {
        if (isPlaying || lines.Count == 0) return;

        isPlaying = true;
        currentIndex = -1;
        onDialogueStart?.Invoke();

        AdvanceLine();
    }

    private void AdvanceLine()
    {
        // Hide whatever is currently showing before moving on.
        if (currentIndex >= 0 && currentIndex < lines.Count)
        {
            SpeechBubble previousSpeaker = lines[currentIndex].speaker;
            if (previousSpeaker != null)
                previousSpeaker.Hide();
        }

        currentIndex++;

        if (currentIndex >= lines.Count)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = lines[currentIndex];

        if (line.speaker == null)
        {
            Debug.LogWarning($"DialogueManager: line {currentIndex} has no Speaker assigned — skipping '{line.text}'.");
            AdvanceLine();
            return;
        }

        line.speaker.Show(line.text);
    }

    private void EndDialogue()
    {
        isPlaying = false;
        onDialogueComplete?.Invoke();

        if (loadSceneOnComplete)
            StartCoroutine(LoadSceneAfterDelay());
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeSceneLoad);

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("DialogueManager: Scene To Load is empty — set it in the Inspector.");
            yield break;
        }

        Debug.Log($"DialogueManager: loading scene '{sceneToLoad}'.");
        SceneManager.LoadScene(sceneToLoad);
    }
}