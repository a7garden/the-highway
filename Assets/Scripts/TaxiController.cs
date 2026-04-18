using UnityEngine;
using System.Collections;

public class TaxiController : MonoBehaviour
{
    [Header("Settings")]
    public float driveSpeed = 10f;
    public float stopDistInFront = 8f; // 플레이어 앞쪽에서 멈출 거리

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

        // 플레이어 Ahead(앞쪽) 도로 위에 택시 스폰
        if (player == null) yield break;

        Vector3 spawnPos = player.position + player.forward * 50f;
        spawnPos.y = player.position.y; // 같은 높이
        transform.position = spawnPos;
        transform.forward = -player.forward; // 플레이어를 향해

        EnableSeg(gameObject);

        // ── 1) 택시가 플레이어 앞쪽에서 멈출 때까지 접근 ──
        while (player != null)
        {
            Vector3 d = player.position - transform.position;
            d.y = 0;
            if (d.magnitude <= stopDistInFront)
            {
                // 정지
                transform.position = player.position - player.forward * stopDistInFront;
                transform.forward = player.forward; // 진행 방향 = 플레이어 반대편
                break;
            }
            transform.position += d.normalized * driveSpeed * Time.deltaTime;
            transform.forward = d.normalized;
            yield return null;
        }

        // ── 2) 카메라가 택시를 향해 회전 ──
        if (cameraTransform != null && playerController != null)
        {
            playerController.SmoothLookAt(transform.position, 1f);
            yield return new WaitForSeconds(1.5f);
        }

        // ── 3) 5초간 멈춰 있음 ──
        yield return new WaitForSeconds(5f);

        // ── 4) 택시가 플레이어 뒤쪽(시야 밖)으로Driving away ──
        Vector3 awayDir = -transform.forward; // 현재 forward의 반대 = 원래 왔던 방향
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

        // ── 5) 택시 비활성화 ──
        DisableSeg(gameObject);

        // ── 6) 플레이어 조작 복원, Room1로 ──
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnableMovement(true);
            GameManager.Instance.SetState(HighwayState.Room1);
        }
    }

    void EnableSeg(GameObject s) { if (s != null) s.SetActive(true); }
    void DisableSeg(GameObject s) { if (s != null) s.SetActive(false); }
}
