using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Running")]
    [SerializeField] float maxRunningSpeed = 15f;
    [SerializeField] float runningVelocity = .8f;
    [SerializeField] float runningBrake = 1.5f;
    [SerializeField] float runningDirBrakePow = .01f;
    [SerializeField] float runningReDirectionForce = .4f;
    [SerializeField] CinemachineFreeLook camera;


    [SerializeField] float rotationSpeed = 8f;
    [SerializeField] AnimationCurve rotationBySpeed;
    [SerializeField] AnimationCurve accelerationCurve;


    [Header("Walking")]
    [SerializeField] private float maxWalkingSpeed = 7f;
    [SerializeField] private float walkingVelocity = .3f;
    [SerializeField] float walkingReDirectionForce = .8f;
    [SerializeField] float walkingDirBrakePow = .02f;
    [Header("Fighting")]
    [SerializeField] private float maxFightingSpeed = 7f;
    [SerializeField] private float fightingVelocity = .3f;
    [SerializeField] float fightingDirBrakePow = .02f;

    [SerializeField] Vector2 move = new Vector2();

    Rigidbody body;
    Animator animator;
    [SerializeField] private FiniteStateMachine<MovementStatus> fsm;
    private bool canCancel;

    // start : Old root motion system, can be deleted
    private bool rootmotion;
    private AnimationClipRootMotionData currentRootMotionData;
    private bool animationHasBegun;
    private bool tryRun;
    private Vector3 baseVisualRotation;
     private Quaternion baseRotation;
    private float differenceAngle;
    // end

    [SerializeField] private bool keepRecenteringDisable;

    public bool Running { get => fsm.GetCurrentState().ID == MovementStatus.Running;}


    public bool Walking { get => fsm.GetCurrentState().ID == MovementStatus.Walking; }
    public bool Fighting { get => fsm.GetCurrentState().ID == MovementStatus.Fighting; }
    public bool Rootmotion { get => rootmotion; set => rootmotion = value; }
    public bool CanCancel { get => canCancel; set => canCancel = value; }
    public bool TryRun { get => tryRun; }

    public enum MovementStatus
    {
        Walking,
        Running,
        Fighting,
        Hurdle,
        Spin,
        Juke
    }
    // Start is called before the first frame update
    void Start()
    {
        body = GetComponentInChildren<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        fsm = new FiniteStateMachine<MovementStatus>();

        var walk = new State<MovementStatus>(MovementStatus.Walking, "Walking",

            delegate {
                SetAnimationLayer(MovementStatus.Walking);
                SetRecenteringCamera(true);
            },
            null,
            delegate {
                UpdateCamera(.1f);
            },
            delegate {
                //ApplyRootMotion(true);
                RotatePlayer();
                //ChangeVelocity(maxWalkingSpeed, walkingVelocity, walkingReDirectionForce, walkingDirBrakePow);
                SetCinemachineNoiseIntensity(body.velocity.magnitude);
                PlayAnimation(move.x / 2f, move.y / 2f);
            });
        walk.TransitionAllowedList.AddRange(new MovementStatus[]{
        MovementStatus.Running,
        MovementStatus.Fighting,
        MovementStatus.Hurdle,
        MovementStatus.Spin,
        MovementStatus.Juke });
        var run = new State<MovementStatus>(MovementStatus.Running, "Running",

            delegate {
                SetAnimationLayer(MovementStatus.Running);
                SetRecenteringCamera(true);
            },
            null,
            delegate {
                UpdateCamera(.1f, 1.5f);
            },
            delegate {
                //ApplyRootMotion(true);
                RotatePlayer();
                //ChangeVelocity(maxRunningSpeed, runningVelocity, runningReDirectionForce, runningDirBrakePow);
                SetCinemachineNoiseIntensity(body.velocity.magnitude);
                PlayAnimation(move.x, move.y);
            });
        run.TransitionAllowedList.AddRange(new MovementStatus[]{
        MovementStatus.Walking,
        MovementStatus.Fighting,
        MovementStatus.Hurdle,
        MovementStatus.Spin,
        MovementStatus.Juke });

        var fight = new State<MovementStatus>(MovementStatus.Fighting, "Fighting",
            delegate {
                SetAnimationLayer(MovementStatus.Fighting);
                SetRecenteringCamera(true);
            },
            null,
            delegate {
                UpdateCamera(.1f);
            },
            delegate {
                //ChangeVelocityTwoDirection(maxFightingSpeed, fightingVelocity, fightingDirBrakePow);
                SetCinemachineNoiseIntensity(body.velocity.magnitude);
                PlayAnimation(move.x, move.y);
            });
        fight.TransitionAllowedList.AddRange(new MovementStatus[]{
        MovementStatus.Walking,
        MovementStatus.Running});


        var hurdle = new State<MovementStatus>(MovementStatus.Hurdle, "Hurdle",
            delegate {
                SetAnimationLayer(MovementStatus.Walking);
                animator.SetTrigger("Hurdle");
            },
            null,
            delegate {
                UpdateCamera(2f);
            },
            delegate {
                //ApplyRootMotion(true);
                SetCinemachineNoiseIntensity(body.velocity.magnitude);
            });
        hurdle.TransitionAllowedList.AddRange(new MovementStatus[]{
        MovementStatus.Walking,
        MovementStatus.Running});

        var spin = new State<MovementStatus>(MovementStatus.Spin, "Spin",
            delegate {
                SetAnimationLayer(MovementStatus.Walking);
                animator.SetTrigger("SpinR");
                UpdateCamera(2f);
                SetRecenteringCamera(false);
            },
            null,
            delegate {
                UpdateCamera(2f);
            },
            delegate {
                //ApplyRootMotion(false);
                SetCinemachineNoiseIntensity(body.velocity.magnitude);
            });
        spin.TransitionAllowedList.AddRange(new MovementStatus[]{
        MovementStatus.Walking,
        MovementStatus.Running});

        var juke = new State<MovementStatus>(MovementStatus.Juke, "Juke",
            delegate {
                SetAnimationLayer(MovementStatus.Walking);
                animator.SetTrigger("Juke");
            },
            null,
            delegate {
                UpdateCamera(2f);
            },
            delegate {
                //ApplyRootMotion(false);
                SetCinemachineNoiseIntensity(body.velocity.magnitude);
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
    }

    private void UpdateCamera(float horizontalDamping, float cameraDistance = 3f)
    {
        //var composer = camera.GetCinemachineComponent<CinemachineComposer>();
        //var thirdPersonFollow = camera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        //composer.m_HorizontalDamping = Mathf.MoveTowards(composer.m_HorizontalDamping, horizontalDamping, 50f * Time.deltaTime);
        //thirdPersonFollow.CameraDistance = Mathf.MoveTowards(thirdPersonFollow.CameraDistance, cameraDistance, 4f * Time.deltaTime);
    }

    private void SetCinemachineNoiseIntensity(float magnitude)
    {
        for(int i = 0; i < 3; i++)
        {
            Debug.Log("SetCinemachineNoiseIntensity : i = " + i);
            var rig = camera.GetRig(i);
            //rig.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = .5f + magnitude / maxRunningSpeed;

            /*if (fsm.GetCurrentState().ID == MovementStatus.Walking)
                rig.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = .5f;
            else*/
                rig.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = .5f + magnitude / maxRunningSpeed;
        }
    }
    public void SetRecenteringCamera(bool recenter)
    {
        if (recenter && !camera.m_RecenterToTargetHeading.m_enabled && keepRecenteringDisable)
        {
            keepRecenteringDisable = false;
            return;
        }
        camera.m_RecenterToTargetHeading.m_enabled = recenter;
    }
    public bool GetRecenteringCamera()
    {
        return camera.m_RecenterToTargetHeading.m_enabled;
    }
    public void KeepRecenteringCameraDisable()
    {
        keepRecenteringDisable = true;
    }

    internal void SetState(MovementStatus state)
    {
        fsm.SetCurrentState(fsm.GetState(state));
    }
    internal MovementStatus GetState()
    {
        return fsm.GetCurrentState().ID;
    }

    private void SetAnimationLayer(MovementStatus state)
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

    // Update is called once per frame
    void FixedUpdate()
    {
        if (tryRun)
            tryRun = !SetRun(true);
        fsm.FixedUpdate();
    }
    // Update is called once per frame
    void Update()
    {
        fsm.Update();
    }

    private void RotatePlayer()
    {
        float rotation = Running ? move.x * (rotationSpeed * rotationBySpeed.Evaluate(body.velocity.magnitude / maxRunningSpeed)) : move.x * rotationSpeed;
        body.transform.rotation *= Quaternion.Euler(0f, rotation, 0f);
    }

    private void ChangeVelocity(float maxSpeed, float force, float reDirectionForce, float dirBrakePow = 0f)
    {
        var dirBrake = (1f + Vector3.Dot(new Vector3(body.velocity.x, 0f, body.velocity.z).normalized, (body.transform.right * move.x + body.transform.forward * move.y).normalized)) / 2f;

        body.velocity = Vector3.MoveTowards(body.velocity, (body.transform.right * move.x + body.transform.forward * move.y).normalized * body.velocity.magnitude, reDirectionForce) * Mathf.Pow(dirBrake, dirBrakePow);

        var addedValue = Running ? 1f : move.y;

        if (addedValue != 0f)
        {
            body.velocity += body.transform.forward * addedValue * force * accelerationCurve.Evaluate(body.velocity.magnitude / maxSpeed);

            Debug.DrawRay(transform.position + Vector3.up * 1f, body.transform.forward * addedValue * force * accelerationCurve.Evaluate(body.velocity.magnitude / maxSpeed), Color.red);
        }
        else if (body.velocity.magnitude > .2f)
            body.velocity /= runningBrake;
        else
            body.velocity = Vector3.zero;

        if (body.velocity.magnitude > maxSpeed * addedValue)
            body.velocity = body.velocity.normalized * maxSpeed * addedValue;
    }
    private void ChangeVelocityTwoDirection(float maxSpeed, float force, float dirBrakePow)
    {
        var dirBrake = (1f + Vector3.Dot(new Vector3(body.velocity.x, 0f, body.velocity.z).normalized, (body.transform.right * move.x + body.transform.forward * move.y).normalized)) / 2f;

        body.velocity *= Mathf.Pow(dirBrake, dirBrakePow);

        if (move != Vector2.zero)
            //body.velocity += (body.transform.forward * move.y + body.transform.right * move.x).normalized * force * accelerationCurve.Evaluate(body.velocity.magnitude / maxSpeed);
            body.velocity += (body.transform.forward * move.y + body.transform.right * move.x).normalized * force;
        else if (body.velocity.magnitude > .2f)
            body.velocity /= runningBrake;
        else
            body.velocity = Vector3.zero;

        if (body.velocity.magnitude > maxSpeed * move.magnitude)
            body.velocity = body.velocity.normalized * maxSpeed * move.magnitude;
    }
    void PlayAnimation(float speedX, float speedY)
    {
        animator.SetFloat("Speed", Mathf.MoveTowards(animator.GetFloat("Speed"), speedY, .05f));
        animator.SetFloat("SpeedX", Mathf.MoveTowards(animator.GetFloat("SpeedX"), speedX, .05f));
    }

    public void SetMove(Vector2 context)
    {
        this.move = context;
    }
    public bool SetRun(bool context)
    {

        Debug.Log("SetRun = " + context);
        if(fsm.GetCurrentState().ID == MovementStatus.Walking
            || fsm.GetCurrentState().ID == MovementStatus.Running)
        {
            if (context)
                fsm.SetCurrentState(fsm.GetState(MovementStatus.Running));
            else
                fsm.SetCurrentState(fsm.GetState(MovementStatus.Walking));

            return true;
        }
        else
        {
            if (context)
                tryRun = true;
            else
                tryRun = false;
            return false;
        }
    }
    public void SetAMove(bool context)
    {
        if (context && fsm.GetCurrentState().ID == MovementStatus.Walking)
            fsm.SetCurrentState(fsm.GetState(MovementStatus.Hurdle));
        else if (context && fsm.GetCurrentState().ID == MovementStatus.Running)
        {
            fsm.SetCurrentState(fsm.GetState(MovementStatus.Hurdle));
            tryRun = true;
        }
        else if (context && canCancel && fsm.GetCurrentState().ID == MovementStatus.Hurdle)
        {
            animator.SetTrigger("Cancel");
        }
    }
    internal void SetXMove(bool context)
    {
        if (context && fsm.GetCurrentState().ID == MovementStatus.Walking)
            fsm.SetCurrentState(fsm.GetState(MovementStatus.Spin));
        else if (context && fsm.GetCurrentState().ID == MovementStatus.Running)
        {
            fsm.SetCurrentState(fsm.GetState(MovementStatus.Spin));
            tryRun = true;
        }
        else if (context && canCancel && fsm.GetCurrentState().ID == MovementStatus.Spin)
        {
            animator.SetTrigger("Cancel");
        }
    }

    internal void SetBMove(bool context)
    {
        if (context && fsm.GetCurrentState().ID == MovementStatus.Walking)
            fsm.SetCurrentState(fsm.GetState(MovementStatus.Juke));
        else if (context && fsm.GetCurrentState().ID == MovementStatus.Running)
        {
            fsm.SetCurrentState(fsm.GetState(MovementStatus.Juke));
            tryRun = true;
        }

        else if(context && canCancel && fsm.GetCurrentState().ID == MovementStatus.Juke)
        {
            animator.SetTrigger("Cancel");
        }
    }
    public void SetFight(bool context)
    {
        if (context)
            fsm.SetCurrentState(fsm.GetState(MovementStatus.Fighting));
        else if(fsm.GetCurrentState().ID == MovementStatus.Fighting)
            fsm.SetCurrentState(fsm.GetState(MovementStatus.Walking));
    }
}
