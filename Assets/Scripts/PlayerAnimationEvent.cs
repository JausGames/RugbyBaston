using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    PlayerController controller;
    private void Start()
    {
        controller = GetComponentInParent<PlayerController>();
    }
    // Update is called once per frame
    void CantBeCancelAnymore()
    {
        controller.CanCancel = false;
    }

    void PlayFootStepSoundEvent()
    {
        controller.SoundManager.PlayFootstep();
    }
}
