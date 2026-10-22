# Bathroom and bedroom hiding places

Added directly to the existing mansion on 21 October 2026:

- One hollow porcelain tub in each upstairs bathroom. Compact 1.6 m and 1.8 m tubs fit the existing rooms, with curved inner basins, faucets, opaque pleated curtains and a narrow front peek gap.
- Two real hollow walnut cupboards in each bedroom: one at the north outside corner, one at the south outside corner. Each has recessed doors, brass handles, an interior shelf, hanging rail and garments. Door gaps remain open for viewing from inside.
- Six additional hiding locations bring the total to twelve.

Look at a cupboard's front or the front of a bathtub curtain and press E, or tap the contextual action on Android. Use the action again to leave. Don must lose sight of you before entry. Existing exit-clearance checks remain active. In multiplayer, human Don can search these locations using the same action as other hides.

The tub camera starts below the rim, 43 cm above the basin floor, looking slightly upward through the curtain gap. It can look 45 degrees up/down and turn within the existing 65-degree low-hide yaw limit. First-person arms are hidden in tubs to avoid a standing-arm pose. Cupboard cameras sit inside the hollow cases and can look around. Host snapshots include the tub pose flag and eye height; client hide entry/exit now snaps to the authoritative position instead of interpolating through an enclosure. Both peers must use the updated build.

The second bathroom toilet moves along the east wall to clear the tub and doorway. Its visual model and collision use the same new position. Decorative wardrobe fluting and south-corner prints that would be hidden behind the new cupboards were removed. Filled navigation exclusions are separate from the hollow physical colliders; agents should not route into a hiding cavity.

Verification: runtime and editor C# compilation passed. Active-source geometry checks pass for all twelve hiding cameras/exits, upstairs room routes, alternate ground-floor paths, window apertures and 1,246 door swing samples. Eight additional hiding checks cover the six new prompts/ranges, 8 cm eye clearance, enclosed cupboard sides, tub rim/rest view, upward peek rays, fixture collision and counts. These are offline checks. Actual Unity camera framing, entry/exit, NavMesh behavior, human searches, LAN delivery and Android performance still need Play-mode/device testing.

To review: stop Play, allow compilation/import, then restart the same scene. Walk upstairs and test all four cupboards, both tubs, leaving while an exit is occupied, and the multiplayer search interaction. No scene recreation or ZIP import is needed.

Recovery archive: workspace outputs/Backups/Before-bathroom-and-bedroom-hides.zip contains all modified pre-change source files.
