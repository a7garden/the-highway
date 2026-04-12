using UnityEngine;
using UnityEditor;

public static class LaneMarkingGenerator
{
    [MenuItem("Horror/Generate Lane Markings")]
    public static void Generate()
    {
        // 기존 차선 정리
        GameObject old = GameObject.Find("LaneMarkings");
        if (old != null) Object.DestroyImmediate(old);

        GameObject root = new GameObject("LaneMarkings");

        Material whiteMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_LaneDash.mat");
        if (whiteMat == null)
        {
            whiteMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            whiteMat.color = new Color(0.95f, 0.95f, 0.9f);
            AssetDatabase.CreateAsset(whiteMat, "Assets/Materials/Mat_LaneDash.mat");
        }
        whiteMat.SetColor("_BaseColor", new Color(0.95f, 0.95f, 0.9f, 1f));

        float dashLen   = 3f;
        float dashGap   = 7f;
        float dashW     = 0.25f;
        float dashH     = 0.06f;
        float roadLen   = 1000f;
        float laneX     = 3.5f;   // 도로 중심에서 좌우 거리

        int count = Mathf.FloorToInt(roadLen / (dashLen + dashGap));

        for (int i = 0; i < count; i++)
        {
            float z = i * (dashLen + dashGap) + dashLen * 0.5f;

            // 왼쪽 차선
            CreateDash(root, whiteMat, new Vector3(-laneX, dashH, z), dashLen, dashW);
            // 오른쪽 차선
            CreateDash(root, whiteMat, new Vector3( laneX, dashH, z), dashLen, dashW);
        }

        EditorUtility.SetDirty(root);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);
        Debug.Log($"[Horror] Lane markings generated: {count * 2} dashes.");
    }

    static void CreateDash(GameObject parent, Material mat, Vector3 pos, float len, float w)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(w, 0.01f, len);
        go.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(go.GetComponent<BoxCollider>());
    }
}
