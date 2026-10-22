Shader "GameOver/Lightning channel"
{
    Properties {_Color("Colour",Color)=(1,1,1,1)}
    SubShader {
        Tags {"Queue"="Transparent+10" "RenderType"="Transparent"}
        Blend SrcAlpha One Cull Off ZWrite Off
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            fixed4 _Color;float _StormFlash;
            struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
            struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
            half4 frag(v2f i):SV_Target {return half4(_Color.rgb,_Color.a*_StormFlash*saturate(1-abs(i.uv.y*2-1)));}
            ENDCG
        }
    }
    Fallback Off
}
