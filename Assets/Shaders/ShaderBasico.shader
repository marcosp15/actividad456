Shader "ShaderBasico"
{
	SubShader
	{
		Pass
		{
			CGPROGRAM
			// declaramos que la función llamada "vert" definirá el shader de vértices 
			#pragma vertex vert 
			// declaramos que la función llamada "frag" definirá el shader de fragmentos 
			#pragma fragment frag 

			uniform float4x4 _ModelMatrix;
			uniform float4x4 _ViewMatrix;
			uniform float4x4 _ProjectionMatrix;

			//datos de cada vértice 
			struct appdata {
				float4 vertex: POSITION;
				fixed4 color : COLOR;
			};

			//datos que pasaremos del shader de vértices al de fragmentos 
			struct v2f {
				float4 vertex: SV_POSITION;
				fixed4 color : COLOR;
			};

			// shader de vértices 
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = mul( mul (_ProjectionMatrix, mul(_ViewMatrix, _ModelMatrix)), v.vertex);
				o.color = v.color;
				return o;
			}

			// shader de fragmentos 
			fixed4 frag(v2f i) : SV_Target
			{
				//establecemos el color de salida return half4(1.0f, 0.0f, 0.0f, 1.0f); 
				return(i.color);
			}
			ENDCG
		}
	}
}