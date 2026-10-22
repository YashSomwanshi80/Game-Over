using UnityEngine;

// Presentation only: the authoritative outcome and existing victory music own match state.
public sealed class EndingReveal {
    float started=-1;GUIStyle title,winner,copy,button;
    public static float Reveal(float age,float delay,float duration)=>Mathf.SmoothStep(0,1,Mathf.Clamp01((age-delay)/duration));
    public static string Winner(bool rescued)=>rescued?"TAPSEE WINS":"DON WINS";
    void Init(){if(title!=null)return;var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title=new GUIStyle{font=font,fontSize=100,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};
        winner=new GUIStyle(title){fontSize=29};copy=new GUIStyle(winner){fontSize=18,fontStyle=FontStyle.Normal,wordWrap=true};
        button=new GUIStyle(copy){fontStyle=FontStyle.Bold};}
    void Fill(Rect rect,Color color){var old=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=old;}
    void Label(Rect rect,string text,GUIStyle style,Color color){style.normal.textColor=color;GUI.Label(rect,text,style);}
    bool Button(Rect r,string text,Color accent){Fill(r,new Color(.025f,.05f,.06f,.97f));Fill(new Rect(r.x,r.y,3,r.height),accent);Label(r,text,button,Color.white);if(!GUI.Button(r,GUIContent.none,GUIStyle.none))return false;MenuClickSound.Play();return true;}
    public FrontEndAction Draw(bool rescued,bool localWin,bool called,float elapsed,bool online){
        Init();if(started<0)started=Time.unscaledTime;float age=Mathf.Max(0,Time.unscaledTime-started);
        var accent=rescued?new Color(.25f,.88f,.7f):new Color(.96f,.25f,.22f);
        Fill(new Rect(-3000,-3000,7000,7000),new Color(.008f,.016f,.025f,.9f*Reveal(age,0,.4f)));
        float intro=Reveal(age,.1f,.85f);var matrix=GUI.matrix;
        if(!GamePreferences.ReducedLightning)GUIUtility.ScaleAroundPivot(Vector2.one*Mathf.Lerp(1.16f,1,intro),new Vector2(550,270));
        Label(new Rect(130,190,840,150),"GAME OVER",title,new Color(.94f,.97f,.96f,intro));GUI.matrix=matrix;
        float rule=Reveal(age,.35f,.7f);Fill(new Rect(550-260*rule,350,520*rule,3),accent);
        var tint=accent;tint.a=Reveal(age,.65f,.5f);Label(new Rect(180,375,740,45),Winner(rescued),winner,tint);
        float detail=Reveal(age,.9f,.45f);var white=new Color(.72f,.81f,.82f,detail);
        string reason=rescued?"Help arrived. Tapsee survived.":elapsed>=360?"Time ran out. Don claimed the house.":"Don caught Tapsee before help arrived.";
        Label(new Rect(180,430,740,50),(localWin?"VICTORY":"DEFEAT")+"  /  "+reason,copy,white);
        int seconds=Mathf.Max(0,Mathf.FloorToInt(elapsed));Label(new Rect(180,485,740,35),(seconds/60).ToString("00")+":"+(seconds%60).ToString("00")+"  /  CALL "+(called?"CONNECTED":"NOT COMPLETED"),copy,white);
        if(age<1.35f)return FrontEndAction.None;
        if(Button(new Rect(280,560,260,56),"PLAY AGAIN",accent))return FrontEndAction.Retry;
        if(Button(new Rect(560,560,260,56),"MAIN MENU",accent))return FrontEndAction.MainMenu;
        if(online)Label(new Rect(230,635,640,26),"Play again to create or join a new room.",copy,new Color(.55f,.65f,.67f));
        return FrontEndAction.None;
    }
}
