using UnityEngine;

// Shared, articulated fallback details. Skinning and facial animation still require an authored rig.
public static class CharacterCraft
{
    public static Transform Hand(Transform parent,Vector3 at,float side,PbrLibrary pbr)
    {
        var hand=VisualMesh.Joint("Anatomical hand",at,parent);
        VisualMesh.Lathe("Wrist to palm contour",new Vector3(0,-.11f,0),new[]{new Vector2(.028f,0),new Vector2(.041f,.04f),new Vector2(.038f,.10f),new Vector2(.026f,.135f)},pbr.Skin,hand,16,.62f);
        for(int i=0;i<4;i++) {
            float x=-.027f+i*.018f,length=i==1?.081f:i==2?.075f:i==0?.069f:.056f;
            var finger=VisualMesh.Joint("Relaxed finger "+i,new Vector3(x,-.105f,.004f),hand);finger.localRotation=Quaternion.Euler(-12-i*4,0,0);
            VisualMesh.Lathe("Tapered finger",new Vector3(0,-length,0),new[]{new Vector2(.004f,0),new Vector2(.008f,.01f),new Vector2(.0085f,length*.55f),new Vector2(.010f,length)},pbr.Skin,finger,10,.8f);
            VisualMesh.Rounded("Finger nail",new Vector3(0,-length+.013f,-.006f),new Vector3(.011f,.016f,.003f),.001f,pbr.Nails,finger);
            VisualMesh.Rounded("Finger knuckle crease",new Vector3(0,-length*.49f,-.007f),new Vector3(.012f,.002f,.002f),.0007f,pbr.SkinCrease,finger);
        }
        var thumb=VisualMesh.Joint("Opposed thumb",new Vector3(side*.034f,-.048f,.005f),hand);thumb.localRotation=Quaternion.Euler(-19,0,side*31);
        VisualMesh.Lathe("Thumb phalanges",new Vector3(0,-.065f,0),new[]{new Vector2(.005f,0),new Vector2(.010f,.012f),new Vector2(.013f,.04f),new Vector2(.014f,.068f)},pbr.Skin,thumb,12,.85f);
        hand.gameObject.AddComponent<StaticVisualBatch>().Build(hand.GetComponentsInChildren<Renderer>());
        return hand;
    }
    public static void Shoe(Transform parent,Vector3 at,PbrLibrary pbr,bool leather)
    {
        var shoe=VisualMesh.Joint(leather?"Tailored leather shoe":"Panelled sneaker",at,parent);var upper=leather?pbr.Leather:pbr.Linen;
        VisualMesh.Rounded("Sculpted shoe upper",Vector3.zero,new Vector3(.15f,.12f,.29f),.048f,upper,shoe);
        VisualMesh.Rounded("Layered outsole",new Vector3(0,-.055f,0),new Vector3(.158f,.028f,.3f),.012f,leather?pbr.Rubber:pbr.Porcelain,shoe);
        VisualMesh.Rounded("Heel counter",new Vector3(0,.02f,-.112f),new Vector3(.13f,.09f,.035f),.012f,upper,shoe);
        for(int i=0;i<4;i++) {
            var lace=VisualMesh.Rounded("Cross lace",new Vector3(0,.065f,-.045f+i*.027f),new Vector3(.09f,.006f,.007f),.002f,leather?pbr.Hair:pbr.Linen,shoe);
            lace.transform.localRotation=Quaternion.Euler(0,i%2==0?17:-17,0);
        }
        shoe.gameObject.AddComponent<StaticVisualBatch>().Build(shoe.GetComponentsInChildren<Renderer>());
    }
    public static void FaceDetails(Transform parent,Vector3 origin,PbrLibrary pbr)
    {
        foreach(float side in new[]{-1f,1f}) {
            var brow=VisualMesh.Rounded("Shaped brow",origin+new Vector3(side*.061f,.218f,.104f),new Vector3(.068f,.012f,.012f),.005f,pbr.Hair,parent);
            brow.transform.localRotation=Quaternion.Euler(0,0,side*8);
            VisualMesh.Rounded("Nostril recess",origin+new Vector3(side*.015f,.116f,.149f),new Vector3(.012f,.007f,.008f),.003f,pbr.SkinCrease,parent);
        }
        VisualMesh.Rounded("Upper lip contour",origin+new Vector3(0,.068f,.113f),new Vector3(.049f,.007f,.009f),.003f,pbr.Lips,parent);
        VisualMesh.Rounded("Lower lip contour",origin+new Vector3(0,.059f,.113f),new Vector3(.052f,.008f,.01f),.003f,pbr.Lips,parent);
    }
}
