Shader "Custom/VertexBlend" {

	Properties{
		_Normal("Normal", Range(0.01,1)) = 0.5
		_Metallic("Metallic", Range(0.01, 1)) = 0.5
		_Smoothness("Smoothness", Range(0,1)) = 0
		_CliffTexture ("CliffTexture", 2D) = ""
		_CliffBump ("CliffBump", 2D) = ""
		_Texture1 ("Texture1", 2D) = ""
		_SurfaceBump ("SurfaceBump", 2D) = ""
		_Texture2 ("Texture2", 2D) = ""
		_Texture3 ("Texture3", 2D) = ""
	}

	Subshader {
		Tags { "RenderType"="Opaque" }

		CGPROGRAM
		#pragma surface surf Standard
		#pragma target 3.0
		struct Input {
			float2 uv_CliffTexture;
			float2 uv_CliffBump;
			float2 uv_Texture1;
			float2 uv_SurfaceBump;
			float2 uv_Texture2;
			float2 uv_Texture3;
			float4 color : COLOR;
		};

		sampler2D _CliffTexture;
		sampler2D _CliffBump;
		sampler2D _Texture1;
		sampler2D _SurfaceBump;
		sampler2D _Texture2;
		sampler2D _Bump2;
		sampler2D _Texture3;
		sampler2D _Bump3;

		float _Normal;
		float _Metallic;
		float _Smoothness;

		void surf (Input IN, inout SurfaceOutputStandard o) {
	
			fixed4 cliff = tex2D(_CliffTexture, IN.uv_CliffTexture);
			fixed4 tex1 = tex2D (_Texture1, IN.uv_Texture1);
			fixed4 tex2 = tex2D (_Texture2, IN.uv_Texture2);
			fixed4 tex3 = tex2D(_Texture3, IN.uv_Texture3);

			fixed alpha = IN.color.a;

			fixed r = (IN.color.r * tex1.r * tex1.r) + (IN.color.g * tex2.r * tex2.r) + (IN.color.b * tex3.r * tex3.r);
			fixed g = (IN.color.r * tex1.g * tex1.g) + (IN.color.g * tex2.g * tex2.g) + (IN.color.b * tex3.g * tex3.g);
			fixed b = (IN.color.r * tex1.b * tex1.b) + (IN.color.g * tex2.b * tex2.b) + (IN.color.b * tex3.b * tex3.b);

			fixed4 sBump = tex2D(_SurfaceBump, IN.uv_SurfaceBump);
			fixed4 cn = tex2D(_CliffBump, IN.uv_CliffBump);

			fixed4 final;
			final.r = r;
			final.g = g;
			final.b = b;
			final.a = 1;

			o.Albedo = lerp (final, cliff, alpha);
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Normal = UnpackNormal(lerp (sBump, cn, alpha) * _Normal) ;
			o.Emission = 0;
			o.Alpha = 1;
		}
		ENDCG
	}
	Fallback "Diffuse"
}