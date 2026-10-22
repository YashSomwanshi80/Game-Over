using System;
// Authority owns charge. Battery use is independent of the mansion breaker.
public sealed class FlashlightBattery
{
    public const float CapacitySeconds=300;
    public float Seconds{get;private set;}=CapacitySeconds;
    public float Charge=>Seconds/CapacitySeconds;
    public bool On{get;private set;}
    public bool Toggle(){if(Seconds<=0){On=false;return false;}On=!On;return true;}
    public void Step(float dt){if(!On||dt<=0||float.IsNaN(dt)||float.IsInfinity(dt))return;Seconds=Math.Max(0,Seconds-dt);if(Seconds<=0)On=false;}
    public void Recharge(float fraction){if(float.IsNaN(fraction)||float.IsInfinity(fraction)||fraction<=0)return;Seconds=Math.Min(CapacitySeconds,Seconds+fraction*CapacitySeconds);}
    public void Apply(float charge,bool on){Seconds=float.IsNaN(charge)||float.IsInfinity(charge)?0:Math.Max(0,Math.Min(1,charge))*CapacitySeconds;On=on&&Seconds>0;}
}
