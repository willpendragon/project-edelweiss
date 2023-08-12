half GetCanvasEffect(v2f i, half4 color)
{
    half4 saturated = saturate((color - _CanvasWeight) * 2);
	half4 canvas = tex2D(_CanvasTex, i.uv + _CanvasOffset);
#ifdef SHADER_API_VULKAN
    return saturate(saturated * canvas * 0.5 + 0.5);
#else
	float4 negative = (1 - saturated) * (1 - canvas);
	return dot(1 - negative, 0.333);
#endif
}