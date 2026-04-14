using UnityEngine;
using System.Collections;

public class AntBehavior : MonoBehaviour
{
    AntSystem sys; float speed; Vector3 dir; bool dead=false;
    public bool IsDead { get { return dead; } }
    LineRenderer outlineLR;
    const int SEGS=20;
    const float RADIUS=0.28f;

    public void Setup(AntSystem s, float spd, Vector3 d)
    { sys=s; speed=spd; dir=d; BuildOutline(); }

    void BuildOutline()
    {
        var go=new GameObject("AntOutline");
        go.transform.SetParent(transform,false);
        outlineLR=go.AddComponent<LineRenderer>();
        outlineLR.useWorldSpace=true;
        outlineLR.loop=true;
        outlineLR.positionCount=SEGS;
        outlineLR.widthMultiplier=0.03f;
        outlineLR.sortingOrder=10;
        var mat=new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color=Color.white;
        outlineLR.material=mat;
        go.SetActive(false);
    }

    void RefreshOutline()
    {
        if (outlineLR==null) return;
        Vector3 c=transform.position; c.y+=0.05f;
        for (int i=0;i<SEGS;i++)
        {
            float a=(float)i/SEGS*Mathf.PI*2f;
            outlineLR.SetPosition(i, c+new Vector3(Mathf.Cos(a)*RADIUS,0f,Mathf.Sin(a)*RADIUS));
        }
    }

    public void SetHover(bool on)
    {
        if (outlineLR==null) return;
        outlineLR.gameObject.SetActive(on && !dead);
        if (on) RefreshOutline();
    }

    void Update()
    {
        if (!dead) transform.position+=dir*speed*Time.deltaTime;
        if (outlineLR!=null && outlineLR.gameObject.activeSelf) RefreshOutline();
    }

    public void Kill()
    {
        if (dead) return; dead=true; SetHover(false);
        sys?.OnAntKilled(); StartCoroutine(DieRoutine());
    }

    System.Collections.IEnumerator DieRoutine()
    {
        float t=0f; Vector3 s0=transform.localScale;
        while (t<0.25f) { t+=Time.deltaTime; transform.localScale=Vector3.Lerp(s0,Vector3.zero,t/0.25f); yield return null; }
        Destroy(gameObject);
    }
}
