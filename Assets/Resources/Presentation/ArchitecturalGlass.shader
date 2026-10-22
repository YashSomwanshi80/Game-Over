Shader "GameOver/Architectural glass"
{
    Properties { _Color("Tint",Color)=(.16,.29,.32,.09) }
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 180
        CGPROGRAM
        #pragma surface surf Standard alpha:fade
        #pragma target 3.0
        fixed4 _Color;float _StormStrength,_StormClock;
        struct Input {float3 viewDir;float3 worldPos;float3 worldNormal;INTERNAL_DATA};
        void surf(Input i,inout SurfaceOutputStandard o) {
            half edge=pow(1-saturate(dot(normalize(i.viewDir),float3(0,0,1))),4);
            o.Albedo=_Color.rgb;o.Metallic=.12;o.Smoothness=.94;
            o.Alpha=_Color.a+edge*.22;
            // World-space rivulets stay on the glazing, never on the camera lens.
            float3 wn=WorldNormalVector(i,float3(0,0,1));
            float2 uv=abs(wn.x)>.5?i.worldPos.zy:i.worldPos.xy;
            float2 cell=floor(uv*float2(12,5));
            float seed=frac(sin(dot(cell,float2(127.1,311.7)))*43758.5453);
            float2 q=frac(uv*float2(12,5)+float2(0,_StormClock*(.10+seed*.16)))-.5;
            float drop=exp(-dot(q*float2(7,2),q*float2(7,2))*12)*step(.28,seed);
            float trail=exp(-q.x*q.x*1200)*(1-smoothstep(-.2,.5,q.y))*.2;
            float exposed=step(6.7,abs(i.worldPos.z))+step(9.7,abs(i.worldPos.x));
            float wet=saturate(exposed)*_StormStrength;
            o.Normal=normalize(float3(q.x*drop*wet*.6,-drop*wet*.15,1));
            o.Alpha=saturate(o.Alpha+(drop+trail)*wet*.18);
        }
        ENDCG
    }
    Fallback "Transparent/VertexLit"
}
