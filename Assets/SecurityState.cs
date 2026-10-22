// Gameplay clock is advanced only during active play, so pause freezes security events.
public sealed class SecurityState
{
    public bool Powered { get; private set; } = true;
    public float Clock { get; private set; }
    public bool MotionAlert => Powered && Clock < alertUntil;
    float alertUntil, nextAlert;
    bool wasInZone;

    public void Tick(float seconds, bool enemyInZone)
    {
        Clock += System.Math.Max(0, seconds);
        if(Powered && enemyInZone && !wasInZone && Clock >= nextAlert) {
            alertUntil = Clock + 4; nextAlert = Clock + 6;
        }
        wasInZone = enemyInZone;
    }
    public void Restore()
    {
        if(Powered) return;
        Powered = true; alertUntil = 0;
    }
    // Called by Don's physical switch interaction (or a client mirroring the host).
    public void SwitchOff(){Powered=false;alertUntil=0;}
}
