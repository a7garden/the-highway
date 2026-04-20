using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI subtitleText;

    [Header("Settings")]
    public float typingSpeed = 0.035f;
    public float defaultAutoHide = 3f;

    private Coroutine _activeRoutine;
    private bool _isDialogueActive;

    public bool IsActive => _isDialogueActive;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void Start()
    {
        if (subtitleText != null) subtitleText.text = "";
    }

    // --- Public API ---

    public void ShowDialogue(string line) => ShowDialogue(line, defaultAutoHide);

    public void ShowDialogue(string line, float holdSeconds)
    {
        StopActive();
        _activeRoutine = StartCoroutine(NarratorLine(line, holdSeconds));
    }

    public void ShowDialogue(string[] lines)
    {
        StopActive();
        _activeRoutine = StartCoroutine(NarratorSequence(lines));
    }

    public IEnumerator PlayLinesCoroutine(params string[] lines)
    {
        StopActive();
        var routine = StartCoroutine(ClickAdvanceSequence(lines));
        _activeRoutine = routine;
        yield return routine;
    }

    public void Hide()
    {
        StopActive();
        if (subtitleText != null) subtitleText.text = "";
    }

    // --- Internal ---

    private void StopActive()
    {
        if (_activeRoutine != null) StopCoroutine(_activeRoutine);
        _activeRoutine = null;
        _isDialogueActive = false;
    }

    private IEnumerator NarratorLine(string line, float holdSeconds)
    {
        _isDialogueActive = true;

        yield return TypeLine(line);

        float elapsed = 0f;
        while (elapsed < holdSeconds)
        {
            // click skips remaining hold
            if (Mouse.current.leftButton.wasPressedThisFrame) break;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        subtitleText.text = "";
        _isDialogueActive = false;
    }

    private IEnumerator NarratorSequence(string[] lines)
    {
        _isDialogueActive = true;

        foreach (string line in lines)
        {
            yield return TypeLine(line);

            float elapsed = 0f;
            while (elapsed < defaultAutoHide)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        subtitleText.text = "";
        _isDialogueActive = false;
    }

    private IEnumerator ClickAdvanceSequence(string[] lines)
    {
        _isDialogueActive = true;

        foreach (string line in lines)
        {
            yield return TypeLine(line);

            // one-frame gap so the click that ended typing isn't re-consumed here
            yield return null;

            yield return new WaitUntil(() => Mouse.current.leftButton.wasPressedThisFrame);
        }

        // one-frame gap before final clear
        yield return null;

        subtitleText.text = "";
        _isDialogueActive = false;
    }

    private IEnumerator TypeLine(string line)
    {
        subtitleText.text = line;
        subtitleText.maxVisibleCharacters = 0;

        float accumulated = 0f;
        int visible = 0;
        int total = line.Length;

        while (visible < total)
        {
            // unscaledDeltaTime so typing survives pause or slow-mo
            accumulated += Time.unscaledDeltaTime;

            // click during typing = instant complete
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                subtitleText.maxVisibleCharacters = total;
                yield break;
            }

            int newVisible = Mathf.Min(total, Mathf.FloorToInt(accumulated / typingSpeed));
            if (newVisible != visible)
            {
                visible = newVisible;
                subtitleText.maxVisibleCharacters = visible;
            }

            yield return null;
        }
    }
}
