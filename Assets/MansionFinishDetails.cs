using UnityEngine;
using System.Collections.Generic;

// Decorative details only. Existing collision, interaction and navigation stay authoritative.
public sealed class MansionFinishDetails : MonoBehaviour
{
    PbrLibrary pbr;
    GameObject R(string name,Vector3 at,Vector3 size,Material material,float bevel=.008f)
    {return VisualMesh.Rounded(name,at,size,bevel,material,transform);}
    public void Build(PbrLibrary library,List<GameObject> geometry)
    {
        pbr=library;
        Entrance();
        var panel=pbr.Tint(pbr.Plaster,new Color(.31f,.34f,.33f),"Satin painted wall panelling");
        foreach(var box in geometry) {
            var size=box.transform.localScale;var pos=box.transform.position;
            var material=box.GetComponent<Renderer>().sharedMaterial;
            bool wall=box.name.ToLowerInvariant().Contains("wall")||material==pbr.Plaster||material==pbr.Travertine;
            // Keep ordinary floor slabs and furniture out of the wall-finishing path.
            if(!wall||size.y<2.4f||pos.y<0||Mathf.Min(size.x,size.z)>.3f)continue;
            bool horizontal=size.x>size.z;float length=horizontal?size.x:size.z;
            if(length<2)continue;
            float floor=pos.y-size.y/2;
            foreach(float sign in new[]{-1f,1f}) {
                var normal=horizontal?Vector3.forward:Vector3.right;
                var face=pos+normal*sign*((horizontal?size.z:size.x)/2+.025f);face.y=floor+.5f;
                R("Painted lower wall panel",face,horizontal?new Vector3(length,.76f,.018f):new Vector3(.018f,.76f,length),panel);
                R("Timber chair rail",face+Vector3.up*.4f,horizontal?new Vector3(length,.035f,.035f):new Vector3(.035f,.035f,length),pbr.Walnut);
                int count=Mathf.FloorToInt(length/1.15f);
                for(int i=1;i<count;i++) {
                    var at=face+(horizontal?Vector3.right:Vector3.forward)*(-length/2+i*length/count);
                    R("Panel joint stile",at,horizontal?new Vector3(.028f,.72f,.025f):new Vector3(.025f,.72f,.028f),panel);
                }
            }
        }
        // Drapery stays against the wall beside the two existing ground-floor windows.
        var curtain=pbr.Tint(pbr.Fabric,new Color(.33f,.34f,.30f),"Heavy woven window drapery");
        foreach(float window in new[]{-4.5f,7f}) {
            R("Curtain brass rail",new Vector3(window,2.78f,6.55f),new Vector3(2.7f,.035f,.035f),pbr.Brass);
            foreach(float side in new[]{-1f,1f})CoutureMesh.Curtain("Continuous pleated linen drape",new Vector3(window+side*1.23f,2.72f,6.52f),.46f,2.35f,curtain,transform);
        }
        // Appliances and worktop dressing share the existing kitchen footprint.
        for(int row=0;row<3;row++)for(int col=0;col<10;col++)
            R("Honed tile backsplash",new Vector3(-9.77f,1.24f+row*.19f,(-1.25f+col*.49f)+2.5f),new Vector3(.025f,.18f,.48f),pbr.Porcelain,.005f);
        R("Kitchen chopping board",new Vector3(-8.72f,1.1f,(1.12f)+2.5f),new Vector3(.65f,.028f,.45f),pbr.Walnut,.025f);
        R("Folded kitchen towel",new Vector3(-8.65f,1.125f,(1.06f)+2.5f),new Vector3(.27f,.02f,.32f),pbr.Linen,.006f);
        VisualMesh.Lathe("Stoneware utensil crock",new Vector3(-9.15f,1.09f,(1.5f)+2.5f),new[]{new Vector2(0,0),new Vector2(.09f,0),new Vector2(.105f,.23f),new Vector2(.09f,.23f),new Vector2(.08f,.03f)},pbr.Porcelain,transform);
        for(int i=0;i<3;i++)R("Timber utensil handle",new Vector3(-9.19f+i*.035f,1.32f,(1.5f)+2.5f),new Vector3(.02f,.32f,.02f),pbr.Walnut).transform.localRotation=Quaternion.Euler(i*8,0,i*9-9);
        // Bedroom cloth folds belong to the continuous draped duvet mesh.
        foreach(var at in new[]{new Vector3(9.76f,.38f,1.4f),new Vector3(-9.76f,.38f,-2.8f),new Vector3(9.76f,3.78f,-4.4f)}) {
            R("Electrical outlet faceplate",at,new Vector3(.018f,.115f,.08f),pbr.Porcelain,.007f);
            foreach(float z in new[]{-.018f,.018f})R("Outlet recessed socket",at+new Vector3(at.x>0?-.012f:.012f,0,z),new Vector3(.005f,.028f,.009f),pbr.Rubber,.003f);
        }
    }
    void Entrance()
    {
        // The moving single entrance leaf, hardware and frame come from BedroomDoors.
        foreach(float x in new[]{-1.16f,1.16f})R("Entrance limestone casing",new Vector3(x,1.25f,-7),new Vector3(.12f,2.5f,.3f),pbr.EstateStone);
        R("Entrance limestone head",new Vector3(0,2.54f,-7),new Vector3(2.44f,.12f,.3f),pbr.EstateStone);
    }
}
