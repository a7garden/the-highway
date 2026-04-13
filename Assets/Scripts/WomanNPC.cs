using UnityEngine;
using System.Collections;

public class WomanNPC : MonoBehaviour
{
    [Header("Dialogue")]
    public string[] lines = new string[]
    {
        "거기 멈춰요!",
        "저쪽은 위험해요...",
        "괴물이 있어요. 진짜예요.",
        "빨리 도망치세요...",
        "...제발."
    };

    [Header("Movement")]
    public float walkSpeed = 1.3f;
    public float erraticSpeed = 2f;

    private Transform player;
    private Animator _anim;

    void Start()
    {
        player = UnityEngine.GameObject.FindWithTag("Player")?.transform;
        _anim  = GetComponentInChildren<Animator>();
    }

    void SetAnim(bool walking)
    {
        if (_anim == null) return;
        // polyperfect Base Controller 파라미터: "Walking" bool 또는 "Speed" float
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

    public void StartSequence() { StartCoroutine(WomanRoutine()); }

    IEnumerator WomanRoutine()
    {
        // Walk toward player
        SetAnim(true);
        while (player != null && Vector3.Distance(transform.position, player.position) > 2.8f)
        {
            Vector3 d = (player.position - transform.position); d.y = 0; d.Normalize();
            transform.position += d * walkSpeed * Time.deltaTime;
            transform.forward = d;
            yield return null;
        }
        SetAnim(false);

        // Dialogue
        GameManager.Instance?.EnableMovement(false);
        foreach (var line in lines)
        {
            DialogueManager.Instance?.ShowDialogue(line);
            yield return new WaitForSeconds(2.4f);
        }
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
