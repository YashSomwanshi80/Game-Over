Shader "GameOver/Scanned surface"
{
 Properties {
  _MainTex("Albedo",2D)="white"{} _Normal("RGB tangent normal",2D)="bump"{} _ARM("Roughness / metallic channels",2D)="white"{} _AO("Occlusion, only if authored",2D)="white"{}
  _AOStrength("Authored occlusion strength",Range(0,1))=0
  _Color("Tint",Color)=(1,1,1,1) _Metallic("Metal factor",Range(0,1))=1 _RoughFactor("Roughness factor",Range(0,1))=1 _Cutoff("Alpha clip",Range(0,1))=.4
 }
 SubShader {
  Tags {"Queue"="AlphaTest" "RenderType"="TransparentCutout"} LOD 250
  Cull Off
  CGPROGRAM
  #pragma surface surf Standard fullforwardshadows addshadow alphatest:_Cutoff
  #pragma target 3.0
  // alphatest generates the _Cutoff uniform; declaring it here duplicates it.
  sampler2D _MainTex,_Normal,_ARM,_AO;fixed4 _Color;half _Metallic,_RoughFactor,_AOStrength;
  struct Input {float2 uv_MainTex;};
  void surf(Input i,inout SurfaceOutputStandard o) {
   fixed4 color=tex2D(_MainTex,i.uv_MainTex)*_Color;
   half3 arm=tex2D(_ARM,i.uv_MainTex).rgb;
   o.Albedo=color.rgb;o.Alpha=color.a;o.Normal=normalize(tex2D(_Normal,i.uv_MainTex).xyz*2-1);
   o.Occlusion=lerp(1,tex2D(_AO,i.uv_MainTex).r,_AOStrength);o.Smoothness=saturate(1-arm.g*_RoughFactor);o.Metallic=arm.b*_Metallic;
  }
  ENDCG
 }
 Fallback "Transparent/Cutout/Diffuse"
}
