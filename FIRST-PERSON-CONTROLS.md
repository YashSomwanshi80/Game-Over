# First-person controls update

Keep using this Unity project. Stop Play mode and allow compilation, then press Play. Settings > Controls provides independent mouse and touch sensitivity (0.25–3.00), vertical inversion and camera-control reset. Pause > Controls lets you adjust the camera during a match.

Both solo and RivalMatch use the same safe-area-scaled touch rotation. Start movement on the visible joystick. One look finger keeps ownership until release/cancellation; a second touch cannot take it over. Hiding view limits remain active.

External runtime/editor compilation and camera-input arithmetic checks passed. Unity settings layout, persistence, input and Android multitouch still require Play-mode/device verification.

## Camera-clearance continuation

The crouch eye transition now permits at most 4 cm of lag above the requested eye height, rather than leaving the standing camera above a newly crouched capsule. Standing still blends smoothly upward. Existing explicit hiding snaps and under-bed eye heights are retained. Near-wall hand retraction reacts immediately and eases back after clearance returns; this is a visual response, not new physical hand collision.

Active runtime/editor C# compilation and all 15 camera-input/eye-transition rule checks passed. Physical camera clearance, hands against actual walls and furniture, both roles and Android input still require Unity/device validation. Test crouching beneath low geometry, standing where blocked, entering/exiting both bed hides and approaching walls at walking/sprint speed.
