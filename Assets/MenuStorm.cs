using UnityEngine;

// Menu-only atmosphere. Independent sources prevent intro/gameplay audio overlap.
public sealed class MenuStorm : MonoBehaviour
{
    AudioSource rain,thunder;
    Texture2D streak,haze;
    bool visible,focused=true;
    float time,nextStrike=4,flashAge=10,thunderDelay=-1;
    readonly System.Random random=new System.Random(408);
    public void Build()
    {
        rain=Source("Menu / continuous rain","Rain",true);thunder=Source("Menu / distant thunder","Thunder",false);
        rain.volume=0;thunder.volume=0;
        streak=new Texture2D(24,128,TextureFormat.RGBA32,false){name="Menu rain streak",wrapMode=TextureWrapMode.Clamp};
        var pixels=new Color[24*128];
        for(int y=0;y<128;y++)for(int x=0;x<24;x++) {
            float t=y/127f,centre=7+t*10;
            float alpha=Mathf.Exp(-Mathf.Pow((x-centre)/1.1f,2))*Mathf.Sin(t*Mathf.PI);
            pixels[y*24+x]=new Color(.58f,.74f,.81f,alpha*.65f);
        }
        streak.SetPixels(pixels);streak.Apply(false,true);
        haze=new Texture2D(128,64,TextureFormat.RGBA32,false){name="Menu drifting storm cloud",wrapMode=TextureWrapMode.Clamp};
        pixels=new Color[128*64];
        for(int y=0;y<64;y++)for(int x=0;x<128;x++) {
            float u=x/127f,v=y/63f;
            float edge=Mathf.Sin(u*Mathf.PI)*Mathf.Sin(v*Mathf.PI);
            pixels[y*128+x]=new Color(.24f,.32f,.36f,edge*(.12f+.32f*Mathf.PerlinNoise(u*5,v*4)));
        }
        haze.SetPixels(pixels);haze.Apply(false,true);
    }
    AudioSource Source(string label,string clip,bool loop)
    {
        var source=new GameObject(label).AddComponent<AudioSource>();source.transform.SetParent(transform,false);
        source.clip=Resources.Load<AudioClip>("Presentation/Audio/"+clip);source.loop=loop;source.playOnAwake=false;
        source.spatialBlend=0;source.volume=0;source.priority=160;return source;
    }
    public void SetVisible(bool value)
    {
        if(visible==value)return;
        visible=value;
        if(!visible){flashAge=10;thunderDelay=-1;if(rain!=null){rain.Stop();rain.volume=0;}if(thunder!=null)thunder.Stop();}
    }
    void Update()
    {
        if(rain==null)return;
        float dt=Mathf.Min(Time.unscaledDeltaTime,.1f);
        bool running=visible&&focused;
        rain.volume=Mathf.MoveTowards(rain.volume,running?.16f*GamePreferences.Atmosphere:0,dt*.24f);
        thunder.volume=running?.4f*GamePreferences.Atmosphere:0;
        if(!running){if(thunder.isPlaying)thunder.Stop();if(rain.volume<=0&&rain.isPlaying)rain.Stop();return;}
        if(!rain.isPlaying&&rain.clip!=null)rain.Play();
        time+=dt;flashAge+=dt;
        if(thunderDelay>=0){thunderDelay-=dt;if(thunderDelay<=0){thunderDelay=-1;if(thunder.clip!=null)thunder.Play();}}
        if(time>=nextStrike){flashAge=0;thunderDelay=.8f+(float)random.NextDouble()*1.2f;nextStrike=time+16+(float)random.NextDouble()*15;}
    }
    void Fill(Rect rect,Color color)
    {GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);}
    void Bolt(Vector2 a,Vector2 b,float intensity)
    {
        var matrix=GUI.matrix;Vector2 delta=b-a;
        GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg,a);
        Fill(new Rect(a.x,a.y-2,delta.magnitude,4),new Color(.47f,.68f,.86f,intensity*.25f));
        Fill(new Rect(a.x,a.y-.5f,delta.magnitude,1),new Color(.82f,.92f,1,intensity));
        GUI.matrix=matrix;
    }
    public void DrawBackground()
    {
        if(streak==null)return;
        Color saved=GUI.color;
        float flash=flashAge<.7f?Mathf.Exp(-flashAge*7):0;
        if(GamePreferences.ReducedLightning)flash*=.12f;
        if(flash>.05f&&!GamePreferences.ReducedLightning) {
            Bolt(new Vector2(962,0),new Vector2(943,53),flash);
            Bolt(new Vector2(943,53),new Vector2(957,74),flash);
            Bolt(new Vector2(957,74),new Vector2(927,136),flash);
            Bolt(new Vector2(943,53),new Vector2(912,76),flash*.55f);
        }
        // The real estate camera supplies the mansion; retain only the weather overlay.
        GUI.color=new Color(1,1,1,.65f);GUI.DrawTexture(new Rect(160+Mathf.Sin(time*.025f)*90,10,1100,340),haze);
        GUI.DrawTexture(new Rect(200+Mathf.Sin(time*.04f)*120,465,1150,200),haze);
        for(int i=0;i<12;i++)Fill(new Rect(520+i*37,565+i%3*8,38+i%4*11,1),new Color(.23f,.39f,.44f,.05f+flash*.1f));
        int count=Application.isMobilePlatform?70:125;
        for(int i=0;i<count;i++) {
            float seed=Mathf.Repeat(i*.618034f,1),depth=.45f+Mathf.Repeat(i*.371f,1)*.55f;
            float x=Mathf.Repeat(i*137.7f-time*(18+depth*22),1180)-40;
            float y=Mathf.Repeat(seed*840+time*(190+depth*290),840)-100;
            // Leave the text side quieter while keeping weather visible across the screen.
            GUI.color=new Color(1,1,1,(x<460?.16f:.37f)*depth);
            GUI.DrawTexture(new Rect(x,y,7+depth*7,30+depth*50),streak);
        }
        Fill(new Rect(0,0,1100,700),new Color(.45f,.63f,.75f,flash*.14f));
        GUI.color=saved;
    }
    void OnApplicationFocus(bool value)
    {focused=value;if(!value){if(rain!=null)rain.Stop();if(thunder!=null)thunder.Stop();thunderDelay=-1;flashAge=10;}}
    void OnApplicationPause(bool value){OnApplicationFocus(!value);}
    void OnDestroy(){if(streak!=null)Destroy(streak);if(haze!=null)Destroy(haze);}
}
