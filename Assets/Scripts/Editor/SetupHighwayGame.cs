using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class SetupHighwayGame
{
    [MenuItem("Horror/Setup Full Game Scene")]
    static void Setup()
    {
        // ── helpers ──────────────────────────────────────────────────
        GameObject MakeGO(string name, Transform parent = null)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent);
            return go;
        }

        GameObject MakeCube(string name, Vector3 pos, Vector3 scale, Color col, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            if (parent != null) go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = scale;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = col;
            go.GetComponent<MeshRenderer>().material = mat;
            return go;
        }

        GameObject MakeCapsule(string name, Vector3 pos, Color col, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            if (parent != null) go.transform.SetParent(parent);
            go.transform.position = pos;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = col;
            go.GetComponent<MeshRenderer>().material = mat;
            return go;
        }

        GameObject MakeTrigger(string name, Vector3 pos, Vector3 size, HighwayState state, Transform parent = null)
        {
            var go = MakeGO(name, parent);
            go.transform.position = pos;
            go.transform.localScale = size;
            var bc = go.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            var wt = go.AddComponent<WalkTrigger>();
            wt.targetState = state;
            return go;
        }

        Transform MakeSpawnPoint(string name, Vector3 pos, Transform parent = null)
        {
            var go = MakeGO(name, parent);
            go.transform.position = pos;
            return go.transform;
        }

        // ── find existing objects ─────────────────────────────────────
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO == null) { Debug.LogError("Player not found!"); return; }
        var playerCam = Camera.main;
        var playerCC = playerGO.GetComponent<CharacterController>();
        var playerCtrl = playerGO.GetComponent<HorrorPlayerController>();

        // ── find existing Road ────────────────────────────────────────
        var roadGO = GameObject.Find("Road");
        MeshRenderer roadRenderer = roadGO != null ? roadGO.GetComponent<MeshRenderer>() : null;

        // ── GameManager ───────────────────────────────────────────────
        var existingGM = Object.FindObjectOfType<GameManager>();
        if (existingGM != null) Object.DestroyImmediate(existingGM.gameObject);

        var gmGO = MakeGO("GameManager");
        var gm = gmGO.AddComponent<GameManager>();
        var bgmSource = gmGO.AddComponent<AudioSource>();
        bgmSource.loop = true; bgmSource.playOnAwake = false; bgmSource.volume = 0.5f;
        gm.bgmSource = bgmSource;
        gm.playerCC = playerCC;
        gm.playerTransform = playerGO.transform;
        gm.playerController = playerCtrl;
        gm.cameraTransform = playerCam != null ? playerCam.transform : null;
        gm.roadRenderer = roadRenderer;

        // blood road material
        var matBlood = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        matBlood.color = new Color(0.4f, 0f, 0f);
        gm.matRoadBloody = matBlood;
        if (roadRenderer != null) gm.matRoadNormal = roadRenderer.sharedMaterial;

        // ── Spawn Points ──────────────────────────────────────────────
        var spawnRoot = MakeGO("SpawnPoints");
        gm.sp_RoadStart  = MakeSpawnPoint("sp_RoadStart",  new Vector3(0, 1, 5),   spawnRoot.transform);
        gm.sp_AfterWoman = MakeSpawnPoint("sp_AfterWoman", new Vector3(0, 1, 110), spawnRoot.transform);
        gm.sp_AfterRoom1 = MakeSpawnPoint("sp_AfterRoom1", new Vector3(0, 1, 160), spawnRoot.transform);
        gm.sp_Room1      = MakeSpawnPoint("sp_Room1",      new Vector3(60, 1, 50), spawnRoot.transform);
        gm.sp_Hospital   = MakeSpawnPoint("sp_Hospital",   new Vector3(120, 1, 50), spawnRoot.transform);
        gm.sp_DrawerRoom = MakeSpawnPoint("sp_DrawerRoom", new Vector3(180, 1, 50), spawnRoot.transform);
        gm.sp_CorpseRoad = MakeSpawnPoint("sp_CorpseRoad", new Vector3(0, 1, 200), spawnRoot.transform);
        gm.sp_House      = MakeSpawnPoint("sp_House",      new Vector3(0, 1, 320), spawnRoot.transform);

        // ── Walk Triggers (on road) ───────────────────────────────────
        var trigRoot = MakeGO("WalkTriggers");
        MakeTrigger("Trigger_AntKilling",   new Vector3(0,1,48),  new Vector3(8,3,2), HighwayState.AntKilling,  trigRoot.transform);
        MakeTrigger("Trigger_TaxiEncounter",new Vector3(0,1,135), new Vector3(8,3,2), HighwayState.TaxiEncounter, trigRoot.transform);
        MakeTrigger("Trigger_BodyScene",    new Vector3(0,1,178), new Vector3(8,3,2), HighwayState.BodyScene,   trigRoot.transform);
        MakeTrigger("Trigger_HouseScene",   new Vector3(0,1,318), new Vector3(8,3,2), HighwayState.HouseScene,  trigRoot.transform);
        MakeTrigger("Trigger_MonsterFight", new Vector3(0,1,358), new Vector3(8,3,2), HighwayState.MonsterFight, trigRoot.transform);

        // ── SEG: Ants ─────────────────────────────────────────────────
        var segAnts = MakeGO("Seg_Ants");
        segAnts.transform.position = new Vector3(2, 0, 55);
        var antSys = segAnts.AddComponent<AntSystem>();
        antSys.antMoveDir = new Vector3(-1f, 0f, 0.05f);
        antSys.antCount = 14;
        gm.seg_Ants = segAnts;

        // ── SEG: CutsceneWoman (dark sphere above player start) ───────
        var segCutWoman = MakeGO("Seg_CutsceneWoman");
        var womanSilhouette = MakeCube("WomanSilhouette",
            new Vector3(0, 8, 8), new Vector3(2f, 2.5f, 0.3f), new Color(0.02f, 0.02f, 0.02f), segCutWoman.transform);
        // head sphere
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "SilhouetteHead";
        head.transform.SetParent(segCutWoman.transform);
        head.transform.position = new Vector3(0, 9.5f, 8f);
        head.transform.localScale = Vector3.one * 1.2f;
        var hmat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        hmat.color = new Color(0.01f, 0.01f, 0.01f);
        head.GetComponent<MeshRenderer>().material = hmat;
        segCutWoman.SetActive(false);
        gm.seg_CutsceneWoman = segCutWoman;

        // ── SEG: WomanNPC ─────────────────────────────────────────────
        var segWoman = MakeGO("Seg_WomanNPC");
        var womanChar = MakeCapsule("WomanNPC", new Vector3(1, 1, 80), new Color(0.6f, 0.1f, 0.15f), segWoman.transform);
        womanChar.AddComponent<WomanNPC>();
        // hair
        var hair = MakeCube("Hair", new Vector3(1, 2.3f, 80), new Vector3(0.7f, 0.4f, 0.7f), Color.black, segWoman.transform);
        segWoman.SetActive(false);
        gm.seg_WomanNPC = segWoman;

        // ── SEG: Taxi ─────────────────────────────────────────────────
        var segTaxi = MakeGO("Seg_Taxi");
        var taxiBody = MakeCube("TaxiBody", new Vector3(1.5f, 1f, 240), new Vector3(2.5f, 1.5f, 5f), new Color(0.9f, 0.75f, 0.1f), segTaxi.transform);
        MakeCube("TaxiTop",  new Vector3(1.5f, 2.2f, 240), new Vector3(2f, 1f, 3f), new Color(0.8f, 0.65f, 0.1f), segTaxi.transform);
        MakeCube("TaxiWheel1", new Vector3(2.8f, 0.3f, 238), new Vector3(0.4f, 0.7f, 0.7f), Color.black, segTaxi.transform);
        MakeCube("TaxiWheel2", new Vector3(0.2f, 0.3f, 238), new Vector3(0.4f, 0.7f, 0.7f), Color.black, segTaxi.transform);
        MakeCube("TaxiWheel3", new Vector3(2.8f, 0.3f, 242), new Vector3(0.4f, 0.7f, 0.7f), Color.black, segTaxi.transform);
        MakeCube("TaxiWheel4", new Vector3(0.2f, 0.3f, 242), new Vector3(0.4f, 0.7f, 0.7f), Color.black, segTaxi.transform);
        var taxiCtrl = segTaxi.AddComponent<TaxiController>();
        taxiCtrl.driveSpeed = 7f; taxiCtrl.stopDist = 5f;
        segTaxi.SetActive(false);
        gm.seg_Taxi = segTaxi;

        // ── SEG: Room1 (TV Room) ──────────────────────────────────────
        var segRoom1 = BuildRoom("Seg_Room1", new Vector3(60, 0, 50), 10, 4, 8, new Color(0.15f, 0.12f, 0.1f));
        // Table
        MakeCube("Table", new Vector3(60, 1f, 52), new Vector3(2f, 0.1f, 1f), new Color(0.3f, 0.2f, 0.1f), segRoom1.transform);
        MakeCube("TableLeg", new Vector3(60, 0.5f, 52), new Vector3(0.1f, 1f, 0.1f), new Color(0.25f, 0.15f, 0.05f), segRoom1.transform);
        // Cigarette on table (pickup)
        var cigGO = MakeCube("Cigarette", new Vector3(60, 1.12f, 52), new Vector3(0.05f, 0.05f, 0.2f), new Color(0.9f, 0.85f, 0.7f), segRoom1.transform);
        var cig = cigGO.AddComponent<PickupItem>();
        cig.itemName = "담배"; cig.triggerState = false;
        // TV screen
        var tvScreen = MakeCube("TVScreen", new Vector3(60, 2.2f, 54.5f), new Vector3(2.5f, 1.5f, 0.15f), Color.black, segRoom1.transform);
        var tvSys = tvScreen.AddComponent<TVSystem>();
        tvSys.screenRenderer = tvScreen.GetComponent<MeshRenderer>();
        segRoom1.SetActive(false);
        gm.seg_Room1 = segRoom1;

        // ── SEG: BodyNPC ──────────────────────────────────────────────
        var segBody = MakeGO("Seg_BodyNPC");
        var bodyChar = MakeCube("BodyNPC", new Vector3(0, 0.5f, 182), new Vector3(0.6f, 0.3f, 1.8f), new Color(0.4f, 0.3f, 0.2f), segBody.transform);
        bodyChar.AddComponent<BodyNPC>();
        // camera pickup appears after body dies
        var camPickupGO = MakeCube("CameraPickup", new Vector3(0.5f, 0.6f, 181), new Vector3(0.4f, 0.3f, 0.6f), new Color(0.2f, 0.2f, 0.2f), segBody.transform);
        var camItem = camPickupGO.AddComponent<CameraItem>();
        camPickupGO.SetActive(false);
        segBody.SetActive(false);
        gm.seg_BodyNPC = segBody;

        // ── SEG: Hospital Room ────────────────────────────────────────
        var segHosp = BuildRoom("Seg_HospitalRoom", new Vector3(120, 0, 50), 8, 3.5f, 8, new Color(0.85f, 0.85f, 0.85f));
        segHosp.AddComponent<HospitalRoom>();
        // Bed
        MakeCube("Bed", new Vector3(120, 0.5f, 50), new Vector3(1.5f, 0.4f, 3f), new Color(0.9f, 0.9f, 0.9f), segHosp.transform);
        // Door
        var hospDoor = MakeCube("HospitalDoor", new Vector3(124, 1.5f, 50), new Vector3(0.15f, 3f, 1.5f), new Color(0.5f, 0.35f, 0.2f), segHosp.transform);
        hospDoor.AddComponent<DoorInteraction>().nextState = HighwayState.BloodyRoad;
        segHosp.SetActive(false);
        gm.seg_HospitalRoom = segHosp;

        // ── SEG: Drawer Room ──────────────────────────────────────────
        var segDrawer = BuildRoom("Seg_DrawerRoom", new Vector3(180, 0, 50), 6, 3, 6, new Color(0.1f, 0.08f, 0.06f));
        segDrawer.AddComponent<DrawerRoom>();
        // Drawer
        var drawerGO = MakeCube("Drawer", new Vector3(180, 1f, 52), new Vector3(1.5f, 1f, 0.5f), new Color(0.35f, 0.22f, 0.1f), segDrawer.transform);
        drawerGO.AddComponent<DrawerInteraction>();
        segDrawer.SetActive(false);
        gm.seg_DrawerRoom = segDrawer;

        // ── SEG: Corpse Road ──────────────────────────────────────────
        var segCorpse = MakeGO("Seg_CorpseRoad");
        for (int i = 0; i < 18; i++)
        {
            float z = 210 + i * 8;
            float x = Random.Range(-3f, 3f);
            MakeCube("Corpse_" + i, new Vector3(x, 0.15f, z),
                new Vector3(0.5f + Random.Range(-0.1f,0.1f), 0.2f, 1.5f + Random.Range(-0.2f,0.2f)),
                new Color(0.3f, 0.1f, 0.05f), segCorpse.transform);
        }
        // Add GunSystem to player
        var gunSys = playerGO.GetComponent<GunSystem>();
        if (gunSys == null) gunSys = playerGO.AddComponent<GunSystem>();
        segCorpse.SetActive(false);
        gm.seg_CorpseRoad = segCorpse;

        // ── SEG: House ────────────────────────────────────────────────
        var segHouse = MakeGO("Seg_House");
        // Exterior walls
        MakeCube("HouseWall_Front", new Vector3(0, 2f, 355), new Vector3(10, 5, 0.3f), new Color(0.3f, 0.28f, 0.25f), segHouse.transform);
        MakeCube("HouseWall_Back",  new Vector3(0, 2f, 362), new Vector3(10, 5, 0.3f), new Color(0.3f, 0.28f, 0.25f), segHouse.transform);
        MakeCube("HouseWall_Left",  new Vector3(-5, 2f, 358.5f), new Vector3(0.3f, 5, 7), new Color(0.3f, 0.28f, 0.25f), segHouse.transform);
        MakeCube("HouseWall_Right", new Vector3(5, 2f, 358.5f),  new Vector3(0.3f, 5, 7), new Color(0.3f, 0.28f, 0.25f), segHouse.transform);
        MakeCube("HouseFloor",      new Vector3(0, 0.05f, 358.5f), new Vector3(10, 0.2f, 7), new Color(0.15f, 0.12f, 0.1f), segHouse.transform);
        // Chair
        MakeCube("Chair",           new Vector3(0, 0.8f, 360), new Vector3(1f, 0.1f, 1f), new Color(0.2f, 0.15f, 0.1f), segHouse.transform);
        MakeCube("ChairBack",       new Vector3(0, 1.6f, 360.5f), new Vector3(1f, 1.5f, 0.1f), new Color(0.2f, 0.15f, 0.1f), segHouse.transform);
        // Door trigger
        var houseDoor = MakeTrigger("HouseDoorTrigger", new Vector3(0,1,355.5f), new Vector3(3,3,1), HighwayState.MonsterFight, segHouse.transform);
        segHouse.SetActive(false);
        gm.seg_House = segHouse;

        // ── SEG: Monster ──────────────────────────────────────────────
        var segMonster = MakeGO("Seg_Monster");
        var monsterBody = MakeCapsule("Monster", new Vector3(0, 1, 361), new Color(0.05f, 0.03f, 0.08f), segMonster.transform);
        monsterBody.transform.localScale = new Vector3(1.4f, 1.8f, 1.4f);
        // glowing red eyes
        MakeCube("EyeL", new Vector3(-0.25f, 2.8f, 360.55f), new Vector3(0.15f, 0.12f, 0.05f), new Color(2f, 0f, 0f), segMonster.transform);
        MakeCube("EyeR", new Vector3( 0.25f, 2.8f, 360.55f), new Vector3(0.15f, 0.12f, 0.05f), new Color(2f, 0f, 0f), segMonster.transform);
        monsterBody.AddComponent<MonsterAI>().chargeSpeed = 5f;
        segMonster.SetActive(false);
        gm.seg_Monster = segMonster;

        // ── Choice UI ─────────────────────────────────────────────────
        var canvas = GameObject.Find("DialogueCanvas");
        if (canvas != null)
        {
            var choicePanel = new GameObject("ChoicePanel");
            choicePanel.transform.SetParent(canvas.transform, false);
            var cpRect = choicePanel.AddComponent<RectTransform>();
            cpRect.anchorMin = new Vector2(0.2f, 0.15f);
            cpRect.anchorMax = new Vector2(0.8f, 0.4f);
            cpRect.offsetMin = cpRect.offsetMax = Vector2.zero;
            var cpImg = choicePanel.AddComponent<Image>();
            cpImg.color = new Color(0, 0, 0, 0.8f);

            // Question text
            var qGO = new GameObject("QuestionText");
            qGO.transform.SetParent(choicePanel.transform, false);
            var qRect = qGO.AddComponent<RectTransform>();
            qRect.anchorMin = new Vector2(0,0.65f); qRect.anchorMax = Vector2.one;
            qRect.offsetMin = qRect.offsetMax = Vector2.zero;
            var qTMP = qGO.AddComponent<TextMeshProUGUI>();
            qTMP.text = ""; qTMP.alignment = TextAlignmentOptions.Center; qTMP.fontSize = 18;

            // Yes button
            var yesGO = new GameObject("BtnYes");
            yesGO.transform.SetParent(choicePanel.transform, false);
            var yRect = yesGO.AddComponent<RectTransform>();
            yRect.anchorMin = new Vector2(0.05f, 0.05f); yRect.anchorMax = new Vector2(0.45f, 0.55f);
            yRect.offsetMin = yRect.offsetMax = Vector2.zero;
            var yImg = yesGO.AddComponent<Image>(); yImg.color = new Color(0.1f, 0.3f, 0.1f);
            var yBtn = yesGO.AddComponent<Button>();
            var yTxtGO = new GameObject("Text"); yTxtGO.transform.SetParent(yesGO.transform, false);
            var yTxtRect = yTxtGO.AddComponent<RectTransform>();
            yTxtRect.anchorMin = Vector2.zero; yTxtRect.anchorMax = Vector2.one;
            yTxtRect.offsetMin = yTxtRect.offsetMax = Vector2.zero;
            var yTMP = yTxtGO.AddComponent<TextMeshProUGUI>();
            yTMP.text = "네"; yTMP.alignment = TextAlignmentOptions.Center; yTMP.fontSize = 16;

            // No button
            var noGO = new GameObject("BtnNo");
            noGO.transform.SetParent(choicePanel.transform, false);
            var nRect = noGO.AddComponent<RectTransform>();
            nRect.anchorMin = new Vector2(0.55f, 0.05f); nRect.anchorMax = new Vector2(0.95f, 0.55f);
            nRect.offsetMin = nRect.offsetMax = Vector2.zero;
            var nImg = noGO.AddComponent<Image>(); nImg.color = new Color(0.3f, 0.1f, 0.1f);
            var nBtn = noGO.AddComponent<Button>();
            var nTxtGO = new GameObject("Text"); nTxtGO.transform.SetParent(noGO.transform, false);
            var nTxtRect = nTxtGO.AddComponent<RectTransform>();
            nTxtRect.anchorMin = Vector2.zero; nTxtRect.anchorMax = Vector2.one;
            nTxtRect.offsetMin = nTxtRect.offsetMax = Vector2.zero;
            var nTMP = nTxtGO.AddComponent<TextMeshProUGUI>();
            nTMP.text = "아니오"; nTMP.alignment = TextAlignmentOptions.Center; nTMP.fontSize = 16;

            var choiceUI = canvas.GetComponent<ChoiceUI>();
            if (choiceUI == null) choiceUI = canvas.AddComponent<ChoiceUI>();
            choiceUI.panel = choicePanel;
            choiceUI.questionText = qTMP;
            choiceUI.btnYes = yBtn;
            choiceUI.btnNo = nBtn;
            choiceUI.btnYesText = yTMP;
            choiceUI.btnNoText = nTMP;
            choicePanel.SetActive(false);
        }

        // ── Cigarette road pickup (after taxi, Yes branch) ────────────
        var roadCig = MakeCube("RoadCigarette", new Vector3(1f, 0.6f, 142), new Vector3(0.05f, 0.05f, 0.2f), new Color(0.9f, 0.85f, 0.7f));
        var roadCigItem = roadCig.AddComponent<PickupItem>();
        roadCigItem.itemName = "담배"; roadCigItem.triggerState = true; roadCigItem.nextState = HighwayState.Room1;
        roadCig.SetActive(false); // activated by TaxiController.SpawnCigarette

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[SetupHighwayGame] Scene setup complete! Press Play to test.");
    }

    static GameObject BuildRoom(string name, Vector3 center, float w, float h, float d, Color wallCol)
    {
        var root = new GameObject(name);
        MakeWall(root, "Floor",  center + new Vector3(0, 0, 0), new Vector3(w, 0.2f, d), wallCol * 0.7f);
        MakeWall(root, "Ceil",   center + new Vector3(0, h, 0), new Vector3(w, 0.2f, d), wallCol * 0.5f);
        MakeWall(root, "WallF",  center + new Vector3(0, h/2, d/2), new Vector3(w, h, 0.2f), wallCol);
        MakeWall(root, "WallB",  center + new Vector3(0, h/2, -d/2), new Vector3(w, h, 0.2f), wallCol);
        MakeWall(root, "WallL",  center + new Vector3(-w/2, h/2, 0), new Vector3(0.2f, h, d), wallCol);
        MakeWall(root, "WallR",  center + new Vector3(w/2, h/2, 0), new Vector3(0.2f, h, d), wallCol);
        return root;
    }

    static void MakeWall(GameObject parent, string n, Vector3 pos, Vector3 scale, Color col)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = n; go.transform.SetParent(parent.transform);
        go.transform.position = pos; go.transform.localScale = scale;
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); mat.color = col;
        go.GetComponent<MeshRenderer>().material = mat;
    }
}
