using UnityEngine;
// One telephone model. Its root moves with the existing office interaction/call anchor.
public sealed class DeskTelephoneModel:MonoBehaviour {
 Material cream,dark;Transform model;
 public void Build(PbrLibrary p){
  cream=new Material(p.Rubber){name="Telephone / ivory moulded plastic"};cream.color=new Color(.74f,.71f,.59f);cream.SetFloat("_Glossiness",.32f);cream.SetTexture("_MainTex",Texture2D.whiteTexture);cream.SetTexture("_DetailNormal",null);cream.SetFloat("_NormalStrength",0);cream.SetTexture("_Roughness",Texture2D.whiteTexture);
  dark=new Material(cream){name="Telephone / charcoal receiver"};dark.color=new Color(.035f,.042f,.047f);
  model=new GameObject("Telephone desk phone assembly").transform;model.SetParent(transform,false);model.localPosition=new Vector3(7,1.02f,4);
  Part("Telephone moulded base",Vector3.zero,new Vector3(.62f,.12f,.44f),.045f,cream);
  Part("Telephone raised rear cradle",new Vector3(0,.07f,.13f),new Vector3(.58f,.11f,.16f),.025f,cream);
  foreach(float x in new[]{-.23f,.23f}){
   Part("Telephone receiver earpiece",new Vector3(x,.18f,.13f),new Vector3(.14f,.12f,.16f),.045f,dark);
   Part("Telephone cradle hook",new Vector3(x,.12f,.13f),new Vector3(.045f,.08f,.06f),.012f,cream);
   for(int i=0;i<5;i++)Part("Telephone receiver grille slot",new Vector3(x-.04f+i*.02f,.243f,.13f),new Vector3(.006f,.004f,.055f),.001f,p.Metal);
  }
  Part("Telephone handset",new Vector3(0,.205f,.13f),new Vector3(.4f,.065f,.095f),.03f,dark);
  Part("Telephone LCD bezel",new Vector3(0,.068f,.025f),new Vector3(.25f,.017f,.075f),.008f,dark);
  Part("Telephone green LCD",new Vector3(0,.079f,.025f),new Vector3(.21f,.008f,.05f),.004f,p.ScreenEmitter);
  string[] keys={"1","2","3","4","5","6","7","8","9","*","0","#"};
  for(int row=0;row<4;row++)for(int col=0;col<3;col++){
   Vector3 at=new Vector3((col-1)*.072f,.07f,-.04f-row*.037f);
   Part("Telephone keypad "+keys[row*3+col],at,new Vector3(.052f,.022f,.029f),.006f,dark);
   var key=new GameObject("Telephone dial number").transform;key.SetParent(model,false);key.localPosition=at+Vector3.up*.013f;key.localRotation=Quaternion.Euler(90,0,0);var text=key.gameObject.AddComponent<TextMesh>();text.text=keys[row*3+col];text.fontSize=48;text.characterSize=.0032f;text.anchor=TextAnchor.MiddleCenter;text.color=Color.white;
  }
  for(int i=0;i<32;i++){float t=i/31f;Part("Telephone coiled cord",new Vector3(-.345f+Mathf.Sin(i*1.65f)*.018f,.17f-t*.32f,.13f-t*.15f),new Vector3(.023f,.018f,.023f),.007f,dark);}
  Part("Telephone emergency label plate",new Vector3(.225f,.066f,-.085f),new Vector3(.12f,.012f,.17f),.007f,dark);
  var label=new GameObject("Telephone police label").transform;label.SetParent(model,false);label.localPosition=new Vector3(.225f,.074f,-.085f);label.localRotation=Quaternion.Euler(90,0,0);var marking=label.gameObject.AddComponent<TextMesh>();marking.text="POLICE\n100";marking.fontSize=64;marking.characterSize=.0028f;marking.anchor=TextAnchor.MiddleCenter;marking.color=new Color(.6f,1,.8f);
 }
 void Part(string name,Vector3 at,Vector3 size,float radius,Material mat){VisualMesh.Rounded(name,at,size,radius,mat,model);}
 void OnDestroy(){if(cream!=null)Destroy(cream);if(dark!=null)Destroy(dark);}
}
