using System;
public static class DoorSwingRules {
 // Local X is the door plane normal. Choose only when closed: never flip a moving leaf.
 public static float AwaySide(float localX,float fraction,float currentSide){if(fraction>.001f||float.IsNaN(localX)||float.IsInfinity(localX))return currentSide;return localX>.05f?-1:localX<-.05f?1:currentSide;}
}
