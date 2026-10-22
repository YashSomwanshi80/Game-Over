using System;

// Screen deltas represent distance, not velocity: do not multiply by frame time.
public static class FirstPersonLookRules
{
    public static float Sensitivity(float value)
    {return float.IsNaN(value)||float.IsInfinity(value)?1:Math.Max(.25f,Math.Min(3,value));}
    public static float Mouse(float delta,float sensitivity)
    {return delta*2*Sensitivity(sensitivity);}
    public static float Touch(float delta,float safeWidth,float safeHeight,float sensitivity)
    {
        float scale=Math.Max(.001f,Math.Min(safeWidth/1100f,safeHeight/700f));
        return delta/scale*.20f*Sensitivity(sensitivity);
    }
    // Capsule height changes immediately on crouch. Keep only 4 cm of eye lag,
    // leaving room for bob and the near plane inside the crouching body.
    public static float EyeTransition(float current,float target,float dt)
    {
        float blend=1-(float)Math.Exp(-Math.Max(0,Math.Min(dt,.1f))*12);
        return Math.Min(target+.04f,current+(target-current)*blend);
    }
    public static float Vertical(float delta,bool invert) => invert?-delta:delta;
}
