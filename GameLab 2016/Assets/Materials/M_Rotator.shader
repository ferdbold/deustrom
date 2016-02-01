// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:4643,x:33015,y:32596,varname:node_4643,prsc:2|diff-3795-RGB,emission-3795-RGB,alpha-4078-OUT;n:type:ShaderForge.SFN_Tex2d,id:3795,x:32531,y:32638,ptovrint:False,ptlb:node_3795,ptin:_node_3795,varname:node_3795,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:f69506eb884372a438ef19676bfce4a4,ntxv:0,isnm:False|UVIN-6881-UVOUT;n:type:ShaderForge.SFN_TexCoord,id:1120,x:31777,y:32253,varname:node_1120,prsc:2,uv:0;n:type:ShaderForge.SFN_Time,id:926,x:31838,y:32712,varname:node_926,prsc:2;n:type:ShaderForge.SFN_Multiply,id:2656,x:32050,y:32766,varname:node_2656,prsc:2|A-926-T,B-5314-OUT;n:type:ShaderForge.SFN_ValueProperty,id:5314,x:31838,y:32929,ptovrint:False,ptlb:node_5314,ptin:_node_5314,varname:node_5314,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:-0.1;n:type:ShaderForge.SFN_Multiply,id:7082,x:31975,y:32364,varname:node_7082,prsc:2|A-1120-U,B-6809-OUT;n:type:ShaderForge.SFN_Multiply,id:5733,x:31975,y:32525,varname:node_5733,prsc:2|A-1120-V,B-1551-OUT;n:type:ShaderForge.SFN_ValueProperty,id:6809,x:31762,y:32436,ptovrint:False,ptlb:panX,ptin:_panX,varname:node_6809,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:6;n:type:ShaderForge.SFN_ValueProperty,id:1551,x:31762,y:32559,ptovrint:False,ptlb:panY,ptin:_panY,varname:node_1551,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:6;n:type:ShaderForge.SFN_Append,id:2798,x:32128,y:32464,varname:node_2798,prsc:2|A-7082-OUT,B-5733-OUT;n:type:ShaderForge.SFN_Rotator,id:6881,x:32352,y:32622,varname:node_6881,prsc:2|UVIN-2798-OUT,PIV-2864-OUT,ANG-2656-OUT;n:type:ShaderForge.SFN_TexCoord,id:4509,x:32111,y:32955,varname:node_4509,prsc:2,uv:0;n:type:ShaderForge.SFN_ComponentMask,id:6229,x:32316,y:32944,varname:node_6229,prsc:2,cc1:1,cc2:-1,cc3:-1,cc4:-1|IN-4509-UVOUT;n:type:ShaderForge.SFN_Multiply,id:7382,x:32630,y:32943,varname:node_7382,prsc:2|A-6229-OUT,B-56-OUT;n:type:ShaderForge.SFN_ValueProperty,id:56,x:32406,y:33160,ptovrint:False,ptlb:White_Power,ptin:_White_Power,varname:node_56,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:3;n:type:ShaderForge.SFN_Multiply,id:4078,x:32781,y:33079,varname:node_4078,prsc:2|A-7382-OUT,B-56-OUT;n:type:ShaderForge.SFN_Vector2,id:2864,x:32117,y:32640,varname:node_2864,prsc:2,v1:-12,v2:4;proporder:3795-5314-6809-1551-56;pass:END;sub:END;*/

Shader "Custom/NewSurfaceShader" {
    Properties {
        _node_3795 ("node_3795", 2D) = "white" {}
        _node_5314 ("node_5314", Float ) = -0.1
        _panX ("panX", Float ) = 6
        _panY ("panY", Float ) = 6
        _White_Power ("White_Power", Float ) = 3
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
                float node_6881_ang = (node_926.g*_node_5314);
                float node_6881_spd = 1.0;
                float node_6881_cos = cos(node_6881_spd*node_6881_ang);
                float node_6881_sin = sin(node_6881_spd*node_6881_ang);
                float2 node_6881_piv = float2(-12,4);
                float2 node_6881 = (mul(float2((i.uv0.r*_panX),(i.uv0.g*_panY))-node_6881_piv,float2x2( node_6881_cos, -node_6881_sin, node_6881_sin, node_6881_cos))+node_6881_piv);
                float4 _node_3795_var = tex2D(_node_3795,TRANSFORM_TEX(node_6881, _node_3795));
                float3 emissive = _node_3795_var.rgb;
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,((i.uv0.g*_White_Power)*_White_Power));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
