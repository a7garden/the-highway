using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class PixelizePass : ScriptableRenderPass
{
    private PixelizeRendererFeature.Settings _settings;
    private Material _material;

    private static readonly int PixelWidthID = Shader.PropertyToID("_PixelWidth");
    private static readonly int DitherStrID  = Shader.PropertyToID("_DitherStrength");
    private static readonly int VigStrID     = Shader.PropertyToID("_VignetteStrength");
    private static readonly int VigColorID   = Shader.PropertyToID("_VignetteColor");

    private class PassData
    {
        public TextureHandle src;
        public Material material;
        public float pixelWidth;
        public float ditherStrength;
        public float vignetteStrength;
        public Color vignetteColor;
    }

    public PixelizePass(PixelizeRendererFeature.Settings settings)
    {
        _settings = settings;
        requiresIntermediateTexture = true;
        _material = CoreUtils.CreateEngineMaterial("Hidden/RetroPixelize");
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (_material == null) return;

        var resourceData = frameData.Get<UniversalResourceData>();
        if (resourceData.isActiveTargetBackBuffer) return;

        TextureHandle src = resourceData.activeColorTexture;
        var desc = renderGraph.GetTextureDesc(src);
        desc.name = "_PixelizeTempTex";
        desc.clearBuffer = false;
        TextureHandle tmp = renderGraph.CreateTexture(desc);

        using (var builder = renderGraph.AddRasterRenderPass<PassData>("RetroPixelize", out var passData))
        {
            passData.src              = src;
            passData.material         = _material;
            passData.pixelWidth       = _settings.pixelWidth;
            passData.ditherStrength   = _settings.ditherStrength;
            passData.vignetteStrength = _settings.vignetteStrength;
            passData.vignetteColor    = _settings.vignetteColor;

            builder.UseTexture(src);
            builder.SetRenderAttachment(tmp, 0);
            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
            {
                data.material.SetFloat(PixelWidthID, data.pixelWidth);
                data.material.SetFloat(DitherStrID,  data.ditherStrength);
                data.material.SetFloat(VigStrID,     data.vignetteStrength);
                data.material.SetColor(VigColorID,   data.vignetteColor);
                Blitter.BlitTexture(ctx.cmd, data.src, new Vector4(1, 1, 0, 0), data.material, 0);
            });
        }

        using (var builder = renderGraph.AddRasterRenderPass<PassData>("RetroPixelize_Copy", out var passData))
        {
            passData.src      = tmp;
            passData.material = _material;
            builder.UseTexture(tmp);
            builder.SetRenderAttachment(src, 0);
            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
            {
                Blitter.BlitTexture(ctx.cmd, data.src, new Vector4(1, 1, 0, 0), 0, false);
            });
        }
    }
}
