
// BT470
inline float4
convertYUV(float y, float u, float v)
{
    float rr = saturate(y + 1.402 * (u - 0.5));
    float gg = saturate(y - 0.344 * (v - 0.5) - 0.714 * (u - 0.5));
    float bb = saturate(y + 1.772 * (v - 0.5));
	return float4(rr, gg, bb, 1.0);
}

// BT709-limited
inline float4
convertYUV_BT709_Limited_RGB(float y, float u, float v)
{
	float rr = saturate( 1.164 * (y - (16.0 / 255.0)) + 1.793 * (u - 0.5) );
	float gg = saturate( 1.164 * (y - (16.0 / 255.0)) - 0.534 * (u - 0.5) - 0.213 * (v - 0.5) );
	float bb = saturate( 1.164 * (y - (16.0 / 255.0)) + 2.115 * (v - 0.5) );
	return float4(rr, gg, bb, 1.0);
}

// BT709-full
inline float4
convertYUV_BT709_Full_RGB(float y, float u, float v)
{
	float rr = saturate( y + 1.793 * (u - 0.5) );
	float gg = saturate( y - 0.534 * (u - 0.5) - 0.213 * (v - 0.5) );
	float bb = saturate( y + 2.115 * (v - 0.5) );
	return float4(rr, gg, bb, 1.0);
}

// https://chilliant.blogspot.co.uk/2012/08/srgb-approximations-for-hlsl.html?m=1
inline float
linearToGammaFloatAccurate(float x)
{
	float result;
	if (x <= 0.0031308F)
		result = x * 12.92F;
	else
		result = 1.055F * pow(x, 0.4166667F) - 0.055F;
	return result;
}

inline half3
linearToGammaAccurate(half3 col)
{
	return half3(linearToGammaFloatAccurate(col.r), linearToGammaFloatAccurate(col.g), linearToGammaFloatAccurate(col.b));
}

inline half3
linearToGammaFast(half3 col)
{
	return max(1.055h * pow(col, 0.416666667h) - 0.055h, 0.h);
}

inline float
TransferSRGB_GammaToLinearFloat(float x)
{
	if (x <= 0.04045)
		x = x / 12.92;
	else
		x = pow((x + 0.055) / 1.055, 2.4);
	return x;
}

inline float
TransferRec709_LinearToGammaFloat(float x)
{
	if (x < 0.018)
		x = x * 4.5;
	else
		x = (1.099 * pow(x, 0.45)) - 0.099;
	return x;
}

inline float
TransferRec709_GammaToLinearFloat(float x)
{
	if (x < 0.081)
		x = x / 4.5;
	else
		x = pow((x + 0.099) / 1.099, 2.22222222);
	return x;
}

inline float3
TransferSRGB_GammaToLinear(float3 col)
{
	return float3(TransferSRGB_GammaToLinearFloat(col.r), TransferSRGB_GammaToLinearFloat(col.g), TransferSRGB_GammaToLinearFloat(col.b));
}

inline float3
TransferRec709_GammaToLinear(float3 col)
{
	return float3(TransferRec709_GammaToLinearFloat(col.r), TransferRec709_GammaToLinearFloat(col.g), TransferRec709_GammaToLinearFloat(col.b));
}