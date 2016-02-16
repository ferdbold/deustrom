// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:4643,x:33450,y:32623,varname:node_4643,prsc:2|diff-3795-RGB,emission-1842-OUT,alpha-226-OUT;n:type:ShaderForge.SFN_Tex2d,id:3795,x:32238,y:32583,ptovrint:False,ptlb:node_3795,ptin:_node_3795,varname:node_3795,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-3401-UVOUT;n:type:ShaderForge.SFN_Panner,id:3401,x:32000,y:32510,varname:node_3401,prsc:2,spu:2,spv:1|UVIN-2798-OUT,DIST-2656-OUT;n:type:ShaderForge.SFN_TexCoord,id:1120,x:31366,y:32168,varname:node_1120,prsc:2,uv:0;n:type:ShaderForge.SFN_Time,id:926,x:31436,y:32613,varname:node_926,prsc:2;n:type:ShaderForge.SFN_Multiply,id:2656,x:31768,y:32621,varname:node_2656,prsc:2|A-926-T,B-5314-OUT;n:type:ShaderForge.SFN_ValueProperty,id:5314,x:31534,y:32780,ptovrint:False,ptlb:node_5314,ptin:_node_5314,varname:node_5314,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Multiply,id:7082,x:31615,y:32271,varname:node_7082,prsc:2|A-1120-U,B-6809-OUT;n:type:ShaderForge.SFN_Multiply,id:5733,x:31615,y:32466,varname:node_5733,prsc:2|A-1120-V,B-1551-OUT;n:type:ShaderForge.SFN_ValueProperty,id:6809,x:31402,y:32343,ptovrint:False,ptlb:panX,ptin:_panX,varname:node_6809,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:1551,x:31419,y:32500,ptovrint:False,ptlb:panY,ptin:_panY,varname:node_1551,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Append,id:2798,x:31768,y:32371,varname:node_2798,prsc:2|A-7082-OUT,B-5733-OUT;n:type:ShaderForge.SFN_TexCoord,id:9854,x:31649,y:32920,varname:node_9854,prsc:2,uv:0;n:type:ShaderForge.SFN_ComponentMask,id:773,x:31821,y:32920,varname:node_773,prsc:2,cc1:1,cc2:-1,cc3:-1,cc4:-1|IN-9854-UVOUT;n:type:ShaderForge.SFN_OneMinus,id:8901,x:31911,y:33159,varname:node_8901,prsc:2|IN-773-OUT;n:type:ShaderForge.SFN_Multiply,id:172,x:32116,y:32940,varname:node_172,prsc:2|A-773-OUT,B-8901-OUT;n:type:ShaderForge.SFN_Multiply,id:6644,x:32876,y:33027,varname:node_6644,prsc:2|A-8038-OUT,B-4968-OUT;n:type:ShaderForge.SFN_ValueProperty,id:4968,x:32727,y:33335,ptovrint:False,ptlb:White_Power,ptin:_White_Power,varname:node_4968,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:5;n:type:ShaderForge.SFN_SwitchProperty,id:226,x:33057,y:32973,ptovrint:False,ptlb:Use_Mask,ptin:_Use_Mask,varname:node_226,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-3363-OUT,B-6644-OUT;n:type:ShaderForge.SFN_Vector1,id:3363,x:32628,y:32920,varname:node_3363,prsc:2,v1:1;n:type:ShaderForge.SFN_Multiply,id:1842,x:32856,y:32537,varname:node_1842,prsc:2|A-3795-RGB,B-4673-OUT;n:type:ShaderForge.SFN_TexCoord,id:6889,x:32124,y:32763,varname:node_6889,prsc:2,uv:0;n:type:ShaderForge.SFN_ComponentMask,id:8668,x:32282,y:32745,varname:node_8668,prsc:2,cc1:1,cc2:-1,cc3:-1,cc4:-1|IN-6889-UVOUT;n:type:ShaderForge.SFN_Add,id:4673,x:32628,y:32711,varname:node_4673,prsc:2|A-8668-OUT,B-6661-OUT;n:type:ShaderForge.SFN_Vector4,id:6661,x:32428,y:32804,varname:node_6661,prsc:2,v1:0.408737,v2:0.6217187,v3:0.7720588,v4:0;n:type:ShaderForge.SFN_OneMinus,id:1350,x:32294,y:32960,varname:node_1350,prsc:2|IN-172-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:13,x:32483,y:33066,ptovrint:False,ptlb:Invert_Mask,ptin:_Invert_Mask,varname:_Use_Mask_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-1350-OUT,B-172-OUT;n:type:ShaderForge.SFN_Power,id:8038,x:32686,y:33066,varname:node_8038,prsc:2|VAL-13-OUT,EXP-875-OUT;n:type:ShaderForge.SFN_ValueProperty,id:875,x:32440,y:33239,ptovrint:False,ptlb:Black_Power,ptin:_Black_Power,varname:_White_Power_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_TexCoord,id:5756,x:31278,y:32577,varname:node_5756,prsc:2,uv:0;n:type:ShaderForge.SFN_RemapRange,id:5598,x:31499,y:32564,varname:node_5598,prsc:2,frmn:0,frmx:1,tomn:-1,tomx:1|IN-5756-UVOUT;n:type:ShaderForge.SFN_Multiply,id:8644,x:31768,y:32536,varname:node_8644,prsc:2|A-5598-OUT,B-5598-OUT;n:type:ShaderForge.SFN_ComponentMask,id:4152,x:31950,y:32516,varname:node_4152,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-8644-OUT;n:type:ShaderForge.SFN_Add,id:9234,x:32098,y:32491,varname:node_9234,prsc:2|A-4152-R,B-4152-G;proporder:3795-5314-6809-1551-4968-226-13-875;pass:END;sub:END;*/

Shader "Custom/NewSurfaceShader" {
    Properties {
        _node_3795 ("node_3795", 2D) = "white" {}
        _node_5314 ("node_5314", Float ) = 0
        _panX ("panX", Float ) = 0
        _panY ("panY", Float ) = 0
        _White_Power ("White_Power", Float ) = 5
        [MaterialToggle] _Use_Mask ("Use_Mask", Float ) = 1
        [MaterialToggle] _Invert_Mask ("Invert_Mask", Float ) = 1
        _Black_Power ("Black_Power", Float ) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 200
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform sampler2D _node_3795; uniform float4 _node_3795_ST;
            uniform float _node_5314;
            uniform float _panX;
            uniform float _panY;
            uniform float _White_Power;
            uniform fixed _Use_Mask;
            uniform fixed _Invert_Mask;
            uniform float _Black_Power;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 node_926 = _Time + _TimeEditor;
                float2 node_3401 = (float2((i.uv0.r*_panX),(i.uv0.g*_panY))+(node_926.g*_node_5314)*float2(2,1));
                float4 _node_3795_var = tex2D(_node_3795,TRANSFORM_TEX(node_3401, _node_3795));
                float3 emissive = (_node_3795_var.rgb*(i.uv0.g+float4(0.408737,0.6217187,0.7720588,0))).rgb;
                float3 finalColor = emissive;
                float node_773 = i.uv0.g;
                fixed4 finalRGBA = fixed4(finalColor,lerp( 1.0, (pow(lerp( (1.0 - (node_773*(1.0 - node_773))), (node_773*(1.0 - node_773)), _Invert_Mask ),_Black_Power)*_White_Power), _Use_Mask ));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
