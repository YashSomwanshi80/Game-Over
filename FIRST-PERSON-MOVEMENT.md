# First-person movement transitions

The existing FirstPersonMotor now accelerates toward intended world-space velocity and brakes faster on release. Walk, sprint and crouch maximum speeds remain unchanged. Diagonal input shares the same acceleration budget. Turning the camera no longer redirects existing momentum instantly.

CCTV/action movement locks, hiding and resets clear horizontal momentum immediately. The same motor is used by solo play and host-authoritative rival gameplay. No scene, mansion geometry, character asset, camera perspective or network packet was replaced.

Runtime/editor C# compilation passed. All 25 movement-rule checks passed, including nine new acceleration/braking/action-lock checks. Unity collisions, stairs, physical input, camera clearance and LAN play still require engine testing; the arithmetic tests do not establish game feel.

In the existing scene, check small joystick movements, walking/sprinting into a stop, rapid direction changes, crouching near furniture, both staircases, hiding entry/exit and CCTV. Check host/client roles separately.
