/*
This shader is used on editable meshes in edge mode.  It supports edge selection
(on top of a custom texture)
*/
Shader "Custom/CustomMeshworkEdge"
{
	Properties {
		_MainTex ("Texture", 2D) = "white" {}

		[Header(Wireframe Properties)]
        _Wireframe_Thickness("Thickness", Range(0, 1)) = 0.01
		_Wireframe_Smoothness("Smoothness", Range(0, 1)) = 0	
		_Wireframe_Diameter("Diameter", Range(0, 1)) = 1
		_Wireframe_Color("Color", Color) = (0,0.7,0.7,1)
		_Wireframe_SelColor("Selected Color", Color) = (0,1,1,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100
		
		Pass {
            // indicate that our pass is the "base" pass in forward
            // rendering pipeline. It gets ambient and main directional
            // light data set up; light direction in _WorldSpaceLightPos0
            // and color in _LightColor0
            Tags {"LightMode"="ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc" // for _LightColor0
            #include "Assets/Amazing Assets/Wireframe Shader/Shaders/cginc/WireframeShader.cginc"   //--------------Path to the Wireframe cginc
			
			// data transferred into the vertex function:
			struct appdata {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 normal : NORMAL;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float4 uv3 : TEXCOORD3;     //--------------Wireframe data is saved inside uv4 buffer (note, in shaders uv4 is read using TEXCOORD3)
			};
			
			// data transferred from the vertex to the fragment (pixel) function:
			struct v2f {
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float4 color : COLOR0;		// vertex color
                float diffuse : COLOR1;		// diffuse lighting amount (0-1)
                float4 vertex : SV_POSITION;
				float3 uv3 : TEXCOORD3;
			};
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Wireframe_Thickness;
			float _Wireframe_Smoothness;
			float _Wireframe_Diameter;
			float4 _Wireframe_Color;
			float4 _Wireframe_SelColor;
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2 = v.uv2;
				o.color = v.color;
				
				// get vertex normal in world space
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				// dot product between normal and light direction for
				// standard diffuse (Lambert) lighting
				float nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diffuse = 0.5 + nl * 0.5;	// softened

				o.uv3 = v.uv3.xyz;      //--------------Sending uv3 data from vertex to pixel stage
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				// sample the texture, apply the lighting, and add the highlight color
				fixed4 col = tex2D(_MainTex, i.uv) * i.diffuse;
				
				// apply wireframe
				// figure out which side of the triangle we're on
				float2 uv = i.uv2;
				int side = 0			// bottom
					+ (uv.x <= 0.5 && uv.y > uv.x)			// left side
					+ (uv.x > 0.5 && uv.y > 1 - uv.x) * 2;	// right side
				// and from that and the vertex color (which should be constant across the triangle),
				// figure out if this side is selected
				int selected = i.color[side];
				// Huzzah!  Finally we can choose the correct color to use.
				float4 wirecolor = _Wireframe_SelColor * selected + _Wireframe_Color * (1 - selected);
				float wireframe = WireframeShaderReadTrangleMassFromUV(i.uv3, _Wireframe_Thickness, _Wireframe_Smoothness, _Wireframe_Diameter);
				return wireframe * wirecolor + (1 - wireframe) * col;
			}
			ENDCG
		}
	}
}