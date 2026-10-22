Shader "Hidden/GameOver/ThrillerBlur"
{
    Properties { _MainTex("Source",2D)="black"{} }
    SubShader {
        Cull Off ZWrite Off ZTest Always
        CGINCLUDE
        #include "UnityCG.cginc"
        sampler2D _MainTex; float4 _MainTex_TexelSize; float4 _Direction; float _Threshold;
        half4 Bright(v2f_img i):SV_Target {
            half3 c=tex2D(_MainTex,i.uv).rgb;
            half peak=max(c.r,max(c.g,c.b));
            return half4(c*max(peak-_Threshold,0)/max(peak,.0001),1);
        }
        half4 Blur(v2f_img i):SV_Target {
            half3 c=tex2D(_MainTex,i.uv).rgb*.227027;
            c+=(tex2D(_MainTex,i.uv+_Direction.xy*1.384615).rgb+tex2D(_MainTex,i.uv-_Direction.xy*1.384615).rgb)*.316216;
            c+=(tex2D(_MainTex,i.uv+_Direction.xy*3.230769).rgb+tex2D(_MainTex,i.uv-_Direction.xy*3.230769).rgb)*.070270;
            return half4(c,1);
        }
        ENDCG
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment Bright
            ENDCG }
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment Blur
            ENDCG }
    }
    Fallback Off
}
