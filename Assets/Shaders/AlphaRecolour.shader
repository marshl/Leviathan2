Shader "Custom/AlphaRecolour" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_AlphaTex ("Recolour Map (Alpha)", 2D) = "white" {}
		_AlphaCol ("Recolour hue (RGB)", Color) = (1,1,1,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		sampler2D _AlphaTex;
		half4 _AlphaCol;

		struct Input {
			float2 uv_MainTex;
			float2 uv_AlphaTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 mainCol = tex2D (_MainTex, IN.uv_MainTex);
			half4 alphaCol = tex2D (_AlphaTex, IN.uv_AlphaTex);
			
			
			if(alphaCol.a > 0)
			{
				//o.Albedo = (mainCol.rgb + alphaCol.rgb) * _AlphaCol.rgb;
				o.Albedo = mainCol.rgb * _AlphaCol.rgb;
				
			}
			else
			{
				o.Albedo = mainCol.rgb;
			}
			
			
			
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
