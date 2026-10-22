using System;
// Focus and app suspension can arrive independently and repeatedly on Android.
public sealed class IntroPlaybackPolicy
{
    public bool Focused=true,Backgrounded;
    public bool Prepared{get;private set;}
    public bool Active=>Focused&&!Backgrounded;
    float preparing,stalled;double lastTime=-1;
    public void Ready(){Prepared=true;stalled=0;lastTime=-1;}
    public string Step(float dt,double time)
    {
        if(!Active||dt<=0||float.IsNaN(dt)||float.IsInfinity(dt))return null;
        dt=Math.Min(dt,.25f);
        if(!Prepared){preparing+=dt;return preparing>=15?"Preparation timed out.":null;}
        if(!double.IsNaN(time)&&!double.IsInfinity(time)&&time>lastTime+.005){lastTime=time;stalled=0;}
        else stalled+=dt;
        return stalled>=15?"Playback stopped advancing.":null;
    }
}
