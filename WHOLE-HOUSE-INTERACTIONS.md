Whole-house lighting and single doors

Current update supersedes the bedroom-only lighting and paired hinged doors described in older update notes.

Removed the landing east guard shown next to the studio desk. Its old position is continuous upper-floor flooring.

25 light circuits now cover bedrooms, bathrooms, kitchen, dining, living room, entrance hall, store, garage, garden room, library, tech studio, landing, basement rooms/passage and both balconies. Every circuit has visible ceiling fittings and a wall switch. Fixtures hang below the ceiling/trim. Room diffusers, architectural emitters and chandeliers follow the same room setting; bedside lamps and bedroom sconces remain connected. Switch feedback reports the room state and explains when the main basement breaker is off. Restoring mains retains each switch setting. Either character can operate switches. State is replicated to the other LAN player.

23 single hinged doors now serve bedrooms, studies, bathrooms, kitchen/dining passages, store, garage, main entrance, garden room and exit, basement entrance and enclosed basement rooms. Wider old openings have fixed side panels so the leaf remains a normal single-door width. The store entrance is offset away from the crossing kitchen partition. Balcony glass doors remain sliding as requested. Doors stop for occupants, can be used by AI, and send their target state and current opening fraction to the other LAN player.

Use E or the mobile action while looking at a switch, leaf, handle or door jamb. Mobile action text now identifies lights and doors for both characters.

Checks completed: runtime and editor C# compilation; circuit mapping, independent switches and breaker override rules; 25 supported switch approaches and clear sightlines; 2,047 single-door poses; 606 balcony sliding-panel poses; room/phone/exterior routes; all 12 hiding spots and their exits. Geometry checks use the same HouseDoorLayout and HouseLightingLayout data as runtime.

Native Unity automated validation could not start in this session: the attempted process produced no editor log or validation result. Actual Play-mode lighting, Physics interaction, Unity NavMesh and two-device LAN behavior remain unverified. The batch validator is Assets/Editor/HouseInteractionValidation.cs.

Stop Play mode, wait for Unity compilation to finish, then press Play in the existing project. No new scene, download or project registration is required.
