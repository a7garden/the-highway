using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AntSystem : MonoBehaviour
{
    [Header("Ant Settings")]
    public int antCount = 18;
    public float antSpacing = 0.55f;
    public float antSpeed = 0.5f;
    public Vector3 antMoveDir = new Vector3(-1f, 0f, 0f);
    public Vector3 antScale = new Vector3(0.35f, 0.12f, 0.5f);

    [Header("S-Curve")]
    public float sCurveAmplitude = 0.8f;
    public float sCurveFrequency = 1f;

    [Header("Ant Model")]
    public GameObject antPrefab;

    private int killCount = 0;
    private List<AntBehavior> ants = new List<AntBehavior>();
    private bool triggered = false;
    private Camera mainCam;
    private AntBehavior hoveredAnt = null;

    void OnEnable()
    {
        killCount = 0; triggered = false;
        mainCam = Camera.main;
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
        Vector3 lateralDir = Vector3.Cross(marchDir, Vector3.up).normalized;
        Vector3 origin = transform.position;
        Material fallbackMat = null;
        if (antPrefab == null)
        {
            fallbackMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            fallbackMat.color = new Color(0.05f, 0.03f, 0.02f);
        }
        for (int i = 0; i < antCount; i++)
        {
            float t = (float)i / Mathf.Max(antCount-1,1);
            float lat = Mathf.Sin(t * sCurveFrequency * Mathf.PI * 2f) * sCurveAmplitude;
            Vector3 pos = origin + marchDir*(i*antSpacing) + lateralDir*lat;
            pos.y = origin.y;
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
            ants.Add(ab);
        }
    }

    void Update()
    {
        if (mainCam==null) return;
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
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
        if (Input.GetMouseButtonDown(0) && hoveredAnt!=null && !hoveredAnt.IsDead)
        { hoveredAnt.Kill(); hoveredAnt=null; }
    }

    public void OnAntKilled()
    {
        killCount++;
        if (killCount>=5 && !triggered)
        { triggered=true; DialogueManager.Instance?.ShowDialogue("..?"); Invoke("TriggerCutscene",1.5f); }
    }

    void TriggerCutscene()
    {
        GameManager.Instance?.RestoreFromAntView();
        GameManager.Instance?.SetState(HighwayState.WomanCutscene);
    }
}
