using UnityEngine;
using UnityEngine.InputSystem;

public class GunSystem : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────
    [Header("References")]
    [Tooltip("PlayerCamera 자식의 GunVisual (Gun.prefab 인스턴스)")]
    public GameObject gunVisual;

    [Header("Hip Position  (기본 대기 — 오른쪽 아래)")]
    public Vector3 hipPosition = new Vector3(0.25f, -0.28f, 0.45f);
    public Vector3 hipRotation = new Vector3(5f, -15f, 0f);

    [Header("Aim Position  (조준 — 화면 중앙)")]
    public Vector3 aimPosition = new Vector3(0f, -0.14f, 0.40f);
    public Vector3 aimRotation = new Vector3(0f, 0f, 0f);

    [Header("Settings")]
    [Tooltip("힙 ↔ 조준 전환 속도")]
    public float aimSpeed = 10f;

    // ── Private ───────────────────────────────────────────────────
    private bool _active   = false;
    private bool _isAiming = false;
    private bool _fired    = false;   // 발사 후 재장전 방지 (보스 1발)
    private Camera    _cam;
    private Animator  _anim;

    // ─────────────────────────────────────────────────────────────
    void Start()
    {
        _cam = Camera.main;
        if (gunVisual != null)
        {
            gunVisual.SetActive(false);
            _anim = gunVisual.GetComponent<Animator>();
        }
    }

    // 서랍에서 총 집었을 때 DrawerInteraction → GameManager → 여기 호출
    public void Enable()
    {
        _active = true;
        if (gunVisual != null)
        {
            gunVisual.SetActive(true);
            // 힙 위치에서 시작
            gunVisual.transform.localPosition    = hipPosition;
            gunVisual.transform.localEulerAngles = hipRotation;
        }
        DialogueManager.Instance?.ShowDialogue("총을 들었다.");
        DialogueManager.Instance?.ShowDialogue("[ 우클릭: 조준   좌클릭: 발사 ]");
    }

    // ─────────────────────────────────────────────────────────────
    void Update()
    {
        if (!_active || gunVisual == null) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        // ── 우클릭 조준 ──────────────────────────────────────────
        _isAiming = mouse.rightButton.isPressed;

        // 목표 위치/회전 lerp
        Vector3 tPos = _isAiming ? aimPosition : hipPosition;
        Vector3 tRot = _isAiming ? aimRotation : hipRotation;
        gunVisual.transform.localPosition = Vector3.Lerp(
            gunVisual.transform.localPosition, tPos, Time.deltaTime * aimSpeed);
        gunVisual.transform.localEulerAngles = Vector3.Lerp(
            gunVisual.transform.localEulerAngles, tRot, Time.deltaTime * aimSpeed);

        // ── 좌클릭 발사 (조준 상태에서만) ───────────────────────
        if (_isAiming && !_fired && mouse.leftButton.wasPressedThisFrame)
        {
            Fire();
        }
    }

    // ─────────────────────────────────────────────────────────────
    void Fire()
    {
        // 발사 애니메이션
        if (_anim != null) _anim.SetTrigger("Fire");

        // 레이캐스트
        if (_cam == null) return;
        Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 80f))
        {
            var monster = hit.collider.GetComponentInParent<MonsterAI>();
            if (monster != null)
            {
                DialogueManager.Instance?.ShowDialogue("탕!");
                _fired  = true;
                _active = false;
                // 애니메이션 끝난 후 총 숨김 (2.5초)
                Invoke("HideGun", 2.5f);
                monster.GetShot();
            }
        }
    }

    void HideGun()
    {
        if (gunVisual != null) gunVisual.SetActive(false);
    }
}
