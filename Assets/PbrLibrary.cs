using UnityEngine;
using System.Collections.Generic;

public sealed class PbrLibrary : MonoBehaviour
{
    readonly List<Material> owned=new List<Material>();
    public Material Plaster,Floor,Walnut,Fabric,Linen,Marble,Metal,Brass,Glass,Skin,Hair,Rubber;
    public Material WarmEmitter,GreenEmitter,RedEmitter,ScreenEmitter,Leather,Porcelain,CharacterCloth,Travertine,ClearLens;
    public Material Nails,Lips,SkinCrease,EyeWhite;
    public Material EstateStone,EstatePaving,EstateEarth,EstateWater,EstateGlow,EstateSage;
    public void Build()
    {
        Plaster=Surface("Limestone plaster","Plaster",new Color(.64f,.62f,.58f),0,.17f,1.5f,.12f);
        Travertine=Surface("Honed ivory travertine","Travertine",new Color(.78f,.77f,.73f),0,.26f,1,.1f);
        Travertine.SetTexture("_DetailNormal",Resources.Load<Texture2D>("Presentation/Textures/Plaster_Normal"));Travertine.SetTexture("_Roughness",Resources.Load<Texture2D>("Presentation/Textures/Plaster_Roughness"));
        Floor=Surface("Oiled parquet / 2 m repeat","Parquet",Color.white,0,.32f,.5f,.28f);
        Floor.SetFloat("_MirrorUV",1);
        Walnut=Surface("American walnut","Walnut",new Color(.48f,.37f,.27f),0,.24f,.7f,.24f);
        Fabric=Surface("Woven charcoal upholstery","Fabric",new Color(.18f,.26f,.28f),0,.06f,3,.24f);
        Linen=Surface("Warm linen","Fabric",new Color(.72f,.70f,.62f),0,.08f,3,.18f);
        Marble=Surface("Honed graphite marble","Marble",Color.white,0,.36f,.6f,.12f);
        Metal=Surface("Brushed gunmetal","BrushedMetal",new Color(.29f,.32f,.34f),.85f,.56f,3,.12f);
        Brass=Surface("Aged satin brass","BrushedMetal",new Color(.52f,.34f,.14f),.8f,.51f,3,.08f);
        Glass=Surface("Smoked reflective glass","Plaster",new Color(.035f,.065f,.08f),.42f,.94f,1,0);
        ClearLens=Plain(new Color(.55f,.68f,.73f,.14f),.94f);ClearLens.name="Clear spectacle lens";
        ClearLens.SetFloat("_Mode",3);ClearLens.SetInt("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.SrcAlpha);ClearLens.SetInt("_DstBlend",(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);ClearLens.SetInt("_ZWrite",0);ClearLens.EnableKeyword("_ALPHABLEND_ON");ClearLens.renderQueue=3000;
        Skin=LocalSurface("Natural skin","Skin",new Color(.57f,.35f,.24f),.27f);
        Nails=Plain(new Color(.62f,.43f,.35f),.4f);Lips=Plain(new Color(.36f,.16f,.13f),.28f);
        SkinCrease=Plain(new Color(.22f,.115f,.075f),.15f);EyeWhite=Plain(new Color(.78f,.75f,.68f),.36f);
        Hair=LocalSurface("Dark hair","Leather",new Color(.024f,.027f,.033f),.22f);
        Leather=LocalSurface("Soft black leather","Leather",new Color(.035f,.031f,.028f),.38f);
        CharacterCloth=LocalSurface("Tailored charcoal cloth","Fabric",new Color(.14f,.16f,.18f),.12f);
        Porcelain=Surface("Glazed ivory ceramic","Porcelain",new Color(.84f,.83f,.79f),0,.72f,1,.025f);
        Rubber=Surface("Rubber","Fabric",new Color(.027f,.03f,.031f),0,.08f,2,.1f);
        WarmEmitter=Emitter("Warm diffuser",new Color(1,.67f,.33f),3);
        GreenEmitter=Emitter("Security diode / online",new Color(.08f,.8f,.48f),2.4f);
        RedEmitter=Emitter("Security diode / offline",new Color(1,.12f,.035f),2.4f);
        ScreenEmitter=Emitter("Telephone LCD",new Color(.17f,.64f,.57f),1.7f);
        // Scanned, metre-scaled surfaces replace the earlier synthetic architectural maps.
        EstateStone=Photo(Travertine,"Estate sandstone","Sandstone",Color.white,.45f,.22f,.20f);
        EstatePaving=Tint(Travertine,new Color(.48f,.49f,.47f),"Estate honed limestone paving");
        EstatePaving.SetFloat("_Glossiness",.22f);
        EstateEarth=Tint(Plaster,new Color(.075f,.10f,.075f),"Estate dark garden ground");EstateEarth.SetFloat("_Glossiness",.03f);
        EstateWater=Tint(Glass,new Color(.035f,.11f,.12f),"Estate shallow reflection basin");
        EstateGlow=Emitter("Independent estate boundary lanterns",new Color(1,.72f,.4f),1.6f);
        var scannedFloor=Photo(Floor,"Estate smoked oak boards","Oak",Color.white,.4f,.24f,.20f);
        Floor=scannedFloor;
        Plaster.color=new Color(.78f,.75f,.68f);Plaster.SetFloat("_NormalStrength",.07f);
        Fabric.color=new Color(.16f,.22f,.23f);EstateSage=Tint(Fabric,new Color(.28f,.32f,.27f),"Estate sage upholstery");
    }
    Material Photo(Material source,string label,string id,Color tint,float tile,float smooth,float normal)
    {
        var m=Tint(source,tint,label);
        m.SetTexture("_MainTex",Resources.Load<Texture2D>("Estate/Textures/"+id+"_Color"));
        m.SetTexture("_DetailNormal",Resources.Load<Texture2D>("Estate/Textures/"+id+"_Normal"));
        m.SetTexture("_Roughness",Resources.Load<Texture2D>("Estate/Textures/"+id+"_Roughness"));
        m.SetFloat("_Tile",tile);m.SetFloat("_MirrorUV",0);m.SetFloat("_Glossiness",smooth);m.SetFloat("_NormalStrength",normal);return m;
    }
    Material Surface(string label,string texture,Color color,float metal,float smooth,float scale,float normal)
    {
        var shader=Resources.Load<Shader>("Presentation/MansionPBR");
        var m=new Material(shader!=null?shader:Shader.Find("Standard")){name=label};
        m.color=color;m.SetFloat("_Metallic",metal);m.SetFloat("_Glossiness",smooth);
        m.SetFloat("_Tile",scale);m.SetFloat("_NormalStrength",normal);
        m.SetTexture("_MainTex",Resources.Load<Texture2D>("Presentation/Textures/"+texture+"_Color"));
        m.SetTexture("_DetailNormal",Resources.Load<Texture2D>("Presentation/Textures/"+texture+"_Normal"));
        m.SetTexture("_Roughness",Resources.Load<Texture2D>("Presentation/Textures/"+texture+"_Roughness"));
        m.enableInstancing=true;owned.Add(m);return m;
    }
    // Mesh UVs stay attached to moving arms, fingers and clothing.
    Material LocalSurface(string label,string texture,Color color,float smooth)
    {
        var m=new Material(Shader.Find("Standard")){name=label,color=color};
        m.mainTexture=Resources.Load<Texture2D>("Presentation/Textures/"+texture+"_Color");
        m.SetFloat("_Glossiness",smooth);m.enableInstancing=true;owned.Add(m);return m;
    }
    public Material Emitter(string label,Color color,float intensity)
    {
        var m=new Material(Shader.Find("Standard")){name=label,color=color*.3f};
        m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*intensity);
        m.SetFloat("_Glossiness",.6f);owned.Add(m);return m;
    }
    public Material Plain(Color c,float smooth=.25f,float metallic=0)
    {
        var m=new Material(Shader.Find("Standard")){color=c};m.SetFloat("_Glossiness",smooth);m.SetFloat("_Metallic",metallic);owned.Add(m);return m;
    }
    public Material Tint(Material source,Color color,string label)
    {
        var material=new Material(source){name=label,color=color};owned.Add(material);return material;
    }
    public Material ResourceMaterial(string path,string label)
    {
        var shader=Resources.Load<Shader>(path);
        var material=new Material(shader!=null?shader:Shader.Find("Standard")){name=label};owned.Add(material);return material;
    }
    void OnDestroy() {foreach(var material in owned) if(material!=null){if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);}}
}
