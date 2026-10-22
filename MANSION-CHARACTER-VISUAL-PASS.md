# Mansion and character visual refinement

Current focus: mansion and character art only. Retain first-person gameplay and the existing mansion. AI and gameplay expansion are deferred during this visual pass.

The original mansion's shared materials now use warmer plaster, satin parquet, oiled walnut and less reflective honed stone. Cloth/linen/plaster normal strength is restrained. World-PBR smoothness responds proportionally to the existing roughness maps (a neutral 0.5 retains the material base smoothness); the storm wet-film override remains separate. No additional mansion shell, floor, wall or duplicate prop was introduced.

Tapsee's existing overshirt, denim and sneaker materials have reduced gloss and tailored normal strengths. The first-person overshirt follows the same cloth response. Existing identity, silhouette, face/hair/eye assets, models, rig, material GUIDs, prefabs and animation controllers are preserved by this file-edit pass; Don's approved appearance is retained.

The character pipeline now refreshes material-only edits independently from model/animation rebuilds. Returning from Play mode queues pending material work. There is also Game Over > Refresh Character Materials Only for a manual refresh if needed; Rebuild Rigged Characters is unnecessary for this pass.

Validation: active runtime/editor C# compilation and source/asset consistency checks passed. Shader source changed, but native Unity shader compilation and visual appearance are not verified by the C# compiler. Source hashes confirm saved scene, FBXs, prefabs and controllers were untouched by file edits. Unity may update its serialized import data during import.

Stop Play mode, allow imports to finish and press Play in the existing scene. Inspect the same living-room angle, nearby wall/plaster, furniture finish, and Tapsee's outfit under warm interior and cool window light; check mains on/off and storm. This is a material refinement, not a finished photoreal character or AAA mansion claim. Production geometry/deformation, facial/skin/hair detail and lighting acceptance remain incomplete.
