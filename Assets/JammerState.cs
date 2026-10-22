using System;
public sealed class JammerState {
 public float Until,ReadyAt;
 public bool Active(float time)=>time<Until;
 public bool TryActivate(float time){if(float.IsNaN(time)||float.IsInfinity(time)||time<ReadyAt||Active(time))return false;Until=time+15;ReadyAt=time+45;return true;}
}
public static class MatchClock {public static string Label(float elapsed){int seconds=(int)Math.Ceiling(Math.Max(0,360-elapsed));return (seconds/60).ToString("0")+":"+(seconds%60).ToString("00");}}
