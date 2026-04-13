using UnityEngine;
using System.Collections;

public class BodyNPC : MonoBehaviour, IInteractable
{
    public string[] lines = { "...제발...", "괴물이...", "살려줘...", "..." };
    private bool dead = false;

    public string InteractPrompt { get { return dead ? "시체 조사" : ""; } }

    void OnEnable() { dead = false; }

    public void StartSequence() { StartCoroutine(BodyRoutine()); }

    IEnumerator BodyRoutine()
    {
        yield return new WaitForSeconds(1f);
        foreach (var l in lines) { DialogueManager.Instance?.ShowDialogue(l); yield return new WaitForSeconds(2.5f); }
        dead = true;
        transform.localScale = new Vector3(transform.localScale.x, 0.08f, transform.localScale.z);
        DialogueManager.Instance?.ShowDialogue("...사망했다.");
        yield return new WaitForSeconds(1.5f);
        DialogueManager.Instance?.ShowDialogue("[ 시체를 조사할 수 있다 ]");
    }

    public void OnInteract()
    {
        if (!dead) return;
        DialogueManager.Instance?.ShowDialogue("카메라를 발견했다.");
        GameManager.Instance?.SetState(HighwayState.CameraPickup);
        gameObject.SetActive(false);
    }
}
