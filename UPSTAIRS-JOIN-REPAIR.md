# Upstairs wall joins and solid balconies

The 20:18 screenshots showed the previous repair was insufficient. Two suite/bathroom joins had short open offsets. Added full-height wall returns at the library/bathroom and studio/bathroom corners. Generated layout bounds confirm each return overlaps both adjoining wall segments; intended room doorways remain open and geometric routes still pass.

Balcony enclosure now uses the existing 1.1 m solid barrier meshes and colliders with travertine, exterior lighting layer, a stone coping and metal handrail. Removed the additional low base and glass panels. This replaces the previous glass-edge approach; it does not create another balcony or mansion. Mains behavior and characters are unchanged.

Runtime/editor compilation, active-source room/balcony routes, hiding/objective clearance and join-overlap checks passed. No post-fix Unity render or actual collision/navigation test was performed. Confirm the bedroom corners and solid balcony walls after stopping and restarting Play. Camera position and power state can affect how much is visible; these checks do not establish visual acceptance.
