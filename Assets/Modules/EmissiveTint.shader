Shader "Hexi/EmissiveTint" {
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
			float3 viewDir;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half rim = saturate(dot (normalize(IN.viewDir), o.Normal));
			half rim2 = pow(rim, 2);
			o.Albedo = _Color.rgb * (0.8 + rim2 * 0.2);
			o.Emission = _Color.rgb * (0.1 + rim2) * _Color.a;
		}

		ENDCG
	}

	Fallback "KT/Mobile/DiffuseTint"
}
