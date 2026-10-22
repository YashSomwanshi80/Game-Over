# Interactive estate doors

Seven paired assemblies: Bedroom 1, Bedroom 2, Library, Tech studio, Main entrance, Garden room and Garden exit. All start closed. Look at a leaf within 2.4 m and press E, or use the contextual mobile button, to open/close. Leaves have walnut panels, brass lever handles/backplates and hinges. Each leaf batches its own decorative geometry.

Host simulation owns the seven-bit target mask and seven actual opening fractions. Both LAN peers need the updated version. NPC route-crossing requests open the same assemblies; obstruction queries can pause motion. Dynamic leaves are excluded from the static NavMesh bake.

Confirmed: runtime/editor C# compilation; 11 door-state checks; 1,246 sampled offline swing poses clear collision and the new architectural dressing. These checks do not verify Unity physics, actual NPC passage, input, sound, rendering or LAN delivery. See ESTATE-REDESIGN.md for the complete scope and engine review steps.
