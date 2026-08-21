Shader "Custom/TileVortexDissolve"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (0.2, 0.8, 0.2, 1)

        _DissolveAmount("Dissolve Amount", Range(0,1)) = 0
        _NoiseScale("Noise Scale", Float) = 6
        _VortexStrength("Vortex Strength", Float) = 3
        _VortexCenter("Vortex Center", Vector) = (0.5, 0.5, 0, 0)

        _EdgeWidth("Edge Width", Range(0.001,0.3)) = 0.08
        [HDR] _EdgeColor("Edge Color", Color) = (1, 0.8, 0, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _DissolveAmount;
                float _NoiseScale;
                float _VortexStrength;
                float4 _VortexCenter;
                float _EdgeWidth;
                float4 _EdgeColor;
            CBUFFER_END

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);

                return frac(p.x * p.y);
            }

            float Noise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);

                f = f * f * (3.0 - 2.0 * f);

                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);

                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                float2 centeredUV = uv - _VortexCenter.xy;

                float distanceFromCenter = length(centeredUV);

                float angle = atan2(centeredUV.y, centeredUV.x);

                float swirlAmount = (1.0 - saturate(distanceFromCenter * 1.5)) * _VortexStrength * _DissolveAmount;

                angle += swirlAmount;

                float2 swirledUV = float2(cos(angle), sin(angle)) * distanceFromCenter + _VortexCenter.xy;

                float noise = Noise(swirledUV * _NoiseScale);

                float vortexMask = saturate(distanceFromCenter + noise * 0.35);

                float dissolveValue = _DissolveAmount * 1.35;

                float visible = step(dissolveValue, vortexMask);

                float edge = smoothstep(dissolveValue, dissolveValue + _EdgeWidth, vortexMask) - visible;

                float3 finalColor =lerp(_EdgeColor.rgb, _BaseColor.rgb, edge);

                float finalAlpha = visible + edge;

                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}