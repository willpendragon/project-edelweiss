Shader "Hidden/ToonSketch/PostProcessEffects"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			#pragma shader_feature _LOFISHADE_ON
			#pragma shader_feature _COLORCRUSH_ON
			#pragma shader_feature _OUTLINE_ON
			#pragma shader_feature _SKETCH_ON
			#pragma shader_feature _SKETCH_MULTI_ON
			#pragma shader_feature _CANVAS_ON
			
			sampler2D _MainTex;
			uniform float4 _MainTex_TexelSize;

			sampler2D _CameraDepthNormalsTexture;
			sampler2D_float _CameraDepthTexture;

			uniform float _ColorAmount;

#ifdef _LOFISHADE_ON
			uniform float _LoFiShadeOffset;
			uniform float _LoFiShadeThreshold1;
			uniform float _LoFiShadeThreshold2;
			uniform float _LoFiShadeThreshold3;
			uniform float _LoFiShadeThreshold4;
			uniform float _LoFiAmount;
#endif

#ifdef _COLORCRUSH_ON
			uniform sampler2D _ColorCrushTex;
			uniform half _ColorCrushScale;
			uniform half _ColorCrushDim;
			uniform half _ColorCrushOffset;
#endif

#ifdef _OUTLINE_ON
			uniform half _OutlineNormalSensitivity;
			uniform half _OutlineDepthSensitivity;
			uniform half _OutlineMinDistance;
			uniform half _OutlineMaxDistance;
#endif

#ifdef _SKETCH_ON
#ifdef _SKETCH_MULTI_ON
			uniform int _SketchAngles;
#endif
			uniform int _SketchSamples;
			uniform half _SketchDetail;
			uniform half _SketchSharpness;
			uniform half _SketchWeight;
#endif

#if defined(_OUTLINE_ON) || defined(_SKETCH_ON)
			uniform half _OutlineAmount;
#endif

#ifdef _CANVAS_ON
			uniform sampler2D _CanvasTex;
			uniform float2 _CanvasOffset;
			uniform half _CanvasWeight;
			uniform half _CanvasAmount;
#endif

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 screenPos : TEXCOORD1;
			};

			v2f vert (appdata_img v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
			#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1 - o.uv.y;
			#endif
				o.screenPos = ComputeScreenPos(o.vertex);
				return o;
			}

#ifdef _LOFISHADE_ON
			#include "LoFi.cginc"
#endif

#ifdef _COLORCRUSH_ON
			#include "ColorCrush.cginc"
#endif

#ifdef _OUTLINE_ON
			#include "Outline.cginc"
#endif

#ifdef _SKETCH_ON
			#include "Sketch.cginc"
#endif

#ifdef _CANVAS_ON
			#include "Canvas.cginc"
#endif

			fixed4 frag (v2f i) : SV_Target
			{
				half4 color = tex2D(_MainTex, i.uv);
				half3 white = half3(1, 1, 1);
#ifdef _LOFISHADE_ON
				half3 hatch = 1;
				half3 loFi = GetLoFiShading(i, color);
				hatch *= lerp(white, loFi, _LoFiAmount);
#endif
				color.rgb = lerp(white, color.rgb, _ColorAmount);
#ifdef _COLORCRUSH_ON
				color.rgb = GetCrushColor(i, color);
#endif
#ifdef _LOFISHADE_ON
				color.rgb *= hatch;
#endif
#if defined(_OUTLINE_ON) || defined(_SKETCH_ON)
				half lines = 1;
	#ifdef _OUTLINE_ON
				half outlines = GetOutlines(i);
				lines = lerp(white, outlines, _OutlineAmount);
	#endif
	#ifdef _SKETCH_ON
				half sketch = GetSketch(i);
				lines = lerp(white, sketch, _OutlineAmount);
	#endif
				color.rgb *= lines;
#endif
#ifdef _CANVAS_ON
				half3 canvas = GetCanvasEffect(i, color);
				color.rgb *= lerp(white, canvas, _CanvasAmount);
#endif
				return color;
            }
			ENDCG
        }
    }
}