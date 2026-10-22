# Multiplayer opening update — source 0.3.6

The existing mansion is preserved. This update is applied to the Downloads project; no new scene or ZIP import is needed.

- Tapsee starts in Bedroom 2. Don starts outside the front entrance and is held at the breach point for the first 60 match seconds, with repeated forced-entry sounds. At 60 seconds the main door opens and Don enters the hall. Capture is disabled until entry.
- The match clock starts after the host receives the joining player's confirmation, rather than when the lobby opens.
- The house starts without mains electricity. Six warm candles are placed in bedrooms, the entrance, landing and basement, with sparse local flickering lights. Individual flashlights remain available and retain their battery rules.
- Multiplayer's phone model, collision, interaction and loud ringing source move to the left side of the existing basement-office desk. The living-room desk remains part of the room. Solo's phone remains in its original location.
- At 120 seconds the opening outage ends automatically. Before that point the phone cannot progress and restoring the breaker cannot bypass the timer. Later power cuts remain meaningful: Tapsee must restore mains power to call.
- A continuous five-second call emits an audible, directional double-bell ring on both peers while call progress is active. Interrupted or completed calls stop the ring. Existing capture/rescue rules continue afterward.
- Door and switch state changes play spatial Foley locally and when replicated. Footsteps now have heel/toe transients and surface-dependent noise/resonance. Multiplayer footsteps and indoor storm sound are stronger; reduced-lightning and volume preferences remain respected.
- Network protocol is now version 2; both devices must use the new build. Old builds are rejected instead of silently mixing incompatible opening rules.
- The preceding Don/LAN crouch-camera smoothing repair is also present in source.

## Evidence and limitations

Full source compilation passed against the installed Unity assemblies. Twenty independent .NET timing/objective checks passed (work/Opening-Rules.log). Six new original synthesized sound assets passed PCM-format and peak checks (work/Opening-Audio-Checks.json); they are authored synthesis, not recorded Foley. Source files match the validation copy (work/Opening-036-Source-Manifest.json).

Native Unity validation did NOT execute: managed batch runs stopped at license initialization before importing the project. The native host/loopback/geometry harness is saved only in work/UnityValidation-20261021/Assets/Editor/GameOverOpeningSmoke.cs. Do not copy that batch harness into a normal project or run its Start method in an interactive Editor: it is designed to exit the batch process. Startup evidence: Native-Multiplayer-Opening-Managed.log. This is an execution blocker, not evidence that the user's Unity license is invalid.

No new APK has been produced. The downloadable/root 0.3.5 APK remains unchanged and does not contain these changes. The source/build helper now identifies the next candidate as 0.3.6/code36. No phone was controlled or installed upon. Native spawn support, phone visibility, candle appearance, sound mixing, real-time phase boundaries and actual two-device LAN need acceptance before release.

## Next acceptance run

1. Run native GameOverOpeningSmoke in the validation copy once the batch execution/license environment works. It checks spawns, reachable office phone, blocked early calls, breach/power boundaries, ring state and a real loopback handshake. Run existing solo gameplay and touch suites for shared presentation/audio changes.
2. Build an ARM64 release candidate, independently check its package/version/stripping and record the APK hash. Keep 0.3.5 available until accepted.
3. With two devices using that same candidate, confirm both host-role choices: Bedroom-2 Tapsee spawn, outside Don spawn; no entry before 60s; no room lights or phone progress before 120s; power restoration at 120s; audible interrupted/complete calls; later breaker sabotage/restoration; door/switch audio; capture/rescue, suspension/disconnect and retry/menu. Review candle placement and storm/footstep balance with headphones and phone speakers.

Backups: work/Before-multiplayer-opening-036, including original settings and affected source. New audio has no external recording licence dependency; all pre-existing asset-licensing obligations remain.
