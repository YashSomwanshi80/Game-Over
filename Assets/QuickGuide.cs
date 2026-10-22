using UnityEngine;

// Optional, non-blocking onboarding. Reads progress but never alters the game state.
public sealed class QuickGuide
{
    bool enabled,begun;int step;float walked,looked,readingTime;
    Vector3 lastPosition,lastLook;
    public bool Enabled=>enabled;
    public int Step=>step;
    public string Title=>step==0?"Move and look":step==1?"Look at an object":step==2?"Hold to call":"Survive until help arrives";
    public string Copy(bool mobile)
    {
        if(step==0)return mobile?"Stick to move; drag to look. Use the crouch, sprint and jump buttons.":"WASD move; mouse look. CTRL crouches, SHIFT sprints, SPACE jumps.";
        if(step==1)return "Look at the phone, a locker or the side of a bed to reveal its action.";
        if(step==2)return mobile?"Hold the phone action for five seconds. Keep looking at the telephone.":"Hold F at the telephone for five seconds. Keep looking at it.";
        return "Break line of sight before hiding. CCTV does not pause the intruder.";
    }
    public void Begin(Vector3 position,Vector3 forward)
    {
        enabled=GamePreferences.QuickGuide;begun=true;step=0;walked=looked=readingTime=0;lastPosition=position;lastLook=forward;
    }
    public void Tick(Vector3 position,Vector3 forward,int context,bool called,bool active,bool freeToRead,float dt)
    {
        if(!begun||!enabled)return;
        float distance=Vector3.Distance(position,lastPosition),angle=Vector3.Angle(forward,lastLook);
        lastPosition=position;lastLook=forward;
        if(!active)return;
        if(freeToRead&&distance<.5f){walked+=distance;looked+=angle;}
        if(step==0&&walked>1.2f&&looked>25)step=1;
        if(step<2&&context!=TacticalHud.None&&context!=TacticalHud.Leave)step=2;
        if(called&&step<3){step=3;readingTime=0;}
        if(step==3&&freeToRead){readingTime+=dt;if(readingTime>=8)Dismiss();}
    }
    public void Dismiss(){enabled=false;GamePreferences.QuickGuide=false;GamePreferences.Save();}
}
