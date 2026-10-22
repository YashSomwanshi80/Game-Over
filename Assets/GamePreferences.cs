using UnityEngine;

public static class GamePreferences
{
    static bool loaded;
    static int frameRate=60,graphics=2; // defaults match the original behaviour: 60 FPS, HIGH
    static DonDifficulty difficulty;
    static float master,atmosphere,mouseSensitivity,touchSensitivity;
    static bool bob,quickGuide,reducedLightning,invertLook;
    static void Load()
    {
        if(loaded)return;loaded=true;
        difficulty=DonDifficultyRules.Valid(PlayerPrefs.GetInt("GameOver.DonDifficulty",1));
        master=Mathf.Clamp01(PlayerPrefs.GetFloat("GameOver.MasterVolume",.85f));
        atmosphere=Mathf.Clamp01(PlayerPrefs.GetFloat("GameOver.AtmosphereVolume",.75f));
        bob=PlayerPrefs.GetInt("GameOver.CameraBob",1)!=0;
        quickGuide=PlayerPrefs.GetInt("GameOver.QuickGuide",1)!=0;
        reducedLightning=PlayerPrefs.GetInt("GameOver.ReducedLightning",0)!=0;
        mouseSensitivity=FirstPersonLookRules.Sensitivity(PlayerPrefs.GetFloat("GameOver.MouseSensitivity",1));
        touchSensitivity=FirstPersonLookRules.Sensitivity(PlayerPrefs.GetFloat("GameOver.TouchSensitivity",1));
        invertLook=PlayerPrefs.GetInt("GameOver.InvertLook",0)!=0;
        int fps=PlayerPrefs.GetInt("GameOver.FrameRate",60);frameRate=fps==30||fps==45?fps:60;
        graphics=Mathf.Clamp(PlayerPrefs.GetInt("GameOver.Graphics",2),0,2);
    }
    public static DonDifficulty Difficulty
    {
        get{Load();return difficulty;}
        set{Load();difficulty=DonDifficultyRules.Valid((int)value);PlayerPrefs.SetInt("GameOver.DonDifficulty",(int)difficulty);}
    }
    public static float Master
    {
        get{Load();return master;}
        set{Load();master=Mathf.Clamp01(value);AudioListener.volume=master;PlayerPrefs.SetFloat("GameOver.MasterVolume",master);}
    }
    public static float Atmosphere
    {
        get{Load();return atmosphere;}
        set{Load();atmosphere=Mathf.Clamp01(value);PlayerPrefs.SetFloat("GameOver.AtmosphereVolume",atmosphere);}
    }
    public static bool CameraBob
    {
        get{Load();return bob;}
        set{Load();bob=value;PlayerPrefs.SetInt("GameOver.CameraBob",bob?1:0);}
    }
    public static void Apply()
    {
        Load();AudioListener.volume=master;
        if(Application.isMobilePlatform){QualitySettings.vSyncCount=0;Application.targetFrameRate=frameRate;}
        else{if(frameRate!=60)QualitySettings.vSyncCount=0;Application.targetFrameRate=frameRate==60?-1:frameRate;}
    }
    // Frame-rate cap: 30, 45 or 60 (default). Lower values run cooler and save battery.
    public static int FrameRate
    {
        get{Load();return frameRate;}
        set{Load();frameRate=value==30||value==45?value:60;PlayerPrefs.SetInt("GameOver.FrameRate",frameRate);Apply();}
    }
    // Graphics level: 0 LOW, 1 MEDIUM, 2 HIGH (default). HIGH on desktop is the original look; on phones it is tuned lighter to run cooler.
    public static int Graphics
    {
        get{Load();return graphics;}
        set{Load();graphics=Mathf.Clamp(value,0,2);PlayerPrefs.SetInt("GameOver.Graphics",graphics);ApplyGraphics();}
    }
    public static void ApplyGraphics()
    {
        Load();bool mobile=Application.isMobilePlatform;
        QualitySettings.pixelLightCount=PixelLightsFor(graphics,mobile);
        QualitySettings.shadows=graphics==0?ShadowQuality.Disable:ShadowQuality.All;
        QualitySettings.shadowDistance=ShadowDistanceFor(graphics,mobile);
        QualitySettings.shadowResolution=mobile?ShadowResolution.Low:ShadowResolution.Medium;
        QualitySettings.antiAliasing=AntiAliasingFor(graphics,mobile);
        QualitySettings.resolutionScalingFixedDPIFactor=RenderScaleFor(graphics,mobile); // only takes effect where the player uses Fixed DPI scaling (Android)
    }
    // Per-level tuning. Phones run cooler with fewer pixel lights and shadows and a lower render scale; desktop values are unchanged.
    public static int PixelLightsFor(int level,bool mobile)=>mobile?(level==2?3:2):level==2?6:level==1?3:2;
    public static float ShadowDistanceFor(int level,bool mobile)=>mobile?(level==2?12:level==1?9:6):level==2?23:level==1?10:6;
    public static int AntiAliasingFor(int level,bool mobile)=>!mobile&&level==2?2:0;
    public static float RenderScaleFor(int level,bool mobile)=>!mobile?1:level==2?.85f:level==1?.75f:.65f;
    public static int MaxLightsFor(int level,bool mobile)=>mobile?(level==2?7:level==1?6:5):level==2?14:level==1?7:5;
    public static int MaxShadowLightsFor(int level,bool mobile)=>mobile?(level==0?0:1):level==2?3:level==1?1:0;
    public static int MaxLights(bool mobile){Load();return MaxLightsFor(graphics,mobile);}
    public static int MaxShadowLights(bool mobile){Load();return MaxShadowLightsFor(graphics,mobile);}
    public static LightShadows FlashlightShadows(bool mobile){Load();return graphics==0?LightShadows.None:graphics==1||mobile?LightShadows.Hard:LightShadows.Soft;}
    public static bool QuickGuide
    {
        get{Load();return quickGuide;}
        set{Load();quickGuide=value;PlayerPrefs.SetInt("GameOver.QuickGuide",quickGuide?1:0);}
    }
    public static float MouseSensitivity
    {
        get{Load();return mouseSensitivity;}
        set{Load();mouseSensitivity=FirstPersonLookRules.Sensitivity(value);PlayerPrefs.SetFloat("GameOver.MouseSensitivity",mouseSensitivity);}
    }
    public static float TouchSensitivity
    {
        get{Load();return touchSensitivity;}
        set{Load();touchSensitivity=FirstPersonLookRules.Sensitivity(value);PlayerPrefs.SetFloat("GameOver.TouchSensitivity",touchSensitivity);}
    }
    public static bool InvertLook
    {
        get{Load();return invertLook;}
        set{Load();invertLook=value;PlayerPrefs.SetInt("GameOver.InvertLook",value?1:0);}
    }
    public static Vector2 MouseLook(Vector2 delta)
    {
        return new Vector2(FirstPersonLookRules.Mouse(delta.x,MouseSensitivity),FirstPersonLookRules.Vertical(FirstPersonLookRules.Mouse(delta.y,MouseSensitivity),InvertLook));
    }
    public static Vector2 TouchLook(Vector2 delta)
    {
        var area=Screen.safeArea;
        return new Vector2(FirstPersonLookRules.Touch(delta.x,area.width,area.height,TouchSensitivity),FirstPersonLookRules.Vertical(FirstPersonLookRules.Touch(delta.y,area.width,area.height,TouchSensitivity),InvertLook));
    }
    public static void ResetLook(){MouseSensitivity=1;TouchSensitivity=1;InvertLook=false;}
    public static void Save(){if(loaded)PlayerPrefs.Save();}
    public static bool ReducedLightning
    {
        get{Load();return reducedLightning;}
        set{Load();reducedLightning=value;PlayerPrefs.SetInt("GameOver.ReducedLightning",value?1:0);}
    }
}
