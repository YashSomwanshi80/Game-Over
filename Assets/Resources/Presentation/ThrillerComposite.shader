Shader "Hidden/GameOver/ThrillerComposite"
{
    Properties { _MainTex("Source",2D)="black"{} _Bloom("Bloom",2D)="black"{} }
    SubShader {
        Cull Off ZWrite Off ZTest Always
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex,_Bloom;float _Danger,_Pulse,_Exposure;
            half3 ACES(half3 x) { return saturate((x*(2.51*x+.03))/(x*(2.43*x+.59)+.14)); }
            half4 frag(v2f_img i):SV_Target {
                half3 c=tex2D(_MainTex,i.uv).rgb;
                c+=tex2D(_Bloom,i.uv).rgb*.08;
                half l=dot(c,half3(.2126,.7152,.0722));
                c=lerp(l.xxx,c,.96);
                c*=lerp(half3(.95,.985,1.02),half3(1.015,1.005,.985),saturate(l));
                c=ACES(c*_Exposure);
                float2 p=i.uv*2-1;
                float edge=smoothstep(.18,1.2,dot(p,p)*.63);
                c*=1-edge*(.14+_Danger*(.09+.05*_Pulse));
                c=lerp(c,c*half3(1.15,.66,.61),edge*_Danger*(.08+.04*_Pulse));
                return half4(c,1);
            }
            ENDCG }
    }
    Fallback Off
}
