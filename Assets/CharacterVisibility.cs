using UnityEngine;
// Keep small moving character parts evaluated when passing doors and camera edges.
// Never force renderer.enabled: role visibility and intentionally hidden meshes retain ownership.
public static class CharacterVisibility {
 public static void Configure(GameObject model){
  foreach(var animator in model.GetComponentsInChildren<Animator>(true))animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
  foreach(var renderer in model.GetComponentsInChildren<Renderer>(true)){
   renderer.allowOcclusionWhenDynamic=false;
   var skin=renderer as SkinnedMeshRenderer;if(skin==null)continue;
   skin.updateWhenOffscreen=true;
   // Expand authored bounds instead of replacing model-specific origins and extents.
   var bounds=skin.localBounds;bounds.Expand(.6f);skin.localBounds=bounds;
  }
 }
}
