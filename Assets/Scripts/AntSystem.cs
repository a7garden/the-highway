using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AntSystem : MonoBehaviour
{
    [Header("Ant Settings")]
    public int antCount = 14;
    public float antSpacing = 0.35f;
    public float antSpeed = 0.6f;
    public Vector3 antMoveDir = new Vector3(-1f, 0f, 0.1f);
    public Vector3 antScale = new Vector3(0.07f, 0.03f, 0.1f);

    private int killCount = 0;
    private List<GameObject> ants = new List<GameObject>();
    private bool triggered = false;

    void OnEnable()
    {
        killCount = 0; triggered = false;
        SpawnAnts();
    }

    void OnDisable()
    {
        foreach (var a in ants) if (a != null) Destroy(a);
        ants.Clear();
    }

    void SpawnAnts()
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.05f, 0.03f, 0.02f);
        Vector3 dir = antMoveDir.normalized;
        Vector3 startPos = transform.position;

        for (int i = 0; i < antCount; i++)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Ant_" + i;
            go.transform.SetParent(transform);
            go.transform.position = startPos + dir * (i * antSpacing);
            go.transform.localScale = antScale;
            go.transform.forward = dir;
            go.GetComponent<MeshRenderer>().material = mat;
            Destroy(go.GetComponent<BoxCollider>());
            var sc = go.AddComponent<SphereCollider>();
            sc.radius = 1.2f;
            go.AddComponent<AntBehavior>().Setup(this, antSpeed, dir);
            ants.Add(go);
        }
    }

    public void OnAntKilled()
    {
        killCount++;
        if (killCount >= 5 && !triggered)
        {
            triggered = true;
            DialogueManager.Instance?.ShowDialogue("..?");
            Invoke("TriggerCutscene", 1.5f);
        }
    }

    void TriggerCutscene()
    {
        // 개미 구간 종료: 카메라 잠금 해제 + 조명 복구
        GameManager.Instance?.RestoreFromAntView();
        // 다음 상태로 전환
        GameManager.Instance?.SetState(HighwayState.WomanCutscene);
    }
}

public class AntBehavior : MonoBehaviour, IInteractable
{
    private AntSystem sys;
    private float speed;
    private Vector3 dir;
    private bool dead = false;

    public string InteractPrompt { get { return dead ? "" : "밟기"; } }

    public void Setup(AntSystem s, float spd, Vector3 d) { sys = s; speed = spd; dir = d; }

    void Update() { if (!dead) transform.position += dir * speed * Time.deltaTime; }

    public void OnInteract()
    {
        if (dead) return;
        dead = true;
        sys?.OnAntKilled();
        StartCoroutine(DieRoutine());
    }

    System.Collections.IEnumerator DieRoutine()
    {
        float t = 0f;
        Vector3 s0 = transform.localScale;
        while (t < 0.25f) { t += Time.deltaTime; transform.localScale = Vector3.Lerp(s0, Vector3.zero, t / 0.25f); yield return null; }
        Destroy(gameObject);
    }
}
