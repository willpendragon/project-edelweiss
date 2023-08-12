float2 GetLUTUV(half4 color)
{
    // Adjust input based on color space
    color.rgb = saturate(color.rgb);
    if (!IsGammaSpace())
        color.rgb = sqrt(color.rgb);
    // Calculate look up coordinates
	float b = floor(color.b * _ColorCrushDim * _ColorCrushDim);
	float by = floor(b / _ColorCrushDim);
	float bx = floor(b - by * _ColorCrushDim);
	float2 uv = color.rg * _ColorCrushScale + _ColorCrushOffset;
	uv += float2(bx, by) / _ColorCrushDim;
	return uv;
}

half3 GetCrushColor(v2f i, half4 color)
{
    return tex2D(_ColorCrushTex, GetLUTUV(color)).rgb;
}