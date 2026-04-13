using UnityEngine;
using System.Collections;

public class MonsterAI : MonoBehaviour
{
    [Header("Settings")]
    public float chargeSpeed = 5f;
    public float chargeDelay = 2f;
    private Transform player;
    private bool charging = false;

    void OnEnable()
    {
        player = GameObject.FindWithTag("Player")?.transform;
    }

    public void StartCharge()
    {
        StartCoroutine(ChargeRoutine());
    }

    IEnumerator ChargeRoutine()
    {
        DialogueManager.Instance?.ShowDialogue("..!!");
        yield return new WaitForSeconds(chargeDelay);
        charging = true;
        GameManager.Instance?.EnableMovement(false);

        while (charging && player != null)
        {
            Vector3 d = player.position - transform.position; d.y = 0;
            if (d.magnitude < 1.8f)
            {
                charging = false;
                if (!GameManager.Instance.hasGun)
                {
                    DialogueManager.Instance?.ShowDialogue("...(총이 없다)...");
                    GameManager.Instance?.SetState(HighwayState.Ending);
                }
                break;
            }
            transform.position += d.normalized * chargeSpeed * Time.deltaTime;
            transform.forward = d.normalized;
            yield return null;
        }
    }

    public void GetShot()
    {
        charging = false;
        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        float t = 0f;
        Quaternion from = transform.rotation;
        Quaternion to = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
        while (t < 1f) { t += Time.deltaTime * 2f; transform.rotation = Quaternion.Lerp(from, to, t); yield return null; }
        yield return new WaitForSeconds(0.5f);
        GameManager.Instance?.SetState(HighwayState.Ending);
    }
}
