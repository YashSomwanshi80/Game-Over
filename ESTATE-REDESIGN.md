# Courtyard estate redesign

Applied directly to C:\Users\HP\Downloads\GameOver-Prototype-Source. Open the same project; no ZIP import and no scene recreation.

## Direction and extent

The user authorized a larger house/layout rebuild after rejecting the prototype appearance. This pass replaces both former exterior generators with one authored estate design. It retains and refinishes the three-level house, its working stair cores, bedroom suites, basement service rooms and character/game systems. Ground-floor partitions, the garden-room extension and exterior circulation are newly authored. This is an AAA-inspired foundation; it is not a finished AAA or hyper-realistic asset set.

The design uses courtyard-house principles researched from [Malvern 01 / Dan Webster Architecture](https://www.archdaily.com/803318/malvern-house-01-dan-webster-architecture): two floors above ground, basement amenities, timber screens and a landscaped courtyard. The game layout is original. No external architectural photos or plan drawings were imported into the game.

## Installed changes

- Basement: office, gym/cellar, game room, safe room, storage, laundry, utility and workshop remain linked by the existing switchback stairs. New lower-wall finishes and service lighting details give this floor a different identity.
- Ground floor: kitchen and dining are divided by a wall with two passages; dining and the foyer also have two passages. Closing off any one passage still leaves another indoor route to the telephone in the offline clearance model. A glazed, furnished garden room connects the kitchen side to the enclosed grounds.
- First floor: two bedroom suites, library, tech studio, bathrooms and two balconies retain their known access points. New upholstered focal walls, sconces, stone/wood surfaces and ceiling details refine those rooms.
- Exterior: sandstone spine, deep roof eaves, timber window screens, recessed window surrounds, portico, garage fascia, rear pergola, enclosed boundary walls, steel fencing, pedestrian/vehicle gates, driveway drainage, landscape lighting, planting beds and a shallow reflection basin. Ten scanned trees replace the former capsule-shaped crowns. Three raised planters provide physical crouching cover.
- Twelve usable hides: kitchen cupboard, two beds, garage locker, workshop locker, garden-room cabinet, four bedroom cupboards and two curtained bathroom tubs. The bathroom/bedroom extension is described in BATHROOM-BEDROOM-HIDES.md. Seven working paired-door assemblies now include the main entrance, the kitchen/garden-room connection and the garden exit. Look at a leaf and press E or use the mobile action button.
- Both garden entrances and the main front door open; boundary gates remain locked containment boundaries. The telephone/survival objective is unchanged. Don's patrol and the survivor bot's destination candidates include the new garden and terrace routes.
- The menu shows the actual estate exterior with a slow camera move and readable overlay. Starting a game restores the first-person camera and existing intro/character selection flow. Don and Tapsee's character assets were preserved.
- Existing heavy rain/thunder/lightning remain. The new roof shelters garden-room rain and sound. Splash emitters are raised above the new outdoor floors. Interior mains lighting still switches off through the existing breaker logic; separate boundary lanterns remain independent.

## Assets and budgets

Added CC0 [tree](https://polyhaven.com/a/tree_small_02), [dining table](https://polyhaven.com/a/dining_table), [dining chair](https://polyhaven.com/a/dining_chair_02), [sandstone](https://polyhaven.com/a/large_sandstone_blocks) and [oak flooring](https://polyhaven.com/a/wood_floor). [Poly Haven's license](https://polyhaven.com/license) permits commercial use. Authors, conversion changes and installed file hashes are recorded in ESTATE-ASSET-CREDITS.json.

Tree source: 2,062,487 triangles. Runtime LODs: 39,999 / 11,499 / 3,142 triangles, shared across placements. Leaf alpha is explicit, textures are 1K, raw normal/roughness maps use linear sampling. This is an asset budget, not a measured phone frame rate. Architecture remains spatially batched; doors batch only their own moving leaves; scanned trees use distance LODs and are excluded from the static batches.

## Verification and limitations

Confirmed: runtime and editor C# compilation against the installed Unity assemblies; eight offline design/asset check groups, including 1,246 sampled door-leaf poses, four alternate-route closure cases, all hiding exits and unobscured window apertures; 11 door-state checks; seven session routing checks; 23 weather checks. The checks use exported active-source collision data and the installed EstateDesign.json, not the stale staging project.

An attempted Unity batch capture exited with status 1 before producing a review log or screenshots; its cause was not established. Do not treat external C# compilation as a Unity import, shader compilation or Play-mode pass. Actual NavMesh traversal through all stairs/doors, visuals, physics, audio, LAN synchronization and mobile performance remain unverified.

The architectural PNG and floor-plan PNG in the workspace's outputs/EstateReview directory are review artifacts. The architectural image is rendered in Blender from the installed design/base geometry and model resources, with review lighting; it is not Unity gameplay footage and does not recreate every legacy room-dressing mesh.

## Review in Unity

Stop Play, let the project import/compile and press Play again. The main menu should now show the exterior. Start as Tapsee, inspect the dining/kitchen passages, then use E on the main entrance or the west garden-room doors to walk outside. Check the cabinet hide, both bedroom suites and basement. In Play mode, run Game Over > Check Navigation in Play Mode, then Game Over > Check Presentation Assets. Use the same updated project version on both multiplayer peers.

Before this redesign, code/shaders and project settings were saved to the workspace outputs/Backups/Before-courtyard-estate-redesign.zip. This is a recovery archive, not a project installation ZIP.
