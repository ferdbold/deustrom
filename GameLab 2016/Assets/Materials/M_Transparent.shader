// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:4643,x:33050,y:32637,varname:node_4643,prsc:2|emission-3176-XYZ,alpha-6842-OUT;n:type:ShaderForge.SFN_Vector4Property,id:3176,x:32458,y:32704,ptovrint:False,ptlb:node_3176,ptin:_node_3176,varname:node_3176,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.3529412,v2:0.8156863,v3:0.9568628,v4:0;n:type:ShaderForge.SFN_TexCoord,id:4102,x:32245,y:32869,varname:node_4102,prsc:2,uv:0;n:type:ShaderForge.SFN_ComponentMask,id:7326,x:32419,y:32905,varname:node_7326,prsc:2,cc1:1,cc2:-1,cc3:-1,cc4:-1|IN-4102-UVOUT;n:type:ShaderForge.SFN_Multiply,id:6842,x:32663,y:32881,varname:node_6842,prsc:2|A-7326-OUT,B-7443-OUT;n:type:ShaderForge.SFN_ValueProperty,id:7443,x:32476,y:33070,ptovrint:False,ptlb:node_7443,ptin:_node_7443,varname:node_7443,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2.3;proporder:3176-7443;pass:END;sub:END;*/

Shader "Custom/NewSurfaceShader" {
    Properties {
        _node_3176 ("node_3176", Vector) = (0.3529412,0.8156863,0.9568628,0)
        _node_7443 ("node_7443", Float ) = 2.3
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
            uniform float4 _node_3176;
            uniform float _node_7443;
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
                float3 emissive = _node_3176.rgb;
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,(i.uv0.g*_node_7443));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
