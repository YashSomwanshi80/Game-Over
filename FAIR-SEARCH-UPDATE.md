# Fair-search update

Power restoration now calls the existing hearing system instead of revealing Tapsee globally. Don must be within the sound range on the same floor; intervening collision geometry reduces the range. An active chase still follows existing visual awareness.

Search points now use a horizontal disk around the last known location, a narrow NavMesh sample, a height check and a complete path from Don. Six bounded attempts avoid unbounded work. Failure retains the last destination until the existing search timeout.

External C# compilation passed. Source checks confirm removal of the global position reveal and broad spherical sample. Actual sound occlusion, stair navigation and AI fairness require Unity playtesting; these are not runtime-verified results. No environment geometry, character, power authority or multiplayer protocol changed.
