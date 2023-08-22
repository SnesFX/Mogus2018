Shader "Unlit/PlayerShader" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_BackColor ("Shadow Color", Vector) = (1,0,0,1)
		_BodyColor ("Body Color", Vector) = (1,1,0,1)
		_VisorColor ("VisorColor", Vector) = (0,1,1,1)
		_Outline ("Outline", Range(0, 1)) = 0
		_OutlineColor ("OutlineColor", Vector) = (1,1,1,1)
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}