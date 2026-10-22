using System;
// Authority derives opening gates from match time, starting only after both peers confirm.
public static class MultiplayerOpeningRules {
 public const float BreakInSeconds=60,BlackoutSeconds=120;
 public static bool Breached(float elapsed)=>elapsed>=BreakInSeconds;
 public static bool Blackout(float elapsed)=>elapsed<BlackoutSeconds;
 public static bool CanCall(float elapsed,bool powered)=>!Blackout(elapsed)&&powered;
 public static int Remaining(float elapsed,float end)=>(int)Math.Ceiling(Math.Max(0,end-elapsed));
}
