Shader "Hidden/RetroPixelize"
{
    Properties
    {
        _MainTex        ("Source", 2D)    = "white" {}
        _PixelWidth     ("Pixel Width",   Float) = 320
        _DitherStrength ("Dither",        Float) = 0.08
        _VignetteStrength("Vignette",     Float) = 0.55
        _VignetteColor  ("Vignette Color",Color) = (0,0,0,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            Name "RetroPixelize"
            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;
                float  _PixelWidth;
                float  _DitherStrength;
                float  _VignetteStrength;
                float4 _VignetteColor;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float Bayer4(float2 p)
            {
                const float bayer[16] = {
                     0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
                    12.0/16.0,  4.0/16.0, 14.0/16.0,  6.0/16.0,
                     3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
                    15.0/16.0,  7.0/16.0, 13.0/16.0,  5.0/16.0
                };
                int2 idx = int2(fmod(p.x, 4), fmod(p.y, 4));
                return bayer[idx.y * 4 + idx.x];
            }

            float4 Frag(Varyings IN) : SV_Target
            {
                float aspect = _MainTex_TexelSize.z / _MainTex_TexelSize.w;
                float pixH = _PixelWidth / aspect;

                float2 pixUV;
                pixUV.x = floor(IN.uv.x * _PixelWidth) / _PixelWidth;
                pixUV.y = floor(IN.uv.y * pixH)        / pixH;

                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, pixUV);

                float2 screenPx = IN.uv * float2(_PixelWidth, pixH);
                float  dither   = Bayer4(floor(screenPx)) - 0.5;
                col.rgb += dither * _DitherStrength;

                col.rgb = floor(col.rgb * 32.0 + 0.5) / 32.0;
                col.rgb *= 0.75;

                float2 vig = IN.uv * 2.0 - 1.0;
                float  vigFactor = 1.0 - dot(vig, vig) * _VignetteStrength;
                vigFactor = saturate(vigFactor);
                col.rgb = lerp(_VignetteColor.rgb, col.rgb, vigFactor);

                return col;
            }
            ENDHLSL
        }
    }
}
