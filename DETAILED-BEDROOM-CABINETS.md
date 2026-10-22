# Detailed bedroom cabinet replacement

Replaced the two existing bedroom dresser visuals with Patrik Pangerl's Modern Wooden Cabinet from Poly Haven, licensed CC0: https://polyhaven.com/a/modern_wooden_cabinet and https://polyhaven.com/license . Source glTF/buffer/textures and checksums are retained in the task's ArtSources archive; BEDROOM-CABINET-ASSET.json records provenance and converted counts.

This is an actual textured furniture mesh replacement, not a material-only adjustment. The model has curved slatted wooden doors, dark framing and slender legs. The converted runtime asset contains 14,867 triangles across three primitives, shared between both placements. It uses 1K albedo, normal and packed roughness/metal textures with neutral missing AO. This is not a measured Android budget or a completed mobile LOD chain.

Uniformly fitted to 2.4 m width at the original bedroom dresser centres. Actual fitted height ~0.669 m and depth ~0.5115 m. Updated the existing collision to 2.4 x 0.69 x 0.53 m and moved the book stack to the measured top. The old procedural dresser is skipped after a successful scan load; a matching short fallback remains for missing resources. Other room geometry and characters are unchanged.

Verified downloaded MD5s, converted finite vertices/normals/UVs, valid indices, material/texture resource paths and colour-space metadata, fit/collision enclosure and source-model front orientation in a Blender preview. Runtime/editor compilation and active-source room/hiding geometric checks passed. Actual Unity import, in-game rendering, NavMesh paths and device performance remain unverified.

Stop Play, allow Unity import/compilation and press Play. Look at the cabinet against the front wall of each bedroom (opposite the headboard, around x=-6 or 5, z=-6.35). It should have visibly slatted doors and metal legs; books should sit on the top. No scene recreation, character rebuild or ZIP download is needed.
