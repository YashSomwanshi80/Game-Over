# In-game mansion storm

Gameplay now has an independent storm controller, rather than menu-only rain visuals and unrelated thunder audio.

- Wind-driven world-space rain curtains cover the exterior and open balconies. A shared shader roof mask excludes the mansion, garage, porch and space below the west balcony; no screen-wide rain overlay is placed inside rooms.
- Surface impact rings, low drifting wind spray, animated glass droplets/rivulets, moving storm clouds and exposed-surface wet sheen/ripple shading add visual layers. The reflecting basin also receives impacts. Wetness stays outside roof volumes; room floors stay dry.
- Lightning has a branched world-space channel, sky illumination and a short-lived directional light with hard shadows. It can illuminate window openings without changing the electrical power state.
- A deterministic match-time schedule coordinates each flash with spatial thunder delayed by distance / 343 m/s. Both LAN roles evaluate the same schedule from their local match time; this is presentation, not a new network-authoritative weather protocol.
- Dedicated HeavyRain and StormThunder PCM assets are authored offline from procedural noise/envelopes. The rain loop is about 16 seconds; thunder has a nine-second rolling tail. These are synthesized sound-design assets, not field recordings.
- Rain and thunder get filtered and quieter indoors, while hiding and in the basement. Atmosphere and Master Volume settings still apply. The previous in-game rain/thunder sources were removed to prevent doubled ambience; menu weather retains its own sources.
- Pausing or losing application focus pauses particles and sound and suppresses lightning. Match time controls droplets and wet ripples. Weather does not switch off when Don cuts mains power.
- Settings now includes Reduced Lightning, which lowers gameplay flashes and hides the bolt; the menu flash also respects this preference.

## Run and verify

Stop Play mode, let Unity finish importing the updated scripts, shaders and audio, then press Play. Start a match and look through the front/north windows or go onto a balcony. The first lightning event occurs roughly 8–15 seconds into active match time. Thunder arrives roughly 0.8–3.3 seconds later. The storm does not need character rebuilding or scene recreation.

Run **Game Over > Check Presentation Assets** to check shader import and audio resources in the actual editor. Also inspect rain exclusion, glass, lightning shadows, pause/resume, basement muffling and mains-off behaviour in Play mode. Run on the target phone before choosing final particle, transparency and shadow budgets.

## Verification limits

Runtime and editor C# compile against the installed Unity assemblies. Nineteen pure storm-rule checks cover 1,000 scheduled events, light/sound delay, repeated-frame and late-join handling, a ten-minute playback simulation, shelter volumes and sound-exposure ordering. Both new audio assets pass non-silence and amplitude checks.

These checks do not verify Unity shader compilation, particle appearance, audio playback or physical-device frame time here. This is a layered storm implementation, not a claim of finished hyperreal rendering. Photoreal asset materials, lighting calibration and device profiling remain separate work.
