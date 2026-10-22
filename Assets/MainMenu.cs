using UnityEngine;

public sealed class MainMenu
{
    static bool nextMultiplayer;
    public static void QueueMultiplayerReplay(){nextMultiplayer=true;}
    public MainMenu(){if(nextMultiplayer){page=2;nextMultiplayer=false;}}
    bool settings;int page;string roomCode="";CharacterMenuPreview preview;
    GUIStyle title,eyebrow,copy,primary,secondary,note,codeInput;
    Texture2D green,greenHover,dark,darkHover;
    Texture2D ColorTexture(Color c)
    {var texture=new Texture2D(1,1);texture.SetPixel(0,0,c);texture.Apply();return texture;}
    void Init()
    {
        if(title!=null)return;
        var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title=new GUIStyle{font=font,fontSize=88,fontStyle=FontStyle.Bold,alignment=TextAnchor.UpperLeft};title.normal.textColor=new Color(.91f,.95f,.94f);
        eyebrow=new GUIStyle{font=font,fontSize=14,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleLeft};eyebrow.normal.textColor=new Color(.26f,.84f,.65f);
        copy=new GUIStyle{font=font,fontSize=19,wordWrap=true};copy.normal.textColor=new Color(.68f,.77f,.78f);
        codeInput=new GUIStyle(GUI.skin.textField){font=font,fontSize=32,alignment=TextAnchor.MiddleCenter,padding=new RectOffset(20,20,5,5)};
        note=new GUIStyle(copy){fontSize=12};note.normal.textColor=new Color(.49f,.60f,.62f);
        green=ColorTexture(new Color(.22f,.79f,.61f));greenHover=ColorTexture(new Color(.36f,.91f,.73f));
        dark=ColorTexture(new Color(.055f,.09f,.11f,.94f));darkHover=ColorTexture(new Color(.09f,.16f,.18f,.97f));
        primary=new GUIStyle(GUI.skin.button){font=font,fontSize=20,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(25,20,0,0),border=new RectOffset(0,0,0,0)};
        primary.normal.background=green;primary.hover.background=primary.focused.background=greenHover;primary.active.background=green;
        primary.normal.textColor=primary.hover.textColor=primary.focused.textColor=primary.active.textColor=new Color(.015f,.05f,.045f);
        secondary=new GUIStyle(primary);secondary.normal.background=dark;secondary.hover.background=secondary.focused.background=darkHover;secondary.active.background=dark;
        secondary.normal.textColor=secondary.hover.textColor=secondary.focused.textColor=secondary.active.textColor=new Color(.84f,.91f,.91f);
    }
    void Fill(Rect rect,Color color)
    {var before=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=before;}
    public bool Draw(TacticalHud hud,Texture glass,MenuStorm storm)
    {
        Init();
        GUI.matrix=Matrix4x4.identity;
        // Keep the actual estate camera sharp behind the menu. Settings still receive the frosted capture.
        Fill(new Rect(0,0,Screen.width,Screen.height),new Color(.006f,.016f,.024f,.32f));
        Fill(new Rect(0,0,Screen.width*.46f,Screen.height),new Color(.006f,.016f,.024f,.35f));
        hud.SetMatrix();
        if(Event.current.type==EventType.Repaint&&storm!=null)storm.DrawBackground();
        Fill(new Rect(46,62,1008,590),new Color(.018f,.035f,.045f,.82f));
        Fill(new Rect(46,62,3,590),new Color(.26f,.84f,.65f,.8f));
        GUI.Label(new Rect(80,656,920,25),"GAME OVER  /  "+Application.version+"                         FIRST PERSON  /  SOLO AND LAN  /  6 MINUTE LIMIT",note);
        preview?.SetVisible(!settings&&page>0);
        if(settings) {
            if(hud.DrawSettings(glass)||Input.GetKeyDown(KeyCode.Escape)){GamePreferences.Save();settings=false;}
            return false;
        }
        if(page!=0) {
            GUI.Label(new Rect(80,88,930,58),page==1?"CHOOSE YOUR CHARACTER":"MULTIPLAYER / TWO PLAYERS",titleSmall());
            GUI.Label(new Rect(80,153,530,60),page==1?"Choose your role. The other character is controlled by AI.":"Same Wi-Fi or hotspot. Share the six-digit room code.",copy);
            if(page!=3){
                if(Button(new Rect(80,230,250,58),"TAPSEE / SURVIVE",MatchOptions.Role==CharacterRole.Tapsee?primary:secondary))MatchOptions.Role=CharacterRole.Tapsee;
                if(Button(new Rect(345,230,250,58),"DON / HUNT",MatchOptions.Role==CharacterRole.Don?primary:secondary))MatchOptions.Role=CharacterRole.Don;
            }
            if(preview==null)preview=new CharacterMenuPreview();preview.Select(MatchOptions.Role);
            Fill(new Rect(650,170,360,450),new Color(.012f,.025f,.035f,.95f));
            if(Event.current.type==EventType.Repaint)preview.Render();
            if(preview.Texture!=null)GUI.DrawTexture(new Rect(674,176,312,380),preview.Texture,ScaleMode.ScaleToFit);
            GUI.Label(new Rect(676,562,310,29),MatchOptions.Role==CharacterRole.Tapsee?"TAPSEE / THE SURVIVOR":"DON / THE HUNTER",eyebrow);
            GUI.Label(new Rect(676,594,310,24),"IN-GAME CHARACTER PREVIEW",note);
            if(page==3){
                GUI.Label(new Rect(80,230,520,28),"SIX-DIGIT ROOM CODE",eyebrow);
                roomCode=GUI.TextField(new Rect(80,268,515,50),roomCode,6,codeInput);
                GUI.Label(new Rect(80,335,515,150),CharacterMenuPreview.Abilities(MatchOptions.Role),copy);
                GUI.Label(new Rect(80,465,515,55),"If your character is taken, choose the available role in the lobby.",note);
                bool enabled=GUI.enabled;GUI.enabled=enabled&&LanRoomDiscovery.ValidCode(roomCode);
                if(Button(new Rect(80,525,515,54),"JOIN ROOM",primary)){MatchOptions.RoomCode=roomCode;MatchOptions.Mode=MatchMode.Join;return StartMatch();}GUI.enabled=enabled;
            }else{
                GUI.Label(new Rect(80,310,515,28),MatchOptions.Role==CharacterRole.Tapsee?"OUTSMART DON. CALL FOR HELP.":"TRACK TAPSEE. DISRUPT HER SECURITY.",eyebrow);
                GUI.Label(new Rect(80,350,515,125),CharacterMenuPreview.Abilities(MatchOptions.Role),copy);
                GUI.Label(new Rect(80,449,515,23),"Both roles: battery flashlight, sprint and crouch.",note);
                if(page==1){
                    if(MatchOptions.Role==CharacterRole.Tapsee)for(int i=0;i<3;i++){var difficulty=(DonDifficulty)i;if(Button(new Rect(80+i*175,475,165,38),difficulty.ToString().ToUpperInvariant(),GamePreferences.Difficulty==difficulty?primary:secondary))GamePreferences.Difficulty=difficulty;}
                    else GUI.Label(new Rect(80,478,515,32),"Your intro plays before the hunt begins.",note);
                    if(Button(new Rect(80,533,515,54),"START SINGLE PLAYER",primary)){MatchOptions.Mode=MatchMode.SinglePlayer;return StartMatch();}
                }else{
                    GUI.Label(new Rect(80,478,515,38),"Don enters at 60s. Office phone unlocks at 120s. Six-minute limit.",note);
                    if(Button(new Rect(80,533,250,54),"HOST MATCH",primary)){MatchOptions.Mode=MatchMode.Host;return StartMatch();}
                    if(Button(new Rect(345,533,250,54),"JOIN MATCH",secondary)){roomCode="";page=3;}
                }
            }
            if(Button(new Rect(80,604,250,37),"BACK",secondary))page=page==3?2:0;
            return false;
        }
        Fill(new Rect(80,107,42,3),new Color(.26f,.84f,.65f));
        GUI.Label(new Rect(80,126,510,28),"A FIRST-PERSON SURVIVAL THRILLER",eyebrow);
        GUI.Label(new Rect(72,165,900,112),"GAME OVER",title);
        GUI.Label(new Rect(82,285,500,50),"One mansion. Two roles. Six minutes.",copy);
        if(Button(new Rect(80,365,470,62),"01   SINGLE PLAYER",primary))page=1;
        if(Button(new Rect(80,440,470,62),"02   MULTIPLAYER",secondary))page=2;
        if(Button(new Rect(80,515,470,56),"03   SETTINGS",secondary))settings=true;
        GUI.Label(new Rect(82,598,480,35),"Play as Tapsee or Don. Survive or hunt.",note);
        Fill(new Rect(620,310,1,290),new Color(.45f,.65f,.68f,.18f));
        GUI.Label(new Rect(665,325,350,30),"SURVIVE OR HUNT",eyebrow);
        GUI.Label(new Rect(665,375,330,100),"TAPSEE  /  Call for help and survive.\n\nDON  /  Track her down. Disrupt her security.",copy);
        Fill(new Rect(665,505,300,1),new Color(.45f,.65f,.68f,.22f));
        GUI.Label(new Rect(665,530,330,65),"A storm outside. An intruder inside.\nHeadphones bring the house to life.",note);
        return false;
    }
    bool StartMatch(){preview?.Dispose();preview=null;return true;}
    bool Button(Rect rect,string text,GUIStyle style){if(!GUI.Button(rect,text,style))return false;MenuClickSound.Play();return true;}
    GUIStyle titleSmall()=>new GUIStyle(title){fontSize=42};
    public void Dispose()
    {
        preview?.Dispose();preview=null;
        if(green!=null)Object.Destroy(green);if(greenHover!=null)Object.Destroy(greenHover);
        if(dark!=null)Object.Destroy(dark);if(darkHover!=null)Object.Destroy(darkHover);
    }
}
