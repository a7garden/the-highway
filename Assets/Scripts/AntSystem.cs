using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class AntSystem : MonoBehaviour
{
    [Header("Ant Settings")]
    public int antCount = 18;
    public float antSpacing = 1.5f;
    public float antSpeed = 2f;
    public Vector3 antMoveDir = new Vector3(-1f, 0f, 0f);
    public Vector3 antScale = new Vector3(0.35f, 0.12f, 0.5f);

    [Header("S-Curve")]
    public float sCurveAmplitude = 0.8f;
    public float sCurveFrequency = 1f;

    [Header("Ant Model")]
    public GameObject antPrefab;

    [Header("References")]
    public Transform cameraTransform;
    public Transform playerTransform;

    private int killCount = 0;
    private int passedCount = 0;
    private List<AntBehavior> ants = new List<AntBehavior>();
    private bool triggered = false;
    private Camera mainCam;
    private AntBehavior hoveredAnt = null;

    void OnEnable()
    {
        killCount = 0; passedCount = 0; triggered = false;
        mainCam = Camera.main;
        cameraTransform = mainCam.transform;
        playerTransform = GameManager.Instance != null ? GameManager.Instance.playerTransform : null;
        var old = new List<GameObject>();
        foreach (Transform c in transform) old.Add(c.gameObject);
        foreach (var c in old) Destroy(c);
        ants.Clear();
        SpawnAnts();
    }

    void OnDisable()
    {
        foreach (var a in ants) if (a != null) Destroy(a.gameObject);
        ants.Clear(); hoveredAnt = null;
    }

    void SpawnAnts()
    {
        Vector3 marchDir = antMoveDir.normalized;
        // 플레이어 위치를 기준으로 개미 스폰
        Vector3 playerPos = playerTransform != null ? playerTransform.position : (cameraTransform != null ? cameraTransform.position : transform.position);

        // 플레이어 기준 오른쪽(X+)에서 스폰, 왼쪽으로 이동
        float spawnX = playerPos.x + 5f;
        float baseZ = playerPos.z; // 플레이어와 같은 Z
        float baseY = playerPos.y - 1f; //地面 (플레이어보다 약간 아래)

        Material fallbackMat = null;
        if (antPrefab == null)
        {
            fallbackMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            fallbackMat.color = new Color(0.3f, 0.2f, 0.1f); // 갈색 개미
        }

        for (int i = 0; i < antCount; i++)
        {
            float t = (float)i / Mathf.Max(antCount-1,1);
            float lat = Mathf.Sin(t * sCurveFrequency * Mathf.PI * 2f) * sCurveAmplitude;
            Vector3 pos = new Vector3(spawnX + i * antSpacing, baseY, baseZ + lat);

            GameObject go;
            if (antPrefab != null)
            {
                go = Instantiate(antPrefab);
                go.transform.SetParent(transform);
                go.transform.position = pos;
                go.transform.localScale = antScale;
                go.transform.forward = marchDir;
                if (go.GetComponent<Collider>()==null)
                { var sc=go.AddComponent<SphereCollider>(); sc.radius=0.25f/antScale.x; }
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.SetParent(transform);
                go.transform.position = pos;
                go.transform.localScale = antScale;
                go.transform.forward = marchDir;
                go.GetComponent<MeshRenderer>().material = fallbackMat;
                Destroy(go.GetComponent<BoxCollider>());
                var sc=go.AddComponent<SphereCollider>(); sc.radius=0.25f/antScale.x;
            }
            go.name = "Ant_"+i;
            var ab = go.AddComponent<AntBehavior>();
            ab.Setup(this, antSpeed, marchDir);
            ab.SetPassThreshold(playerPos.x - 10f);
            ants.Add(ab);
        }
    }

    void Update()
    {
        if (mainCam==null) return;
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCam.ScreenPointToRay(mousePos);
        RaycastHit hit;
        AntBehavior newHover = null;
        if (Physics.Raycast(ray, out hit, 300f))
        {
            var ab = hit.collider.GetComponentInParent<AntBehavior>();
            if (ab!=null && !ab.IsDead) newHover=ab;
        }
        if (newHover != hoveredAnt)
        {
            if (hoveredAnt!=null) hoveredAnt.SetHover(false);
            hoveredAnt = newHover;
            if (hoveredAnt!=null) hoveredAnt.SetHover(true);
        }
        if (Mouse.current.leftButton.wasPressedThisFrame && hoveredAnt!=null && !hoveredAnt.IsDead)
        { hoveredAnt.Kill(); hoveredAnt=null; }

        // Check if all ants passed (killed or off-screen)
        if (!triggered && passedCount >= antCount)
        {
            triggered = true;
            EndAntEvent();
        }
    }

    public void OnAntKilled()
    {
        killCount++;
        passedCount++;
    }

    public void OnAntPassed()
    {
        passedCount++;
    }

    void EndAntEvent()
    {
        GameManager.Instance?.EndAntEvent();
    }
}
