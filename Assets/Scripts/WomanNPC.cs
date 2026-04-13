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

    void Start() { player = GameObject.FindWithTag("Player")?.transform; }

    public void StartSequence() { StartCoroutine(WomanRoutine()); }

    IEnumerator WomanRoutine()
    {
        // Walk toward player
        while (player != null && Vector3.Distance(transform.position, player.position) > 2.8f)
        {
            Vector3 d = (player.position - transform.position); d.y = 0; d.Normalize();
            transform.position += d * walkSpeed * Time.deltaTime;
            transform.forward = d;
            yield return null;
        }

        // Dialogue
        GameManager.Instance?.EnableMovement(false);
        foreach (var line in lines)
        {
            DialogueManager.Instance?.ShowDialogue(line);
            yield return new WaitForSeconds(2.4f);
        }
        GameManager.Instance?.EnableMovement(true);

        // Walk away erratically
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

        GameManager.Instance?.SetState(HighwayState.WalkToTaxi);
    }
}
