using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableRecenterCameraBehavior : StateMachineBehaviour
{
    private PlayerController controller;
    [SerializeField] float finalBias = 0f;
    [SerializeField] float duration = 0f;
    [SerializeField] bool useClip = false;
    [SerializeField] AnimationClip clip = null;


    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!controller)
            controller = animator.GetComponentInParent<PlayerController>();

        controller.SetBiasOffset(finalBias, useClip ? clip.length : duration);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        /*var controller = animator.GetComponentInParent<PlayerController>();
        controller.SetRecenteringCamera(true);*/
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
