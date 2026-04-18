using UnityEngine;
using UnityEditor;

public class GameDebugWindow : EditorWindow
{
    private static readonly (string label, string desc, HighwayState state)[] Events =
    {
        ("① 도로 시작",         "게임 시작 지점 — 플레이어 sp_RoadStart",                  HighwayState.Intro),
        ("② 개미 잡기",         "sp_AntView 이동, 카메라 바닥 고정, 밝은 햇빛",             HighwayState.AntKilling),
        ("③ 여자 컷씬",         "카메라 -75° 틸트, 여자 NPC 등장 연출",                     HighwayState.WomanCutscene),
        ("④ 여자 NPC 사라짐",   "플레이어 접근 → 5초 응시 → NPC 시야 밖으로 사라짐",            HighwayState.WomanDisappear),
        ("⑤ 도로 걷기 (택시前)", "sp_AfterWoman — 택시 트리거 향해 걷기",                    HighwayState.WalkToTaxi),
        ("⑥ 택시 기사 대화",    "car-taxi 달려옴, '여자 봤냐' 선택지",                      HighwayState.TaxiEncounter),
        ("⑦ 방 (TV 장면)",     "sp_Room1, TV 채널 4 → 도로 영상 → 다음 이동",              HighwayState.Room1),
        ("⑧ 도로 걷기 (시체前)", "sp_AfterRoom1 — 시체 트리거 향해 걷기",                    HighwayState.WalkToBody),
        ("⑨ 죽어가는 사람",     "BodyNPC 쓰러짐 연출 → 클릭 → 카메라 픽업",                HighwayState.BodyScene),
        ("⑩ 카메라 줍기",       "CameraItem 활성화, 우클릭 줌 / 좌클릭 촬영",               HighwayState.CameraPickup),
        ("⑪ 병실 사진 보기",    "sp_Hospital, 앨범·아이 사진 대화 시퀀스",                  HighwayState.HospitalScene),
        ("⑫ 피묻은 도로",       "도로 머티리얼 → 피, sp_AfterRoom1 복귀 걷기",              HighwayState.BloodyRoad),
        ("⑬ 서랍 (총 꺼내기)", "sp_DrawerRoom, 서랍 열면 Pistol1_01 등장",                 HighwayState.DrawerScene),
        ("⑭ 도로 시체 보기",    "sp_CorpseRoad, 총 소지 상태로 걷기",                       HighwayState.CorpseRoad),
        ("⑮ 집 입장",          "sp_House, seg_House 활성화",                                HighwayState.HouseScene),
        ("⑯ 괴물 전투",        "MonsterAI 돌진 시작, 총으로 격파",                          HighwayState.MonsterFight),
        ("⑰ 엔딩",             "\"도로는 다시 조용해졌다.\"  자막 시퀀스",                  HighwayState.Ending),
    };

    private Vector2 _scroll;
    private static GUIStyle _rowNormal;
    private static GUIStyle _rowActive;
    private static GUIStyle _btnStyle;
    private static GUIStyle _descStyle;
    private static GUIStyle _headerStyle;

    [MenuItem("Horror/\U0001f3ae Game Debug Panel")]
    public static void ShowWindow()
    {
        var win = GetWindow<GameDebugWindow>("\U0001f3ae Game Debug");
        win.minSize = new UnityEngine.Vector2(340, 540);
    }

    void InitStyles()
    {
        if (_rowNormal != null) return;

        _rowNormal = new GUIStyle(GUI.skin.box);
        _rowNormal.padding = new RectOffset(6, 6, 5, 5);
        _rowNormal.margin  = new RectOffset(2, 2, 1, 1);

        _rowActive = new GUIStyle(_rowNormal);
        var activeTex = new Texture2D(1, 1);
        activeTex.SetPixel(0, 0, new Color(0.1f, 0.35f, 0.1f, 0.95f));
        activeTex.Apply();
        _rowActive.normal.background = activeTex;

        _btnStyle = new GUIStyle(GUI.skin.button);
        _btnStyle.alignment  = TextAnchor.MiddleLeft;
        _btnStyle.fontStyle  = FontStyle.Bold;
        _btnStyle.fontSize   = 12;
        _btnStyle.fixedHeight = 26;

        _descStyle = new GUIStyle(EditorStyles.miniLabel);
        _descStyle.wordWrap = true;
        _descStyle.normal.textColor = new Color(0.72f, 0.72f, 0.72f);

        _headerStyle = new GUIStyle(EditorStyles.boldLabel);
        _headerStyle.fontSize = 13;
    }

    void OnGUI()
    {
        InitStyles();

        bool inPlay = Application.isPlaying;
        GameManager gm = inPlay ? GameManager.Instance : null;
        HighwayState cur = (gm != null) ? gm.State : HighwayState.Intro;
        Color prevCol = GUI.color;

        // ── 헤더 ──
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("The Highway  |  이벤트 편집 패널", _headerStyle);
        GUI.color = inPlay ? new Color(0.5f, 1f, 0.5f) : new Color(1f, 0.65f, 0.2f);
        EditorGUILayout.LabelField(inPlay
            ? $"▶ Play 중   현재 상태: {cur}"
            : "⏸ 에디터 모드 — Play 후 버튼 활성화",
            EditorStyles.miniLabel);
        GUI.color = prevCol;
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("클릭 즉시 해당 구간으로 이동합니다.", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.Space(6);

        // ── 이벤트 목록 ──
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        for (int i = 0; i < Events.Length; i++)
        {
            var (label, desc, state) = Events[i];
            bool isCurrent = inPlay && cur == state;
            var rowStyle = isCurrent ? _rowActive : _rowNormal;

            EditorGUILayout.BeginVertical(rowStyle);
            EditorGUILayout.BeginHorizontal();

            // 이동 버튼
            GUI.enabled = inPlay;
            GUI.color = isCurrent ? Color.green : (inPlay ? Color.white : new Color(0.55f, 0.55f, 0.55f));
            if (GUILayout.Button(label, _btnStyle, GUILayout.Width(200)))
            {
                if (gm != null)
                {
                    gm.SetState(state);
                    Debug.Log($"[GameDebug] SetState → {state}");
                }
            }
            GUI.color = prevCol;
            GUI.enabled = true;

            if (isCurrent)
                GUILayout.Label("◀ 현재", EditorStyles.boldLabel);
            else
                GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(desc, _descStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.Space(6);

        // ── 하단 유틸 ──
        EditorGUILayout.BeginHorizontal();
        if (!inPlay)
        {
            if (GUILayout.Button("▶  Play 시작", EditorStyles.miniButton))
                EditorApplication.isPlaying = true;
        }
        else
        {
            if (GUILayout.Button("■  Play 종료", EditorStyles.miniButton))
                EditorApplication.isPlaying = false;
        }
        if (GUILayout.Button("↺  새로고침", EditorStyles.miniButton))
            Repaint();
        EditorGUILayout.EndHorizontal();

        if (inPlay) Repaint();
    }
}
