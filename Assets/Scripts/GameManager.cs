using UnityEngine;
using System.Collections;

public enum HighwayState
{
    Intro, AntKilling, WomanCutscene, WomanDisappear,
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

    void Update()
    {
        // Woman NPC proximity check
        if (State == HighwayState.Intro && seg_WomanNPC != null && seg_WomanNPC.activeInHierarchy && playerTransform != null)
        {
            float dist = Vector3.Distance(playerTransform.position, seg_WomanNPC.transform.position);
            if (dist < 5f)
            {
                // NPC 떠나기 이벤트 시작 (카메라가 NPC를 바라보게 함)
                EnableMovement(false);
                State = HighwayState.WomanDisappear;
                StartCoroutine(WomanDisappearRoutine());
            }
        }

        // Taxi 트리거: WalkToTaxi에서 앞으로 많이 걸으면 TaxiEncounter
        if (State == HighwayState.WalkToTaxi && playerTransform != null)
        {
            if (playerTransform.position.z > 30f)
            {
                SetState(HighwayState.TaxiEncounter);
            }
        }
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
                // 현재 위치에서 이동 잠금 → 카메라 아래 부드럽게 전환 → 조명만 변경
                if (playerController != null) playerController.SmoothLockCameraDown(0.5f);
                EnableMovement(false);
                SetAntLighting(true);
                EnableSeg(seg_Ants);
                break;

            case HighwayState.WomanCutscene:
                EnableMovement(false);
                EnableSeg(seg_CutsceneWoman);
                StartCoroutine(WomanCutsceneRoutine());
                break;

            case HighwayState.WomanDisappear:
                // Update에서 StartCoroutine(WomanDisappearRoutine()) 호출
                break;

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

        // Woman NPC를 플레이어 앞쪽 도로 우측에 스폰
        if (seg_WomanNPC != null && playerTransform != null)
        {
            Vector3 npcPos = new Vector3(5f, playerTransform.position.y, playerTransform.position.z + 20f);
            seg_WomanNPC.transform.position = npcPos;

            // AI 스크립트 비활성화를 먼저 (SetActive보다 먼저)
            var aiScripts = seg_WomanNPC.GetComponentsInChildren<UnityEngine.MonoBehaviour>(true);
            foreach (var s in aiScripts)
            {
                string typeName = s.GetType().Name;
                if (typeName.Contains("Wander") || typeName.Contains("AI") || typeName.Contains("Common"))
                    s.enabled = false;
            }

            EnableSeg(seg_WomanNPC);

            // 기존 Visual child들 제거하고 box로 교체
            ReplaceNpcVisual(seg_WomanNPC.transform);
        }

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

    void ReplaceNpcVisual(Transform npcRoot)
    {
        // WomanNPC_Visual 찾기
        var visual = npcRoot.Find("WomanNPC_Visual");
        if (visual == null) return;

        // 기존 children 모두 제거
        foreach (Transform child in visual)
            UnityEngine.Object.Destroy(child.gameObject);

        // NPC_Body를 WomanNPC_Visual child로 생성
        var npc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        npc.name = "NPC_Body";
        npc.transform.SetParent(visual); // WomanNPC_Visual의 child로 설정
        npc.transform.localPosition = new Vector3(0f, 1f, 0f);
        npc.transform.localScale = new Vector3(0.5f, 1f, 0.5f);

        // 간단한 material 할당
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.9f, 0.7f, 0.6f); // 살구색
        npc.GetComponent<Renderer>().material = mat;
    }

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
        SetState(HighwayState.Intro);
    }

    // ── 여자 NPC 사라지는 이벤트 ─────────────────────────────────────
    IEnumerator WomanDisappearRoutine()
    {
        if (seg_WomanNPC == null || playerController == null || cameraTransform == null)
        {
            EnableMovement(true);
            State = HighwayState.Intro;
            yield break;
        }

        Vector3 npcPos = seg_WomanNPC.transform.position;

        // 1) 카메라가 NPC 위치로 부드럽게 회전
        playerController.SmoothLookAt(npcPos, 1f);
        yield return new WaitForSeconds(1.5f); // 카메라 회전 완료 대기

        // 2) 5초간 NPC 응시 (카메라 고정)
        yield return new WaitForSeconds(5f);

        // 3) NPC가 플레이어 뒤/시야 밖으로 부드럽게 이동
        Vector3 awayDir = (playerTransform.position - npcPos).normalized;
        // NPC를 플레이어 반대편 + 약간 뒤로 이동
        Vector3 targetPos = playerTransform.position + awayDir * 30f + new Vector3(0f, 0f, -10f);
        float duration = 2f;
        float elapsed = 0f;
        Vector3 startPos = seg_WomanNPC.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            seg_WomanNPC.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        // 4) NPC 비활성화
        DisableSeg(seg_WomanNPC);

        // 5) 카메라가 다시 앞을 바라보게 (플레이어 방향)
        Quaternion targetCamRot = Quaternion.Euler(0f, 0f, 0f);
        float camElapsed = 0f;
        Quaternion camStart = cameraTransform.localRotation;
        while (camElapsed < 1f)
        {
            camElapsed += Time.deltaTime;
            cameraTransform.localRotation = Quaternion.Slerp(camStart, targetCamRot, camElapsed);
            yield return null;
        }
        cameraTransform.localRotation = targetCamRot;

        // 6) 플레이어 조작 복원, WalkToTaxi 상태로
        EnableMovement(true);
        State = HighwayState.WalkToTaxi;
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
