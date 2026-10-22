0.3.4 garage / joystick / interaction update

Garage: sedan center moved to (13.45,0,.5), with 2.4 x 1.44 x 4.7 m authoritative blocker. Locker center is (11.5,.1,6.375), backing to the north wall at z6.875. Hide prompt/exit and navigation exclusion move together. Native route from the garage entrance to the locker approach passes; all 12 hides still enter/exit. Decorative concrete floor overlays the existing physical floor; an additional ceiling fixture belongs to the garage circuit and mains override.

Car: original rounded box body/cabin replaced with a compact authored mesh: continuous hull and wheel cutaways, raked glazing, pillars, roof, alloy rims/spokes, tyres, mirrors, shut lines, handles, lamps, grille and plates. Original assets are not downloaded or licensed as a real vehicle brand. Rendering has been reviewed in Unity, not yet on an Android phone.

Input: push the joystick beyond its upper rim to sprint; ease back to walk or release to stop. Engage/release thresholds use hysteresis. Separate RUN button removed. Desktop SHIFT is unchanged. Use/hide/door control moves inward, with bright green ring and bold text; unavailable use regions permit camera swipes. Started gestures retain ownership when passing over controls.

Feedback: bottom mobile messages use a larger bold font and dark backing. Other compact HUD elements remain.

Checks: 48 native synthetic gesture/layout/look assertions; 59 native gameplay assertions; 140 native AI/navigation assertions. Actual D3D11 renders at outputs/Garage-Review-034-Accepted. Known Editor SearchDatabase startup exception is separately logged. Physical phone touch feel, new Android visuals and real two-device LAN remain pending.
