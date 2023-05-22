using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateRootMotionBehavior : StateMachineBehaviour
{

    [SerializeField] AnimationClip clip;
    [SerializeField] float speed;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var controller = animator.GetComponentInParent<PlayerController>();
        RootMotionExtractor.OnAnimationChange(clip, speed);
        controller.SetRootMotion(true, RootMotionExtractor.OnAnimationChange(clip, speed));
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var controller = animator.GetComponentInParent<PlayerController>();
        if (animator.GetCurrentAnimatorClipInfo(layerIndex)[0].clip.name == clip.name) return;
        controller.SetRootMotion(false);
        /*if (controller.GetState() != PlayerController.MovementStatus.Walking)
        {
            controller.SetState(PlayerController.MovementStatus.Walking);
        }*/
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
