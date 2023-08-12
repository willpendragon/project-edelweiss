float4 GetSketchColor(float2 pos)
{
    float2 uv = pos / _ScreenParams.xy;
    float4 color = tex2D(_MainTex, uv);
    float detail = _SketchDetail;
    if (!IsGammaSpace())
        detail *= 0.1;
    color = min(color, detail);
    return color;
}

float GetSketchValue(float2 pos)
{
    float4 color = GetSketchColor(pos);
    return pow(dot(color, float3(0.333, 0.333, 0.333)), 1.0) * 1.0;
}

float2 GetSketchSample(float2 pos, float shift)
{
	float2 offset = float2(shift, 0);
    return float2(
        GetSketchValue(pos + offset.xy) - GetSketchValue(pos - offset.xy),
        GetSketchValue(pos + offset.yx) - GetSketchValue(pos - offset.yx)
	) / shift / 2.0;
}

half GetSketch(v2f i)
{
    // Get position/offsets
    float2 pos = i.uv * _ScreenParams.xy;
    float offset = _ScreenParams.y / 920.0;
    float shift = lerp(5, 0.5, _SketchSharpness);
    // Set constant values
#ifdef _SKETCH_MULTI_ON
    half angles = _SketchAngles;
#else
    half angles = 4;
#endif
    half samples = _SketchSamples;
	float PI2 = 6.28318530717959;
    float anglePi = PI2 / float(angles);
    // Calculate sketch output
    float3 color1 = float3(0, 0, 0);
    float3 color2 = float3(0, 0, 0);
    float sum = 0.0;
    // Get sample lookup points
#ifdef _SKETCH_MULTI_ON
    for (int i = 0; i < angles; i++)
    {
		float angle = anglePi * (float(i) + 0.8);
		float2 v1 = float2(cos(angle), sin(angle));
#else
    float2 v1 = float2(0.30901699437, 0.95105651629);
    float2 v2 = float2(-0.95105651629, 0.30901699437);
    float2 v3 = float2(-0.30901699437, -0.95105651629);
    float2 v4 = float2(-0.30901699437, 0.95105651629);
#endif
        // Get samples
        for (int j = 0; j < samples; j++)
        {
#ifdef _SKETCH_MULTI_ON
            // Work out sample positions
            float2 pa1 = v1.yx * float2(1, -1) * float(j) * offset;
            float2 pb1 = v1.xy * float(j * j) / float(samples) * 0.5 * offset;
            float2 p1 = pos + shift * pa1 + pb1;
            // Get edge sample
            float2 e1 = GetSketchSample(p1, shift);
            // Calculate colour for this sample
            float ca1 = dot(e1, v1) - 0.5 * abs(dot(e1, v1.yx * float2(1, -1)));
            float cb1 = dot(normalize(e1 + float2(0.0001, 0.0001)), v1.yx * float2(1, -1));
            ca1 = clamp(ca1, 0.0, 0.05);
            cb1 = abs(cb1);
            ca1 *= 1.0 - float(j) / float(samples);
            // Update output
            color1 += ca1;
            color2 += cb1;
            sum += cb1;
#else
            // Work out sample positions
            float2 pa1 = v1.yx * float2(1, -1) * float(j) * offset;
            float2 pb1 = v1.xy * float(j * j) / float(samples) * 0.5 * offset;
            float2 p1 = pos + shift * pa1 + pb1;
            float2 pa2 = v2.yx * float2(1, -1) * float(j) * offset;
            float2 pb2 = v2.xy * float(j * j) / float(samples) * 0.5 * offset;
            float2 p2 = pos + shift * pa2 + pb2;
            float2 pa3 = v3.yx * float2(1, -1) * float(j) * offset;
            float2 pb3 = v3.xy * float(j * j) / float(samples) * 0.5 * offset;
            float2 p3 = pos + shift * pa3 + pb3;
            float2 pa4 = v4.yx * float2(1, -1) * float(j) * offset;
            float2 pb4 = v4.xy * float(j * j) / float(samples) * 0.5 * offset;
            float2 p4 = pos + shift * pa4 + pb4;
            // Get edge sample
            float2 e1 = GetSketchSample(p1, shift);
            float2 e2 = GetSketchSample(p2, shift);
            float2 e3 = GetSketchSample(p3, shift);
            float2 e4 = GetSketchSample(p4, shift);
            // Calculate colour for this sample
            float ca1 = dot(e1, v1) - 0.5 * abs(dot(e1, v1.yx * float2(1, -1)));
            float cb1 = dot(normalize(e1 + float2(0.0001, 0.0001)), v1.yx * float2(1, -1));
            ca1 = clamp(ca1, 0.0, 0.05);
            cb1 = abs(cb1);
            ca1 *= 1.0 - float(j) / float(samples);
            float ca2 = dot(e2, v2) - 0.5 * abs(dot(e2, v2.yx * float2(1, -1)));
            float cb2 = dot(normalize(e2 + float2(0.0001, 0.0001)), v2.yx * float2(1, -1));
            ca2 = clamp(ca2, 0.0, 0.05);
            cb2 = abs(cb2);
            ca2 *= 1.0 - float(j) / float(samples);
            float ca3 = dot(e3, v3) - 0.5 * abs(dot(e3, v3.yx * float2(1, -1)));
            float cb3 = dot(normalize(e3 + float2(0.0001, 0.0001)), v3.yx * float2(1, -1));
            ca3 = clamp(ca3, 0.0, 0.05);
            cb3 = abs(cb3);
            ca3 *= 1.0 - float(j) / float(samples);
            float ca4 = dot(e4, v4) - 0.5 * abs(dot(e4, v4.yx * float2(1, -1)));
            float cb4 = dot(normalize(e4 + float2(0.0001, 0.0001)), v4.yx * float2(1, -1));
            ca4 = clamp(ca4, 0.0, 0.05);
            cb4 = abs(cb4);
            ca4 *= 1.0 - float(j) / float(samples);
            // Update output
            color1 += (ca1 + ca2 + ca3 + ca4);
            color2 += (cb1 + cb2 + cb3 + cb4);
            sum += (cb1 + cb2 + cb3 + cb4);
#endif
        }
#ifdef _SKETCH_MULTI_ON
    }
#endif
    // Get line weight
    float weight = lerp(2, 0.4, _SketchWeight);
    if (!IsGammaSpace())
        weight *= 0.1;
    // Final outlines
    color1 /= float(samples * angles) * weight / sqrt(_ScreenParams.y);
    color2 /= sum;
    color1.r *= 1.6;
    color1.r = 1.0 - color1.r;
    color1.r *= color1.r * color1.r;
    return color1.r * color2;
}