# Existing hair-card lighting correction

The existing two-sided cutout hair shader now reads VFACE and flips its tangent-space normal for back-facing hair cards. Front faces retain their previous normal direction. Hair meshes, coverage texture, cutoff, hairstyle and rig remain unchanged. Don's standard atlas material does not use this shader.

Reference: Unity's shader semantics documentation defines VFACE as positive for front faces and negative for back faces: https://docs.unity3d.com/ru/2018.4/Manual/SL-ShaderSemantics.html

Source checks confirm the VFACE input, normal assignment and single cutoff property. Native Unity shader compilation and visible hair lighting are NOT verified by these checks. After import, run Game Over > Check Rigged Characters for shader/import diagnostics, then inspect Tapsee from front/back with interior and window lighting while playing as Don or in LAN. This is a shading correction, not a new production-quality character or a photoreal claim.
