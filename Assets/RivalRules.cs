using System;

// Shared authority rules. Outcome: 0 playing, 1 Tapsee rescued, 2 Don wins.
public sealed class RivalRules
{
    public const float CallSeconds=4;
    public float Progress,Rescue=100;
    public bool Called;
    public int Outcome;
    public void Call(bool valid,float dt,float elapsed=-1)
    {
        if(Called||Outcome!=0)return;
        if(elapsed>=MaximumSeconds)valid=false;
        dt=Math.Max(0,Math.Min(dt,.1f));Progress=valid?Math.Min(CallSeconds,Progress+dt):0;
        if(Progress>=CallSeconds-.0001f){Progress=CallSeconds;Called=true;if(elapsed>=0)Rescue=Math.Min(100,Math.Max(0,MaximumSeconds-elapsed));}
    }
    // Call this before interaction branches that can return without reaching Call().
    public void MaintainCall(bool action,bool held,bool eligible){if(action||!held||!eligible)Call(false,0);}
    public bool TimedOut;
    public const float MaximumSeconds=360;
    public void Limit(float elapsed){
        if(Outcome!=0)return;
        if(Called)Rescue=Math.Min(Rescue,Math.Max(0,MaximumSeconds-elapsed));
        if(elapsed>=MaximumSeconds){TimedOut=true;Outcome=Called?1:2;}
    }
    public void Resolve(bool captured,float dt)
    {
        if(Outcome!=0)return;
        if(captured){Outcome=2;return;}
        if(!Called)return;
        Rescue=Math.Max(0,Rescue-Math.Max(0,Math.Min(dt,.1f)));
        if(Rescue<=.0001f){Rescue=0;Outcome=1;}
    }
}
