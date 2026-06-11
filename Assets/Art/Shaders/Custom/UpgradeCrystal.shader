Shader "Custom/UpgradeCrystalShader"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1, 1, 1, 0.5)
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.9
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0

        _ReflectionCubemap("Reflection Cubemap", Cube) = "" {}
        _ReflectionIntensity("Reflection Intensity", Range(0.0, 1.0)) = 1.0

        // OUTLINE
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness("Outline Thickness", Range(0.0, 0.05)) = 0.01
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        // -----------------------------------------------------------------------
        // OUTLINE PASS
        // -----------------------------------------------------------------------
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Front
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineThickness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 posWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                posWS += normalWS * _OutlineThickness;

                output.positionHCS = TransformWorldToHClip(posWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }

            ENDHLSL
        }

        // -----------------------------------------------------------------------
        // MAIN LIT PASS
        // -----------------------------------------------------------------------
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURECUBE(_ReflectionCubemap);
            SAMPLER(sampler_ReflectionCubemap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _Smoothness;
                float _Metallic;
                float _ReflectionIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings o;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                o.positionHCS = TransformWorldToHClip(float4(positionWS, 1));
                o.uv = input.uv;
                o.normalWS = normalWS;
                o.viewDirWS = GetCameraPositionWS() - positionWS;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).rgb * _BaseColor.rgb;
                float alpha = _BaseColor.a;

                float3 N = normalize(i.normalWS);
                float3 V = normalize(i.viewDirWS);
                float3 R = reflect(-V, N);

                float3 reflectionSample =
                    SAMPLE_TEXTURECUBE(_ReflectionCubemap, sampler_ReflectionCubemap, R).rgb *
                    _ReflectionIntensity;

                float3 specColor = (_Metallic > 0)
                    ? lerp(float3(0.04,0.04,0.04), baseColor, _Metallic)
                    : baseColor;

                // Specular highlight
                float spec = pow(max(dot(R, V), 0), _Smoothness * 128);
                float3 litColor = lerp(baseColor, specColor * spec, _Smoothness);

                // Add reflection
                float3 finalColor = lerp(litColor, reflectionSample, _ReflectionIntensity);

                return half4(finalColor, alpha);
            }

            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}