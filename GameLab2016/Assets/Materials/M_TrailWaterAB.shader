// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True;n:type:ShaderForge.SFN_Final,id:4795,x:32716,y:32678,varname:node_4795,prsc:2|emission-2393-OUT,alpha-948-OUT;n:type:ShaderForge.SFN_Tex2d,id:6074,x:32028,y:32408,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:a4d86fc22c06692458050927213f67d7,ntxv:0,isnm:False|UVIN-4250-UVOUT;n:type:ShaderForge.SFN_Multiply,id:2393,x:32495,y:32758,varname:node_2393,prsc:2|A-6033-OUT,B-2053-RGB,C-797-RGB,D-9248-OUT;n:type:ShaderForge.SFN_VertexColor,id:2053,x:32017,y:32997,varname:node_2053,prsc:2;n:type:ShaderForge.SFN_Color,id:797,x:32017,y:33155,ptovrint:True,ptlb:Color,ptin:_TintColor,varname:_TintColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Vector1,id:9248,x:32017,y:33306,varname:node_9248,prsc:2,v1:2;n:type:ShaderForge.SFN_Multiply,id:798,x:32495,y:32923,varname:node_798,prsc:2|A-5330-OUT,B-2053-A,C-797-A;n:type:ShaderForge.SFN_TexCoord,id:7878,x:31533,y:32289,varname:node_7878,prsc:2,uv:0;n:type:ShaderForge.SFN_Time,id:3864,x:31533,y:32456,varname:node_3864,prsc:2;n:type:ShaderForge.SFN_Panner,id:4250,x:31809,y:32384,varname:node_4250,prsc:2,spu:-8,spv:0|UVIN-7878-UVOUT,DIST-3864-TSL;n:type:ShaderForge.SFN_Tex2d,id:5408,x:31871,y:32765,ptovrint:False,ptlb:MainTex_copy,ptin:_MainTex_copy,varname:_MainTex_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:a4d86fc22c06692458050927213f67d7,ntxv:0,isnm:False|UVIN-2871-UVOUT;n:type:ShaderForge.SFN_TexCoord,id:3667,x:31354,y:32727,varname:node_3667,prsc:2,uv:0;n:type:ShaderForge.SFN_Time,id:1738,x:31354,y:32940,varname:node_1738,prsc:2;n:type:ShaderForge.SFN_Panner,id:2871,x:31646,y:32782,varname:node_2871,prsc:2,spu:-16,spv:0|UVIN-3667-UVOUT,DIST-1738-TSL;n:type:ShaderForge.SFN_TexCoord,id:5062,x:31353,y:33548,varname:node_5062,prsc:2,uv:0;n:type:ShaderForge.SFN_ComponentMask,id:9339,x:31571,y:33548,varname:node_9339,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-5062-UVOUT;n:type:ShaderForge.SFN_Multiply,id:948,x:32531,y:33125,varname:node_948,prsc:2|A-798-OUT,B-745-OUT;n:type:ShaderForge.SFN_Power,id:3714,x:32469,y:33528,varname:node_3714,prsc:2|VAL-9339-OUT,EXP-7561-OUT;n:type:ShaderForge.SFN_ValueProperty,id:7561,x:32265,y:33750,ptovrint:False,ptlb:White_Power,ptin:_White_Power,varname:node_7561,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:60;n:type:ShaderForge.SFN_SwitchProperty,id:745,x:32489,y:33314,ptovrint:False,ptlb:Use_Extra_Mask?,ptin:_Use_Extra_Mask,varname:node_745,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True|A-5846-OUT,B-3714-OUT;n:type:ShaderForge.SFN_Vector1,id:5846,x:32017,y:33408,varname:node_5846,prsc:2,v1:1;n:type:ShaderForge.SFN_SwitchProperty,id:6033,x:32413,y:32468,ptovrint:False,ptlb:UseFast_D,ptin:_UseFast_D,varname:node_6033,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True|A-6074-RGB,B-5408-RGB;n:type:ShaderForge.SFN_SwitchProperty,id:5330,x:32219,y:32757,ptovrint:False,ptlb:UseFast_A,ptin:_UseFast_A,varname:_UseFast_D_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True|A-6074-A,B-5408-A;n:type:ShaderForge.SFN_Add,id:727,x:31761,y:33548,varname:node_727,prsc:2|A-9339-OUT,B-9339-OUT;n:type:ShaderForge.SFN_Add,id:3273,x:31976,y:33535,varname:node_3273,prsc:2|A-727-OUT,B-727-OUT;n:type:ShaderForge.SFN_Add,id:7505,x:32184,y:33523,varname:node_7505,prsc:2|A-3273-OUT,B-3273-OUT;proporder:6074-797-5408-745-7561-6033-5330;pass:END;sub:END;*/

Shader "Shader Forge/M_TrailWaterAB" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _TintColor ("Color", Color) = (0.5,0.5,0.5,1)
        _MainTex_copy ("MainTex_copy", 2D) = "white" {}
        [MaterialToggle] _Use_Extra_Mask ("Use_Extra_Mask?", Float ) = 0
        _White_Power ("White_Power", Float ) = 60
        [MaterialToggle] _UseFast_D ("UseFast_D", Float ) = 0
        [MaterialToggle] _UseFast_A ("UseFast_A", Float ) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
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
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _TintColor;
            uniform sampler2D _MainTex_copy; uniform float4 _MainTex_copy_ST;
            uniform float _White_Power;
            uniform fixed _Use_Extra_Mask;
            uniform fixed _UseFast_D;
            uniform fixed _UseFast_A;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float4 node_3864 = _Time + _TimeEditor;
                float2 node_4250 = (i.uv0+node_3864.r*float2(-8,0));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_4250, _MainTex));
                float4 node_1738 = _Time + _TimeEditor;
                float2 node_2871 = (i.uv0+node_1738.r*float2(-16,0));
                float4 _MainTex_copy_var = tex2D(_MainTex_copy,TRANSFORM_TEX(node_2871, _MainTex_copy));
                float3 emissive = (lerp( _MainTex_var.rgb, _MainTex_copy_var.rgb, _UseFast_D )*i.vertexColor.rgb*_TintColor.rgb*2.0);
                float3 finalColor = emissive;
                float node_9339 = i.uv0.r;
                fixed4 finalRGBA = fixed4(finalColor,((lerp( _MainTex_var.a, _MainTex_copy_var.a, _UseFast_A )*i.vertexColor.a*_TintColor.a)*lerp( 1.0, pow(node_9339,_White_Power), _Use_Extra_Mask )));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
