Current layout, readability and flashlight update

Utility room: removed overlapping added doorway headers and side infill, made the original front partition fit its single door, and closed its missing west side. There is now one enclosed utility room with one entrance. The heater and main breaker keep their existing positions.

Kitchen/dining: removed all four kitchen/dining/hall passage doors and their extra partition walls. The kitchen, dining room and entrance approach are open plan. The enclosed store and exterior security/weather doors remain. Switches formerly mounted on the removed walls were relocated to actual walls with accessible standing positions.

Cupboard sketch: Bedroom 1 north cupboard moved right beneath its bathroom wall, back flush against that wall and opening toward the bedroom. Bedroom 2 north cupboard moved onto the right exterior wall and rotated 90 degrees to open toward the room. Both south cupboards stay in place. Collision, interiors, camera positions, prompts, exits and navigation exclusions follow the new placements.

Lighting: brighter ceiling spots, broad room fills, extra basement fixtures and brighter powered ambient lighting improve wall and floor visibility. Room switches and the main breaker still control the fixtures. Shadow and active-light budgets remain limited for mobile; real device performance is unverified.

Flashlights: L on keyboard, LIGHT button on mobile. Each player has an independent full battery at the start of a match, providing five minutes of on-time. Charge drains only while on and simulation is running, stays unchanged while off, and depletion turns the beam off. Flashlights work during a main power cut. The HUD shows charge and state. The host owns both batteries in multiplayer; beam state, aim and remaining charge are sent to the other player. Beam origin retracts against nearby geometry to avoid projecting from the far side of a wall. Batteries reset with a new match; no automatic recharge was added.

Validation: runtime/editor C# compilation; 13 battery conservation/depletion/snapshot checks; room circuit mapping and breaker checks; all 25 switch approaches; all 12 hide cameras/exits; 1,691 single-door poses and 606 balcony slider poses. Unity Play-mode rendering, native physics/navigation and actual two-device LAN behavior remain unverified.

Stop Play mode, allow Unity to recompile, then press Play in the same project. No scene rebuild or ZIP extraction is required.
