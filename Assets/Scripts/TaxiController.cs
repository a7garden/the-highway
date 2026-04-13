using UnityEngine;
using System.Collections;

public class TaxiController : MonoBehaviour
{
    [Header("Settings")]
    public float driveSpeed = 7f;
    public float stopDist = 5f;
    private Transform player;

    void OnEnable() { player = GameObject.FindWithTag("Player")?.transform; }

    public void BeginApproach() { StartCoroutine(ApproachRoutine()); }

    IEnumerator ApproachRoutine()
    {
        while (player != null)
        {
            Vector3 d = player.position - transform.position; d.y = 0;
            if (d.magnitude < stopDist) break;
            transform.position += d.normalized * driveSpeed * Time.deltaTime;
            transform.forward = d.normalized;
            yield return null;
        }

        yield return new WaitForSeconds(0.6f);
        DialogueManager.Instance?.ShowDialogue("잠깐만요.");
        yield return new WaitForSeconds(1.5f);

        var choiceUI = FindObjectOfType<ChoiceUI>();
        if (choiceUI != null)
            choiceUI.Show("아까 이쪽으로 가던 여자 봤어요?", "네, 봤어요", "못 봤어요", OnYes, OnNo);
    }

    void OnYes()
    {
        GameManager.Instance.playerSaidYesToTaxi = true;
        StartCoroutine(YesSequence());
    }

    IEnumerator YesSequence()
    {
        string[] ls = { "고맙소.", "이거 받아요.", "담배 한 개피야." };
        foreach (var l in ls) { DialogueManager.Instance?.ShowDialogue(l); yield return new WaitForSeconds(2f); }
        // Activate cigarette pickup
        var cig = FindObjectOfType<PickupItem>();
        if (cig != null) cig.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        yield return DriveAway();
    }

    void OnNo()
    {
        GameManager.Instance.playerSaidYesToTaxi = false;
        StartCoroutine(NoSequence());
    }

    IEnumerator NoSequence()
    {
        string[] ls = { "...", "이 근처에 정신 나간 여자가 있어요.", "만나면 피하시오." };
        foreach (var l in ls) { DialogueManager.Instance?.ShowDialogue(l); yield return new WaitForSeconds(2f); }
        yield return DriveAway();
    }

    IEnumerator DriveAway()
    {
        yield return new WaitForSeconds(0.5f);
        float t = 0f;
        while (t < 3f) { t += Time.deltaTime; transform.position += transform.forward * driveSpeed * Time.deltaTime; yield return null; }
        GameManager.Instance?.SetState(HighwayState.Room1);
    }
}
