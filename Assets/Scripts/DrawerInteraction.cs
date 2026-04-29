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
        // slide drawer out
        float t = 0f;
        Vector3 s = transform.localPosition;
        Vector3 e = s + Vector3.forward * 0.35f;
        while (t < 0.5f) { t += Time.deltaTime * 2f; transform.localPosition = Vector3.Lerp(s, e, t); yield return null; }

        if (DialogueManager.Instance != null)
        {
            yield return DialogueManager.Instance.PlayLinesCoroutine(
                "서랍 안에 총이 있다.",
                "총을 집어들었다."
            );
        }

        GameManager.Instance?.SetState(HighwayState.CorpseRoad);
    }
}
