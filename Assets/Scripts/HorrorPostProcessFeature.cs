using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class HorrorPostProcessFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class HorrorSettings
    {
        [Header("Pixelize")]
        public bool enablePixelize = true;
        [Range(64, 1920)]
        public int pixelWidth = 320;

        [Header("Glitch")]
        public bool enableGlitch = true;
        [Range(0f, 1f), Tooltip("Glitch band frequency")]
        public float glitchIntensity = 0.25f;
        [Range(0.5f, 30f)]
        public float glitchSpeed = 8f;
        [Range(0.01f, 0.3f)]
        public float glitchBandSize = 0.06f;
        [Range(0f, 0.05f), Tooltip("RGB channel split")]
        public float chromaticAberration = 0.012f;

        [Header("Depth Fog")]
        public bool enableFog = true;
        [Range(0f, 100f)]
        public float fogStart = 3f;
        [Range(1f, 500f)]
        public float fogEnd = 25f;
        [ColorUsage(false, false)]
        public Color fogColor = Color.black;
        [Range(0f, 1f)]
        public float fogDensity = 1f;
    }

    public HorrorSettings settings = new HorrorSettings();
    private HorrorPostProcessPass m_Pass;
    private Material m_Material;

    public override void Create()
    {
        var shader = Shader.Find("Hidden/HorrorPostProcess");
        if (shader == null) { Debug.LogWarning("[HorrorPostProcess] Shader not found!"); return; }
        m_Material = CoreUtils.CreateEngineMaterial(shader);
        m_Pass = new HorrorPostProcessPass(m_Material, settings);
        m_Pass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (m_Material == null || m_Pass == null) return;
        m_Pass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);
        renderer.EnqueuePass(m_Pass);
    }

    protected override void Dispose(bool disposing) { CoreUtils.Destroy(m_Material); }
}

internal class HorrorPostProcessPass : ScriptableRenderPass
{
    private readonly Material m_Mat;
    private readonly HorrorPostProcessFeature.HorrorSettings m_Cfg;

    static readonly int K_EnablePx  = Shader.PropertyToID("_EnablePixelize");
    static readonly int K_PxWidth   = Shader.PropertyToID("_PixelWidth");
    static readonly int K_EnableGl  = Shader.PropertyToID("_EnableGlitch");
    static readonly int K_GlInt     = Shader.PropertyToID("_GlitchIntensity");
    static readonly int K_GlSpeed   = Shader.PropertyToID("_GlitchSpeed");
    static readonly int K_GlBand    = Shader.PropertyToID("_GlitchBandSize");
    static readonly int K_CA        = Shader.PropertyToID("_ChromaticAberration");
    static readonly int K_EnableFg  = Shader.PropertyToID("_EnableFog");
    static readonly int K_FgStart   = Shader.PropertyToID("_FogStart");
    static readonly int K_FgEnd     = Shader.PropertyToID("_FogEnd");
    static readonly int K_FgColor   = Shader.PropertyToID("_FogColor");
    static readonly int K_FgDensity = Shader.PropertyToID("_FogDensity");
    static readonly int K_DepthTex  = Shader.PropertyToID("_CameraDepthTexture");

    class PassData { public TextureHandle src, depth; public Material mat; public bool hasDepth; }

    public HorrorPostProcessPass(Material mat, HorrorPostProcessFeature.HorrorSettings cfg)
    { m_Mat = mat; m_Cfg = cfg; requiresIntermediateTexture = true; }

    void SyncMat()
    {
        m_Mat.SetInt  (K_EnablePx,  m_Cfg.enablePixelize ? 1 : 0);
        m_Mat.SetFloat(K_PxWidth,   m_Cfg.pixelWidth);
        m_Mat.SetInt  (K_EnableGl,  m_Cfg.enableGlitch ? 1 : 0);
        m_Mat.SetFloat(K_GlInt,     m_Cfg.glitchIntensity);
        m_Mat.SetFloat(K_GlSpeed,   m_Cfg.glitchSpeed);
        m_Mat.SetFloat(K_GlBand,    m_Cfg.glitchBandSize);
        m_Mat.SetFloat(K_CA,        m_Cfg.chromaticAberration);
        m_Mat.SetInt  (K_EnableFg,  m_Cfg.enableFog ? 1 : 0);
        m_Mat.SetFloat(K_FgStart,   m_Cfg.fogStart);
        m_Mat.SetFloat(K_FgEnd,     m_Cfg.fogEnd);
        m_Mat.SetColor(K_FgColor,   m_Cfg.fogColor);
        m_Mat.SetFloat(K_FgDensity, m_Cfg.fogDensity);
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var res = frameData.Get<UniversalResourceData>();
        var cam = frameData.Get<UniversalCameraData>();
        if (cam.isPreviewCamera || m_Mat == null) return;

        SyncMat();

        TextureHandle src = res.activeColorTexture;
        if (!src.IsValid()) return;

        var desc = renderGraph.GetTextureDesc(src);
        desc.name = "_HorrorTmp"; desc.depthBufferBits = 0; desc.clearBuffer = false;
        TextureHandle dst = renderGraph.CreateTexture(desc);

        TextureHandle dtex = res.cameraDepth;
        bool useDep = m_Cfg.enableFog && dtex.IsValid();
        var sb = new Vector4(1, 1, 0, 0);

        // Pass A: apply effects (source -> dst)
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Horror_FX", out var pd))
        {
            pd.src = src; pd.mat = m_Mat; pd.hasDepth = useDep;
            builder.UseTexture(src, AccessFlags.Read);
            if (useDep) { pd.depth = dtex; builder.UseTexture(dtex, AccessFlags.Read); }
            builder.SetRenderAttachment(dst, 0, AccessFlags.Write);
            builder.AllowPassCulling(false);
            builder.AllowGlobalStateModification(true);  // required for SetGlobalTexture
            builder.SetRenderFunc((PassData d, RasterGraphContext c) =>
            {
                if (d.hasDepth) c.cmd.SetGlobalTexture(K_DepthTex, d.depth);
                Blitter.BlitTexture(c.cmd, d.src, sb, d.mat, 0);
            });
        }

        // Pass B: copy dst back to active color
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Horror_Back", out var pd))
        {
            pd.src = dst;
            builder.UseTexture(dst, AccessFlags.Read);
            builder.SetRenderAttachment(src, 0, AccessFlags.Write);
            builder.AllowPassCulling(false);
            builder.SetRenderFunc((PassData d, RasterGraphContext c) =>
            {
                Blitter.BlitTexture(c.cmd, d.src, sb, 0.0f, false);
            });
        }
    }
}
