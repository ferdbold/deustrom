// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:False,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:4643,x:33188,y:32602,varname:node_4643,prsc:2|emission-8867-OUT,alpha-7134-OUT,voffset-1753-OUT;n:type:ShaderForge.SFN_TexCoord,id:4102,x:31586,y:32730,varname:node_4102,prsc:2,uv:0;n:type:ShaderForge.SFN_ComponentMask,id:7326,x:31790,y:32732,varname:node_7326,prsc:2,cc1:1,cc2:-1,cc3:-1,cc4:-1|IN-4102-UVOUT;n:type:ShaderForge.SFN_Multiply,id:6842,x:32048,y:32762,varname:node_6842,prsc:2|A-7326-OUT,B-7443-OUT;n:type:ShaderForge.SFN_ValueProperty,id:7443,x:31726,y:32938,ptovrint:False,ptlb:Fade,ptin:_Fade,varname:node_7443,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2.3;n:type:ShaderForge.SFN_DepthBlend,id:2999,x:32178,y:31727,varname:node_2999,prsc:2|DIST-5957-OUT;n:type:ShaderForge.SFN_Vector3,id:6898,x:32123,y:31515,varname:node_6898,prsc:2,v1:0.1479779,v2:0.7050467,v3:0.875;n:type:ShaderForge.SFN_Vector3,id:7715,x:31958,y:31963,varname:node_7715,prsc:2,v1:1,v2:1,v3:1;n:type:ShaderForge.SFN_Lerp,id:5015,x:32550,y:31581,varname:node_5015,prsc:2|A-7715-OUT,B-6898-OUT,T-6603-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:1383,x:32415,y:32802,ptovrint:False,ptlb:OPA_Ctrl,ptin:_OPA_Ctrl,varname:node_1383,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True|A-3789-OUT,B-4585-OUT;n:type:ShaderForge.SFN_ValueProperty,id:3789,x:31943,y:32700,ptovrint:False,ptlb:Opacity,ptin:_Opacity,varname:node_3789,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Lerp,id:8867,x:32560,y:31961,varname:node_8867,prsc:2|A-5015-OUT,B-8514-OUT,T-5867-OUT;n:type:ShaderForge.SFN_Vector3,id:8514,x:32116,y:32089,varname:node_8514,prsc:2,v1:0.3529412,v2:0.8156863,v3:0.9568628;n:type:ShaderForge.SFN_DepthBlend,id:7030,x:32057,y:32261,varname:node_7030,prsc:2|DIST-6373-OUT;n:type:ShaderForge.SFN_ValueProperty,id:6373,x:31759,y:32261,ptovrint:False,ptlb:Depth,ptin:_Depth,varname:_Depth_2,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.9;n:type:ShaderForge.SFN_Floor,id:5867,x:32293,y:32261,varname:node_5867,prsc:2|IN-7030-OUT;n:type:ShaderForge.SFN_Multiply,id:9709,x:32606,y:33324,varname:node_9709,prsc:2|A-3203-OUT,B-1005-OUT,C-4175-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1005,x:32020,y:33441,ptovrint:False,ptlb:Bulge Scale,ptin:_BulgeScale,varname:_BulgeScale,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.2;n:type:ShaderForge.SFN_NormalVector,id:4175,x:32416,y:33424,prsc:2,pt:False;n:type:ShaderForge.SFN_Sin,id:3838,x:32169,y:33253,varname:node_3838,prsc:2|IN-3825-TDB;n:type:ShaderForge.SFN_Time,id:3825,x:31863,y:33281,varname:node_3825,prsc:2;n:type:ShaderForge.SFN_Divide,id:3203,x:32416,y:33240,varname:node_3203,prsc:2|A-3838-OUT,B-3828-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:1753,x:32807,y:33298,ptovrint:False,ptlb:Waves ?,ptin:_Waves,varname:node_1753,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-7183-OUT,B-9709-OUT;n:type:ShaderForge.SFN_Vector1,id:7183,x:32634,y:33240,varname:node_7183,prsc:2,v1:0;n:type:ShaderForge.SFN_Multiply,id:4330,x:32888,y:32619,varname:node_4330,prsc:2|A-9460-OUT,B-6816-OUT;n:type:ShaderForge.SFN_OneMinus,id:9460,x:32535,y:32356,varname:node_9460,prsc:2|IN-5867-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:7134,x:33045,y:32780,ptovrint:False,ptlb:Switch_ON_Waves,ptin:_Switch_ON_Waves,varname:_Waves_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-1383-OUT,B-4330-OUT;n:type:ShaderForge.SFN_Power,id:4585,x:32230,y:32887,varname:node_4585,prsc:2|VAL-6842-OUT,EXP-7498-OUT;n:type:ShaderForge.SFN_ValueProperty,id:7498,x:32007,y:32992,ptovrint:False,ptlb:Fade_Power,ptin:_Fade_Power,varname:_Fade_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Power,id:6816,x:32679,y:32658,varname:node_6816,prsc:2|VAL-8477-OUT,EXP-9277-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9277,x:31917,y:32625,ptovrint:False,ptlb:Wave_Fade_Power,ptin:_Wave_Fade_Power,varname:_Fade_Power_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_TexCoord,id:6536,x:31214,y:32513,varname:node_6536,prsc:2,uv:0;n:type:ShaderForge.SFN_RemapRange,id:8418,x:31435,y:32500,varname:node_8418,prsc:2,frmn:0,frmx:1,tomn:-1,tomx:1|IN-6536-UVOUT;n:type:ShaderForge.SFN_Multiply,id:9174,x:31704,y:32472,varname:node_9174,prsc:2|A-8418-OUT,B-8418-OUT;n:type:ShaderForge.SFN_Add,id:2859,x:32034,y:32427,varname:node_2859,prsc:2|A-905-R,B-905-G;n:type:ShaderForge.SFN_ComponentMask,id:905,x:31886,y:32452,varname:node_905,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-9174-OUT;n:type:ShaderForge.SFN_Add,id:9855,x:32204,y:32455,varname:node_9855,prsc:2|A-2859-OUT,B-2859-OUT;n:type:ShaderForge.SFN_Add,id:7049,x:32361,y:32496,varname:node_7049,prsc:2|A-9855-OUT,B-9855-OUT;n:type:ShaderForge.SFN_Add,id:8477,x:32501,y:32553,varname:node_8477,prsc:2|A-7049-OUT,B-7049-OUT;n:type:ShaderForge.SFN_ValueProperty,id:5957,x:31975,y:31823,ptovrint:False,ptlb:Shadow_Depth,ptin:_Shadow_Depth,varname:_Depth_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.1;n:type:ShaderForge.SFN_Floor,id:6603,x:32372,y:31711,varname:node_6603,prsc:2|IN-2999-OUT;n:type:ShaderForge.SFN_ValueProperty,id:3828,x:32199,y:33520,ptovrint:False,ptlb:Wave_Speed,ptin:_Wave_Speed,varname:node_3828,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:6;proporder:7443-1383-3789-6373-1005-1753-7134-7498-9277-5957-3828;pass:END;sub:END;*/

Shader "Custom/NewSurfaceShader" {
    Properties {
        _Fade ("Fade", Float ) = 2.3
        [MaterialToggle] _OPA_Ctrl ("OPA_Ctrl", Float ) = 0
        _Opacity ("Opacity", Float ) = 1
        _Depth ("Depth", Float ) = 0.9
        _BulgeScale ("Bulge Scale", Float ) = 0.2
        [MaterialToggle] _Waves ("Waves ?", Float ) = 0
        [MaterialToggle] _Switch_ON_Waves ("Switch_ON_Waves", Float ) = 0
        _Fade_Power ("Fade_Power", Float ) = 1
        _Wave_Fade_Power ("Wave_Fade_Power", Float ) = 1
        _Shadow_Depth ("Shadow_Depth", Float ) = 0.1
        _Wave_Speed ("Wave_Speed", Float ) = 6
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
            uniform float4 _TimeEditor;
            uniform float _Fade;
            uniform fixed _OPA_Ctrl;
            uniform float _Opacity;
            uniform float _Depth;
            uniform float _BulgeScale;
            uniform fixed _Waves;
            uniform fixed _Switch_ON_Waves;
            uniform float _Fade_Power;
            uniform float _Wave_Fade_Power;
            uniform float _Shadow_Depth;
            uniform float _Wave_Speed;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
                float4 projPos : TEXCOORD2;
                UNITY_FOG_COORDS(3)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 node_3825 = _Time + _TimeEditor;
                v.vertex.xyz += lerp( 0.0, ((sin(node_3825.b)/_Wave_Speed)*_BulgeScale*v.normal), _Waves );
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 normalDirection = i.normalDir;
                float sceneZ = max(0,LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)))) - _ProjectionParams.g);
                float partZ = max(0,i.projPos.z - _ProjectionParams.g);
////// Lighting:
////// Emissive:
                float node_5867 = floor(saturate((sceneZ-partZ)/_Depth));
                float3 emissive = lerp(lerp(float3(1,1,1),float3(0.1479779,0.7050467,0.875),floor(saturate((sceneZ-partZ)/_Shadow_Depth))),float3(0.3529412,0.8156863,0.9568628),node_5867);
                float3 finalColor = emissive;
                float2 node_8418 = (i.uv0*2.0+-1.0);
                float2 node_905 = (node_8418*node_8418).rg;
                float node_2859 = (node_905.r+node_905.g);
                float node_9855 = (node_2859+node_2859);
                float node_7049 = (node_9855+node_9855);
                fixed4 finalRGBA = fixed4(finalColor,lerp( lerp( _Opacity, pow((i.uv0.g*_Fade),_Fade_Power), _OPA_Ctrl ), ((1.0 - node_5867)*pow((node_7049+node_7049),_Wave_Fade_Power)), _Switch_ON_Waves ));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform float _BulgeScale;
            uniform fixed _Waves;
            uniform float _Wave_Speed;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float3 normalDir : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 node_3825 = _Time + _TimeEditor;
                v.vertex.xyz += lerp( 0.0, ((sin(node_3825.b)/_Wave_Speed)*_BulgeScale*v.normal), _Waves );
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 normalDirection = i.normalDir;
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
