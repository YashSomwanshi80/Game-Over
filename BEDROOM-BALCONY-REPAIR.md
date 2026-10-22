# Bedroom, stair sensor and balcony repair

Reviewed Screen Recording 2026-10-20 200940.mp4. The source contained a real 0.8 m height mismatch: central stairwell side walls stopped at 5.8 m while bedroom roofs begin at 6.6 m. Extended those same wall boxes to the roof underside, preserving their base/footprint and the intended doorways.

The black box/lens over the stairs is the passage PIR motion sensor, not the CCTV camera. Its old position was inside the stair flight. Moved its existing collision proxy, housing and lens together to mount against the inner east stairwell wall at the entrance. Security-zone behavior was retained.

Balconies retain their existing slabs, barriers and glass/metal rail system. Slabs now use travertine and the exterior-light layer. Added a 0.42 m stone base/coping under the existing railing and resized the glass above it, so the perimeter is readable at night. No replacement mansion was created; character assets are unchanged.

Validation: active runtime/editor C# compilation passed. Active-source upper-floor checks passed for both bedrooms/bathrooms/balconies and library/studio, plus both bed-hide exits. Hiding/objective geometry checks passed. BEDROOM-BALCONY-CHECKS.txt records ten source checks for wall height, sensor mounting and balcony configuration. Post-fix rendering, navigation and player collision are NOT verified inside Unity.

Stop Play, let Unity compile, then restart the existing scene. Inspect the bedroom walls along both sides of the stairwell near the ceiling, the relocated sensor and both balcony perimeters. Use the existing Game Over > Check Navigation in Play Mode command to verify actual engine routes. Compare with mains on first. No scene recreation or ZIP import is needed.
