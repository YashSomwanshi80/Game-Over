using System;

public sealed class BedroomDoorState
{
    public bool Open{get;private set;}
    public float Fraction{get;private set;}
    public void SetOpen(bool value){Open=value;}
    public void Toggle(){Open=!Open;}
    public float Next(float dt)
    {
        float step=Math.Max(0,Math.Min(dt,.1f))*3;
        return Open?Math.Min(1,Fraction+step):Math.Max(0,Fraction-step);
    }
    public void Advance(float dt,bool clear){if(clear)Fraction=Next(dt);}
    public void Apply(bool open,float fraction){Open=open;Fraction=Math.Max(0,Math.Min(1,fraction));}
}
