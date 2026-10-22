Shader "GameOver/Storm rain"
{
    Properties {_Splash("Impact ring",Float)=0 _Mist("Wind haze",Float)=0}
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}
        Blend SrcAlpha OneMinusSrcAlpha Cull Off ZWrite Off
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "StormShelter.cginc"
            float _StormStrength,_StormFlash,_Splash,_Mist;
            struct appdata {float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
            struct v2f {float4 pos:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;UNITY_FOG_COORDS(2)};
            v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.uv=v.uv;o.color=v.color;UNITY_TRANSFER_FOG(o,o.pos);return o;}
            half4 frag(v2f i):SV_Target {
                clip(.5-StormSheltered(i.world));clip(i.world.y+.08);
                half alpha=pow(saturate(1-abs(i.uv.x*2-1)),2)*sin(saturate(i.uv.y)*3.14159);
                float ring=length(i.uv-.5)-.32;alpha=lerp(alpha,exp(-ring*ring*1800),_Splash);
                alpha=lerp(alpha,pow(saturate(1-length(i.uv-.5)*2),3),_Mist);
                half4 c=half4(i.color.rgb*(.65+_StormFlash*2.8),alpha*i.color.a*_StormStrength);
                UNITY_APPLY_FOG(i.fogCoord,c);return c;
            }
            ENDCG
        }
    }
    Fallback Off
}
