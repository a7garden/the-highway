Shader "Hidden/HorrorPostProcess"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            Name "HorrorPostProcess"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // --- Pixelize ---
            int   _EnablePixelize;
            float _PixelWidth;

            // --- Glitch ---
            int   _EnableGlitch;
            float _GlitchIntensity;
            float _GlitchSpeed;
            float _GlitchBandSize;
            float _ChromaticAberration;

            // --- Depth Fog ---
            int   _EnableFog;
            float _FogStart;
            float _FogEnd;
            float4 _FogColor;
            float _FogDensity;

            TEXTURE2D_X_FLOAT(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            // Simple hash noise
            float hashNoise(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float aspect = _ScreenParams.x / max(_ScreenParams.y, 1.0);

                // ===== PIXELIZE =====
                float2 sampleUV = uv;
                if (_EnablePixelize != 0)
                {
                    float pw = max(_PixelWidth, 1.0);
                    float ph = pw / aspect;
                    sampleUV = float2(floor(uv.x * pw) / pw,
                                     floor(uv.y * ph) / ph);
                }

                // ===== GLITCH =====
                float2 finalUV = sampleUV;
                if (_EnableGlitch != 0 && _GlitchIntensity > 0.001)
                {
                    float t       = floor(_Time.y * _GlitchSpeed);
                    float bandIdx = floor(uv.y / max(_GlitchBandSize, 0.001));
                    float r1      = hashNoise(float2(bandIdx, t));
                    float r2      = hashNoise(float2(bandIdx * 3.7 + 1.1, t * 1.3 + 0.5));
                    float on      = step(1.0 - _GlitchIntensity, r1);
                    float shift   = (r2 - 0.5) * 0.12;
                    finalUV.x     = frac(sampleUV.x + on * shift);
                }

                // ===== SAMPLE COLOR (Chromatic Aberration) =====
                float4 col;
                if (_ChromaticAberration > 0.0001)
                {
                    float2 dir = (finalUV - 0.5) * _ChromaticAberration;
                    float r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, finalUV + dir).r;
                    float g = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, finalUV      ).g;
                    float b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, finalUV - dir).b;
                    col = float4(r, g, b, 1.0);
                }
                else
                {
                    col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, finalUV);
                }

                // ===== DEPTH FOG =====
                if (_EnableFog != 0)
                {
                    float rawDepth    = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, sampleUV).r;
                    float linearDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                    float fogRange    = max(_FogEnd - _FogStart, 0.001);
                    float fogT        = saturate((linearDepth - _FogStart) / fogRange) * _FogDensity;
                    col.rgb           = lerp(col.rgb, _FogColor.rgb, saturate(fogT));
                }

                return col;
            }
            ENDHLSL
        }
    }
}
