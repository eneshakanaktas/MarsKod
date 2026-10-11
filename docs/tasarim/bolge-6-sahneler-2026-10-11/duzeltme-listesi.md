# Task 1 — fix round 1 findings (art director Enes rejected the look-down; reviewer: Needs fixes)

Enes (art director, strict about realism/logic) on s60-1-asagi.png: "KAMERANIN AŞAĞI BAKIŞI ÇOK MANTIKSIZ DURUYOR BU NE ?"
Controller's diagnosis (agreed by Enes): the rift ends in a closed flat wall (a box, not a path to the canyon); camera straight above sees no sky/horizon, no direction, no scale; walls smooth like plastic; ledge looks like a paper strip glued on; floor and bend not visible.

## Enes's decision — new camera framing (binding)
Camera at the HEAD of the rift, behind and above Kivilcim (behind the two posts), looking ALONG the rift toward the canyon mouth on the horizon, pitched down ~35 deg. In one frame: sky + canyon mouth at the top of the screen; the rift widening and deepening as it recedes and visibly OPENING into the canyon mouth (no closed end wall anywhere); rough rocky layered walls; the path ledge descending along the right wall; later Kivilcim and Serce on it. Redefine `ViewPoint`/`ViewTarget` (+ fov) accordingly. The UI covers roughly the top 18% and bottom 20% of the screen (title, code card) — compose so the rift and canyon mouth sit in the free band between.

## Visual findings (spec 3b.2)
1. Rift must not end in a closed wall. Physically it widens and deepens going away and runs out to where the ground meets the backdrop, joining the painted canyon mouth/sky line seamlessly. (Spec's "daralıp" = perspective narrowing on screen.) Revisit `HalfWidth`/`Depth`/`Length` and the taper; keep C# and HLSL formulas identical.
2. Normal view (s60-0): rift must read as a clear dark cleft from right behind the posts to the canyon mouth; the sky line (`canyonRiftToMouth`) wide/dark enough and continuous with the 3D rift at the horizon.
3. Walls: rough, broken rock — irregular rim, noise-displaced faces, visible stepped strata (each layer steps back, not just painted lines), a few fallen boulders on ledges. Strata colours consistent with the far mesas.
4. Path ledge: visibly CARVED into the right wall (recess with back wall / overhang, rocky outer lip, darker underside), not a flat strip.
5. Light: sun low from the left → left wall in shadow, right wall lit only in its upper part, lower parts and floor in shadow (real shadows: check shadow distance/cascades cover the rift; the left wall must cast onto the right wall). Darkening with depth by shading/shadow is fine; NO fog/haze.
6. Floor and the bend rock visible in the new framing; bend rock grounded (not floating).

## Reviewer code findings (Important)
7. `CameraShot.cs:52-64`, `:103-104`: `Begin` with photo scale < 1 (keyboard open, `_Picture.x < 1`) computes the offset with `picture.x` but `Apply` forces `_Picture.x = 1` → sky jumps on the first frame. Store `picture.x` and blend it to 1 in `MoveTo`/`Cut`; compute the offset consistently with what `Apply` writes.
8. `CameraShot.cs:107`: `Release` only flips the flag; spec 3b.1 requires a smooth return. Add `IEnumerator Return(float seconds)` that eases position/rotation/fov/shift/picture back to the pose captured in `Begin`, then releases. Keep instant `Release()` for Restore/retry and for the 55 blink-cut. Later tasks will call `Return`.

## Minor (fix while there)
9. `CanyonRift.cs:197` hard-coded `7f` duplicates `PathEnd`; tie to one constant. Add a comment on the C# constants naming the HLSL twin (`Ground.shader riftClip`) and vice versa.
10. Guard `Quaternion.LookRotation(lookAt - to)` against a zero vector.

## Deliverables
Frames in task-1-shots/ (overwrite): s60-0-normal, s60-1-asagi (new framing), performance variants; also one keyboard-open check if cheap. Unchanged 41/51/59. `dotnet test` passes. Build clean. Unity processes closed afterwards.
