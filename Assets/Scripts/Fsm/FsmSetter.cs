using UnityEngine;
using static PlayerController;


public static class FsmSetter
{
    static public  FiniteStateMachine<MovementStatus> SetUpControllerFsm(PlayerController controller, Rigidbody body, PlayerAnimatorController animator, PlayerCameraController camera)
    {
        var fsm = new FiniteStateMachine<MovementStatus>();

        var walk = new State<MovementStatus>(MovementStatus.Walking, "Walking",

            delegate
            {
                animator.SetAnimationLayer(MovementStatus.Walking);
                camera.SetRecenteringCamera(true);
            },
            null,
            delegate
            {
                camera.UpdateCamera(.1f);
            },
            delegate
            {
                if (animator.GetSpeed() != 0f || controller.Move.y != 0)
                    controller.RotatePlayer();

                var animX = (animator.GetSpeed() == 0f) && (controller.Move.y == 0) ? controller.Move.x : controller.Move.x / 2f;

                animator.PlayLocomotion(animX, controller.Move.y / 2f);

                camera.SetCinemachineNoiseIntensity(body.velocity.magnitude, controller.MaxRunningSpeed);
                controller.UpdateVfx();
            },
            delegate
            {
                controller.RotateRootBone(controller.Move.y == 0 ? 0f : animator.GetSpeedX());
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
                animator.SetAnimationLayer(MovementStatus.Running);
                camera.SetRecenteringCamera(true);
            },
            null,
            delegate
            {
                camera.UpdateCamera(.1f, 2.2f, 1.9f);
            },
            delegate
            {
                //ApplyRootMotion(true);
                controller.RotatePlayer();
                //ChangeVelocity(maxRunningSpeed, runningVelocity, runningReDirectionForce, runningDirBrakePow);
                camera.SetCinemachineNoiseIntensity(body.velocity.magnitude, controller.MaxRunningSpeed);
                animator.PlayLocomotion(controller.Move.x, 1f);
                controller.UpdateVfx();
            },
            delegate
            {
                controller.RotateRootBone(animator.GetSpeedX());
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
                animator.SetAnimationLayer(MovementStatus.Fighting);
                camera.SetRecenteringCamera(true);
            },
            null,
            delegate
            {
                camera.UpdateCamera(.1f);
            },
            delegate
            {
                //ChangeVelocityTwoDirection(maxFightingSpeed, fightingVelocity, fightingDirBrakePow);
                camera.SetCinemachineNoiseIntensity(body.velocity.magnitude, controller.MaxRunningSpeed);
                animator.PlayLocomotion(controller.Move.x, controller.Move.y);
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
                animator.SetAnimationLayer(MovementStatus.Walking);
                animator.PlayHurdle();
            },
            null,
            delegate
            {
                camera.UpdateCamera(2f);
                controller.UpdateVfx();
            },
            delegate
            {
                //ApplyRootMotion(true);
                camera.SetCinemachineNoiseIntensity(body.velocity.magnitude, controller.MaxRunningSpeed);
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
                animator.SetAnimationLayer(MovementStatus.Walking);
                animator.PlaySpinR();
                camera.UpdateCamera(2f);
                camera.SetRecenteringCamera(false);
            },
            null,
            delegate
            {
                camera.UpdateCamera(2f);
                controller.UpdateVfx();
            },
            delegate
            {
                //ApplyRootMotion(false);
                camera.SetCinemachineNoiseIntensity(body.velocity.magnitude, controller.MaxRunningSpeed);
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
                animator.SetAnimationLayer(MovementStatus.Walking);
                animator.PlayJukeBack();
            },
            null,
            delegate
            {
                camera.UpdateCamera(2f);
                controller.UpdateVfx();
            },
            delegate
            {
                //ApplyRootMotion(false);
                camera.SetCinemachineNoiseIntensity(body.velocity.magnitude, controller.MaxRunningSpeed);
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