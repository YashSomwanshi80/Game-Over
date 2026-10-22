## Don spotting-response continuation

Solo AI suspicion now controls investigation and chase instead of serving only as a HUD value. Visible awareness rises at 1.7/s standing and 0.85/s crouched, investigation begins at 0.12, and pursuit at 0.45. These are initial tuning values, not playtested difficulty presets. Existing visibility/occlusion and capture checks remain authoritative; close-range visible capture remains immediate. Breaking sight stops updating the target and decays awareness, with the existing last-known-location investigation/search retained. Active visible pursuit does not restart the delay.

This changes the AI opponent in Tapsee solo play, not a human Don in LAN play. Runtime/editor compilation and 10 pure detection-rule checks passed, including brief glimpses, sustained sightings, crouch response, awareness decay and equal 30/60 Hz progression. Actual Unity line-of-sight, corner behavior, hiding and match balance remain unverified.
