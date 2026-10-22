Shader "GameOver/Tailored coat"
{
    Properties {_Color("Coat",Color)=(.026,.033,.043,1) _Glossiness("Smoothness",Range(0,1))=.22}
    SubShader {
        Tags {"RenderType"="Opaque"} LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0
        fixed4 _Color;half _Glossiness;
        struct Input {float3 viewDir;float3 worldPos;};
        void surf(Input i,inout SurfaceOutputStandard o) {
            half weave=sin(i.worldPos.y*850)*sin(i.worldPos.x*850)*.035;
            o.Albedo=_Color.rgb*(1+weave);o.Smoothness=_Glossiness;o.Metallic=.03;
            o.Emission=half3(.03,.052,.065)*pow(1-saturate(dot(normalize(i.viewDir),float3(0,0,1))),4);
            o.Alpha=1;
        }
        ENDCG
    }
    Fallback "Standard"
}
