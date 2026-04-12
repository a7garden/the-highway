Shader "Horror/RetroSurface"
{
    Properties
    {
        _BaseColor      ("Base Color",    Color)   = (1,1,1,1)
        _DarkMultiplier ("Darkness",      Range(0,1)) = 0.6
        _FogColor       ("Fog Color",     Color)   = (0.02, 0.02, 0.05, 1)
        _FogStart       ("Fog Start",     Float)   = 20
        _FogEnd         ("Fog End",       Float)   = 120
        _NoiseScale     ("Noise Scale",   Float)   = 40
        _NoiseStrength  ("Noise Strength",Range(0,0.15)) = 0.04
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float  _DarkMultiplier;
                float4 _FogColor;
                float  _FogStart;
                float  _FogEnd;
                float  _NoiseScale;
                float  _NoiseStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float2 uv         : TEXCOORD2;
            };

            // Simple pseudo-random noise
            float Hash(float2 p)
            {
                p = frac(p * float2(234.34, 435.345));
                p += dot(p, p + 34.23);
                return frac(p.x * p.y);
            }

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vpi = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = vpi.positionCS;
                OUT.positionWS = vpi.positionWS;
                OUT.normalWS   = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv         = IN.uv;
                return OUT;
            }

            float4 Frag(Varyings IN) : SV_Target
            {
                // Base color with darkness
                float3 col = _BaseColor.rgb * _DarkMultiplier;

                // Noise grain for retro texture feel
                float noise = Hash(floor(IN.positionWS.xz / _NoiseScale * 64.0));
                col += (noise - 0.5) * _NoiseStrength;

                // Simple diffuse lighting
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(normalize(IN.normalWS), mainLight.direction));
                col *= (0.3 + 0.7 * NdotL) * mainLight.color;

                // Distance fog
                float dist = length(IN.positionWS - _WorldSpaceCameraPos);
                float fogT  = saturate((dist - _FogStart) / (_FogEnd - _FogStart));
                col = lerp(col, _FogColor.rgb, fogT);

                // Palette quantize (retro)
                col = floor(col * 24.0 + 0.5) / 24.0;

                return float4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
