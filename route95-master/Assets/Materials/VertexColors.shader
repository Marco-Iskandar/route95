Shader "Custom/VertexColors" {
	Properties {
	}
	SubShader {
		Tags { "Queue"="Geometry" }
		Lighting Off

		CGPROGRAM
		#pragma surface surf Lambert

		struct Input {
			float4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = IN.color.rgb;
			o.Alpha = IN.color.a;
		}

		ENDCG
	}
}
