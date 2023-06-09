using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetControllerStateBehavior : StateMachineBehaviour
{
    private PlayerCameraController camera;
    private PlayerController controller;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (camera == null)
            camera = animator.GetComponentInParent<PlayerCameraController>();
        if (controller == null)
            controller = animator.GetComponentInParent<PlayerController>();


        if(controller.TryRun)
            controller.SetState(PlayerController.MovementStatus.Running);
        else
            controller.SetState(PlayerController.MovementStatus.Walking);


        camera.SetBiasOffset(0f, 0.1f);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

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
