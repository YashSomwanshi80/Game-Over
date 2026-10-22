Mobile controls 0.3.3

User feedback: right-thumb camera swipes hit movement/action buttons; too much HUD text; heavy/glitchy look.

Changes: retain left joystick. RUN above it toggles sprint, freeing the right thumb. Jump, crouch and context action occupy the right edge; flashlight is above the rail; CCTV is at the top. The center-right look area is free of control hitboxes. A touch owns its initial role until release/cancel. Camera swipes cannot activate buttons or hold the phone call when crossing them. Use-button touches stop calling if dragged away, without turning into look gestures. Crouch cancels armed run; suspension resets it.

Mobile HUD: one compact phone-distance/rescue line, relevant danger/hidden/power alerts, stamina bar only when depleted; remove duplicated flashlight footer and desktop hints. Flashlight toggles no longer show a long explanatory toast. Desktop HUD remains detailed. Solo Don/LAN use the same gesture ownership and layout.

Touch look remains direct, distance-based and resolution-normalized; default response increases from 0.13 to 0.20 degrees per canvas pixel. No frame-time multiplication or extra smoothing delay. Mobile targets 60 FPS; this is not a measured guarantee. Skip additional frosted HUD capture/blur passes during active mobile gameplay, retaining the grading/bloom path and menu/pause capture.

Validation so far: 45 native synthetic gesture/layout/look assertions and 59 native gameplay regressions. Phone test and updated release build pending. Do not claim real two-device LAN or measured sustained FPS.
