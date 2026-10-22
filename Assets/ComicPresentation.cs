using UnityEngine;

// Code-drawn storyboard art: temporary visual storytelling, not final character portraits.
public sealed class ComicPresentation
{
    int page;
    GUIStyle heading, copy, caption, button;
    readonly Color ink=new Color(.025f,.035f,.05f), paper=new Color(.93f,.88f,.76f), teal=new Color(.25f,.78f,.72f);
    static readonly string[] Titles={"01 / THE BREACH","02 / YOU ARE NOT ALONE","03 / MAKE THE CALL"};
    static readonly string[] Lines={
        "A figure crosses the fence. Don has entered the property.",
        "Tapsee notices the intrusion. The house cameras offer only fragments of what is happening.",
        "Reach the living-room telephone. Call for help, then survive until the police arrive."
    };
    void Styles()
    {
        if(heading!=null) return;
        heading=new GUIStyle(GUI.skin.label){fontSize=42,fontStyle=FontStyle.Bold,wordWrap=true}; heading.normal.textColor=paper;
        copy=new GUIStyle(GUI.skin.label){fontSize=24,wordWrap=true}; copy.normal.textColor=paper;
        caption=new GUIStyle(copy){fontSize=17}; caption.normal.textColor=teal;
        button=new GUIStyle(GUI.skin.button){fontSize=21,fontStyle=FontStyle.Bold};
    }
    void Fill(Rect rect,Color color) { var before=GUI.color; GUI.color=color; GUI.DrawTexture(rect,Texture2D.whiteTexture); GUI.color=before; }
    void Person(float x,float y,bool hoodie,float scale=1)
    {
        var outfit=hoodie?new Color(.24f,.3f,.38f):ink;
        Fill(new Rect(x-24*scale,y-8*scale,48*scale,58*scale),outfit);
        Fill(new Rect(x-15*scale,y,30*scale,35*scale),new Color(.57f,.36f,.24f));
        Fill(new Rect(x-35*scale,y+42*scale,70*scale,hoodie?85*scale:135*scale),outfit);
        Fill(new Rect(x-47*scale,y+47*scale,15*scale,90*scale),outfit);
        Fill(new Rect(x+32*scale,y+47*scale,15*scale,90*scale),outfit);
        Fill(new Rect(x-29*scale,y+125*scale,23*scale,93*scale),ink);
        Fill(new Rect(x+6*scale,y+125*scale,23*scale,93*scale),ink);
        Fill(new Rect(x-18*scale,y+10*scale,36*scale,8*scale),ink);
        if(hoodie) {
            Fill(new Rect(x-10*scale,y+46*scale,2*scale,27*scale),paper);
            Fill(new Rect(x+10*scale,y+46*scale,2*scale,27*scale),paper);
        }
    }
    void Art(int scene,bool rescue=false)
    {
        Fill(new Rect(55,160,560,360),paper); Fill(new Rect(61,166,548,348),new Color(.1f,.16f,.21f));
        for(int i=0;i<8;i++) Fill(new Rect(75+i*70,180,2,310),new Color(.14f,.2f,.25f));
        if(scene==0) {
            Fill(new Rect(125,260,400,240),new Color(.28f,.29f,.28f));
            Fill(new Rect(110,245,430,18),ink);
            for(int row=0;row<2;row++) for(int col=0;col<4;col++)
                Fill(new Rect(150+col*91,285+row*95,50,65),new Color(.78f,.58f,.3f));
            Person(380,270,false,.9f);
            for(int i=0;i<12;i++) Fill(new Rect(78+i*45,385,8,120),ink);
            Fill(new Rect(65,410,540,9),ink);
        } else if(scene==1) {
            Fill(new Rect(330,205,230,155),ink);
            Fill(new Rect(340,215,210,135),new Color(.17f,.34f,.32f));
            Person(440,230,false,.45f);
            for(int i=0;i<12;i++) Fill(new Rect(340,220+i*10,210,1),new Color(.2f,.4f,.36f));
            Person(190,245,true,1.1f);
        } else {
            Person(180,240,true,1.1f);
            Fill(new Rect(330,405,245,18),new Color(.45f,.29f,.17f));
            Fill(new Rect(350,375,180,30),ink);
            Fill(new Rect(340,350,200,15),teal);
            Fill(new Rect(340,350,25,35),teal); Fill(new Rect(515,350,25,35),teal);
            if(rescue) { Fill(new Rect(345,205,85,70),new Color(.25f,.42f,.8f)); Fill(new Rect(440,205,85,70),new Color(.8f,.22f,.22f)); }
        }
    }
    // Returns true only when the user requests play. Scene progression never advances gameplay time.
    public bool DrawIntro()
    {
        Styles(); Fill(new Rect(0,0,1100,700),ink);
        GUI.Label(new Rect(55,34,700,65),"GAME OVER",heading);
        GUI.Label(new Rect(58,105,700,30),"A STEALTH-SURVIVAL STORY / PROTOTYPE STORYBOARD",caption);
        Art(page);
        GUI.Label(new Rect(655,175,390,105),Titles[page],heading);
        GUI.Label(new Rect(655,290,380,150),Lines[page],copy);
        GUI.Label(new Rect(655,440,380,140),page==2?"WASD / left drag: move\nMouse / right drag: look\nE: hide or interact / C: CCTV\nHold F / CALL for 5 seconds.\nSurvive 100 seconds after connecting.":"Observe. Move. Hide.\nYou do not have to confront Don.",caption);
        for(int i=0;i<3;i++) Fill(new Rect(55+i*45,555,32,5),i==page?teal:new Color(.2f,.25f,.28f));
        if(page>0&&GUI.Button(new Rect(55,600,150,55),"BACK",button)) page--;
        if(GUI.Button(new Rect(655,600,390,55),page==2?"ENTER THE HOUSE":"NEXT PANEL",button)) {
            if(page==2) return true; page++;
        }
        return GUI.Button(new Rect(420,600,195,55),"SKIP INTRO",button);
    }
    public bool DrawEnding(bool won,bool called,float elapsed)
    {
        Styles(); Fill(new Rect(0,0,1100,700),ink);
        GUI.Label(new Rect(55,34,1000,65),won?"HELP HAS ARRIVED":"GAME OVER",heading);
        GUI.Label(new Rect(58,105,900,30),won?"TAPSEE SURVIVED":"CAUGHT / A DIFFERENT ROUTE MAY SAVE YOU",caption);
        Art(won?2:1,won);
        GUI.Label(new Rect(655,190,385,140),won?"The police arrive.\nThe immediate danger is over.":"Don found Tapsee before help arrived.",copy);
        GUI.Label(new Rect(655,350,385,160),"Time survived: "+Mathf.FloorToInt(elapsed)+" seconds\nPolice call: "+(called?"connected":"not completed")+"\n\n"+(won?"You used the house to survive.":"Break line of sight before hiding. CCTV does not pause Don."),caption);
        return GUI.Button(new Rect(655,600,390,55),"PLAY AGAIN",button);
    }
}
