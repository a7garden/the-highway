using UnityEngine;
using UnityEditor;

public static class DebrisGenerator
{
    [MenuItem("Horror/Generate Road Debris")]
    public static void Generate()
    {
        GameObject old = GameObject.Find("RoadDebris");
        if (old != null) Object.DestroyImmediate(old);

        GameObject root = new GameObject("RoadDebris");

        Material debrisMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        debrisMat.SetColor("_BaseColor", new Color(0.35f, 0.2f, 0.05f, 1f));
        AssetDatabase.CreateAsset(debrisMat, "Assets/Materials/Mat_Debris.mat");

        Random.InitState(42);

        int count = 300;
        for (int i = 0; i < count; i++)
        {
            float z    = Random.Range(2f, 950f);
            float side = Random.value > 0.5f ? 1f : -1f;
            float x    = side * Random.Range(5f, 50f);
            float sizeX = Random.Range(0.1f, 0.5f);
            float sizeY = Random.Range(0.01f, 0.06f);
            float sizeZ = Random.Range(0.1f, 0.45f);
            float rotY  = Random.Range(0f, 360f);

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Debris";
            go.transform.SetParent(root.transform);
            go.transform.position = new Vector3(x, sizeY * 0.5f, z);
            go.transform.localScale = new Vector3(sizeX, sizeY, sizeZ);
            go.transform.eulerAngles = new Vector3(0, rotY, 0);
            go.GetComponent<Renderer>().sharedMaterial = debrisMat;
            Object.DestroyImmediate(go.GetComponent<BoxCollider>());
        }

        EditorUtility.SetDirty(root);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);
        Debug.Log($"[Horror] Road debris generated: {count} pieces.");
    }
}
