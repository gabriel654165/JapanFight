// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MFEP_Stream"
{
	Properties
	{
		_Ripples_Displacement("Ripples_Displacement", 2D) = "gray" {}
		_Ripples("Ripples", 2D) = "bump" {}
		_Ripples2("Ripples2", 2D) = "bump" {}
		_Color("Color", Color) = (0,0,0,0)
		_Displacement("Displacement", Range( 0 , 1)) = 0
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Base_Smoothness("Base_Smoothness", Range( 0 , 1)) = 0
		_Speed("Speed", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform sampler2D _Ripples;
		uniform float _Speed;
		uniform sampler2D _Ripples2;
		uniform float4 _Ripples2_ST;
		uniform float4 _Color;
		uniform float _Metallic;
		uniform float _Base_Smoothness;
		uniform sampler2D _Ripples_Displacement;
		uniform float _Displacement;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 temp_cast_0 = (_Speed).xx;
			float2 panner21 = ( v.texcoord.xy + _Time.x * temp_cast_0);
			float3 ase_vertexNormal = v.normal.xyz;
			v.vertex.xyz += ( tex2Dlod( _Ripples_Displacement, float4( panner21, 0, 0.0) ) * float4( ( ase_vertexNormal * _Displacement ) , 0.0 ) ).rgb;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 temp_cast_0 = (_Speed).xx;
			float2 panner21 = ( i.uv_texcoord + _Time.x * temp_cast_0);
			float2 uv_Ripples2 = i.uv_texcoord * _Ripples2_ST.xy + _Ripples2_ST.zw;
			float3 temp_output_23_0 = ( UnpackNormal( tex2D( _Ripples, panner21 ) ) + UnpackNormal( tex2D( _Ripples2, uv_Ripples2 ) ) );
			o.Normal = temp_output_23_0;
			o.Albedo = ( _Color * i.vertexColor ).rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Base_Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=14201
468;861;1501;517;1511.778;325.4232;2.194915;True;True
Node;AmplifyShaderEditor.TimeNode;20;-1871.671,216.2547;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexCoordVertexDataNode;22;-1840.133,-16.54401;Float;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;64;-1878.191,133.6389;Float;False;Property;_Speed;Speed;10;0;Create;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;34;-590.7322,682.2824;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;35;-555.7047,853.9885;Float;False;Property;_Displacement;Displacement;6;0;Create;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;21;-1523.418,142.8052;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-2,0;False;1;FLOAT;1.0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;1;-1246.355,7.005381;Float;True;Property;_Ripples;Ripples;1;0;Create;None;f366779a17f54444485071fb5eab8ce2;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;47;-1218.631,523.1285;Float;True;Property;_Ripples_Displacement;Ripples_Displacement;0;0;Create;None;7b1ac0833a59dfc44a00632776814b20;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;17;-1223.633,237.2322;Float;True;Property;_Ripples2;Ripples2;2;0;Create;None;f366779a17f54444485071fb5eab8ce2;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-139.1631,638.165;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0.0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexColorNode;4;-739.226,210.3829;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;3;66.41543,-316.6964;Float;False;Property;_Color;Color;3;0;Create;0,0,0,0;0.7944891,0.7921713,0.8161765,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;154.9446,404.7332;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-202.9833,477.3842;Float;False;Property;_Transparency;Transparency;5;0;Create;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;62;326.7956,156.4028;Float;False;Property;_Base_Smoothness;Base_Smoothness;9;0;Create;0;0.99;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;63;304.6681,79.69765;Float;False;Property;_Metallic;Metallic;8;0;Create;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-582.72,562.823;Float;False;Property;_EdgeOcclusion;Edge Occlusion;4;0;Create;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;11;-192.9156,321.83;Float;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;61;385.1858,-96.65038;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;57;-326.7911,-78.13969;Float;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;59;-530.2942,-90.4701;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;58;-314.0287,102.8693;Float;False;True;True;False;False;1;0;FLOAT3;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;619.7104,-216.9084;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;236.3967,288.6221;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;28;167.8342,581.2643;Float;False;Property;_IndexofRefraction;Index of Refraction;7;0;Create;0;0;-3;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;23;-709.2515,77.35963;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ScreenColorNode;55;134.7069,-93.17452;Float;False;Global;_GrabScreen0;Grab Screen 0;10;0;Create;Object;-1;True;False;1;0;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;56;-44.82303,-79.73502;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1125.228,-135.2759;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;MFEP_Stream;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;Back;0;0;False;0;0;Opaque;0.53;True;True;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;0;4;10;25;False;0.5;True;0;SrcAlpha;OneMinusSrcAlpha;0;One;OneMinusSrcAlpha;Add;Add;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;0;0;False;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;21;0;22;0
WireConnection;21;2;64;0
WireConnection;21;1;20;1
WireConnection;1;1;21;0
WireConnection;47;1;21;0
WireConnection;36;0;34;0
WireConnection;36;1;35;0
WireConnection;37;0;47;0
WireConnection;37;1;36;0
WireConnection;11;0;4;0
WireConnection;11;1;9;0
WireConnection;61;0;55;0
WireConnection;57;0;59;0
WireConnection;58;0;23;0
WireConnection;65;0;3;0
WireConnection;65;1;4;0
WireConnection;60;0;11;0
WireConnection;60;1;12;0
WireConnection;23;0;1;0
WireConnection;23;1;17;0
WireConnection;55;0;56;0
WireConnection;56;0;57;0
WireConnection;56;1;58;0
WireConnection;0;0;65;0
WireConnection;0;1;23;0
WireConnection;0;3;63;0
WireConnection;0;4;62;0
WireConnection;0;11;37;0
ASEEND*/
//CHKSM=7513819187E83F17ED955F3A2B464ABA7AD2CA2E