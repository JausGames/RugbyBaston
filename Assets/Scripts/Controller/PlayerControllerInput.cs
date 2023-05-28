using UnityEngine;
using static PlayerController;

public class PlayerControllerInput : MonoBehaviour
{
    private PlayerController controller;
    private void Awake()
    {
        controller = GetComponent<PlayerController>();
    }
    private void Update()
    {
        if (controller.TryRun)
            controller.TryRun = !SetRun(true);
    }
    public void SetMove(Vector2 context)
    {
        controller.Move = context;
    }
    public bool SetRun(bool context)
    {

        Debug.Log("SetRun = " + context);
        if (controller.Fsm.GetCurrentState().ID == MovementStatus.Walking
            || controller.Fsm.GetCurrentState().ID == MovementStatus.Running)
        {
            if (context)
                controller.Fsm.SetCurrentState(controller.Fsm.GetState(MovementStatus.Running));
            else
                controller.Fsm.SetCurrentState(controller.Fsm.GetState(MovementStatus.Walking));

            return true;
        }
        else
        {
            if (context)
                controller.TryRun = true;
            else
                controller.TryRun = false;
            return false;
        }
    }
    public void SetAMove(bool context)
    {
        if (context && controller.Fsm.GetCurrentState().ID == MovementStatus.Walking)
            controller.Fsm.SetCurrentState(controller.Fsm.GetState(MovementStatus.Hurdle));
        else if (context && controller.Fsm.GetCurrentState().ID == MovementStatus.Running)
        {
            controller.Fsm.SetCurrentState(controller.Fsm.GetState(MovementStatus.Hurdle));
            controller.TryRun = true;
        }
        else if (context && controller.CanCancel && controller.Fsm.GetCurrentState().ID == MovementStatus.Hurdle)
        {
            controller.Animator.PlayCancel();
        }
    }
    internal void SetXMove(bool context)
    {
        if (context && controller.Fsm.GetCurrentState().ID == MovementStatus.Walking)
            controller.Fsm.SetCurrentState(controller.Fsm.GetState(MovementStatus.Spin));
        else if (context && controller.Fsm.GetCurrentState().ID == MovementStatus.Running)
        {
            controller.Fsm.SetCurrentState(controller.Fsm.GetState(MovementStatus.Spin));
            controller.TryRun = true;
        }
        else if (context && controller.CanCancel && controller.Fsm.GetCurrentState().ID == MovementStatus.Spin)
        {
            controller.Animator.PlayCancel();
        }
    }

    internal void SetBMove(bool context)
    {
        if (context && controller.Fsm.GetCurrentState().ID == MovementStatus.Walking)
            controller.Fsm.SetCurrentState(controller.Fsm.GetState(MovementStatus.Juke));
        else if (context && controller.Fsm.GetCurrentState().ID == MovementStatus.Running)
        {
            controller.Fsm.SetCurrentState(controller.Fsm.GetState(MovementStatus.Juke));
            controller.TryRun = true;
        }

        else if (context && controller.CanCancel && controller.Fsm.GetCurrentState().ID == MovementStatus.Juke)
        {
            controller.Animator.PlayCancel();
        }
    }
    public void SetFight(bool context)
    {
        if (context)
            controller.Fsm.SetCurrentState(controller.Fsm.GetState(MovementStatus.Fighting));
        else if (controller.Fsm.GetCurrentState().ID == MovementStatus.Fighting)
            controller.Fsm.SetCurrentState(controller.Fsm.GetState(MovementStatus.Walking));
    }
}