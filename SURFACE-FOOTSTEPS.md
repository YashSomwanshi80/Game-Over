## Surface-footstep continuation

The existing solo and rival audio paths now share one SurfaceFootsteps cache with three takes each for wood, hard floors and carpet. Classification uses a downward collision query and existing rug/carpet renderer bounds (including originals retained by spatial batching). Rugs reduce audible step gain and solo AI hearing range to 65 percent. The breaker click remains independent of carpet damping. Existing stance/speed cadence, spatial sources and occlusion handling remain.

The nine short clips are procedurally synthesized source audio, not recorded production Foley. Their sound quality and mix need review; licensed/recorded shoe-on-surface Foley remains a production asset requirement. Hard-floor selection currently follows collider names; bathrooms sharing a bedroom slab still use that slab's base profile until distinct surface metadata is authored.

Runtime/editor compilation passed. No Unity audio playback, actual rug classification, hearing-balance playthrough or two-device LAN audio was verified. Check stepping on/off the living-room rug, upstairs carpets, garage and basement; compare crouch/walk/sprint and test both roles. No scene, mansion shell or character geometry changed.
