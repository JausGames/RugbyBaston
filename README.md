# RugbyBaston

## To do

### Controller : Ball handler

*   [x] Add sounds
    *   [x] Walk and Run sounds are superposed when not full speed : corrected using clip assignation then Play
    *   [ ] To make it even smoother :
        *   [ ] Separate AudioSource in left/right foot, create separate animation event
        *   [ ] Add buffer time to each foot separatly

*   [x] Run & Slow pace strafe tilt
    *   By rotating *root* bone in LateUpdate event of fsm State

*   [ ] Strafe animations
*   [ ] Ball handler avatar mask (or animations ? or IK ?)

### Controller : Chaser

*   [ ] Attack animations
    *   [ ] Charge
    *   [ ] Takedown
    *   [ ] Legs dive
*   [ ] Hit states & animations (KB, KO, KD)
    *   [ ] KnockBack
    *   [ ] KnockOut
    *   [ ] KnockDown