using UnityEngine;
using System.Collections;

public enum HighwayState
{
    Intro, AntKilling, WomanCutscene, WomanDialogue,
    WalkToTaxi, TaxiEncounter, Room1, WalkToBody,
    BodyScene, CameraPickup, HospitalScene,
    BloodyRoad, DrawerScene, CorpseRoad, HouseScene, MonsterFight, Ending
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public HighwayState State { get; private set; }

    [Header("Player")]
    public CharacterController playerCC;
    public Transform playerTransform;
    public HorrorPlayerController playerController;
    public Transform cameraTransform;

    [Header("Segments")]
    public GameObject seg_Ants;
    public GameObject seg_WomanNPC;
    public GameObject seg_CutsceneWoman;
    public GameObject seg_Taxi;
    public GameObject seg_Room1;
    public GameObject seg_BodyNPC;
    public GameObject seg_HospitalRoom;
    public GameObject seg_DrawerRoom;
    public GameObject seg_CorpseRoad;
    public GameObject seg_House;
    public GameObject seg_Monster;

    [Header("Road")]
    public MeshRenderer roadRenderer;
    public Material matRoadNormal;
    public Material matRoadBloody;

    [Header("Spawn Points")]
    public Transform sp_RoadStart;
    public Transform sp_AntView;        // 개미 구간 전용 시점 위치 (위에서 내려다보는 곳)
    public Transform sp_AfterWoman;
    public Transform sp_AfterRoom1;
    public Transform sp_Room1;
    public Transform sp_Hospital;
    public Transform sp_DrawerRoom;
    public Transform sp_CorpseRoad;
    public Transform sp_House;

    [Header("Ant Segment Lighting")]
    public Light antSunLight;           // 개미 구간 전용 밝은 방향광

    [Header("Normal Scene Lighting")]
    public Light flashLight;            // 평소 손전등
    public Light moonLight;             // 평소 달빛

    [Header("Audio")]
    public AudioSource bgmSource;

    [HideInInspector] public bool playerSaidYesToTaxi;
    [HideInInspector] public bool hasCamera;
    [HideInInspector] public bool hasGun;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        if (bgmSource) bgmSource.Play();
        // 개미 전용 조명은 처음엔 꺼둠
        if (antSunLight != null) antSunLight.enabled = false;
        SetState(HighwayState.Intro);
    }

    public void SetState(HighwayState next)
    {
        DisableAllSegments();
        State = next;

        switch (next)
        {
            case HighwayState.Intro:
                EnableMovement(true);
                Teleport(sp_RoadStart);
                break;

            case HighwayState.AntKilling:
                // 전용 위치로 이동 → 카메라 아래 부드럽게 전환 → 밝은 조명 ON
                if (sp_AntView != null) Teleport(sp_AntView);
                if (playerController != null) playerController.SmoothLockCameraDown(0.5f);
                SetAntLighting(true);
                EnableSeg(seg_Ants);
                break;

            case HighwayState.WomanCutscene:
                EnableMovement(false);
                EnableSeg(seg_CutsceneWoman);
                StartCoroutine(WomanCutsceneRoutine());
                break;

            case HighwayState.WomanDialogue:
            {
                DisableSeg(seg_CutsceneWoman);
                EnableMovement(false);
                EnableSeg(seg_WomanNPC);
                var w = seg_WomanNPC != null ? seg_WomanNPC.GetComponentInChildren<WomanNPC>() : null;
                if (w != null) w.StartSequence();
                break;
            }
            case HighwayState.WalkToTaxi:
                EnableMovement(true);
                Teleport(sp_AfterWoman);
                break;

            case HighwayState.TaxiEncounter:
            {
                EnableMovement(false);
                EnableSeg(seg_Taxi);
                var t = seg_Taxi != null ? seg_Taxi.GetComponentInChildren<TaxiController>() : null;
                if (t != null) t.BeginApproach();
                break;
            }
            case HighwayState.Room1:
                Teleport(sp_Room1);
                EnableSeg(seg_Room1);
                EnableMovement(true);
                DialogueManager.Instance?.ShowDialogue("...");
                break;

            case HighwayState.WalkToBody:
                Teleport(sp_AfterRoom1);
                EnableMovement(true);
                break;

            case HighwayState.BodyScene:
            {
                EnableSeg(seg_BodyNPC);
                var b = seg_BodyNPC != null ? seg_BodyNPC.GetComponentInChildren<BodyNPC>() : null;
                if (b != null) b.StartSequence();
                break;
            }
            case HighwayState.CameraPickup:
            {
                var ci = FindObjectOfType<CameraItem>();
                if (ci != null) ci.gameObject.SetActive(true);
                break;
            }
            case HighwayState.HospitalScene:
            {
                Teleport(sp_Hospital);
                EnableSeg(seg_HospitalRoom);
                EnableMovement(true);
                var h = seg_HospitalRoom != null ? seg_HospitalRoom.GetComponent<HospitalRoom>() : null;
                if (h != null) h.StartScene();
                break;
            }
            case HighwayState.BloodyRoad:
                if (roadRenderer != null && matRoadBloody != null)
                    roadRenderer.material = matRoadBloody;
                Teleport(sp_AfterRoom1);
                EnableMovement(true);
                DialogueManager.Instance?.ShowDialogue("도로가 붉다.");
                break;

            case HighwayState.DrawerScene:
            {
                Teleport(sp_DrawerRoom);
                EnableSeg(seg_DrawerRoom);
                EnableMovement(false);
                var dr = seg_DrawerRoom != null ? seg_DrawerRoom.GetComponent<DrawerRoom>() : null;
                if (dr != null) dr.StartScene();
                break;
            }
            case HighwayState.CorpseRoad:
            {
                Teleport(sp_CorpseRoad);
                EnableSeg(seg_CorpseRoad);
                hasGun = true;
                EnableMovement(true);
                var g = FindObjectOfType<GunSystem>();
                if (g != null) g.Enable();
                break;
            }
            case HighwayState.HouseScene:
                EnableMovement(true);
                EnableSeg(seg_House);
                break;

            case HighwayState.MonsterFight:
            {
                EnableMovement(false);
                EnableSeg(seg_Monster);
                var m = seg_Monster != null ? seg_Monster.GetComponentInChildren<MonsterAI>() : null;
                if (m != null) m.StartCharge();
                break;
            }
            case HighwayState.Ending:
                EnableMovement(false);
                StartCoroutine(EndingRoutine());
                break;
        }
    }

    // ── 개미 구간 조명 전환 ──────────────────────────────────────────
    public void SetAntLighting(bool antMode)
    {
        // 개미 전용 밝은 햇빛 (화면을 하얗게 만들므로 일단 끄기)
        if (antSunLight != null)
            antSunLight.enabled = false;

        // 평소 조명은 반대로
        if (flashLight != null) flashLight.enabled = !antMode;
        if (moonLight  != null) moonLight.enabled  = !antMode;

        // 앰비언트만 약간 조절
        RenderSettings.ambientLight = antMode
            ? new Color(0.2f, 0.2f, 0.25f)
            : new Color(0.15f, 0.15f, 0.2f);
    }

    // ── 개미 구간 종료 시 호출 (AntSystem에서 부름) ──────────────────
    public void RestoreFromAntView()
    {
        // 조명 복구
        SetAntLighting(false);
        // 이동 재개
        EnableMovement(true);
    }

    // ── 개미가 모두 지나갈 때 호출 ─────────────────────────────────
    public void EndAntEvent()
    {
        StartCoroutine(EndAntEventRoutine());
    }

    System.Collections.IEnumerator EndAntEventRoutine()
    {
        if (playerController != null)
            playerController.SmoothUnlockCamera(0.5f);
        yield return new WaitForSeconds(0.6f);
        DisableSeg(seg_Ants);
        SetAntLighting(false);
        EnableMovement(true);
        // 상태를 Intro로 (단, 텔레포트 없이 현재 위치 유지)
        State = HighwayState.Intro;
    }

    // ─────────────────────────────────────────────────────────────────

    void DisableAllSegments()
    {
        DisableSeg(seg_Ants); DisableSeg(seg_WomanNPC); DisableSeg(seg_CutsceneWoman);
        DisableSeg(seg_Taxi); DisableSeg(seg_Room1); DisableSeg(seg_BodyNPC);
        DisableSeg(seg_HospitalRoom); DisableSeg(seg_DrawerRoom);
        DisableSeg(seg_CorpseRoad); DisableSeg(seg_House); DisableSeg(seg_Monster);
    }

    void EnableSeg(GameObject s)  { if (s != null) s.SetActive(true); }
    void DisableSeg(GameObject s) { if (s != null) s.SetActive(false); }

    public void Teleport(Transform t)
    {
        if (t == null || playerTransform == null) return;
        if (playerCC != null) playerCC.enabled = false;
        playerTransform.position = t.position;
        playerTransform.rotation = t.rotation;
        if (playerCC != null) playerCC.enabled = true;
    }

    public void EnableMovement(bool on)
    {
        if (playerController != null) playerController.enabled = on;
    }

    IEnumerator WomanCutsceneRoutine()
    {
        float t = 0f;
        Quaternion from = cameraTransform.localRotation;
        Quaternion toUp = Quaternion.Euler(-75f, 0f, 0f);
        while (t < 1f)
        {
            t += Time.deltaTime * 1.5f;
            cameraTransform.localRotation = Quaternion.Lerp(from, toUp, t);
            yield return null;
        }
        yield return new WaitForSeconds(2.5f);
        t = 0f;
        from = cameraTransform.localRotation;
        Quaternion toNormal = Quaternion.Euler(0f, 0f, 0f);
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            cameraTransform.localRotation = Quaternion.Lerp(from, toNormal, t);
            yield return null;
        }
        SetState(HighwayState.WomanDialogue);
    }

    IEnumerator EndingRoutine()
    {
        yield return new WaitForSeconds(1f);
        DialogueManager.Instance?.ShowDialogue("괴물은 쓰러졌다.");
        yield return new WaitForSeconds(3f);
        DialogueManager.Instance?.ShowDialogue("도로는 다시 조용해졌다.");
        yield return new WaitForSeconds(4f);
        DialogueManager.Instance?.ShowDialogue("[ The Highway  -  END ]");
    }
}
