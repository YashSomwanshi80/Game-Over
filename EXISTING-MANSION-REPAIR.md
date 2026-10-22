# Existing mansion repair — 20 October 2026

Continue with the original mansion. Do not prepare/place a replacement living-room prefab or recreate the scene.

The recording shows the basement through missing sections of the living-room floor. The replacement art path hid the original floor and introduced a second room shell. This update stops loading that shell, restores the original floor, frames and scanned furniture, removes the replacement console collision and disables its outdated floor occlusion map. The shader AO correction, grading, characters, storm and gameplay systems remain.

Stop Play mode, import the updated project files and allow compilation to finish. Open your existing scene and press Play. Previously placed LivingRoomArtAnchor overrides are automatically disabled during play. To persist that cleanup in the saved scene, use Game Over > Existing Mansion > Disable Legacy Room Overrides, then save. This action supports Undo and preserves the inactive objects.

The old room assets remain recoverable but are no longer spawned. The old room creation menus are removed. Create Prototype Scene now retains an existing scene instead of overwriting it.

Validation: runtime/editor compilation and automated movement, security, role, session, hiding and upper-floor checks passed. Source checks verify the original floor path and absence of the replacement collider. Unity rendering, full matches and physical-device performance still require validation.

In Play mode, inspect the living-room floor around the rug, both windows, the garage doorway and telephone route. The basement must only be visible through its intended stairs.
