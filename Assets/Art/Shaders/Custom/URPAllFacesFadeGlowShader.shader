Shader "Custom/URPAllFacesFadeGlowShader"
{
    Properties
    {
        _Color("Glow Color", Color) = (1,1,1,1)
        _GlowIntensity("Glow Intensity", Range(0,1)) = 1.0
        _FadeHeight("Fade Height", Float) = 0.2
    }
        SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha One
            ZWrite Off

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
                float3 worldPos : TEXCOORD2;
            };

            float4 _Color;
            float _GlowIntensity;
            float _FadeHeight;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float4 pos = IN.positionOS;
                OUT.positionHCS = TransformObjectToHClip(pos);
                OUT.uv = IN.uv;
                OUT.worldPos = TransformObjectToWorld(pos).xyz; // Use world position for uniform effect
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = _Color;
                float gradient = saturate(IN.worldPos.y / _FadeHeight); // Use world position for gradient
                color.a = lerp(_GlowIntensity, 0.0, gradient);
                return color;
            }
            ENDHLSL
        }
    }
        FallBack "Diffuse"
}
