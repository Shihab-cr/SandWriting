#include "TessellationLogic.hlsl"
#pragma hull Hull
#pragma domain domain

void ForceTress_float(in float2 uv,in float3 worldPos, in float3 worldNormal, out float Dummy){
	Dummy = 0;
}