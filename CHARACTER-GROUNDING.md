# Existing character grounding

Measured active FBX mesh bounds in Blender: Don ~1.79981 m tall; Tapsee ~1.71947 m tall, lowest point -0.0117228 m relative to the model origin. Raised the instantiated Tapsee visual by 0.011723 m in RivalMatch, used by Play as Don and LAN views. This does not change the controller, camera, rig, mesh, animation or character proportions.

This is a small sole-placement correction, not new character art. It is not directly visible when playing first-person Tapsee because her full-body model is hidden from her own view. Existing Don appearance is unchanged.

Mesh bounds were measured by actual FBX import; runtime/editor C# compilation passed. Animation foot contact on stairs and rendered results in Unity remain unverified.
