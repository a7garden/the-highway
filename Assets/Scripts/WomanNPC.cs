using UnityEngine;
using System.Collections;

public class WomanNPC : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    public string[] lines = new string[]
    {
        "괴물이 있어요.",
        "...내 안에.",
        "가지 마세요.",
        "살려주세요...",
        "제발."
    };

    [Header("Movement")]
    public float walkSpeed = 1.3f;
    public float erraticSpeed = 2f;

    // IInteractable
    public string InteractPrompt => _awaitingClick ? "말을 건다" : "";

    private Transform player;
    private Animator _anim;
    private bool _awaitingClick = false;

    void Start()
    {
        player = UnityEngine.GameObject.FindWithTag("Player")?.transform;
        _anim  = GetComponentInChildren<Animator>();
    }

    // IInteractable.OnInteract — called by InteractionSystem on click
    public void OnInteract()
    {
        if (!_awaitingClick) return;
        _awaitingClick = false;
        StartCoroutine(DialogueAndLeaveRoutine());
    }

    void SetAnim(bool walking)
    {
        if (_anim == null) return;
        // polyperfect Base Controller: "Walking" bool or "Speed" float
        if (_anim.parameters.Length > 0)
        {
            foreach (var p in _anim.parameters)
            {
                if (p.name == "Walking" && p.type == AnimatorControllerParameterType.Bool)
                    { _anim.SetBool("Walking", walking); return; }
                if (p.name == "Speed" && p.type == AnimatorControllerParameterType.Float)
                    { _anim.SetFloat("Speed", walking ? 1f : 0f); return; }
            }
        }
    }

    public void StartSequence() { StartCoroutine(ApproachRoutine()); }

    IEnumerator ApproachRoutine()
    {
        // Walk toward player until close enough
        SetAnim(true);
        while (player != null && Vector3.Distance(transform.position, player.position) > 2.8f)
        {
            Vector3 d = (player.position - transform.position); d.y = 0; d.Normalize();
            transform.position += d * walkSpeed * Time.deltaTime;
            transform.forward = d;
            yield return null;
        }
        SetAnim(false);

        // Stop and wait for player click
        _awaitingClick = true;
    }

    IEnumerator DialogueAndLeaveRoutine()
    {
        // Click-advance dialogue (Paratopic style)
        GameManager.Instance?.EnableMovement(false);
        yield return DialogueManager.Instance.PlayLinesCoroutine(lines);
        GameManager.Instance?.EnableMovement(true);

        // Walk away erratically
        SetAnim(true);
        Vector3 wander = (transform.forward + new Vector3(0.6f, 0f, 0.2f)).normalized;
        float timer = 0f;
        while (timer < 5f)
        {
            timer += Time.deltaTime;
            if (timer % 0.7f < Time.deltaTime)
                wander = Quaternion.Euler(0, Random.Range(-70f, 70f), 0) * wander;
            transform.position += wander * erraticSpeed * Time.deltaTime;
            transform.forward = wander;
            yield return null;
        }
        SetAnim(false);

        GameManager.Instance?.SetState(HighwayState.WalkToTaxi);
    }
}
