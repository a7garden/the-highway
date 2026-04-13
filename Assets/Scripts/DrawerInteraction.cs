using UnityEngine;
using System.Collections;

public class DrawerInteraction : MonoBehaviour, IInteractable
{
    private bool opened = false;
    public string InteractPrompt { get { return opened ? "" : "서랍 열기"; } }

    public void OnInteract()
    {
        if (opened) return;
        opened = true;
        StartCoroutine(OpenRoutine());
    }

    IEnumerator OpenRoutine()
    {
        float t = 0f;
        Vector3 s = transform.localPosition;
        Vector3 e = s + Vector3.forward * 0.35f;
        while (t < 0.5f) { t += Time.deltaTime * 2f; transform.localPosition = Vector3.Lerp(s, e, t); yield return null; }
        DialogueManager.Instance?.ShowDialogue("서랍 안에 총이 있다.");
        yield return new WaitForSeconds(1.5f);
        DialogueManager.Instance?.ShowDialogue("총을 집어들었다.");
        yield return new WaitForSeconds(1f);
        GameManager.Instance?.SetState(HighwayState.CorpseRoad);
    }
}
