using System;

// Receives a verified visibility result; never guesses a target position.
public static class DonDetection
{
    public const float InvestigateThreshold=.12f,ChaseThreshold=.45f;
    public static float Advance(float awareness,bool visible,bool crouched,float dt,float response=1)
    {
        dt=Math.Max(0,Math.Min(dt,.1f));
        float rate=visible?(crouched?.85f:1.7f)*Math.Max(0,Math.Min(response,2)):-.4f;
        return Math.Max(0,Math.Min(1,awareness+rate*dt));
    }
    public static bool Pursue(float awareness,bool alreadyChasing) => alreadyChasing||awareness>=ChaseThreshold;
}
