using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI subtitleText;

    [Header("Settings")]
    public float typingSpeed = 0.04f;
    public float autoHideDelay = 3f;

    private Coroutine _typingCoroutine;
    private Coroutine _hideCoroutine;
    private bool _isDialogueActive = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (subtitleText != null)
            subtitleText.text = "";
    }

    public void ShowDialogue(string[] lines)
    {
        StartCoroutine(PlayLines(lines));
    }

    public void ShowDialogue(string line)
    {
        ShowDialogue(new string[] { line });
    }

    private IEnumerator PlayLines(string[] lines)
    {
        _isDialogueActive = true;

        // 플레이어 커서 잠금 유지 (대화 중에도)
        foreach (string line in lines)
        {
            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);

            yield return _typingCoroutine = StartCoroutine(TypeLine(line));

            // 클릭으로 다음 줄 넘기기
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        }

        // 마지막 줄 후 자동 숨김
        _hideCoroutine = StartCoroutine(HideAfterDelay());
        _isDialogueActive = false;
    }

    private IEnumerator TypeLine(string line)
    {
        subtitleText.text = "";
        foreach (char c in line)
        {
            subtitleText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(autoHideDelay);
        subtitleText.text = "";
    }

    public bool IsActive() => _isDialogueActive;
}
