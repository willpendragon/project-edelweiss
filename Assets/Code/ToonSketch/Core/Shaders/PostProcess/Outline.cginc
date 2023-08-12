half CheckSame(half2 normal, half depth, half4 sample)
{
	// Check difference in normal
	half2 diffNormal = abs(normal - sample.xy) * _OutlineNormalSensitivity;
	half sameNormal = (diffNormal.x + diffNormal.y) * _OutlineNormalSensitivity < 0.1;
	// Check difference in depth
	half diffDepth = abs(depth - DecodeFloatRG(sample.zw));
	half sameDepth = diffDepth * _OutlineDepthSensitivity < 0.09 * depth;
	// Return result
	return sameNormal * sameDepth;
}

half GetOutlines(v2f i)
{
	// Get center depth value
	half depth = normalize(tex2D(_CameraDepthTexture, i.uv));
	// Get sample distance value based on depth
	float sampleDistance = lerp(_OutlineMinDistance, _OutlineMaxDistance, depth);
	// Get depth samples
	float2 uvDist = sampleDistance * _MainTex_TexelSize.xy;
	half4 sample1 = tex2D(_CameraDepthNormalsTexture, i.uv + uvDist * float2(1, 1));
	half4 sample2 = tex2D(_CameraDepthNormalsTexture, i.uv + uvDist * float2(-1, -1));
	half4 sample3 = tex2D(_CameraDepthNormalsTexture, i.uv + uvDist * float2(-1, 1));
	half4 sample4 = tex2D(_CameraDepthNormalsTexture, i.uv + uvDist * float2(1, -1));
	// Calculate outlines
	half outlines = 1.0;
	outlines *= CheckSame(sample1.xy, DecodeFloatRG(sample1.zw), sample2);
	outlines *= CheckSame(sample3.xy, DecodeFloatRG(sample3.zw), sample4);
	return outlines;
}