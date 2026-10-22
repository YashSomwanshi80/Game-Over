using UnityEngine;

// Queries ignore observer/target bodies explicitly; steep camera rays can otherwise hit self.
public static class InteractionRaycast
{
    static readonly RaycastHit[] hits=new RaycastHit[64];
    static bool Owned(Collider hit,Collider actor)=>actor!=null&&(hit==actor||hit.transform.IsChildOf(actor.transform));
    public static bool First(Vector3 origin,Vector3 direction,float distance,Collider observer,out RaycastHit nearest,Collider target=null)
    {
        nearest=default(RaycastHit);if(distance<=0||direction.sqrMagnitude<.000001f)return false;
        int count=Physics.RaycastNonAlloc(origin,direction.normalized,hits,distance,~(1<<8),QueryTriggerInteraction.Ignore);
        RaycastHit[] candidates=hits;
        // A full buffer must not silently omit an occluding wall.
        if(count==hits.Length){candidates=Physics.RaycastAll(origin,direction.normalized,distance,~(1<<8),QueryTriggerInteraction.Ignore);count=candidates.Length;}
        float best=float.PositiveInfinity;
        for(int i=0;i<count;i++) {
            var h=candidates[i];if(h.collider==null||Owned(h.collider,observer)||Owned(h.collider,target))continue;
            if(h.distance<best){best=h.distance;nearest=h;}
        }
        return best<float.PositiveInfinity;
    }
    public static bool Clear(Vector3 from,Vector3 to,Collider observer=null,Collider target=null)
    {RaycastHit hit;var delta=to-from;return !First(from,delta,delta.magnitude,observer,out hit,target);}
}
