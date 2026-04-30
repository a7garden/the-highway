using UnityEngine;
using System.Collections;

public class BodyNPC : MonoBehaviour, IInteractable
{
    public string[] lines = { "...제발...", "괴물이...", "살려줘...", "..." };
    private bool dead = false;

    [Tooltip("Animator on the laying-man visual. Auto-found in children if left empty. Disabled on death so the body stops breathing/twitching.")]
    public Animator visualAnimator;

    public string InteractPrompt { get { return dead ? "시체 조사" : ""; } }

    void OnEnable()
    {
        dead = false;
        if (visualAnimator == null) visualAnimator = GetComponentInChildren<Animator>(true);
        if (visualAnimator != null) visualAnimator.enabled = true;
    }

    public void StartSequence() { StartCoroutine(BodyRoutine()); }

    IEnumerator BodyRoutine()
    {
        yield return new WaitForSeconds(1f);

        // dying man click-advance dialogue
        if (DialogueManager.Instance != null && lines != null && lines.Length > 0)
            yield return DialogueManager.Instance.PlayLinesCoroutine(lines);

        // death pulse — the model is already laying down (laying.anim), so we just
        // freeze the animator to halt any subtle breathing/twitching motion.
        GameManager.Instance?.cameraTransform?.GetComponent<CameraSway>()?.Pulse(1.8f, 0.08f, 0.15f, 0.6f);
        dead = true;
        if (visualAnimator != null) visualAnimator.enabled = false;

        DialogueManager.Instance?.ShowDialogue("...사망했다.", 2.0f);
        yield return new WaitForSeconds(2.0f);
        DialogueManager.Instance?.ShowDialogue("[ 시체를 조사할 수 있다 ]", 4f);
    }

    public void OnInteract()
    {
        if (!dead) return;
        DialogueManager.Instance?.ShowDialogue("카메라를 발견했다.", 2f);
        GameManager.Instance?.SetState(HighwayState.CameraPickup);
        gameObject.SetActive(false);
    }
}
