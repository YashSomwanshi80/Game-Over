using UnityEngine;
using System;
// Cached floor plans from actual level colliders. No enemy data and no extra render camera.
public sealed class MansionMinimap:IDisposable {
 public static readonly Rect[] Worlds={new Rect(-11,-8.38f,22,16.76f),new Rect(-11,-10.667f,28,21.333f),new Rect(-13,-10.143f,24,18.286f)};
 public static int FloorAt(float y)=>y< -1.5f?0:y>2?2:1;
 public static Vector2 Project(Vector3 at,Rect world,Rect canvas)=>new Vector2(Mathf.Lerp(canvas.xMin,canvas.xMax,Mathf.InverseLerp(world.xMin,world.xMax,at.x)),Mathf.Lerp(canvas.yMax,canvas.yMin,Mathf.InverseLerp(world.yMin,world.yMax,at.z)));
 const int TW=672,TH=512; // 24 px per metre so the map stays smooth when scaled
 readonly Texture2D[] plans=new Texture2D[3];Texture2D arrow,dot,disc;GUIStyle small,marker;
 public Texture2D Plan(int floor){if(plans[floor]==null)Build();return plans[floor];}
 void Build(){
  var boxes=UnityEngine.Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None);
  for(int floor=0;floor<3;floor++){
   var pixels=new Color32[TW*TH];for(int i=0;i<pixels.Length;i++)pixels[i]=new Color32(8,18,24,255);float level=(floor-1)*3.4f;
   foreach(var box in boxes){if(!box.enabled||box.isTrigger)continue;var bounds=box.bounds;string name=box.name.ToLowerInvariant();
    bool surface=(name.Contains("floor")||name.Contains("slab")||name.Contains("paving")||name.Contains("terrace")||name.Contains("driveway")||name.Contains("deck"))&&Mathf.Abs(bounds.max.y-level)<.3f;
    if(surface)Paint(pixels,Worlds[floor],bounds,new Color32(38,57,64,255));
   }
   foreach(var box in boxes){if(!box.enabled||box.isTrigger)continue;var bounds=box.bounds;string name=box.name.ToLowerInvariant();
    bool wall=name.Contains("wall")||name.Contains("partition")||name.Contains("divider")||name.Contains("railing")||name.Contains("fence");
    if(wall&&bounds.min.y<level+1.15f&&bounds.max.y>level+1.15f)Paint(pixels,Worlds[floor],bounds,new Color32(125,157,158,255));
   }
   plans[floor]=new Texture2D(TW,TH,TextureFormat.RGBA32,true){name="Minimap floor "+floor,filterMode=FilterMode.Trilinear,anisoLevel=4,wrapMode=TextureWrapMode.Clamp};plans[floor].SetPixels32(pixels);plans[floor].Apply(true,true);
  }
 }
 static void Paint(Color32[] pixels,Rect world,Bounds b,Color32 color){var rect=new Rect(b.min.x,b.min.z,b.size.x,b.size.z);if(!world.Overlaps(rect))return;int x0=Mathf.Clamp(Mathf.FloorToInt((rect.xMin-world.xMin)/world.width*TW),0,TW-1),x1=Mathf.Clamp(Mathf.CeilToInt((rect.xMax-world.xMin)/world.width*TW),0,TW-1),y0=Mathf.Clamp(Mathf.FloorToInt((rect.yMin-world.yMin)/world.height*TH),0,TH-1),y1=Mathf.Clamp(Mathf.CeilToInt((rect.yMax-world.yMin)/world.height*TH),0,TH-1);for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)pixels[y*TW+x]=color;}
 static Texture2D Shape(int n,System.Func<float,float,Color> at){var t=new Texture2D(n,n,TextureFormat.RGBA32,true){filterMode=FilterMode.Trilinear,wrapMode=TextureWrapMode.Clamp};var px=new Color[n*n];for(int y=0;y<n;y++)for(int x=0;x<n;x++){Color sum=Color.clear;for(int sy=0;sy<4;sy++)for(int sx=0;sx<4;sx++)sum+=at((x+(sx+.5f)/4)/n,(y+(sy+.5f)/4)/n);px[y*n+x]=sum/16f;}t.SetPixels(px);t.Apply(true,true);return t;}
 void Init(){if(small!=null)return;var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");small=new GUIStyle{font=font,fontSize=12,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};small.normal.textColor=Color.white;
  Color none=new Color(1,1,1,0);
  arrow=Shape(64,(u,v)=>{float x=u*24,y=v*24;return y>=2&&y<22&&Mathf.Abs(x-11.5f)<(22-y)*.44f?Color.white:none;});
  disc=Shape(64,(u,v)=>Vector2.Distance(new Vector2(u,v),new Vector2(.5f,.5f))<.48f?Color.white:none);
  dot=Shape(64,(u,v)=>{float d=Vector2.Distance(new Vector2(u,v),new Vector2(.5f,.5f));return d<.33f?new Color(1,.3f,.3f,1):d<.48f?Color.white:none;});}
 // Text is drawn at the real screen size, not scaled up from a tiny font, so it stays sharp on phones.
 static void Label(Rect r,string text,int size,Color color,GUIStyle style){var m=GUI.matrix;float k=m.m00;GUI.matrix=Matrix4x4.identity;int old=style.fontSize;var oldColor=style.normal.textColor;style.fontSize=Mathf.Max(8,Mathf.RoundToInt(size*k));style.normal.textColor=color;GUI.Label(new Rect(m.m03+r.x*k,m.m13+r.y*k,r.width*k,r.height*k),text,style);style.fontSize=old;style.normal.textColor=oldColor;GUI.matrix=m;}
 static void Fill(Rect r,Color c){var old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
 void Symbol(Rect canvas,Rect world,Vector3 point,string letter,Color color){var p=Project(point,world,canvas);var old=GUI.color;GUI.color=new Color(.015f,.04f,.05f,.82f);GUI.DrawTexture(new Rect(p.x-8,p.y-8,16,16),disc);GUI.color=old;Label(new Rect(p.x-8,p.y-8,16,16),letter,12,color,small);}
 public void Draw(Rect panel,Vector3 position,float yaw,Vector3 phone,bool called,bool cameraDots=false){
  Init();int floor=FloorAt(position.y);var texture=Plan(floor);Fill(panel,new Color(.012f,.025f,.035f,.55f));Fill(new Rect(panel.x,panel.y,2,panel.height),new Color(.25f,.83f,.69f,.7f));
  Label(new Rect(panel.x+8,panel.y+2,panel.width-16,20),floor==0?"BASEMENT":floor==2?"FIRST FLOOR":"GROUND FLOOR",12,new Color(.76f,.87f,.87f),small);
  var canvas=new Rect(panel.x+6,panel.y+22,panel.width-12,128);var tint=GUI.color;GUI.color=new Color(1,1,1,.66f);GUI.DrawTexture(canvas,texture);GUI.color=tint;Label(new Rect(canvas.xMax-14,canvas.y,14,14),"N",11,new Color(.76f,.87f,.87f),small);
  if(floor==0)Symbol(canvas,Worlds[floor],new Vector3(3.2f,-3.4f,3),"S",new Color(1,.72f,.35f));
  else if(floor==2)Symbol(canvas,Worlds[floor],new Vector3(0,3.4f,5.5f),"S",new Color(1,.72f,.35f));
  else{Symbol(canvas,Worlds[floor],new Vector3(0,0,.8f),"S",new Color(1,.72f,.35f));Symbol(canvas,Worlds[floor],new Vector3(2,0,3),"S",new Color(1,.72f,.35f));}
  if(!called&&FloorAt(phone.y)==floor)Symbol(canvas,Worlds[floor],phone,"P",new Color(.3f,1,.73f));
  if(cameraDots)for(int i=0;i<HouseCctv.Positions.Length;i++)if(HouseCctv.FloorOf(i)==floor){var c=Project(HouseCctv.Positions[i],Worlds[floor],canvas);GUI.DrawTexture(new Rect(c.x-6,c.y-6,12,12),dot);}
  var point=Project(position,Worlds[floor],canvas);var matrix=GUI.matrix;GUIUtility.RotateAroundPivot(yaw,point);GUI.DrawTexture(new Rect(point.x-6,point.y-8,12,16),arrow);GUI.matrix=matrix;
  Label(new Rect(panel.x+8,panel.yMax-15,panel.width-16,14),called?"SURVIVE  /  S STAIRS":FloorAt(phone.y)==floor?"P PHONE  /  S STAIRS":FloorAt(phone.y)<floor?"PHONE DOWN  /  S STAIRS":"PHONE UP  /  S STAIRS",11,new Color(.76f,.87f,.87f),small);
 }
 public void Dispose(){foreach(var plan in plans)if(plan!=null)UnityEngine.Object.Destroy(plan);if(arrow!=null)UnityEngine.Object.Destroy(arrow);if(disc!=null)UnityEngine.Object.Destroy(disc);if(dot!=null)UnityEngine.Object.Destroy(dot);}
}
