﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TargetingArrowAnimation"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_NewColor("Replacement Color", Color) = (1, 1, 1, 1)
		_Cutoff ("Alpha Cutoff", Float) = 0.5
	}
	SubShader
	{

		Pass
		{
			Cull Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float _Cutoff;
			uniform float4 _NewColor;
			uniform float4 _MainTex_ST;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float2 tex : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				//o.tex = v.uvcoord;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.tex = float2(v.texcoord.x - fmod(_Time.y, 60), v.texcoord.y ); //_MainTex_ST.zw;
				return o;
			}
			
			fixed4 frag (v2f i) : COLOR
			{

				fixed4 col = tex2D(_MainTex, i.tex.xy);
				if (col.r > _Cutoff && col.g > _Cutoff && col.b > _Cutoff)
				{
					discard;
				}
				else 
				{
					col = _NewColor;
				}
				return col;
			}
			ENDCG
		}
	}
}
