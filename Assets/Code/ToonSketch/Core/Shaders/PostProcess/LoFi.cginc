half3 GetLoFiShading(v2f i, half4 color)
{
	half intensity = saturate(color.rgb);
	half3 hatch = half3(1.0, 1.0, 1.0);
	float2 pos = i.screenPos * _ScreenParams.xy;
    if (intensity < _LoFiShadeThreshold1)
    {
	    if (fmod(floor(pos.x + pos.y), 10.0) == 0)
		    hatch = half3(0.0, 0.0, 0.0);
    }
    if (intensity < _LoFiShadeThreshold2)
    {
	    if (fmod(floor(pos.x - pos.y), 10.0) == 0)
		    hatch = half3(0.0, 0.0, 0.0);
    }
    if (intensity < _LoFiShadeThreshold3)
    {
	    if (fmod(floor(pos.x + pos.y - _LoFiShadeOffset), 10.0) == 0.0)
		    hatch = half3(0.0, 0.0, 0.0);
    }
    if (intensity < _LoFiShadeThreshold4)
    {
	    if (fmod(floor(pos.x - pos.y - _LoFiShadeOffset), 10.0) == 0.0)
		    hatch = half3(0.0, 0.0, 0.0);
    }
    return hatch;
}