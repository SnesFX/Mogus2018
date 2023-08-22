Shader "Unlit/GradientShader" {
	Properties {
		_Color1 ("Color1", Vector) = (0,0,0,1)
		_Color2 ("Color2", Vector) = (0,0,0,1)
		_Center ("Center", Float) = 0.25
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
}