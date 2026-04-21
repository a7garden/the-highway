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

    private CameraSway _sway;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        _sway = cameraTransform != null ? cameraTransform.GetComponent<CameraSway>() : null;
        _sway?.SetIntensity(1f, 0f);

        if (bgmSource) bgmSource.Play();
        // 개미 전용 조명은 처음엔 꺼둠
        if (antSunLight != null) antSunLight.enabled = false;
        SetState(HighwayState.Intro);
    }

    void Update()
    {
        // Taxi 트리거: WalkToTaxi에서 앞으로 많이 걸으면 TaxiEncounter
        if (State == HighwayState.WalkToTaxi && playerTransform != null)
        {
            if (playerTransform.position.z > 30f)
            {
                SetState(HighwayState.TaxiEncounter);
            }
        }

        // WalkToBody 트리거: 시체 근처까지 걸으면 BodyScene
        if (State == HighwayState.WalkToBody && playerTransform != null && seg_BodyNPC != null)
        {
            var body = seg_BodyNPC.transform.Find("BodyNPC");
            if (body != null && Vector3.Distance(playerTransform.position, body.position) < 4f)
                SetState(HighwayState.BodyScene);
        }

        // CorpseRoad 트리거: 집 근처까지 걸으면 HouseScene
        if (State == HighwayState.CorpseRoad && playerTransform != null)
        {
            if (playerTransform.position.z > 320f)
                SetState(HighwayState.HouseScene);
        }

        // HouseScene 트리거: 의자에 가까이 가면 MonsterFight
        if (State == HighwayState.HouseScene && playerTransform != null && seg_House != null)
        {
            var chair = seg_House.transform.Find("Chair");
            if (chair != null && Vector3.Distance(playerTransform.position, chair.position) < 3.5f)
                SetState(HighwayState.MonsterFight);
        }
    }

    static readonly System.Collections.Generic.HashSet<HighwayState> _cinematicStates =
        new System.Collections.Generic.HashSet<HighwayState>
        {
            HighwayState.AntKilling, HighwayState.WomanCutscene,
            HighwayState.Room1, HighwayState.BodyScene, HighwayState.HospitalScene,
            HighwayState.BloodyRoad, HighwayState.DrawerScene, HighwayState.CorpseRoad,
            HighwayState.HouseScene, HighwayState.MonsterFight, HighwayState.Ending
        };

    public void SetState(HighwayState next)
    {
        if (_cinematicStates.Contains(next) && Director.Instance != null)
            StartCoroutine(TransitionTo(next));
        else
            ApplyState(next);
    }

    IEnumerator TransitionTo(HighwayState next)
    {
        yield return StartCoroutine(Director.Instance.FadeOut(0.4f));
        ApplyState(next);
        yield return new WaitForSecondsRealtime(0.15f);

        if (next == HighwayState.Ending) yield break;

        if (next == HighwayState.AntKilling)
        {
            Director.Instance.SetOverlayColor(Color.white, 1f);
            yield return new WaitForSecondsRealtime(0.02f);
            yield return StartCoroutine(Director.Instance.FadeIn(0.8f));
        }
        else if (next == HighwayState.BloodyRoad)
        {
            Director.Instance.SetOverlayColor(new Color(0.8f, 0f, 0f), 1f);
            yield return new WaitForSecondsRealtime(0.04f);
            yield return StartCoroutine(Director.Instance.FadeIn(0.5f));
        }
        else
        {
            yield return StartCoroutine(Director.Instance.FadeIn(0.6f));
        }
    }

    static float SwayIntensityFor(HighwayState s) => s switch
    {
        HighwayState.Intro          => 1.0f,
        HighwayState.WalkToTaxi     => 1.0f,
        HighwayState.WalkToBody     => 1.0f,
        HighwayState.CameraPickup   => 1.0f,
        HighwayState.Room1          => 1.0f,
        HighwayState.AntKilling     => 1.3f,
        HighwayState.WomanCutscene  => 1.2f,
        HighwayState.WomanDisappear => 1.2f,
        HighwayState.TaxiEncounter  => 1.2f,
        HighwayState.BodyScene      => 1.2f,
        HighwayState.HospitalScene  => 1.1f,
        HighwayState.BloodyRoad     => 1.5f,
        HighwayState.DrawerScene    => 1.2f,
        HighwayState.CorpseRoad     => 1.4f,
        HighwayState.HouseScene     => 1.4f,
        HighwayState.MonsterFight   => 1.8f,
        HighwayState.Ending         => 0.6f,
        _                           => 1.0f,
    };

    void ApplyState(HighwayState next)
    {
        DisableAllSegments();
        State = next;
        _sway?.SetIntensity(SwayIntensityFor(next), 2.5f);

        switch (next)
        {
            case HighwayState.Intro:
                EnableMovement(true);
                Teleport(sp_RoadStart);
                break;

            case HighwayState.AntKilling:
                // 카메라 아래 부드럽게 전환 → 이동 잠금 (코루틴이 먼저 시작되어야 함)
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
                // WomanNPC handles its own sequence via IInteractable
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
                var ci = FindFirstObjectByType<CameraItem>();
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
                var g = FindFirstObjectByType<GunSystem>();
                if (g != null) g.Enable();
                break;
            }
            case HighwayState.HouseScene:
                Teleport(sp_House);
                EnableSeg(seg_House);
                EnableSeg(seg_Monster);  // monster sits in chair, not yet charging
                EnableMovement(true);
                DialogueManager.Instance?.ShowDialogue("...누군가 의자에 앉아 있다.", 3.5f);
                break;

            case HighwayState.MonsterFight:
            {
                EnableSeg(seg_House);  // keep room visible during charge
                EnableSeg(seg_Monster);
                EnableMovement(false);
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
        DisableSeg(seg_Ants);

        // ── Mother silhouette cutscene: still in bright ant lighting ──
        yield return StartCoroutine(MotherSilhouetteSequence());

        // ── Hard cut back to dark road ──
        if (Director.Instance != null)
            yield return StartCoroutine(Director.Instance.FadeOut(0.6f));

        DisableSeg(seg_CutsceneWoman);
        SetAntLighting(false);

        if (playerController != null) playerController.UnlockCamera();

        // Spawn bleeding woman ahead on the road
        if (seg_WomanNPC != null && playerTransform != null)
        {
            Vector3 npcPos = new Vector3(5f, playerTransform.position.y, playerTransform.position.z + 20f);
            seg_WomanNPC.transform.position = npcPos;

            // Disable any prior AI scripts before activating visual
            var aiScripts = seg_WomanNPC.GetComponentsInChildren<UnityEngine.MonoBehaviour>(true);
            foreach (var s in aiScripts)
            {
                string typeName = s.GetType().Name;
                if (typeName.Contains("Wander") || typeName.Contains("AI") || typeName.Contains("Common"))
                    s.enabled = false;
            }

            EnableSeg(seg_WomanNPC);
            ReplaceNpcVisual(seg_WomanNPC.transform);

            // ReplaceNpcVisual之后，新 NPC_Body 会有 WomanNPC 组件，直接获取并启动
            var newWomanNpc = seg_WomanNPC.transform.Find("WomanNPC_Visual/NPC_Body")?.GetComponent<WomanNPC>();
            if (newWomanNpc != null) newWomanNpc.StartSequence();
        }

        if (Director.Instance != null)
            yield return StartCoroutine(Director.Instance.FadeIn(0.8f));

        EnableMovement(true);
        State = HighwayState.Intro;
    }

    // Mother silhouette: camera tilts up, backlit silhouette against bright sky,
    // mother calls out. Keeps AntKilling lighting/camera state intact.
    IEnumerator MotherSilhouetteSequence()
    {
        if (seg_CutsceneWoman == null || playerTransform == null || playerController == null)
            yield break;

        // Position silhouette above and in front of player so upward tilt frames it
        Vector3 silPos = playerTransform.position + playerTransform.forward * 4f + Vector3.up * 0f;
        seg_CutsceneWoman.transform.position = silPos;
        EnableSeg(seg_CutsceneWoman);

        // Tilt camera up toward silhouette's head
        Vector3 headTarget = silPos + Vector3.up * 9.5f;
        playerController.SmoothLookAt(headTarget, 1.2f);
        yield return new WaitForSeconds(1.4f);

        if (DialogueManager.Instance != null)
        {
            yield return StartCoroutine(DialogueManager.Instance.PlayLinesCoroutine(
                "<i>엄마: ...어디 있니.</i>",
                "<i>엄마: 이리 와.</i>",
                "<i>엄마: ...왜 거기 있어.</i>",
                "<i>엄마: 이리 오라니까.</i>"
            ));
        }

        yield return new WaitForSeconds(0.6f);
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

        // 기존 children 모두 제거 (NPC_Body 등은 여기서 파괴)
        foreach (Transform child in visual)
            UnityEngine.Object.Destroy(child.gameObject);

        //旧的 WomanNPC (MakeCapsule으로 만든 원본) 파괴
        var oldNpc = npcRoot.Find("WomanNPC");
        if (oldNpc != null) UnityEngine.Object.Destroy(oldNpc.gameObject);

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

        // WomanNPC 컴포넌트 부착 (IInteractable + CapsuleCollider 공존)
        npc.AddComponent<WomanNPC>();
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
        if (playerController != null) playerController.movementLocked = !on;
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

    IEnumerator EndingRoutine()
    {
        // Slow fade to black, then ending text over it (DialogueCanvas sortingOrder=10000 > Director's 9999)
        if (Director.Instance != null)
            yield return StartCoroutine(Director.Instance.FadeOut(3f));
        else
            yield return new WaitForSeconds(1f);
        DialogueManager.Instance?.ShowDialogue("괴물은 쓰러졌다.");
        yield return new WaitForSeconds(3f);
        DialogueManager.Instance?.ShowDialogue("도로는 다시 조용해졌다.");
        yield return new WaitForSeconds(4f);
        DialogueManager.Instance?.ShowDialogue("[ The Highway  -  END ]");
    }
}
