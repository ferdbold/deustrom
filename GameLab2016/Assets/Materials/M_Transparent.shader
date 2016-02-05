// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:False,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:4643,x:33050,y:32637,varname:node_4643,prsc:2|emission-8867-OUT,alpha-1383-OUT;n:type:ShaderForge.SFN_TexCoord,id:4102,x:31843,y:33019,varname:node_4102,prsc:2,uv:0;n:type:ShaderForge.SFN_ComponentMask,id:7326,x:32047,y:33021,varname:node_7326,prsc:2,cc1:1,cc2:-1,cc3:-1,cc4:-1|IN-4102-UVOUT;n:type:ShaderForge.SFN_Multiply,id:6842,x:32261,y:33031,varname:node_6842,prsc:2|A-7326-OUT,B-7443-OUT;n:type:ShaderForge.SFN_ValueProperty,id:7443,x:31983,y:33227,ptovrint:False,ptlb:node_7443,ptin:_node_7443,varname:node_7443,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2.3;n:type:ShaderForge.SFN_DepthBlend,id:2999,x:31510,y:31904,varname:node_2999,prsc:2;n:type:ShaderForge.SFN_Vector3,id:6898,x:31871,y:31828,varname:node_6898,prsc:2,v1:0,v2:0.3793104,v3:1;n:type:ShaderForge.SFN_Vector3,id:7715,x:31958,y:31963,varname:node_7715,prsc:2,v1:1,v2:1,v3:1;n:type:ShaderForge.SFN_Lerp,id:5015,x:31717,y:31775,varname:node_5015,prsc:2|A-7715-OUT,B-6898-OUT,T-2999-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:1383,x:32587,y:33107,ptovrint:False,ptlb:OPA_Ctrl,ptin:_OPA_Ctrl,varname:node_1383,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True|A-3789-OUT,B-6842-OUT;n:type:ShaderForge.SFN_ValueProperty,id:3789,x:32387,y:33250,ptovrint:False,ptlb:Opacity,ptin:_Opacity,varname:node_3789,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Lerp,id:8867,x:32405,y:32012,varname:node_8867,prsc:2|A-7715-OUT,B-8514-OUT,T-5867-OUT;n:type:ShaderForge.SFN_Vector3,id:8514,x:32116,y:32089,varname:node_8514,prsc:2,v1:0.3529412,v2:0.8156863,v3:0.9568628;n:type:ShaderForge.SFN_DepthBlend,id:7030,x:32034,y:32281,varname:node_7030,prsc:2|DIST-6373-OUT;n:type:ShaderForge.SFN_ValueProperty,id:6373,x:31793,y:32244,ptovrint:False,ptlb:Depth,ptin:_Depth,varname:_Depth_2,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.9;n:type:ShaderForge.SFN_Floor,id:5867,x:32293,y:32261,varname:node_5867,prsc:2|IN-7030-OUT;proporder:7443-1383-3789-6373;pass:END;sub:END;*/

Shader "Custom/NewSurfaceShader" {
    Properties {
        _node_7443 ("node_7443", Float ) = 2.3
        [MaterialToggle] _OPA_Ctrl ("OPA_Ctrl", Float ) = 0
        _Opacity ("Opacity", Float ) = 1
        _Depth ("Depth", Float ) = 0.9
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
            uniform sampler2D _CameraDepthTexture;
            uniform float _node_7443;
            uniform fixed _OPA_Ctrl;
            uniform float _Opacity;
            uniform float _Depth;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 projPos : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float sceneZ = max(0,LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)))) - _ProjectionParams.g);
                float partZ = max(0,i.projPos.z - _ProjectionParams.g);
////// Lighting:
////// Emissive:
                float3 node_7715 = float3(1,1,1);
                float3 emissive = lerp(node_7715,float3(0.3529412,0.8156863,0.9568628),floor(saturate((sceneZ-partZ)/_Depth)));
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,lerp( _Opacity, (i.uv0.g*_node_7443), _OPA_Ctrl ));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
