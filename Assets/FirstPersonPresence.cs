using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

// View-model only: bones never move the controller or contribute collision/visibility sources.
[DefaultExecutionOrder(100)]
public sealed class FirstPersonPresence : MonoBehaviour
{
    sealed class Arm {public Transform root,wrist,grip,index;public float side;}
    readonly List<Arm> arms=new List<Arm>();
    Transform rig;Light fill;
    Material cloth,cuff,skin,nails;PbrLibrary pbr;
    float phase,weight,speed,callBlend,hiddenBlend,gesture,wallBlend;
    float eyeHeight=1.55f,targetEyeHeight=1.55f;
    bool dressedAsDon;
    public void SetWardrobe(bool asDon)
    {
        if(cloth==null||asDon==dressedAsDon)return;dressedAsDon=asDon;
        cloth.color=asDon?new Color(.065f,.072f,.083f):new Color(.10f,.15f,.19f);
        cuff.color=asDon?new Color(.035f,.038f,.045f):new Color(.09f,.13f,.16f);
        cloth.SetFloat("_Glossiness",asDon?.17f:.12f);
        skin.color=asDon?new Color(.5f,.29f,.18f):new Color(.57f,.35f,.24f);
    }
    bool active,calling,hiding,visible,initialized,lastHidden,lastPower;
    public void SetMotion(float horizontalSpeed,bool enabledMotion){speed=horizontalSpeed;active=enabledMotion;}
    public void SetEyeHeight(float height,bool snap)
    {
        targetEyeHeight=height;
        if(snap){eyeHeight=height;transform.localPosition=new Vector3(0,height,0);}
    }
    public void SetInteraction(bool callInProgress,bool hidden,bool cameraOpen,bool powered,bool playing)
    {
        calling=callInProgress;hiding=hidden;visible=playing&&!cameraOpen;
        if(initialized&&(lastHidden!=hidden||!lastPower&&powered))gesture=.65f;
        initialized=true;lastHidden=hidden;lastPower=powered;
    }
    public void Build(PbrLibrary library)
    {
        pbr=library;
        cloth=pbr.Tint(pbr.CharacterCloth,new Color(.10f,.15f,.19f),"Tapsee / slate overshirt fabric");
        cloth.SetFloat("_Glossiness",.12f);
        cuff=pbr.Tint(pbr.CharacterCloth,new Color(.09f,.13f,.16f),"Tapsee / overshirt cuffs");
        skin=pbr.Tint(pbr.Skin,pbr.Skin.color,"First-person skin");nails=pbr.Tint(pbr.Skin,new Color(.60f,.39f,.29f),"Natural nails");nails.SetFloat("_Glossiness",.42f);
        rig=VisualMesh.Joint("Tapsee / first-person arms",Vector3.zero,transform);
        BuildArm(-1);BuildArm(1);
        foreach(var renderer in rig.GetComponentsInChildren<Renderer>()) {
            renderer.gameObject.layer=8;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
        }
        // Only merge meshes on one bone; finger and wrist pose pivots stay independent.
        foreach(var bone in rig.GetComponentsInChildren<Transform>()) {
            var attached=new List<Renderer>();foreach(Transform child in bone){var r=child.GetComponent<Renderer>();if(r!=null)attached.Add(r);}
            if(attached.Count>1)bone.gameObject.AddComponent<StaticVisualBatch>().Build(attached);
        }
        fill=new GameObject("First-person soft fill / layer 8 only").AddComponent<Light>();
        fill.transform.SetParent(rig,false);fill.transform.localPosition=new Vector3(0,-.05f,.12f);
        fill.type=LightType.Point;fill.range=1.25f;fill.intensity=.35f;fill.color=new Color(.72f,.81f,.9f);fill.cullingMask=1<<8;fill.shadows=LightShadows.None;
        rig.gameObject.SetActive(false);
    }
    GameObject Rounded(string name,Vector3 p,Vector3 size,float bevel,Material material,Transform parent)
    {return VisualMesh.Rounded(name,p,size,bevel,material,parent);}
    void BuildArm(float side)
    {
        var arm=new Arm{side=side};arm.root=VisualMesh.Joint(side<0?"Left hoodie arm":"Right hoodie arm",new Vector3(side*.31f,-.51f,.2f),rig);
        var sleeve=VisualMesh.Lathe("Tapered hoodie sleeve",Vector3.zero,new[]{new Vector2(.10f,0),new Vector2(.095f,.08f),new Vector2(.085f,.25f),new Vector2(.072f,.36f)},cloth,arm.root,20,.9f);
        sleeve.transform.localRotation=Quaternion.Euler(90,0,0);
        var rib=VisualMesh.Lathe("Ribbed hoodie wrist cuff",new Vector3(0,0,.34f),new[]{new Vector2(.073f,0),new Vector2(.074f,.02f),new Vector2(.064f,.072f),new Vector2(.056f,.072f),new Vector2(.06f,0)},cuff,arm.root,20,.9f);
        rib.transform.localRotation=Quaternion.Euler(90,0,0);
        Rounded("Sleeve stitched seam",new Vector3(side*.079f,-.023f,.19f),new Vector3(.008f,.008f,.25f),.003f,cuff,arm.root);
        arm.wrist=VisualMesh.Joint("Wrist pose",new Vector3(0,0,.405f),arm.root);
        Rounded("Wrist",new Vector3(0,0,.012f),new Vector3(.085f,.047f,.06f),.018f,skin,arm.wrist);
        Rounded("Hand palm",new Vector3(0,0,.062f),new Vector3(.095f,.055f,.106f),.022f,skin,arm.wrist);
        arm.grip=VisualMesh.Joint("Relaxed finger group",Vector3.zero,arm.wrist);
        arm.index=VisualMesh.Joint("Index finger pose",Vector3.zero,arm.wrist);
        Finger(arm.index,-side*.033f,.078f,12);
        Finger(arm.grip,-side*.011f,.085f,21);Finger(arm.grip,side*.011f,.079f,27);Finger(arm.grip,side*.033f,.066f,33);
        var thumb=VisualMesh.Joint("Thumb",new Vector3(-side*.048f,-.007f,-.028f),arm.wrist);
        thumb.localRotation=Quaternion.Euler(0,-side*44,-side*12);Finger(thumb,0,.065f,18);
        arms.Add(arm);
    }
    void Finger(Transform parent,float x,float length,float curl)
    {
        Vector3 from=new Vector3(x,-.002f,.108f);
        for(int i=0;i<3;i++) {
            float part=length*(i==0?.44f:i==1?.32f:.24f),width=.019f-i*.002f;
            Quaternion rotation=Quaternion.Euler(curl+i*15,0,0);Vector3 to=from+rotation*Vector3.forward*part;
            var segment=Rounded("Shaped finger phalanx",(from+to)*.5f,new Vector3(width,width*.86f,part+.003f),.006f,skin,parent);segment.transform.localRotation=rotation;
            if(i==2) {
                var nail=Rounded("Natural fingernail",(from+to)*.5f+rotation*Vector3.up*(width*.43f),new Vector3(width*.65f,.002f,part*.65f),.0008f,nails,parent);nail.transform.localRotation=rotation;
            }
            from=to;
        }
    }
    float Obstruction()
    {
        float nearest=.72f;
        for(int i=-1;i<=1;i++) {
            RaycastHit hit;Vector3 origin=transform.position+transform.right*(i*.15f);
            if(Physics.Raycast(origin,transform.forward,out hit,nearest,~(1<<8),QueryTriggerInteraction.Ignore))nearest=hit.distance;
        }
        return 1-Mathf.Clamp01(nearest/.72f);
    }
    void LateUpdate()
    {
        float dt=Time.deltaTime;
        weight=Mathf.MoveTowards(weight,active&&GamePreferences.CameraBob?Mathf.Clamp01(speed/2.7f):0,dt*5);
        if(active)phase+=dt*speed*2.2f;
        eyeHeight=FirstPersonLookRules.EyeTransition(eyeHeight,targetEyeHeight,dt);
        transform.localPosition=new Vector3(Mathf.Sin(phase)*.009f*weight,eyeHeight+Mathf.Sin(phase*2)*.014f*weight,0);
        if(rig==null)return;
        bool show=visible&&active;if(rig.gameObject.activeSelf!=show)rig.gameObject.SetActive(show);
        if(!show)return;
        float blend=1-Mathf.Exp(-dt*9);
        callBlend=Mathf.Lerp(callBlend,calling?1:0,blend);hiddenBlend=Mathf.Lerp(hiddenBlend,hiding?1:0,blend);
        float obstruction=Obstruction();
        // Retract immediately; ease back out only after the wall is clear.
        wallBlend=obstruction>wallBlend?obstruction:Mathf.Lerp(wallBlend,obstruction,blend);
        if(gesture>0)gesture=Mathf.Max(0,gesture-dt);
        float action=Mathf.Sin((1-gesture/.65f)*Mathf.PI)*(gesture>0?1:0);
        rig.localPosition=Vector3.down*(wallBlend*.36f)+Vector3.back*(wallBlend*.16f);
        foreach(var arm in arms) {
            float right=arm.side>0?1:0;
            Vector3 rest=new Vector3(arm.side*.31f,-.51f,.2f);
            Vector3 reach=new Vector3(arm.side*.22f,-.30f,.20f);
            arm.root.localPosition=Vector3.Lerp(rest,reach,callBlend*right)+new Vector3(0,Mathf.Sin(phase+arm.side)*.009f*weight+action*.07f*right-hiddenBlend*.055f,0);
            arm.root.localRotation=Quaternion.Euler(-4-callBlend*right*8,arm.side*(-12-hiddenBlend*6),arm.side*(4+Mathf.Sin(phase)*weight*2));
            arm.wrist.localRotation=Quaternion.Euler(-callBlend*right*8+action*6,0,-arm.side*callBlend*3);
            arm.index.localRotation=Quaternion.Euler(-callBlend*right*12,0,0);
            arm.grip.localRotation=Quaternion.Euler(hiddenBlend*12+callBlend*right*5,0,0);
        }
    }
}
