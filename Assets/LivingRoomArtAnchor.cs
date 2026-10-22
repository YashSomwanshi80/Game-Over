using UnityEngine;

// Marks editor-authored art that the normal gameplay builder should reuse.
[DisallowMultipleComponent]
public sealed class LivingRoomArtAnchor : MonoBehaviour
{
    [HideInInspector] public Vector3 referencePosition;
    [HideInInspector] public Quaternion referenceRotation=Quaternion.identity;
    [HideInInspector] public Vector3 referenceScale=Vector3.one;
    [Tooltip("Includes the scanned sofa, chairs, coffee table, vase and planter.")] public bool includesFurniture;
    public bool Aligned => Vector3.Distance(transform.position,referencePosition)<.01f
        &&Quaternion.Angle(transform.rotation,referenceRotation)<.1f
        &&Vector3.Distance(transform.lossyScale,referenceScale)<.01f;
    public bool HasGeometry {
        get {
            foreach(var group in GetComponentsInChildren<LODGroup>()) {
                var levels=group.GetLODs();if(!group.enabled||levels.Length<2)continue;
                bool valid=true;
                foreach(var level in levels){if(level.renderers.Length==0){valid=false;break;}foreach(var r in level.renderers)if(r==null)valid=false;}
                if(valid)return true;
            }
            return false;
        }
    }
    public void CaptureAlignment()
    {referencePosition=transform.position;referenceRotation=transform.rotation;referenceScale=transform.lossyScale;}
    public bool HasRoomShell {
        get {foreach(var r in GetComponentsInChildren<MeshRenderer>())if(r.name.Contains("_PLASTER_"))return true;return false;}
    }
}
