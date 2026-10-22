using UnityEngine;

public sealed class HidingDecor : MonoBehaviour
{
    public static bool Replaces(string name)=>name.StartsWith("Washroom 1 bathtub")||name.StartsWith("Washroom 2 bathtub");
    public static Material Finish(PbrLibrary p,string material)=>material=="wood"?p.Walnut:material=="porcelain"?p.Porcelain:material=="curtain"||material=="fabric"?p.Linen:p.Metal;
    public void Build(PbrLibrary pbr)
    {
        foreach(var spot in HidingLayout.Spots()) {
            if(spot.bed)continue;
            var root=VisualMesh.Joint(spot.name+" details",new Vector3(spot.x,spot.y-(spot.tub?.16f:.1f),spot.z),transform);
            root.localRotation=Quaternion.Euler(0,spot.tub?0:spot.yaw-180,0);
            if(spot.tub){Bathtub(spot,root,pbr);continue;}
            bool bedroom=spot.name.StartsWith("Bedroom");float center=bedroom?.525f:.54f;
            foreach(float side in new[]{-center,center}) {
                VisualMesh.Rounded("Recessed cupboard door panel",new Vector3(side,1.2f,-.557f),new Vector3(bedroom?.87f:.84f,2.12f,.02f),.012f,spot.wooden?pbr.Walnut:pbr.Metal,root);
                float handle=side+(side<0?.32f:-.32f);
                VisualMesh.Rounded("Cupboard brass handle",new Vector3(handle,1.12f,-.58f),new Vector3(.04f,.25f,.055f),.012f,pbr.Brass,root);
                if(!spot.wooden)for(int i=0;i<5;i++)VisualMesh.Rounded("Locker vent slot",new Vector3(side,1.65f+i*.07f,-.572f),new Vector3(.58f,.021f,.007f),.003f,pbr.Rubber,root);
            }
            if(bedroom) {
                VisualMesh.Rounded("Cupboard fitted crown",new Vector3(0,2.43f,0),new Vector3(2.08f,.10f,1.04f),.015f,pbr.Walnut,root);
                VisualMesh.Rounded("Cupboard interior shelf",new Vector3(0,.40f,.04f),new Vector3(1.76f,.04f,.76f),.008f,pbr.Walnut,root);
                VisualMesh.Rounded("Cupboard hanging rail",new Vector3(0,2.06f,.24f),new Vector3(1.76f,.025f,.025f),.008f,pbr.Brass,root);
                foreach(float x in new[]{-.60f,.60f}) {
                    CoutureMesh.Cushion("Hanging folded garment",new Vector3(x,1.5f,.24f),new Vector3(.38f,.9f,.13f),.045f,pbr.Fabric,root);
                    VisualMesh.Rounded("Garment hanger",new Vector3(x,2.0f,.24f),new Vector3(.36f,.025f,.025f),.006f,pbr.Walnut,root);
                }
            }
        }
    }
    static void Bathtub(HidingLayout.Spot spot,Transform root,PbrLibrary p)
    {
        float radius=spot.tubLength/2;
        // The profile returns down the inner wall to a real basin floor; the top is open.
        var profile=new[]{new Vector2(0,.08f),new Vector2(radius*.65f,.08f),new Vector2(radius*.90f,.23f),new Vector2(radius*.98f,.52f),new Vector2(radius,.62f),new Vector2(radius*.97f,.66f),new Vector2(radius*.90f,.64f),new Vector2(radius*.80f,.45f),new Vector2(radius*.64f,.19f),new Vector2(radius*.30f,.16f),new Vector2(0,.16f)};
        VisualMesh.Lathe("Hollow curved porcelain bathtub",Vector3.zero,profile,p.Porcelain,root,40,.8f/spot.tubLength);
        VisualMesh.Lathe("Basin chrome drain",new Vector3(0,.168f,0),new[]{new Vector2(0,0),new Vector2(.027f,0),new Vector2(.027f,.004f),new Vector2(0,.004f)},p.Metal,root,16);
        VisualMesh.Rounded("Bath faucet upright",new Vector3(.30f,.80f,-.37f),new Vector3(.035f,.30f,.035f),.009f,p.Metal,root);
        VisualMesh.Rounded("Bath faucet spout",new Vector3(.30f,.95f,-.27f),new Vector3(.035f,.035f,.24f),.009f,p.Metal,root);
        VisualMesh.Rounded("Bath mixer control",new Vector3(.15f,.72f,-.37f),new Vector3(.11f,.025f,.04f),.007f,p.Metal,root);
        float width=(spot.tubLength-.14f)/2;
        foreach(float side in new[]{-1f,1f}) {
            CoutureMesh.Curtain("Opaque bath curtain / front peek gap",new Vector3(side*(.07f+width/2),2.15f,.45f),width,1.49f,p.Linen,root);
            var end=CoutureMesh.Curtain("Opaque bath curtain / end panel",new Vector3(side*radius,2.15f,0),.90f,1.49f,p.Linen,root);
            end.transform.localRotation=Quaternion.Euler(0,90,0);
            VisualMesh.Rounded("Bath side curtain rail",new Vector3(side*radius,2.20f,.02f),new Vector3(.025f,.025f,.93f),.008f,p.Metal,root);
            VisualMesh.Rounded("Bath rail ceiling hanger",new Vector3(side*radius,2.68f,.05f),new Vector3(.015f,.96f,.015f),.004f,p.Metal,root);
        }
        VisualMesh.Rounded("Bath curtain front rail",new Vector3(0,2.20f,.45f),new Vector3(spot.tubLength+.08f,.025f,.025f),.008f,p.Metal,root);
    }
}
