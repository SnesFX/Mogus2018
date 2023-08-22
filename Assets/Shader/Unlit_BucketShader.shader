Shader "Unlit/BucketShader" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_EmptyColor ("EmptyColor", Vector) = (0,0.5,0,1)
		_FullColor ("FullColor", Vector) = (0,1,0,1)
		_Buckets ("Buckets", Float) = 5
		_FullBuckets ("FullBuckets", Float) = 3
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