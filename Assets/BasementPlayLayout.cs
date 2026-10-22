using System.Collections.Generic;
public static class BasementPlayLayout {
 public static List<FirstFloorLayout.Part> Parts(){var p=new List<FirstFloorLayout.Part>();
  p.Add(new FirstFloorLayout.Part("Basement play / radio console",-4.25f,-3.05f,-.85f,.65f,.7f,.55f,"wood"));
  p.Add(new FirstFloorLayout.Part("Basement play / radio",-4.25f,-2.5f,-.85f,.48f,.4f,.30f,"metal"));
  p.Add(new FirstFloorLayout.Part("Basement play / supply drawer",7.15f,-2.31f,-5.65f,.60f,.24f,.44f,"metal"));
  p.Add(new FirstFloorLayout.Part("Basement play / linen base",9,-3.27f,3.75f,1.45f,.12f,1,"metal"));
  p.Add(new FirstFloorLayout.Part("Basement play / linen hood",9,-2.28f,3.75f,1.45f,.08f,1,"fabric"));
  foreach(float x in new[]{8.30f,9.70f})p.Add(new FirstFloorLayout.Part("Basement play / linen side",x,-2.78f,3.75f,.04f,.94f,1,"fabric"));
  p.Add(new FirstFloorLayout.Part("Basement play / linen back",9,-2.78f,4.23f,1.4f,.94f,.04f,"fabric"));
  foreach(float x in new[]{8.61f,9.39f})p.Add(new FirstFloorLayout.Part("Basement play / linen front",x,-2.78f,3.27f,.62f,.94f,.04f,"fabric"));
  return p;
 }
 public static List<FirstFloorLayout.Part> Exclusions()=>new List<FirstFloorLayout.Part>{new FirstFloorLayout.Part("Linen trolley exclusion",9,-2.8f,3.75f,1.5f,1.2f,1.05f,"blocked")};
}
