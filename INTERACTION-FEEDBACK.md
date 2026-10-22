## Interaction-feedback continuation

RivalMatch now gives actor-specific feedback for insufficient headroom, trying to hide while airborne or visible to Don, successful hiding, blocked hiding exits and leaving hiding. Power/door/search feedback goes to the actor who performed the action. Police-call feedback is phrased separately for each role.

Previously these messages were host-local. Host snapshots now carry the joining player's latest notice with an incrementing ID and four-second send lifetime. The client displays each notice ID once instead of refreshing it on every snapshot. Notices do not alter gameplay authority. This is latest-message feedback, not a guaranteed event-history channel; a later notice can supersede an earlier one.

Active runtime/editor compilation passed. Existing match-rule checks passed. Actual host/client delivery, disconnect behavior, UI legibility and blocked-action conditions need a two-instance Unity/LAN playthrough. Use updated project files on both peers. No scene, mansion or character replacement is required.
