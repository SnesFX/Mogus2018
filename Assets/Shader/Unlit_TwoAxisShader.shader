Shader "Unlit/TwoAxisShader" {
	Properties {
		_Color ("Bg Color", Vector) = (1,1,1,1)
		_CrossHair ("CrossHair Pos", Vector) = (0.5,0.5,0,0)
		_CrossColor ("CrossHair Color", Vector) = (1,1,1,1)
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = _Color.rgb;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
}