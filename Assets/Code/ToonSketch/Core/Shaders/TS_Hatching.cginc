#ifndef TOONSKETCH_HATCHING_INCLUDED
#define TOONSKETCH_HATCHING_INCLUDED

#include "../../Shared/Shaders/TS_Main.cginc"
#include "../../Shared/Shaders/TS_Lighting.cginc"

#define TS_VERTEXFORWARDBASE_SHADINGPASS TS_HatchShading
#define TS_VERTEXFORWARDADD_SHADINGPASS TS_HatchShading

sampler2D	_Hatch0, _Hatch1, _Hatch2, _Hatch3, _Hatch4, _Hatch5;
half		_HatchScale;
half		_HatchThreshold;
half		_HatchStrength;
int			_HatchUVs;

half3 TS_GetHatchShade(FragmentCommonData s, TS_LightingData l)
{
	// Luminance
	half lumAmbient = Luminance(l.indirect.diffuse);
	half lumMain = Luminance(l.light.color);
	// Color
	half3 output = (half3(1, 1, 1) + l.ambientSpec) * lumAmbient * TS_GetRamp(l.ambientDiff);
	output += (half3(1, 1, 1) + l.specular) * lumMain * TS_GetRamp(l.diffuse);
#ifdef _TS_RIMLIGHT_ON
	if (_RimType == 0)
		output += l.rim;
	else if (_RimType == 1)
		output = lerp(output, output * l.rim, l.rim);
#endif
	output *= TS_GetRamp(l.attenuation);
	return output;
}

half3 TS_GetHatchMask(half light, float2 uv)
{
	half3 output;
	if (light > 0.9)
		output = half3(1, 1, 1);
	else if (light > 0.75)
		output = tex2D(_Hatch0, uv);
	else if (light > 0.6)
		output = tex2D(_Hatch1, uv);
	else if (light > 0.45)
		output = tex2D(_Hatch2, uv);
	else if (light > 0.3)
		output = tex2D(_Hatch3, uv);
	else if (light > 0.15)
		output = tex2D(_Hatch4, uv);
	else
		output = tex2D(_Hatch5, uv);
	return 1 - output;
}

half4 TS_HatchShading(FragmentCommonData s, TS_LightingData l)
{
	// UV
	float2 uv;
	if (_HatchUVs == 0)
		uv = l.uv * _HatchScale;
	else if (_HatchUVs == 1)
		uv = s.posWorld.xy * _HatchScale;
	else if (_HatchUVs == 2)
		uv = s.posWorld.xz * _HatchScale;
	// Lighting
	half light = TS_GetHatchShade(s, l) * (2 * _HatchThreshold);
	// Shading
	half3 hatching = TS_GetHatchMask(light, uv);
	half3 sketch = saturate(hatching * hatching) * _HatchStrength;
	// Color
	half4 color;
#ifdef UNITY_PASS_FORWARDBASE
	color.rgb = l.color - l.color * sketch;
#else
	color.rgb = l.color * (1.0 - sketch);
#endif
	color.a = l.alpha;
	// Alpha/Attenuation
#ifndef UNITY_PASS_FORWARDBASE
	color *= l.attenuation;
#if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
	color *= s.alpha;
#endif
#endif
	// Output
	return color;
}

#endif