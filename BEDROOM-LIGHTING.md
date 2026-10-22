# Bedroom lighting refinement

Moved each existing bedroom ceiling fixture from behind the headboard (z=1) over the bed/entrance area (z=-2.1). Existing fixture count and main intensities remain unchanged. Added a short-range warm point light at each existing bedside diffuser: 2850 K, range 2.4 m, intensity 0.65. The two lamp lights join the existing switched fixture list and cannot consume the shadow-light budget.

Mains-off disables these lamps along with the ceiling lights. Their glow geometry was already switched by MansionRoomDetails. No automatic power failure or independent always-on interior light was added. These sources approximate local direct lamp light, not baked indirect illumination.

Runtime/editor C# compilation and source-integration checks passed. Actual exposure, occlusion/light leakage, lamp mix, shadows and device cost still require Unity review. Start with mains ON; inspect both bedroom entrances, bed areas and lamps, then compare OFF and restored power. Existing room geometry, characters and controllers are unchanged.
