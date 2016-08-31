// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Mobile/HalfUnlit" {
Properties {
	_Color ("Tint", Color) = (1,1,1,1)
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 150

CGPROGRAM
#pragma surface surf Lambert noforwardadd

fixed4 _Color;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = _Color * 0.5 + unity_4LightAtten0 * _Color;
	o.Albedo = c.rgb;
	o.Alpha = c.a;
}

ENDCG
}

Fallback "Mobile/DiffuseTint"
}
