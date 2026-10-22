using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

// Built-in forward renderer: no package migration, compatible with the current scene.
public sealed class ThrillerRendering : MonoBehaviour
{
    Camera view;
    Material composite, blur;
    RenderTexture glass;
    float danger;
    bool playing;
    bool captureGlass=true;
    public Texture GlassBackground => glass;
    public void Configure(Camera camera,bool frostedHud=true)
    {
        captureGlass=frostedHud;
        view=camera;
        view.renderingPath=RenderingPath.Forward;
        view.allowHDR=SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
        view.allowMSAA=true;
        view.farClipPlane=Mathf.Min(view.farClipPlane,70);
        view.backgroundColor=new Color(.014f,.025f,.035f);
        view.clearFlags=RenderSettings.skybox!=null?CameraClearFlags.Skybox:CameraClearFlags.SolidColor;
        GamePreferences.ApplyGraphics();
        QualitySettings.shadowResolution=ShadowResolution.Medium;
        QualitySettings.shadowProjection=ShadowProjection.StableFit;
        QualitySettings.shadowCascades=0;
        RenderSettings.ambientMode=AmbientMode.Trilight;
        RenderSettings.ambientSkyColor=new Color(.14f,.15f,.17f);
        RenderSettings.ambientEquatorColor=new Color(.075f,.085f,.095f);
        RenderSettings.ambientGroundColor=new Color(.045f,.048f,.052f);
        RenderSettings.ambientIntensity=1;
        RenderSettings.fog=true;
        RenderSettings.fogMode=FogMode.ExponentialSquared;
        RenderSettings.fogColor=new Color(.019f,.03f,.039f);
        RenderSettings.fogDensity=.014f;
        composite=Create("Presentation/ThrillerComposite");
        blur=Create("Presentation/ThrillerBlur");
        if(composite==null||blur==null)
            Debug.LogWarning("Thriller post effects unavailable. Lighting and HUD remain active.");
    }
    static Material Create(string path)
    {
        var shader=Resources.Load<Shader>(path);
        return shader!=null&&shader.isSupported?new Material(shader){hideFlags=HideFlags.HideAndDontSave}:null;
    }
    public void SetState(float threat,bool active)
    {
        playing=active;
        danger=Mathf.MoveTowards(danger,active?threat:0,Time.unscaledDeltaTime*2.5f);
    }
    void OnRenderImage(RenderTexture source,RenderTexture destination)
    {
        if(composite==null||blur==null) { Graphics.Blit(source,destination); return; }
        int w=Mathf.Max(1,source.width/4),h=Mathf.Max(1,source.height/4);
        var format=view.allowHDR?RenderTextureFormat.ARGBHalf:RenderTextureFormat.ARGB32;
        var a=RenderTexture.GetTemporary(w,h,0,format);
        var b=RenderTexture.GetTemporary(w,h,0,format);
        a.filterMode=b.filterMode=FilterMode.Bilinear;
        // HDR threshold limits bloom to emitters and very bright specular highlights.
        blur.SetFloat("_Threshold",1.25f);
        Graphics.Blit(source,a,blur,0);
        blur.SetVector("_Direction",new Vector4(1f/w,0,0,0)); Graphics.Blit(a,b,blur,1);
        blur.SetVector("_Direction",new Vector4(0,1f/h,0,0)); Graphics.Blit(b,a,blur,1);
        composite.SetTexture("_Bloom",a);
        composite.SetFloat("_Danger",danger);
        composite.SetFloat("_Pulse",playing?.5f+.5f*Mathf.Sin(Time.unscaledTime*5):0);
        composite.SetFloat("_Exposure",1.10f);
        if(!captureGlass||(Application.isMobilePlatform&&playing)) {
            Graphics.Blit(source,destination,composite,0);
            RenderTexture.ReleaseTemporary(a);RenderTexture.ReleaseTemporary(b);return;
        }
        var graded=RenderTexture.GetTemporary(w,h,0,format);
        if(glass==null||glass.width!=w||glass.height!=h) {
            if(glass!=null) { glass.Release(); Destroy(glass); }
            glass=new RenderTexture(w,h,0,RenderTextureFormat.ARGB32){name="HUD frosted scene",filterMode=FilterMode.Bilinear}; glass.Create();
        }
        // Use the same grading for the frosted HUD, then blur it at quarter resolution.
        Graphics.Blit(source,graded,composite,0);
        blur.SetVector("_Direction",new Vector4(2f/w,0,0,0)); Graphics.Blit(graded,b,blur,1);
        blur.SetVector("_Direction",new Vector4(0,2f/h,0,0)); Graphics.Blit(b,glass,blur,1);
        Graphics.Blit(source,destination,composite,0);
        RenderTexture.ReleaseTemporary(a); RenderTexture.ReleaseTemporary(b);RenderTexture.ReleaseTemporary(graded);
    }
    void OnDestroy()
    {
        if(composite!=null) Destroy(composite);
        if(blur!=null) Destroy(blur);
        if(glass!=null) { glass.Release(); Destroy(glass); }
    }
}

