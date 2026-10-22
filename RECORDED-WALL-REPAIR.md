# Recorded wall repair — 20 October 2026

Reviewed frames from Screen Recording 2026-10-20 200012.mp4. The video shows hard wall/trim transitions and very dark upper rooms once the mains indicator becomes OFFLINE. The later power-off lighting is an existing gameplay state, not proof of missing walls.

Source-confirmed defect: the basement west/east shaft walls extended from -3.4 m up to +1.1 m, overlapping the ground-floor enclosure. They now stop at y=0. Existing ground-floor walls cover both full side intervals with matching shaft-facing surfaces; the basement horizontal footprints remain unchanged. Central divider and north return guards were retained.

Existing wall/floor classification now recognizes the original surface material in addition to names. Thin plaster sections such as Downstairs sides, Safe room sides and Washroom doorway sections no longer fall through the furniture-style rounded-mesh path. Existing wall finishes recognize these sections too. This refines the current mansion, without another shell/prefab.

Validation: runtime/editor C# compile, upper-room route geometry and five hiding/objective clearance checks passed. STAIR-WALL-CHECKS.txt verifies wall endpoints, horizontal footprints and coverage by the existing ground-floor walls. No post-fix Unity rendering or physical stair-navigation test was performed. This does not claim every visible wall/lighting issue is fixed.

Stop Play, allow scripts to compile, then restart the existing scene. Inspect both sides of the basement entrance while moving the camera, walk down/up the return stairs, and compare with power ON. Run Game Over > Check Navigation in Play Mode for engine route checks. Character models and unrelated gameplay were unchanged.
