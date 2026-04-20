using System.Collections;
using UnityEngine;

public class Room1Controller : MonoBehaviour
{
    private bool _dialogueComplete;
    private Coroutine _sequence;

    public bool IsDialogueComplete => _dialogueComplete;

    void OnEnable()
    {
        _dialogueComplete = false;
        _sequence = StartCoroutine(PlayRoomSequence());
    }

    void OnDisable()
    {
        if (_sequence != null)
        {
            StopCoroutine(_sequence);
            _sequence = null;
        }
        _dialogueComplete = false;
    }

    private IEnumerator PlayRoomSequence()
    {
        yield return new WaitForSeconds(1.5f);

        if (DialogueManager.Instance != null)
        {
            yield return DialogueManager.Instance.PlayLinesCoroutine(
                "<i>엄마: ...또 그러고 있어.</i>",
                "<i>아빠: ...알아.</i>",
                "<i>엄마: 어제도 이상한 그림을 그리더라.</i>",
                "<i>엄마: 웃고 있었어. 혼자서.</i>",
                "<i>아빠: ...조용히 해.</i>",
                "<i>엄마: 무서워. 정말 무서워.</i>",
                "<i>아빠: ...나도 그래.</i>",
                "<i>엄마: 저 애를... 우리가 키워야 할까?</i>",
                "...",
                "<i>아빠: ...모르겠어.</i>"
            );
        }

        DialogueManager.Instance?.ShowDialogue("[ 방에 TV가 있다. ]", 3f);

        _dialogueComplete = true;
    }
}
