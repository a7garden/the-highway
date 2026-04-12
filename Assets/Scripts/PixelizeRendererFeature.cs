using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelizeRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        [Range(64, 512)]
        public int pixelWidth = 320;
        [Range(0f, 1f)]
        public float ditherStrength = 0.08f;
        [Range(0f, 1f)]
        public float vignetteStrength = 0.55f;
        public Color vignetteColor = new Color(0f, 0f, 0f, 1f);
    }

    public Settings settings = new Settings();
    private PixelizePass _pass;

    public override void Create()
    {
        _pass = new PixelizePass(settings);
        _pass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
            renderer.EnqueuePass(_pass);
    }
}
