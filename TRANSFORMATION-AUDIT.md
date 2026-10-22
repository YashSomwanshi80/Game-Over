# GAME OVER — production roadmap (current)

Updated 20 October 2026. This document supersedes prior third-person and replacement-room plans.

## Binding direction

First-person gameplay for Tapsee and Don, solo and LAN multiplayer. Retain and refine the existing mansion, its playable floor plan and scene; do not construct another mansion or load an overlapping room shell. Preserve approved character identities, main-menu/intro flow, hiding, CCTV, mains sabotage, capture, rescue and restart. Main power changes only through Don's action and Tapsee's restoration. Work directly in this active Unity project; no recurring ZIP import workflow.

## Current technical and visual audit

Unity 6000.3.21f1; built-in forward pipeline and Linear colour space. Packages are engine modules, not URP. The existing custom shaders and OnRenderImage effects make an unplanned URP switch risky. Retain the pipeline until comparative device measurements justify migration.

Prototype.unity uses the existing GameOver bootstrap: geometry/colliders and navigation are built at runtime, then dressed by MansionEnvironment and its existing detail components. The replacement living-room shim is inert and disables legacy marked overrides. The original saved scene and character FBXs were preserved during this controls pass.

Six converted CC0 furniture/plant sets and rigged character resources exist. Much architecture and dressing still uses rounded procedural geometry. Detailed meshes, convincing facial/cloth/hair assets, baked indirect lighting and approved reference captures remain unfinished. Neither more small props nor additional full-screen effects alone can meet the photoreal quality target. No finished AAA asset quality is claimed.

Existing mechanics include first-person movement, stamina, crouch/jump, five hiding spots, CCTV, phone/rescue, Don patrol/search/chase/sabotage, role selection and LAN play. Their source presence and pure-rule checks do not establish end-to-end gameplay acceptance.

Most impactful remaining work, in order:
1. Validate the repaired existing living-room/telephone route in Unity: floor continuity, window openings, materials, exposure, shadows, hiding and all match outcomes. Correct the actual captured defects in the original generators/assets.
2. Calibrate consistent material scale/roughness and room lighting on those same surfaces; establish a compatible indirect-light workflow before claiming baked GI. Review power-on/off and storm reference views.
3. Polish first-person controls, hands, collision, stairs and contextual animation. Match touch behavior across solo/roles/multiplayer and retain motion-accessibility controls.
4. Improve reviewed character assets, state-driven animation, fair sensory AI and reusable environmental interactions; expand approved visual quality room by room.
5. Benchmark a named minimum Android device, then tune actual texture, LOD, shadow, reflection, particle and resolution costs. Do not label graphics profiles or Ultra as validated without measurements.

## Phases and acceptance gates

| Phase | Work | Current evidence / gate |
|---|---|---|
| 0 Audit | Active project, packages, architecture, assets and risks | Source audit refreshed; asset snapshot accompanies this document |
| 1 Existing mansion reference area | Repair and refine original living room, materials, lighting, furniture, objective approach | Floor/layer repair, overlap cleanup, fitted collision and reflection scheduling implemented; Unity visual/gameplay acceptance OPEN |
| 2 First-person/character polish | Responsive first-person controller, rig/hand presentation, animation and mobile controls | Shared configurable look input implemented; complete physics, animation and touch review OPEN |
| 3 Mansion completion | Upgrade original rooms, exterior, collision and lighting using approved reference quality | Partially dressed; no replacement mansion; room-by-room acceptance OPEN |
| 4 AI/interactivity | Fair Don perception/search/difficulty, doors/items/clues/puzzles/distractions | Existing AI/actions present; expanded systems and balance OPEN |
| 5 Presentation | Spatial/contextual audio, cohesive UI, skippable narrative/capture/rescue moments | Menu/intro/weather/HUD present; final polish and flow review OPEN |
| 6 Device performance | Measured 30 FPS baseline; optional 60 FPS; measured graphics tiers | Android device specification requested; builds/profiling NOT verified |
| 7 Release candidate | Stable Android build, regression, licences and known issues | NOT ready; no release claim |

Do not mark phases complete based on compilation or generated assets. Phase 1 review remains the critical gate before a whole-house visual rollout.

## Current pass: first-person controls

The solo mode used screen-width-scaled touch rotation while RivalMatch used raw pixels, giving different camera response across modes and screen densities. Both now use FirstPersonLookRules through GamePreferences. Rotation scales with the same safe-area coordinate system as the HUD, without an extra delta-time multiplier. A second finger cannot steal an active look gesture; movement begins on the visible joystick in both paths.

Saved mouse sensitivity, touch sensitivity, invert vertical look and reset are available under Settings > Controls. Controls are also available while paused (reset is on the main Settings page). Mouse defaults are preserved. The new shared touch default deliberately aligns all modes with HUD scale; adjust it for comfort on a real phone. Hiding pitch limits, CCTV, movement authority and network state remain controlled by their original systems.

## Verification and next Unity checks

Confirmed automatically: active-project runtime/editor C# compile against installed Unity assemblies; 10 camera-input rule checks (density/aspect scaling, total swipe distance across input rates, inversion, preference bounds and invalid values), plus existing movement/security/match/routing/hiding/upper-floor checks. These do not execute Unity input, physics, rendering or network transport.

Not confirmed this pass: Unity editor playthrough, shader import/rendering, touch gestures on Android, two-device LAN session, mobile build, FPS/memory/thermal measurements. No native editor success is inferred from the external C# compiler.

In the existing scene, test Settings and pause > Controls, save/relaunch preferences, normal/inverted mouse and swipe look, crouch/sprint/jump/stairs, hidden look/CCTV/exit, phone interruption/completion, Don sabotage and Tapsee restoration, capture/rescue/restart. Check both roles and a host/client pair. Capture the existing living room with power on, power off and lightning; verify the basement is visible only through intended stair openings.

## Remaining asset integration requirements

Characters: retain current approved Don and techie Tapsee. Improve production topology/skinning, facial/eye/hair/cloth response and motion transitions on reviewed assets; validate Generic rigs before any Humanoid reimport. Procedural arms and current bodies are not declared finished photoreal models. Environment: improve existing surfaces/furnishings with metre scale, compatible materials, clean normals, appropriate UV/lightmap data and documented provenance. New assets need licences, bounds, triangle/texture counts, LOD/collision and target-device cost records. Do not create another room shell to conceal defects.
