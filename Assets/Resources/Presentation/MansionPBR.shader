Shader "GameOver/World PBR"
{
    Properties {
        _Color("Tint",Color)=(1,1,1,1)
        _MainTex("Albedo",2D)="white"{}
        _DetailNormal("Unpacked RGB normal",2D)="bump"{}
        _Roughness("Roughness",2D)="gray"{}
        _Metallic("Metallic",Range(0,1))=0
        _Glossiness("Smoothness",Range(0,1))=.3
        _NormalStrength("Normal strength",Range(0,1))=.3
        _Tile("Repeats per metre",Float)=1
        _MirrorUV("Mirror edge tiling",Range(0,1))=0
        _LocalAO("Mapped ambient occlusion",2D)="white"{}
        _UseLocalAO("Use mapped ambient occlusion",Range(0,1))=0
        _LocalAOStrength("Mapped AO strength",Range(0,1))=.55
        _LocalAOOrigin("AO origin in world X,Z,Y",Vector)=(3.8,-7,0,0)
        _LocalAOSpan("AO span X,Z and height tolerance",Vector)=(6.2,14,.12,0)
    }
    SubShader {
        Tags {"RenderType"="Opaque"} LOD 250
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0
        #include "StormShelter.cginc"
        float _StormStrength,_StormClock;
        sampler2D _MainTex,_DetailNormal,_Roughness,_LocalAO;
        float _UseLocalAO,_LocalAOStrength;float4 _LocalAOOrigin,_LocalAOSpan;
        fixed4 _Color;half _Metallic,_Glossiness,_NormalStrength;float _Tile,_MirrorUV;
        struct Input {float3 worldPos;float3 worldNormal;INTERNAL_DATA};
        void surf(Input i,inout SurfaceOutputStandard o) {
            float3 n=normalize(WorldNormalVector(i,float3(0,0,1)));
            float3 weights=pow(abs(n),8);weights/=max(weights.x+weights.y+weights.z,.0001);
            float3 p=i.worldPos*_Tile;
            float3 mirrorSign=lerp(float3(1,1,1),1-2*step(1,frac(p*.5)*2),_MirrorUV);
            p=lerp(p,1-abs(frac(p*.5)*2-1),_MirrorUV);
            fixed3 albedo=tex2D(_MainTex,p.zy).rgb*weights.x+tex2D(_MainTex,p.xz).rgb*weights.y+tex2D(_MainTex,p.xy).rgb*weights.z;
            half rough=tex2D(_Roughness,p.zy).r*weights.x+tex2D(_Roughness,p.xz).r*weights.y+tex2D(_Roughness,p.xy).r*weights.z;
            float2 nx=(tex2D(_DetailNormal,p.zy).xy*2-1)*_NormalStrength;
            float2 ny=(tex2D(_DetailNormal,p.xz).xy*2-1)*_NormalStrength;
            float2 nz=(tex2D(_DetailNormal,p.xy).xy*2-1)*_NormalStrength;
            float3 perturb=float3(0,nx.y,nx.x)*weights.x+float3(ny.x,0,ny.y)*weights.y+float3(nz.x,nz.y,0)*weights.z;
            float3 worldN=normalize(n+perturb*mirrorSign);
            half wet=(1-StormSheltered(i.worldPos+float3(0,.15,0)))*_StormStrength;
            float2 cell=floor(i.worldPos.xz*1.4);
            float seed=frac(sin(dot(cell,float2(127.1,311.7)))*43758.5453);
            float2 delta=frac(i.worldPos.xz*1.4)-float2(.25+seed*.5,.75-seed*.5);
            float age=frac(_StormClock*.8+seed*7),r=length(delta);
            float ripple=cos(r*65-age*19)*exp(-r*7)*(1-age)*wet*saturate(n.y);
            worldN=normalize(worldN+float3(delta.x,0,delta.y)*ripple*.08);
            float3 t=normalize(WorldNormalVector(i,float3(1,0,0)));
            float3 b=normalize(WorldNormalVector(i,float3(0,1,0)));
            float3 z=normalize(WorldNormalVector(i,float3(0,0,1)));
            o.Normal=float3(dot(worldN,t),dot(worldN,b),dot(worldN,z));
            o.Albedo=albedo*_Color.rgb*lerp(1,.77,wet);o.Metallic=_Metallic;
            half film=wet*lerp(.35,.85,saturate(n.y))*smoothstep(.12,.8,rough+seed*.3);
            o.Smoothness=lerp(saturate(_Glossiness*2*(1-rough)),.91,film);o.Occlusion=1;o.Alpha=1;
            if(_UseLocalAO>.5) {
                float2 uv=(i.worldPos.xz-_LocalAOOrigin.xy)/max(_LocalAOSpan.xy,float2(.001,.001));
                float inside=step(0,uv.x)*step(uv.x,1)*step(0,uv.y)*step(uv.y,1)*step(.9,n.y);
                inside*=1-step(max(_LocalAOSpan.z,.05),abs(i.worldPos.y-_LocalAOOrigin.z));
                o.Occlusion=lerp(1,tex2D(_LocalAO,saturate(uv)).r,saturate(_LocalAOStrength)*inside);
            }
        }
        ENDCG
    }
    Fallback "Standard"
}
