using UnityEngine;
using static PlayerController;

public class PlayerAnimatorController
{
    Animator animator;

    public PlayerAnimatorController(Animator animator)
    {
        this.animator = animator;
    }
    public void SetAnimationLayer(MovementStatus state)
    {
        switch (state)
        {
            case MovementStatus.Walking:
            case MovementStatus.Running:
                animator.SetLayerWeight(0, 1f);
                animator.SetLayerWeight(1, 0f);
                break;
            case MovementStatus.Fighting:
                animator.SetLayerWeight(0, 0f);
                animator.SetLayerWeight(1, 1f);
                break;
            default:
                break;
        }
    }


    public void PlayLocomotion(float speedX, float speedY)
    {
        animator.SetFloat("Speed", Mathf.MoveTowards(animator.GetFloat("Speed"), speedY, .02f));
        animator.SetFloat("SpeedX", Mathf.MoveTowards(animator.GetFloat("SpeedX"), speedX, .1f));
    }

    public void PlayHurdle() { animator.SetTrigger("Hurdle"); }
    public void PlaySpinR() { animator.SetTrigger("SpinR"); }
    public void PlayJukeBack() { animator.SetTrigger("Juke"); }
    public void PlayCancel() { animator.SetTrigger("Cancel"); }


    public float GetSpeed(){return animator.GetFloat("Speed");}
    public float GetSpeedX() { return animator.GetFloat("SpeedX"); }
}