using UnityEngine;
using System.Collections.Generic;

public sealed class TacticalHud
{
    public const int None=0,Phone=1,Power=2,Hide=3,Leave=4,Entrance=5,Door=6,Item=7;
    public static readonly Rect Joystick=new Rect(30,480,180,180);
    public static Rect Action=>new Rect(800+WideOffset*.35f,402,120,120);
    public static Rect CameraButton=>new Rect(920+WideOffset,24,64,56);
    public static Rect FlashlightButton=>new Rect(980+WideOffset,284,76,70);
    public static Rect PauseButton=>new Rect(1030+WideOffset,22,42,42);
    public static readonly Rect GuideDismiss=new Rect(42,370,250,40);
    public static Rect CrouchButton=>new Rect(980+WideOffset,500,80,76);
    public static Rect JumpButton=>new Rect(980+WideOffset,592,80,76);
    public static float EdgeOffset(float width,float height){float s=Mathf.Max(.001f,Mathf.Min(width/1100,height/700));return Mathf.Max(0,(width/s-1100)*.5f);}
    static float WideOffset=>Application.isMobilePlatform?EdgeOffset(Screen.safeArea.width,Screen.safeArea.height):0;
    public static Rect LookArea=>new Rect(340,110,590+WideOffset,560);
    public static Rect MinimapPanel=>new Rect(26,24,180,168);
    readonly MansionMinimap minimap=new MansionMinimap();
    public void DrawMinimap(Vector3 position,float yaw,Vector3 phone,bool called,bool cameraDots=false){SetMatrix();minimap.Draw(MinimapPanel,position,yaw,phone,called,cameraDots);}
    bool guideShown,controlPreferences;
    public struct State
    {
        public bool called,hidden,powered,motion,cameras,jammed;
        public float rescue,progress,danger,time;
        public int context;public string floor,room,notice;
        public Vector3 prompt;public Camera view;
        public Texture feed,glass;
        public Vector2 move;
        public bool guideVisible;public int guideStep;public string guideTitle,guideCopy;
        public float phoneBearing,phoneDistance;
        public bool crouched,sprinting,grounded,sprintArmed;public float stamina;public string hideAction,hideName;
    }
    readonly Dictionary<string,Texture2D> icons=new Dictionary<string,Texture2D>();
    Texture2D disc,ring,radial;
    int radialStep=-1;
    int presentedContext;float actionBegan;
    GUIStyle label,caption,heading,status,centeredCaption,wrappedNotice,mobileNotice,actionCaption,pauseTitle,pauseCopy,pauseHelp,pauseButton,sliderTrack,sliderThumb,resultTitle;
    readonly Color32[] radialPixels=new Color32[96*96];
    readonly Color white=new Color(.89f,.94f,.94f),muted=new Color(.52f,.63f,.65f),green=new Color(.21f,.85f,.64f),amber=new Color(1,.67f,.26f),red=new Color(1,.23f,.29f);
    float scale,offsetX,offsetY;
    public void SetMatrix()
    {
        Rect safe=Screen.safeArea;
        scale=Mathf.Min(safe.width/1100,safe.height/700);
        offsetX=safe.x+(safe.width-1100*scale)/2;
        offsetY=Screen.height-safe.yMax+(safe.height-700*scale)/2;
        GUI.matrix=Matrix4x4.TRS(new Vector3(offsetX,offsetY,0),Quaternion.identity,new Vector3(scale,scale,1));
    }
    public Vector2 Position(Vector2 screen)
    {
        Rect safe=Screen.safeArea;float s=Mathf.Min(safe.width/1100,safe.height/700);
        return new Vector2((screen.x-safe.x-(safe.width-1100*s)/2)/s,(Screen.height-screen.y-(Screen.height-safe.yMax)-(safe.height-700*s)/2)/s);
    }
    static bool MenuButton(Rect rect,string text,GUIStyle style){if(!GUI.Button(rect,text,style))return false;MenuClickSound.Play();return true;}
    public bool IsGuideControl(Vector2 p)=>guideShown&&GuideDismiss.Contains(p);
    public bool IsControl(Vector2 p) => MinimapPanel.Contains(p)||FlashlightButton.Contains(p)||Joystick.Contains(p)||Action.Contains(p)||CameraButton.Contains(p)||PauseButton.Contains(p)||CrouchButton.Contains(p)||JumpButton.Contains(p)||(guideShown&&GuideDismiss.Contains(p));
    void Init()
    {
        if(label!=null) return;
        var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label=new GUIStyle{font=font,fontSize=18,alignment=TextAnchor.MiddleLeft,clipping=TextClipping.Clip};label.normal.textColor=white;
        caption=new GUIStyle(label){fontSize=12};caption.normal.textColor=muted;
        heading=new GUIStyle(label){fontSize=14,fontStyle=FontStyle.Bold};
        status=new GUIStyle(label){fontSize=17,fontStyle=FontStyle.Bold,wordWrap=false};
        centeredCaption=new GUIStyle(caption){alignment=TextAnchor.MiddleCenter};
        wrappedNotice=new GUIStyle(label){fontSize=16,wordWrap=true};
        mobileNotice=new GUIStyle(wrappedNotice){fontSize=22,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};
        actionCaption=new GUIStyle(centeredCaption){fontSize=16,fontStyle=FontStyle.Bold};
        pauseTitle=new GUIStyle(status){fontSize=32};pauseCopy=new GUIStyle(label){wordWrap=true};pauseHelp=new GUIStyle(caption){wordWrap=true};
        resultTitle=new GUIStyle(status){fontSize=42};
        pauseButton=new GUIStyle(GUI.skin.button){fontSize=18};
        disc=Make((p)=>(.5f-p.magnitude)*96);
        ring=Make((p)=>(.018f-Mathf.Abs(p.magnitude-.44f))*96);
        sliderTrack=new GUIStyle(GUI.skin.horizontalSlider){fixedHeight=4};sliderTrack.normal.background=Texture2D.whiteTexture;
        sliderThumb=new GUIStyle(GUI.skin.horizontalSliderThumb){fixedWidth=20,fixedHeight=20};
        sliderThumb.normal.background=sliderThumb.hover.background=sliderThumb.active.background=disc;
        foreach(string name in new[]{"camera","phone","power","eye","hide","pause","arrow","sprint","crouch","jump"}) icons[name]=Make(p=>IconDistance(name,p)*96);
        radial=new Texture2D(96,96,TextureFormat.RGBA32,false){name="Call radial progress",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
    }
    Texture2D Make(System.Func<Vector2,float> coverage)
    {
        var t=new Texture2D(96,96,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
        var data=new Color32[96*96];
        for(int y=0;y<96;y++) for(int x=0;x<96;x++) {float a=Mathf.Clamp01(coverage(new Vector2((x+.5f)/96-.5f,(y+.5f)/96-.5f))+.5f);data[y*96+x]=new Color(1,1,1,a);}
        t.SetPixels32(data);t.Apply(false,true);return t;
    }
    static float Segment(Vector2 p,Vector2 a,Vector2 b)
    {var v=b-a;return (p-a-v*Mathf.Clamp01(Vector2.Dot(p-a,v)/v.sqrMagnitude)).magnitude;}
    static float BoxLine(Vector2 p,Vector2 half)
    {var a=new Vector2(Mathf.Abs(p.x),Mathf.Abs(p.y))-half;return Mathf.Abs(Mathf.Max(a.x,a.y));}
    static float IconDistance(string icon,Vector2 p)
    {
        float d=1;
        if(icon=="camera") {d=BoxLine(p+new Vector2(.06f,0),new Vector2(.23f,.18f));d=Mathf.Min(d,Segment(p,new Vector2(.2f,.1f),new Vector2(.38f,.22f)));d=Mathf.Min(d,Segment(p,new Vector2(.38f,.22f),new Vector2(.38f,-.22f)));d=Mathf.Min(d,Segment(p,new Vector2(.38f,-.22f),new Vector2(.2f,-.1f)));}
        else if(icon=="power") {d=Mathf.Abs(p.magnitude-.29f);if(p.y>.16f&&Mathf.Abs(p.x)<.16f)d=1;d=Mathf.Min(d,Segment(p,new Vector2(0,.36f),new Vector2(0,.02f)));}
        else if(icon=="phone") {d=Segment(p,new Vector2(-.2f,.22f),new Vector2(.20f,-.22f));d=Mathf.Min(d,Segment(p,new Vector2(-.2f,.22f),new Vector2(-.02f,.3f)));d=Mathf.Min(d,Segment(p,new Vector2(.2f,-.22f),new Vector2(.3f,-.04f)));}
        else if(icon=="eye") {float eye=Mathf.Abs(p.y)-.22f*(1-Mathf.Pow(Mathf.Clamp01(Mathf.Abs(p.x)/.4f),1.5f));d=Mathf.Abs(eye);if(Mathf.Abs(p.x)>.4f)d=1;d=Mathf.Min(d,Mathf.Abs(p.magnitude-.09f));}
        else if(icon=="hide") {d=BoxLine(p,new Vector2(.27f,.35f));d=Mathf.Min(d,Segment(p,new Vector2(0,-.34f),new Vector2(0,.34f)));d=Mathf.Min(d,Segment(p,new Vector2(-.07f,-.02f),new Vector2(-.07f,.09f)));d=Mathf.Min(d,Segment(p,new Vector2(.07f,-.02f),new Vector2(.07f,.09f)));}
        else if(icon=="pause") {d=Mathf.Min(Segment(p,new Vector2(-.13f,-.25f),new Vector2(-.13f,.25f)),Segment(p,new Vector2(.13f,-.25f),new Vector2(.13f,.25f)));}
        else if(icon=="sprint") {
            d=Mathf.Abs((p-new Vector2(.09f,.28f)).magnitude-.065f);
            d=Mathf.Min(d,Segment(p,new Vector2(.04f,.16f),new Vector2(-.1f,-.07f)));
            d=Mathf.Min(d,Segment(p,new Vector2(-.03f,.06f),new Vector2(.22f,.02f)));
            d=Mathf.Min(d,Segment(p,new Vector2(-.03f,.06f),new Vector2(-.26f,.10f)));
            d=Mathf.Min(d,Segment(p,new Vector2(-.1f,-.07f),new Vector2(.18f,-.2f)));
            d=Mathf.Min(d,Segment(p,new Vector2(.18f,-.2f),new Vector2(.08f,-.33f)));
            d=Mathf.Min(d,Segment(p,new Vector2(-.1f,-.07f),new Vector2(-.3f,-.29f)));
        }
        else if(icon=="crouch") {
            d=Mathf.Abs((p-new Vector2(.02f,.23f)).magnitude-.065f);
            d=Mathf.Min(d,Segment(p,new Vector2(0,.12f),new Vector2(-.12f,-.08f)));
            d=Mathf.Min(d,Segment(p,new Vector2(-.12f,-.08f),new Vector2(.15f,-.08f)));
            d=Mathf.Min(d,Segment(p,new Vector2(.15f,-.08f),new Vector2(.25f,-.3f)));
            d=Mathf.Min(d,Segment(p,new Vector2(-.04f,.05f),new Vector2(.22f,.02f)));
        }
        else if(icon=="jump") {
            d=Segment(p,new Vector2(0,-.2f),new Vector2(0,.30f));
            d=Mathf.Min(d,Segment(p,new Vector2(0,.3f),new Vector2(-.18f,.1f)));
            d=Mathf.Min(d,Segment(p,new Vector2(0,.3f),new Vector2(.18f,.1f)));
            d=Mathf.Min(d,Segment(p,new Vector2(-.3f,-.31f),new Vector2(.3f,-.31f)));
        }
        else {d=Segment(p,new Vector2(-.3f,0),new Vector2(.3f,0));d=Mathf.Min(d,Segment(p,new Vector2(.3f,0),new Vector2(.08f,.22f)));d=Mathf.Min(d,Segment(p,new Vector2(.3f,0),new Vector2(.08f,-.22f)));}
        return .025f-d;
    }
    void Fill(Rect r,Color c){var old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
    void Image(Rect r,Texture t,Color c){var old=GUI.color;GUI.color=c;GUI.DrawTexture(r,t);GUI.color=old;}
    void Text(Rect r,string text,GUIStyle style,Color? color=null)
    {
        // Draw at real screen size instead of enlarging a small glyph bitmap, which looked pixelated on phones.
        var old=style.normal.textColor;int oldSize=style.fontSize;var m=GUI.matrix;
        if(color.HasValue) style.normal.textColor=color.Value;
        GUI.matrix=Matrix4x4.identity;style.fontSize=Mathf.Max(8,Mathf.RoundToInt(oldSize*scale));
        GUI.Label(new Rect(offsetX+r.x*scale,offsetY+r.y*scale,r.width*scale,r.height*scale),text,style);
        GUI.matrix=m;style.fontSize=oldSize;style.normal.textColor=old;
    }
    void Card(Rect r,Texture glass,Color accent,float alpha=.84f)
    {
        if(glass!=null) {
            var uv=new Rect((offsetX+r.x*scale)/Screen.width,1-(offsetY+r.yMax*scale)/Screen.height,r.width*scale/Screen.width,r.height*scale/Screen.height);
            GUI.DrawTextureWithTexCoords(r,glass,uv,false);
        }
        Fill(r,new Color(.022f,.038f,.049f,alpha));
        Fill(new Rect(r.x,r.y,r.width,1),new Color(.6f,.75f,.77f,.22f));
        Fill(new Rect(r.x,r.yMax-1,r.width,1),new Color(.6f,.75f,.77f,.12f));
        Fill(new Rect(r.x,r.y,2,r.height),accent);
    }
    void UpdateRadial(float value)
    {
        int step=Mathf.RoundToInt(value*64);if(step==radialStep)return;radialStep=step;
        for(int y=0;y<96;y++) for(int x=0;x<96;x++) {
            Vector2 p=new Vector2((x+.5f)/96-.5f,(y+.5f)/96-.5f);
            float angle=Mathf.Repeat(Mathf.Atan2(p.x,p.y)/(Mathf.PI*2),1);
            float a=Mathf.Clamp01((.018f-Mathf.Abs(p.magnitude-.44f))*96+.5f)*(angle<=step/64f?1:0);
            radialPixels[y*96+x]=new Color(1,1,1,a);
        }
        radial.SetPixels32(radialPixels);radial.Apply(false,false);
    }
    public bool Draw(State s)
    {
        Init();SetMatrix();
        Color threat=s.hidden?green:s.danger>.1f?red:muted;
        if(!s.hidden&&s.danger>.1f) threat.a=.75f+.25f*Mathf.Sin(s.time*5);
        if(presentedContext!=s.context) {presentedContext=s.context;actionBegan=Time.unscaledTime;}
        bool mobile=Application.isMobilePlatform||Input.touchCount>0;
        if(mobile) {
            Card(new Rect(222,24,380,44),null,green,.6f);
            Text(new Rect(236,29,352,32),s.called?"Survive  /  "+Mathf.CeilToInt(s.rescue)+"s":"Telephone  /  "+Mathf.CeilToInt(s.phoneDistance)+"m",status);
            string alert=s.hidden?"HIDDEN":s.danger>.1f?"SEEN - FIND COVER":!s.powered?"POWER OFF":s.motion?"MOTION ALERT":"";
            Text(new Rect(616,29,290,32),alert,status,s.hidden?green:s.danger>.1f?red:amber);
            Text(new Rect(224,73,280,26),"MATCH / "+MatchClock.Label(s.time),caption);
            Image(PauseButton,icons["pause"],white);
        } else {
        Card(new Rect(222,24,365,88),s.glass,green,.6f);
        Text(new Rect(238,32,330,22),s.called?"POLICE DISPATCH  /  CONNECTED":"OBJECTIVE  /  EMERGENCY CALL",caption);
        Text(new Rect(238,56,330,27),s.called?"Survive until help arrives":"Reach the living-room telephone",label);
        Text(new Rect(238,85,330,18),s.called?"EST. ARRIVAL   "+Mathf.CeilToInt(s.rescue)+" SEC":"Hold the handset action for "+RivalRules.CallSeconds+" seconds",caption);
        if(!s.called&&!s.cameras) {
            Card(new Rect(222,120,290,74),s.glass,green,.6f);
            Text(new Rect(237,127,260,20),"TELEPHONE  /  GROUND FLOOR",caption);
            var matrix=GUI.matrix;
            GUI.matrix=matrix*Matrix4x4.TRS(new Vector3(252,168,0),Quaternion.Euler(0,0,s.phoneBearing-90),Vector3.one)*Matrix4x4.Translate(new Vector3(-252,-168,0));
            Image(new Rect(238,154,28,28),icons["arrow"],green);GUI.matrix=matrix;
            Text(new Rect(280,152,210,32),Mathf.CeilToInt(s.phoneDistance)+" m  /  direct distance",label);
        }
        Color signal=!s.powered?red:s.motion?amber:green;
        Card(new Rect(524,124,260,64),s.glass,signal);
        float pulse=s.motion?.6f+.4f*Mathf.Sin(s.time*5):1;
        Image(new Rect(540,139,8,8),disc,new Color(signal.r,signal.g,signal.b,pulse));
        Text(new Rect(557,130,215,25),!s.powered?"SECURITY OFFLINE":s.motion?"MOTION ALERT":"SECURITY ONLINE",heading,signal);
        Text(new Rect(540,157,238,22),!s.powered?"Breaker: basement utility room":s.motion?"Movement in the central passage":"Passage sensor armed",caption);
        Card(new Rect(784,24,232,86),s.glass,threat);
        Image(new Rect(798,36,34,34),icons["eye"],threat);
        Text(new Rect(841,33,165,27),s.hidden?"CONCEALED":s.danger>.1f?"DANGER":s.crouched?"CROUCHED":s.sprinting?"SPRINTING":"STAY QUIET",status,threat);
        Text(new Rect(841,62,165,21),s.hidden?"Listen before leaving":s.danger>.1f?"BREAK LINE OF SIGHT":"Keep out of his sight",caption);
        for(int i=0;i<18;i++) Fill(new Rect(800+i*11,91,7,3),i/18f<s.danger?threat:new Color(.24f,.34f,.37f,.55f));
        Image(PauseButton,icons["pause"],new Color(.8f,.9f,.9f,.7f));
        Text(new Rect(28,204,320,18),s.floor+"  /  "+s.room,caption);
        }
        if(!s.cameras) {
            Image(new Rect(547,347,6,6),disc,new Color(.8f,.93f,.94f,.65f));
            if(s.context!=None) {
                if(s.context!=Leave&&s.view!=null&&!mobile) {
                    Vector3 screen=s.view.WorldToScreenPoint(s.prompt);Vector2 p=Position(screen);
                    if(screen.z>0) {
                        string name=s.context==Phone?"CALL FOR HELP":s.context==Power?"RESTORE POWER":s.context==Entrance?"CHECK MAIN DOOR":s.hideAction;
                        float x=Mathf.Clamp(p.x-92,330,770),y=Mathf.Clamp(p.y-48,240,430);
                        Card(new Rect(x,y,184,38),s.glass,green);Text(new Rect(x+12,y,164,38),name,heading);
                    }
                }
                float entrance=1-Mathf.Pow(1-Mathf.Clamp01((Time.unscaledTime-actionBegan)/.18f),3);
                float size=Mathf.Lerp(.85f,1,entrance);
                var matrix=GUI.matrix;
                GUI.matrix=matrix*Matrix4x4.TRS(new Vector3(Action.center.x,Action.center.y,0),Quaternion.identity,new Vector3(size,size,1))*Matrix4x4.Translate(new Vector3(-Action.center.x,-Action.center.y,0));
                Image(Action,disc,new Color(.025f,.14f,.105f,.95f));
                Image(Action,ring,new Color(.25f,1,.74f,.95f));
                string icon=s.context==Phone?"phone":s.context==Power?"power":s.context==Leave||s.context==Entrance||s.context==Door||s.context==Item?"arrow":"hide";
                Image(new Rect(Action.center.x-28,Action.y+18,56,56),icons[icon],green);
                Text(new Rect(Action.x-30,Action.y+82,180,30),s.context==Phone?"HOLD TO CALL":s.context==Power?"RESTORE":s.context==Leave?"LEAVE":s.context==Entrance?"CHECK DOOR":s.context==Door||s.context==Item?s.hideAction:"HIDE",actionCaption,white);
                if(s.context==Phone) {UpdateRadial(s.progress/RivalRules.CallSeconds);Image(Action,radial,green);}
                GUI.matrix=matrix;
            }
        }
        Image(CameraButton,disc,new Color(.035f,.08f,.09f,.8f));
        Image(new Rect(CameraButton.x+12,CameraButton.y+9,40,37),icons["camera"],s.cameras?green:!s.powered?red:white);
        if(Application.isMobilePlatform||Input.touchCount>0) {
            Image(Joystick,ring,new Color(.67f,.8f,.81f,.18f));
            Vector2 center=Joystick.center+s.move*new Vector2(53,-53);
            Image(new Rect(center.x-26,center.y-26,52,52),disc,new Color(.7f,.84f,.85f,.26f));
        }
        if(!mobile&&s.hidden&&!s.cameras) Text(new Rect(360,622,400,32),"HIDDEN  /  "+s.hideName,caption,green);
        if(!mobile&&!s.hidden) {
            Text(new Rect(408,620,245,22),"STAMINA  "+Mathf.RoundToInt(s.stamina*100)+"%",caption);
            Fill(new Rect(408,646,230,4),new Color(.18f,.27f,.29f,.7f));
            Fill(new Rect(408,646,230*s.stamina,4),s.stamina<.25f?amber:green);
        }
        if(Application.isMobilePlatform||Input.touchCount>0) {
            bool available=!s.hidden&&!s.cameras;
            Text(new Rect(24,450,192,24),s.sprinting?"SPRINTING":"PUSH UP TO RUN",actionCaption,s.sprinting?green:muted);
            MovementButton(CrouchButton,"crouch","CROUCH",s.crouched,available);
            MovementButton(JumpButton,"jump","JUMP",false,available&&s.grounded&&!s.crouched);
        } else if(!s.hidden&&!s.cameras)Text(new Rect(365,668,600,24),"CTRL crouch   /   SHIFT sprint   /   SPACE jump",caption);
        if(mobile&&!s.hidden&&s.stamina<.98f){Fill(new Rect(64,678,122,3),muted);Fill(new Rect(64,678,122*s.stamina,3),s.stamina<.25f?amber:green);}
        if(!string.IsNullOrEmpty(s.notice)) {
            if(mobile){Card(new Rect(278,592,630,78),null,amber);Text(new Rect(290,596,606,68),s.notice,mobileNotice);}
            else {Card(new Rect(320,414,520,54),s.glass,amber);Text(new Rect(336,420,488,42),s.notice,wrappedNotice);}
        }
        if(s.cameras) DrawCamera(s);
        guideShown=s.guideVisible&&!s.cameras&&!s.hidden;
        if(guideShown) {
            Card(new Rect(26,238,282,184),s.glass,green);
            Text(new Rect(42,248,250,20),"QUICK GUIDE  /  "+(s.guideStep+1)+" OF 4",caption);
            Text(new Rect(42,274,250,26),s.guideTitle,status);
            Text(new Rect(42,305,250,61),s.guideCopy,wrappedNotice);
            if(MenuButton(GuideDismiss,"GOT IT",pauseButton)){guideShown=false;return true;}
        }
        return false;
    }
    public void DrawMobileUse(string text)
    {
        Init();Image(Action,disc,new Color(.025f,.14f,.105f,.95f));Image(Action,ring,new Color(.25f,1,.74f,.95f));
        Image(new Rect(Action.center.x-28,Action.y+18,56,56),icons["arrow"],green);
        Text(new Rect(Action.x-30,Action.y+82,180,30),text.Replace("\n"," "),actionCaption,white);
    }
    void MovementButton(Rect rect,string icon,string text,bool on,bool available)
    {
        Image(rect,disc,new Color(.035f,.08f,.09f,.8f));
        Image(new Rect(rect.x+19,rect.y+9,38,38),icons[icon],!available?new Color(.3f,.4f,.43f):on?green:white);
        Text(new Rect(rect.x-7,rect.y+48,rect.width+14,23),text,centeredCaption,on?green:muted);
    }
    public void DrawRivalMobile(string objective,string alert,Vector2 move,bool sprinting,bool crouched,bool grounded,bool available,bool canCamera,bool cameras,int battery,bool lightOn,string jammerLabel=null)
    {
        Init();Card(new Rect(222,24,470,46),null,green,.6f);Text(new Rect(236,29,445,34),objective,status);
        Text(new Rect(704,29,206,34),alert,status,amber);Image(PauseButton,icons["pause"],white);
        Image(Joystick,ring,new Color(.67f,.8f,.81f,.35f));Vector2 center=Joystick.center+move*new Vector2(53,-53);Image(new Rect(center.x-26,center.y-26,52,52),disc,new Color(.7f,.84f,.85f,.7f));
        Text(new Rect(24,450,192,24),sprinting?"SPRINTING":"PUSH UP TO RUN",actionCaption,sprinting?green:muted);
        MovementButton(CrouchButton,"crouch","CROUCH",crouched,available);MovementButton(JumpButton,"jump","JUMP",false,available&&grounded&&!crouched);
        if(canCamera){Image(CameraButton,disc,new Color(.035f,.08f,.09f,.8f));Image(new Rect(CameraButton.x+12,CameraButton.y+9,40,37),icons["camera"],cameras?green:white);}
        if(!canCamera&&!string.IsNullOrEmpty(jammerLabel)){Image(CameraButton,disc,new Color(.15f,.055f,.04f,.9f));Image(new Rect(CameraButton.x+18,CameraButton.y+5,30,30),icons["power"],amber);Text(new Rect(CameraButton.x-18,CameraButton.y+35,100,30),jammerLabel,centeredCaption,amber);}
        Image(FlashlightButton,disc,new Color(.035f,.08f,.09f,.8f));Image(new Rect(FlashlightButton.x+22,FlashlightButton.y+7,32,32),icons["power"],lightOn?green:muted);Text(new Rect(FlashlightButton.x,FlashlightButton.y+39,76,25),battery+"%",centeredCaption);
    }
    public void DrawSurveillance(Texture feed,bool powered,float time,Texture glass,bool jammed=false){Init();DrawCamera(new State{feed=feed,powered=powered,time=time,glass=glass,jammed=jammed});}
    void DrawCamera(State s)
    {
        Card(new Rect(204,124,692,448),s.glass,green);
        Text(new Rect(224,138,500,28),"SURVEILLANCE / "+HouseCctv.Label,heading);
        Fill(new Rect(224,178,652,340),Color.black);
        if(s.powered&&s.feed!=null) {
            GUI.DrawTexture(new Rect(224,178,652,340),s.feed,ScaleMode.ScaleToFit,false);
            Image(new Rect(240,192,7,7),disc,red);
            Text(new Rect(255,184,250,24),"LIVE   "+Mathf.FloorToInt(s.time).ToString("0000")+" s",caption,white);
            for(int y=180;y<518;y+=5) Fill(new Rect(224,y,652,1),new Color(0,0,0,.12f));
        } else {
            Text(new Rect(300,285,500,38),s.jammed?"SIGNAL JAMMED":"SIGNAL LOST",status,red);
            Text(new Rect(300,322,500,26),s.jammed?"Don has interrupted CCTV and motion sensors":"Restore the basement utility-room breaker",label);
        }
        if(MenuButton(new Rect(224,528,135,32),"PREVIOUS",pauseButton))HouseCctv.Select(-1);
        if(MenuButton(new Rect(741,528,135,32),"NEXT",pauseButton))HouseCctv.Select(1);
        Text(new Rect(370,532,360,23),"The intruder keeps moving",caption);
    }
    void DimScreen(Texture glass,float opacity)
    {
        guideShown=false;
        GUI.matrix=Matrix4x4.identity;
        if(glass!=null)GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),glass,ScaleMode.StretchToFill,false);
        Fill(new Rect(0,0,Screen.width,Screen.height),new Color(.005f,.012f,.02f,opacity));SetMatrix();
    }
    public FrontEndAction DrawPause(Texture glass)
    {
        Init();DimScreen(glass,.75f);
        Card(new Rect(320,104,460,550),glass,green);
        Text(new Rect(350,121,400,50),"PAUSED",pauseTitle);
        Text(new Rect(350,179,400,55),"Reach the telephone. Hold the call for "+RivalRules.CallSeconds+" seconds. Survive until help arrives.",pauseCopy);
        Text(new Rect(350,240,400,48),Application.isMobilePlatform?"Left stick: move / Drag centre: look\nPush joystick above its rim to run / ease back to walk\nUse the action and CCTV buttons":"WASD: move / Mouse: look / E: action\nCTRL crouch / SHIFT sprint / SPACE jump\nHold F: call / C: CCTV / Esc: pause",pauseHelp);
        PreferenceTabs(295);
        if(controlPreferences)LookControls(345,false);else PreferenceControls(345);
        if(MenuButton(new Rect(350,520,400,50),"RESUME",pauseButton))return FrontEndAction.Resume;
        if(MenuButton(new Rect(350,584,190,42),"RESTART",pauseButton))return FrontEndAction.Retry;
        if(MenuButton(new Rect(560,584,190,42),"MAIN MENU",pauseButton))return FrontEndAction.MainMenu;
        return FrontEndAction.None;
    }
    readonly EndingReveal endingReveal=new EndingReveal();
    public FrontEndAction DrawEnding(bool won,bool called,float elapsed,Texture glass)
    {
        Init();SetMatrix();return endingReveal.Draw(won,won,called,elapsed,false);
    }
    public bool DrawSettings(Texture glass)
    {
        Init();SetMatrix();
        Card(new Rect(320,145,460,460),glass,green);
        Text(new Rect(350,164,400,50),"SETTINGS",pauseTitle);
        Text(new Rect(350,214,400,20),"Audio, weather and camera comfort",label);
        PreferenceTabs(240);
        if(controlPreferences)LookControls(283);else {
        PreferenceControls(283);
        Text(new Rect(350,453,230,38),"QUICK GUIDE NEXT RUN",heading);
        if(MenuButton(new Rect(644,455,106,38),GamePreferences.QuickGuide?"ON":"OFF",pauseButton))GamePreferences.QuickGuide=!GamePreferences.QuickGuide;
        Text(new Rect(350,497,230,30),"REDUCED LIGHTNING",heading);
        if(MenuButton(new Rect(644,497,106,30),GamePreferences.ReducedLightning?"ON":"OFF",pauseButton))GamePreferences.ReducedLightning=!GamePreferences.ReducedLightning;
        }
        return MenuButton(new Rect(350,535,400,50),"BACK",pauseButton);
    }
    void PreferenceTabs(float y)
    {
        if(MenuButton(new Rect(350,y,195,40),controlPreferences?"AUDIO / COMFORT":"AUDIO / COMFORT •",pauseButton))controlPreferences=false;
        if(MenuButton(new Rect(555,y,195,40),controlPreferences?"CONTROLS •":"CONTROLS",pauseButton))controlPreferences=true;
    }
    void LookControls(float y,bool showReset=true)
    {
        float mouse=LookSlider("MOUSE SENSITIVITY",y,GamePreferences.MouseSensitivity);
        if(!Mathf.Approximately(mouse,GamePreferences.MouseSensitivity))GamePreferences.MouseSensitivity=mouse;
        float touch=LookSlider("TOUCH SENSITIVITY",y+63,GamePreferences.TouchSensitivity);
        if(!Mathf.Approximately(touch,GamePreferences.TouchSensitivity))GamePreferences.TouchSensitivity=touch;
        Text(new Rect(350,y+126,230,32),"INVERT VERTICAL LOOK",heading);
        if(MenuButton(new Rect(644,y+126,106,34),GamePreferences.InvertLook?"ON":"OFF",pauseButton))GamePreferences.InvertLook=!GamePreferences.InvertLook;
        if(showReset&&MenuButton(new Rect(350,y+166,400,42),"RESET CAMERA CONTROLS",pauseButton))GamePreferences.ResetLook();
    }
    float LookSlider(string name,float y,float value)
    {
        Text(new Rect(350,y,300,23),name,heading);
        Text(new Rect(688,y,62,23),value.ToString("0.00")+"×",caption);
        var previous=GUI.color;GUI.color=green;
        float next=GUI.HorizontalSlider(new Rect(350,y+30,400,23),value,.25f,3,sliderTrack,sliderThumb);GUI.color=previous;return next;
    }
    void PreferenceControls(float y)
    {
        float master=VolumeSlider("MASTER VOLUME",y,GamePreferences.Master);
        if(!Mathf.Approximately(master,GamePreferences.Master))GamePreferences.Master=master;
        float ambience=VolumeSlider("ATMOSPHERE",y+63,GamePreferences.Atmosphere);
        if(!Mathf.Approximately(ambience,GamePreferences.Atmosphere))GamePreferences.Atmosphere=ambience;
        Text(new Rect(350,y+123,230,32),"CAMERA HEAD BOB",heading);
        if(MenuButton(new Rect(644,y+122,106,34),GamePreferences.CameraBob?"ON":"OFF",pauseButton))GamePreferences.CameraBob=!GamePreferences.CameraBob;
    }
    float VolumeSlider(string name,float y,float value)
    {
        Text(new Rect(350,y,300,23),name,heading);
        Text(new Rect(703,y,47,23),Mathf.RoundToInt(value*100)+"%",caption);
        var previous=GUI.color;GUI.color=green;
        float next=GUI.HorizontalSlider(new Rect(350,y+30,400,23),value,0,1,sliderTrack,sliderThumb);GUI.color=previous;
        return next;
    }
    public void Dispose()
    {
        minimap.Dispose();
        foreach(var t in icons.Values) Object.Destroy(t);
        if(disc!=null) Object.Destroy(disc);if(ring!=null) Object.Destroy(ring);if(radial!=null) Object.Destroy(radial);
    }
}
