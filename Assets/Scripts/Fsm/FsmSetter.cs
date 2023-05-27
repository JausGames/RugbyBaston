using UnityEngine;
using static PlayerController;


public static class FsmSetter
{
    static public  FiniteStateMachine<MovementStatus> SetUpControllerFsm(PlayerController controller, Rigidbody body, Animator animator)
    {
        var fsm = new FiniteStateMachine<MovementStatus>();

        var walk = new State<MovementStatus>(MovementStatus.Walking, "Walking",

            delegate
            {
                controller.SetAnimationLayer(MovementStatus.Walking);
                controller.SetRecenteringCamera(true);
            },
            null,
            delegate
            {
                controller.UpdateCamera(.1f);
            },
            delegate
            {
                if (animator.GetFloat("Speed") != 0f || controller.Move.y != 0)
                    controller.RotatePlayer();

                var animX = (animator.GetFloat("Speed") == 0f) && (controller.Move.y == 0) ? controller.Move.x : controller.Move.x / 2f;

                controller.PlayAnimation(animX, controller.Move.y / 2f);

                controller.SetCinemachineNoiseIntensity(body.velocity.magnitude);
                controller.UpdateVfx();
            },
            delegate
            {
                controller.RotateRootBone(controller.Move.y == 0 ? 0f : animator.GetFloat("SpeedX"));
            });
        walk.TransitionAllowedList.AddRange(new MovementStatus[]{
        MovementStatus.Running,
        MovementStatus.Fighting,
        MovementStatus.Hurdle,
        MovementStatus.Spin,
        MovementStatus.Juke });
        var run = new State<MovementStatus>(MovementStatus.Running, "Running",

            delegate
            {
                controller.SetAnimationLayer(MovementStatus.Running);
                controller.SetRecenteringCamera(true);
            },
            null,
            delegate
            {
                controller.UpdateCamera(.1f, 2.2f, 1.9f);
            },
            delegate
            {
                //ApplyRootMotion(true);
                controller.RotatePlayer();
                //ChangeVelocity(maxRunningSpeed, runningVelocity, runningReDirectionForce, runningDirBrakePow);
                controller.SetCinemachineNoiseIntensity(body.velocity.magnitude);
                controller.PlayAnimation(controller.Move.x, 1f);
                controller.UpdateVfx();
            },
            delegate
            {
                controller.RotateRootBone(animator.GetFloat("SpeedX"));
            });

        run.TransitionAllowedList.AddRange(new MovementStatus[]{
        MovementStatus.Walking,
        MovementStatus.Fighting,
        MovementStatus.Hurdle,
        MovementStatus.Spin,
        MovementStatus.Juke });

        var fight = new State<MovementStatus>(MovementStatus.Fighting, "Fighting",
            delegate
            {
                controller.SetAnimationLayer(MovementStatus.Fighting);
                controller.SetRecenteringCamera(true);
            },
            null,
            delegate
            {
                controller.UpdateCamera(.1f);
            },
            delegate
            {
                //ChangeVelocityTwoDirection(maxFightingSpeed, fightingVelocity, fightingDirBrakePow);
                controller.SetCinemachineNoiseIntensity(body.velocity.magnitude);
                controller.PlayAnimation(controller.Move.x, controller.Move.y);
            },
            delegate
            {
                if (controller.DesiredRootBoneRotation != 0f)
                    controller.RotateRootBone(0f);
            });
        fight.TransitionAllowedList.AddRange(new MovementStatus[]{
        MovementStatus.Walking,
        MovementStatus.Running});


        var hurdle = new State<MovementStatus>(MovementStatus.Hurdle, "Hurdle",
            delegate
            {
                controller.SetAnimationLayer(MovementStatus.Walking);
                animator.SetTrigger("Hurdle");
            },
            null,
            delegate
            {
                controller.UpdateCamera(2f);
                controller.UpdateVfx();
            },
            delegate
            {
                //ApplyRootMotion(true);
                controller.SetCinemachineNoiseIntensity(body.velocity.magnitude);
            },
            delegate
            {
                if (controller.DesiredRootBoneRotation != 0f)
                    controller.RotateRootBone(0f);
            });
        hurdle.TransitionAllowedList.AddRange(new MovementStatus[]{
        MovementStatus.Walking,
        MovementStatus.Running});

        var spin = new State<MovementStatus>(MovementStatus.Spin, "Spin",
            delegate
            {
                controller.SetAnimationLayer(MovementStatus.Walking);
                animator.SetTrigger("SpinR");
                controller.UpdateCamera(2f);
                controller.SetRecenteringCamera(false);
            },
            null,
            delegate
            {
                controller.UpdateCamera(2f);
                controller.UpdateVfx();
            },
            delegate
            {
                //ApplyRootMotion(false);
                controller.SetCinemachineNoiseIntensity(body.velocity.magnitude);
            },
            delegate
            {
                if (controller.DesiredRootBoneRotation != 0f)
                    controller.RotateRootBone(0f);
            });
        spin.TransitionAllowedList.AddRange(new MovementStatus[]{
        MovementStatus.Walking,
        MovementStatus.Running});

        var juke = new State<MovementStatus>(MovementStatus.Juke, "Juke",
            delegate
            {
                controller.SetAnimationLayer(MovementStatus.Walking);
                animator.SetTrigger("Juke");
            },
            null,
            delegate
            {
                controller.UpdateCamera(2f);
                controller.UpdateVfx();
            },
            delegate
            {
                //ApplyRootMotion(false);
                controller.SetCinemachineNoiseIntensity(body.velocity.magnitude);
            },
            delegate
            {
                if (controller.DesiredRootBoneRotation != 0f)
                    controller.RotateRootBone(0f);
            });
        juke.TransitionAllowedList.AddRange(new MovementStatus[]{
        MovementStatus.Walking,
        MovementStatus.Running});

        fsm.Add(walk);
        fsm.Add(run);
        fsm.Add(fight);
        fsm.Add(hurdle);
        fsm.Add(spin);
        fsm.Add(juke);

        fsm.SetCurrentState(walk);

        return fsm;
    }
}