Shader "GameOver/Nocturnal sky"
{
    SubShader {
        Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"}
        Cull Off ZWrite Off
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            float _StormStrength,_StormFlash,_StormClock;
            struct appdata {float4 vertex:POSITION;};
            struct v2f {float4 pos:SV_POSITION;float3 direction:TEXCOORD0;};
            v2f vert(appdata v) {v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.direction=mul((float3x3)unity_ObjectToWorld,v.vertex.xyz);return o;}
            float hash(float2 p) {return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            half4 frag(v2f i):SV_Target {
                float3 d=normalize(i.direction);
                half3 c=lerp(half3(.035,.054,.068),half3(.005,.012,.026),pow(saturate(d.y),.45));
                float cloud=sin(d.x*17+d.z*9+_StormClock*.016+sin(d.z*11))*sin(d.z*23-d.y*9-_StormClock*.022);
                c*=1-saturate(cloud*.7+.25)*.26;
                float2 uv=float2(atan2(d.z,d.x)/6.283185,asin(clamp(d.y,-1,1))/3.141593)*float2(600,300);
                float star=step(.998,hash(floor(uv)))*(1-smoothstep(.02,.13,length(frac(uv)-.5)))*smoothstep(.08,.28,d.y);
                c+=star*half3(.23,.28,.31)*(1-_StormStrength);
                float angle=dot(d,normalize(float3(-.50,.55,-.65)));
                float moon=smoothstep(.99973,.99984,angle),halo=pow(saturate(angle),180)*.012;
                c+=moon*half3(.42,.45,.44)+halo*half3(.25,.36,.48);
                c*=1-_StormStrength*.32;
                c+=_StormFlash*half3(.25,.35,.52)*(.65+cloud*.18);
                c=lerp(half3(.01,.018,.022),c,smoothstep(-.15,.03,d.y));
                return half4(c,1);
            }
            ENDCG
        }
    }
    Fallback Off
}
