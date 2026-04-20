using UnityEngine;
using System.Collections;

public class TaxiController : MonoBehaviour
{
    [Header("Settings")]
    public float driveSpeed = 10f;
    public float stopDistInFront = 8f;

    private Transform player;
    private Transform cameraTransform;
    private HorrorPlayerController playerController;
    private bool running = false;

    void OnEnable()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        var gm = GameManager.Instance;
        if (gm != null)
        {
            playerController = gm.playerController;
            cameraTransform = gm.cameraTransform;
        }
        running = false;
    }

    public void BeginApproach()
    {
        if (running) return;
        StartCoroutine(ApproachRoutine());
    }

    IEnumerator ApproachRoutine()
    {
        running = true;

        if (player == null) yield break;

        Vector3 spawnPos = player.position + player.forward * 50f;
        spawnPos.y = player.position.y;
        transform.position = spawnPos;
        transform.forward = -player.forward;

        EnableSeg(gameObject);

        while (player != null)
        {
            Vector3 d = player.position - transform.position;
            d.y = 0;
            if (d.magnitude <= stopDistInFront)
            {
                transform.position = player.position - player.forward * stopDistInFront;
                transform.forward = player.forward;
                break;
            }
            transform.position += d.normalized * driveSpeed * Time.deltaTime;
            transform.forward = d.normalized;
            yield return null;
        }

        if (cameraTransform != null && playerController != null)
        {
            playerController.SmoothLookAt(transform.position, 1f);
            yield return new WaitForSeconds(1.5f);
        }

        // driver dialogue
        if (DialogueManager.Instance != null)
        {
            yield return StartCoroutine(DialogueManager.Instance.PlayLinesCoroutine(
                "운전기사: ...",
                "운전기사: 혹시 저 아래에서, 여자 한 명 못 봤소?",
                "운전기사: 피를 흘리면서 뛰어가는 여자…"
            ));
        }

        // unlock cursor so choice buttons are clickable
        playerController?.SetCursorLock(false);

        bool chose = false;
        bool yes = false;

        var choiceUI = UnityEngine.Object.FindObjectOfType<ChoiceUI>(true);
        if (choiceUI != null)
        {
            choiceUI.Show(
                "뭐라고 대답할까?",
                "봤습니다",
                "못 봤습니다",
                () => { yes = true;  chose = true; },
                () => { yes = false; chose = true; }
            );
            yield return new WaitUntil(() => chose);
        }
        else
        {
            chose = true;
        }

        playerController?.SetCursorLock(true);

        if (GameManager.Instance != null)
            GameManager.Instance.playerSaidYesToTaxi = yes;

        if (yes)
        {
            if (DialogueManager.Instance != null)
            {
                yield return StartCoroutine(DialogueManager.Instance.PlayLinesCoroutine(
                    "운전기사: ...그래.",
                    "운전기사: 피곤해 보여. 이거 받아 둬.",
                    "(운전기사가 창밖으로 담배 한 개비를 떨어뜨린다.)"
                ));
            }
        }
        else
        {
            if (DialogueManager.Instance != null)
            {
                yield return StartCoroutine(DialogueManager.Instance.PlayLinesCoroutine(
                    "운전기사: 그래... 조심히 가."
                ));
            }
        }

        SpawnCigarette();

        Vector3 awayDir = -transform.forward;
        float elapsed = 0f;
        float duration = 3f;
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, startPos + awayDir * 100f, t);
            yield return null;
        }

        DisableSeg(gameObject);

        // cigarette pickup handles SetState(Room1); restore movement only
        GameManager.Instance?.EnableMovement(true);
    }

    void SpawnCigarette()
    {
        if (player == null) return;

        // drop the cigarette just behind the taxi (from driver's window), slightly toward player's right
        Vector3 dropPos = transform.position - transform.forward * 1.5f + player.right * 0.8f;
        dropPos.y = player.position.y + 0.05f;

        foreach (var item in UnityEngine.Object.FindObjectsOfType<PickupItem>(true))
        {
            if (item.gameObject.name == "RoadCigarette")
            {
                item.transform.position = dropPos;
                item.gameObject.SetActive(true);
                return;
            }
        }
    }

    void EnableSeg(GameObject s)  { if (s != null) s.SetActive(true); }
    void DisableSeg(GameObject s) { if (s != null) s.SetActive(false); }
}
