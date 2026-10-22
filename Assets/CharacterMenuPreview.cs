using UnityEngine;
using System;
// A single isolated portrait camera exists only during character selection.
public sealed class CharacterMenuPreview:IDisposable {
 const int Layer=12;GameObject stage,model;Camera camera;RenderTexture target;AnimationClip idle;CharacterRole selected;bool loaded;float nextFrame;
 public Texture Texture=>target;
 public static string Abilities(CharacterRole role)=>role==CharacterRole.Tapsee?"CCTV  /  Watch and switch house cameras.\nHIDE  /  Use cupboards, beds and bathtubs.\nRESCUE  /  Hold the phone to call, then survive.":"JAMMER  /  Disable CCTV and sensors for 15s.\nRECHARGE  /  Available every 45 seconds.\nHUNT  /  Catch Tapsee, search hiding spots, cut power.";
 public void SetVisible(bool visible){if(stage!=null)stage.SetActive(visible);}
 public void Select(CharacterRole role){
  if(loaded&&selected==role)return;selected=role;loaded=true;
  if(stage==null){stage=new GameObject("Character selection portrait stage");stage.hideFlags=HideFlags.HideAndDontSave;stage.transform.position=new Vector3(10000,10000,10000);
   camera=new GameObject("Character portrait camera").AddComponent<Camera>();camera.transform.SetParent(stage.transform,false);camera.enabled=false;camera.cullingMask=1<<Layer;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.024f,.045f,.058f);camera.fieldOfView=32;camera.nearClipPlane=.05f;camera.farClipPlane=12;
   target=new RenderTexture(384,512,24,RenderTextureFormat.ARGB32){name="Selected character portrait"};target.Create();camera.targetTexture=target;
   LightAt(new Vector3(-1.3f,2.3f,2),new Color(.8f,.9f,1),2.1f);LightAt(new Vector3(1.1f,1.8f,1.1f),new Color(1,.8f,.62f),1.5f);LightAt(new Vector3(.3f,2,-1),new Color(.3f,1,.8f),2.3f);
  }
  stage.SetActive(true);if(model!=null){model.SetActive(false);UnityEngine.Object.Destroy(model);}
  var prefab=Resources.Load<GameObject>("Characters/"+role);if(prefab==null)return;
  model=UnityEngine.Object.Instantiate(prefab,stage.transform);model.name="Selection preview / "+role;model.transform.localPosition=Vector3.zero;model.transform.localRotation=Quaternion.identity;
  foreach(var t in model.GetComponentsInChildren<Transform>(true))t.gameObject.layer=Layer;
  foreach(var c in model.GetComponentsInChildren<Collider>())c.enabled=false;
  foreach(var animator in model.GetComponentsInChildren<Animator>()){animator.applyRootMotion=false;animator.Rebind();animator.Update(0);animator.enabled=false;}
  CharacterVisibility.Configure(model);foreach(var lod in model.GetComponentsInChildren<LODGroup>())lod.ForceLOD(0);
  var clips=Resources.LoadAll<AnimationClip>("Characters/Animations/Idle");idle=null;foreach(var clip in clips)if(!clip.name.StartsWith("__")){idle=clip;break;}if(idle!=null)idle.SampleAnimation(model,.25f);
  var renderers=model.GetComponentsInChildren<Renderer>();Bounds bounds=new Bounds(model.transform.position+Vector3.up,Vector3.zero);bool first=true;foreach(var renderer in renderers){if(first){bounds=renderer.bounds;first=false;}else bounds.Encapsulate(renderer.bounds);}
  var center=bounds.center;float height=Mathf.Max(1,bounds.size.y);camera.transform.position=center+new Vector3(0,height*.035f,height*2.05f);camera.transform.LookAt(center+Vector3.up*height*.025f);nextFrame=0;
 }
 void LightAt(Vector3 at,Color color,float intensity){var light=new GameObject("Portrait soft light").AddComponent<Light>();light.transform.SetParent(stage.transform,false);light.transform.localPosition=at;light.type=LightType.Point;light.range=7;light.intensity=intensity;light.color=color;light.cullingMask=1<<Layer;light.shadows=LightShadows.None;}
 public void Render(){if(camera==null||model==null||!stage.activeSelf||Time.unscaledTime<nextFrame)return;nextFrame=Time.unscaledTime+.1f;if(idle!=null)idle.SampleAnimation(model,Time.unscaledTime%Mathf.Max(.01f,idle.length));camera.Render();}
 public void Dispose(){if(camera!=null)camera.targetTexture=null;if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}if(stage!=null){stage.SetActive(false);UnityEngine.Object.Destroy(stage);}target=null;stage=null;camera=null;model=null;loaded=false;}
}
