# Mansion estate refinement

Two new furnished rooms occupy the north halves of the upstairs suites: a fitted library with individual books, reading chair and clerestory window, and a dual-monitor tech studio with desk, chair, acoustic slats and a window. The two original bedrooms, en-suite bathrooms, balconies and rear stair-landing routes remain. New partitions and major furniture are part of the gameplay collision and NavMesh build data.

Interior additions include ceiling coffers, brass chandeliers, wall mouldings, gallery frames, layered bedroom rugs, pleated curtains, kitchen upper cabinets and backsplash, table settings, ceramic vessels, botanical stems, a cellar bottle rack and basement service pipes. Existing scanned chairs and vases are reused with their existing asset licences. No new external downloads are required.

The front elevation now has actual glazed openings on both floors, rather than surface decorations over solid walls. Added exterior details include a limestone portico, timber soffit and facade screen, masonry joints, roof parapet, garage coping, individual driveway pavers, planted stone beds, downpipes, courtyard benches and a reflecting basin. Garden additions are exterior backdrop scenery beyond the locked house boundary; they are not new playable garden rooms.

The new library and studio lights participate in the existing power and shadow budgets. Interior luminous materials use the existing switched materials. Static fine details enter the existing spatial material batches; scanned meshes retain their LOD groups. This is not a measured mobile frame-rate guarantee.

## Use the update

Open the updated GameOver Unity project. Stop Play mode, let scripts import, then press Play again. Mansion geometry and furnishings are built by the existing gameplay bootstrap; rebuilding the character assets or recreating the prototype scene is unnecessary for this change. Run **Game Over > Check Navigation in Play Mode** to validate actual NavMesh routes, including the two new rooms.

The source ZIP is updated; any previously built Windows executable remains an older build until rebuilt in Unity.

## Verification

Runtime/editor C# compilation and the geometric upper-floor route check are run against this update. The clearance check uses a 0.35 m agent radius and checks bedrooms, bathrooms, balconies, both new rooms and both under-bed exits. It does not replace actual Unity NavMesh, lighting, shader, camera or phone performance tests. Native Play-mode rendering is unverified in this environment; the supplied plan is a diagram generated from collision layout data, not a gameplay screenshot.

This is a broader mansion detail pass, not completed AAA environment production. Bespoke sculpted furniture, authored artwork, more natural vegetation, consistent baked lighting and measured device optimization remain further work.
