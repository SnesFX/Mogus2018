Shader "Unlit/CooldownShader" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Cooldown Color", Vector) = (0.5,0.5,0.5,1)
		_Percent ("PercentCool", Float) = 1
		_Desat ("Desaturation", Float) = 0
		_NormalizedUvs ("NormUvs", Vector) = (0,1,0,0)
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}