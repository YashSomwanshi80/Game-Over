Shader "GameOver/Character hair"
{
 Properties {
  _MainTex("Hair colour / coverage",2D)="white"{}
  _Color("Tint",Color)=(1,1,1,1)
  _Glossiness("Smoothness",Range(0,1))=.18
  _Cutoff("Coverage clip",Range(0,1))=.38
 }
 SubShader {
  Tags {"Queue"="AlphaTest" "RenderType"="TransparentCutout"}
  Cull Off
  CGPROGRAM
  #pragma surface surf Standard fullforwardshadows addshadow alphatest:_Cutoff
  #pragma target 3.0
  sampler2D _MainTex;fixed4 _Color;half _Glossiness;
  struct Input {float2 uv_MainTex;float facing:VFACE;};
  void surf(Input i,inout SurfaceOutputStandard o) {
   fixed4 color=tex2D(_MainTex,i.uv_MainTex)*_Color;
   // Cull Off exposes both sides: flip the tangent-space normal on back faces.
   o.Normal=float3(0,0,i.facing>=0?1:-1);
   o.Albedo=color.rgb;o.Alpha=color.a;o.Metallic=0;o.Smoothness=_Glossiness;
  }
  ENDCG
 }
 Fallback "Transparent/Cutout/Diffuse"
}
