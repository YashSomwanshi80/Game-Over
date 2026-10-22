# Rigged character asset pass

The project now ships real skinned FBX replacements for both roles. This supersedes the earlier procedural-only CHARACTER-REFINEMENT.md pass. It is a substantial asset upgrade, not a claim of completed AAA character production.

## Load in Unity

Open `GameOver` with Unity 6000.3.21f1 and let the imports finish. The editor automatically creates `Resources/Characters/Don.prefab` and `Tapsee.prefab`, materials, LOD groups and locomotion controllers. The existing gameplay replacement paths load these prefabs.

If regeneration is needed, choose **Game Over > Rebuild Rigged Characters**, followed by **Game Over > Check Rigged Characters**. Enter Play mode after import and check both roles. No scene regeneration is required just for these assets.

## Assets

- Don: the user's existing Hi3D black-coat export, preserving the face, sunglasses, hair, coat folds and footwear. Converted from approximately two million triangles, fitted to a skinning rig and reweighted around the layered coat.
- Tapsee: a simpler techie outfit: slate overshirt, dark tee, jeans, sneakers, glasses, ponytail and smartwatch. Fitted photographic eye/skin textures and a subtle baked skin micro-normal improve surface definition. This is an adapted anatomical character, not an exact portrait likeness.
- Each character has a 65-bone hierarchy, at most four skin influences per vertex, three LODs, 2K maximum albedo maps, and idle/walk/run/crouch-idle/crouch-walk clips derived from the user's supplied animations.
- Main meshes remain below 50k triangles. Both characters have two reduced distance meshes. Actual exported triangle counts are recorded in `CharacterSources/export-qa.json`.
- Imported colliders are disabled. The existing movement controllers, AI and LAN authority still control position. Speed and crouch drive the imported animators; root motion is disabled. Don's remote crouch is connected as well as Tapsee's.
- The FBX animation files contain a tiny bind-pose carrier mesh. It preserves the skeleton's real bind matrices during export. Only their animation clips are referenced by the game; the carrier is never instantiated in gameplay.

## Verification and limits

Runtime and editor C# compile against the installed Unity assemblies. Sixteen existing movement-rule checks pass. Blender re-imports the actual shipped FBXs and renders idle, walking, running and crouching for both characters. The export report checks bone counts, weights, material slots, LOD budgets and pose bounds. The crouch exports now lower the characters correctly rather than returning a standing pose.

Unity Play-mode import/rendering, shader compilation and physical-device frame time have not been verified in this environment. Previous native Unity batch attempts were blocked by licensing. The Unity menu check is supplied for the actual editor import.

This is the first rigged replacement pass. Further work includes coat/cuff clipping cleanup across the full animation range, dedicated skin/eye/hair material work, improved facial likeness, expression and interaction animation, first-person arm replacement, and real mobile profiling. Don currently uses his supplied colour atlas rather than a complete separately authored skin/cloth normal and roughness set. Cloth simulation and facial animation are not included.

Editable Blender sources: `CharacterSources/Don_HeatRigged.blend` and `CharacterSources/Tapsee_Rigged.blend`. `Don_Rigged.blend` is an intermediate bind preparation file, not the final Don source. Studio renders in `CharacterSources/Previews` are asset previews, not screenshots of Unity gameplay.

## Provenance

Don's Hi3D model and the Mixamo-format FBX animation files were already present in the user's supplied asset folders. Their existing source terms remain applicable.

Tapsee uses MakeHuman CC0 core assets: hm08 base, female-young macro targets, young Caucasian female skin, male_casualsuit01 overshirt, female_casualsuit01 jeans, shoes05, ponytail01, high-poly eyes and brown eye atlas. Glasses, watch and crew-neck patch are custom; skin micro-normal was baked from a native seamless procedural material. Original licence headers remain in the authoring sources. No MakeHuman application code is shipped.
