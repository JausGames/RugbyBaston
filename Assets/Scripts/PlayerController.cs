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
    private FiniteStateMachine<MovementStatus> fsm;
    private bool rootmotion;
    private AnimationClipRootMotionData currentRootMotionData;
    [SerializeField] private bool tryRun;

    public bool Running { get => fsm.GetCurrentState().ID == MovementStatus.Running;}


    public bool Walking { get => fsm.GetCurrentState().ID == MovementStatus.Walking; }
    public bool Fighting { get => fsm.GetCurrentState().ID == MovementStatus.Fighting; }

    public enum MovementStatus
    {
        Walking,
        Running,
        Fighting,
        Hurdle
    }
    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        fsm = new FiniteStateMachine<MovementStatus>();

        var walk = new State<MovementStatus>(MovementStatus.Walking, "Walking",

            delegate {
                SetAnimationLayer(MovementStatus.Walking);
            },
            null,
            null,
            delegate {
                RotatePlayer();
                ChangeVelocity(maxWalkingSpeed, walkingVelocity, walkingReDirectionForce, walkingDirBrakePow);
                PlayAnimation(move.x / 2f, move.y / 2f);
            });
        var run = new State<MovementStatus>(MovementStatus.Running, "Running",

            delegate {
                SetAnimationLayer(MovementStatus.Running);
            },
            null,
            null,
            delegate {
                RotatePlayer();
                ChangeVelocity(maxRunningSpeed, runningVelocity, runningReDirectionForce, runningDirBrakePow);
                PlayAnimation(move.x, move.y);
            });

        var fight = new State<MovementStatus>(MovementStatus.Fighting, "Fighting",
            delegate {
                SetAnimationLayer(MovementStatus.Fighting);
            },
            null,
            null,
            delegate {
                ChangeVelocityTwoDirection(maxFightingSpeed, fightingVelocity, fightingDirBrakePow);
                PlayAnimation(move.x, move.y);
            });

        var hurdle = new State<MovementStatus>(MovementStatus.Hurdle, "Hurdle",
            delegate {
                SetAnimationLayer(MovementStatus.Walking);
                animator.SetTrigger("Hurdle");
            },
            null,
            null,
            delegate {
                ApplyRootMotion();
            });

        fsm.Add(walk);
        fsm.Add(run);
        fsm.Add(fight);
        fsm.Add(hurdle);

        fsm.SetCurrentState(walk);
    }
    internal void SetState(MovementStatus state)
    {
        fsm.SetCurrentState(fsm.GetState(state));
    }

    void ApplyRootMotion()
    {

        if (rootmotion)
        {

            var curveX = currentRootMotionData.curveX;
            var curveY = currentRootMotionData.curveY;
            var curveZ = currentRootMotionData.curveZ;

            var currentTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

            var xDelta = (curveX.Evaluate(currentTime * currentRootMotionData.length + Time.fixedDeltaTime) - curveX.Evaluate((currentTime * currentRootMotionData.length))) / Time.fixedDeltaTime;
            var yDelta = (curveY.Evaluate(currentTime * currentRootMotionData.length + Time.fixedDeltaTime) - curveY.Evaluate((currentTime * currentRootMotionData.length))) / Time.fixedDeltaTime;
            var zDelta = (curveZ.Evaluate(currentTime * currentRootMotionData.length + Time.fixedDeltaTime) - curveZ.Evaluate((currentTime * currentRootMotionData.length))) / Time.fixedDeltaTime;

            var y = currentRootMotionData.speed * body.transform.up * yDelta;


            var unclampedXZ = currentRootMotionData.speed * (body.transform.right * xDelta + body.transform.forward * zDelta);
            var clampedXZ = Vector3.MoveTowards(
                new Vector3(body.velocity.x, 0f, body.velocity.z),
                currentRootMotionData.speed * (body.transform.right * xDelta + body.transform.forward * zDelta), 
                .02f);

            body.velocity = unclampedXZ.magnitude > clampedXZ.magnitude ? unclampedXZ + y: clampedXZ + y;
        }
    }

    internal void SetRootMotion(bool value, AnimationClipRootMotionData data = null)
    {
        rootmotion = value;
        currentRootMotionData = data;
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
        {
            tryRun = false;
            SetRun(true);
        }
        fsm.FixedUpdate();
    }

    private void RotatePlayer()
    {
        float rotation = Running ? move.x * (rotationSpeed * rotationBySpeed.Evaluate(body.velocity.magnitude / maxRunningSpeed)) : move.x * rotationSpeed;
        body.rotation *= Quaternion.Euler(0f, rotation, 0f);
    }

    private void ChangeVelocity(float maxSpeed, float force, float reDirectionForce, float dirBrakePow = 0f)
    {
        var dirBrake = (1f + Vector3.Dot(new Vector3(body.velocity.x, 0f, body.velocity.z).normalized, (body.transform.right * move.x + body.transform.forward * move.y).normalized)) / 2f;

        body.velocity = Vector3.MoveTowards(body.velocity, (body.transform.right * move.x + body.transform.forward * move.y).normalized * body.velocity.magnitude, reDirectionForce) * Mathf.Pow(dirBrake, dirBrakePow);

        var addedValue = Running ? 1f : move.y;

        if (addedValue != 0f)
            body.velocity += body.transform.forward * addedValue * force * accelerationCurve.Evaluate(body.velocity.magnitude / maxSpeed);
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
    public void SetRun(bool context)
    {

        Debug.Log("SetRun = " + context);
        if(fsm.GetCurrentState().ID == MovementStatus.Walking 
            || fsm.GetCurrentState().ID == MovementStatus.Running 
            || fsm.GetCurrentState().ID == MovementStatus.Fighting)
        {
            if (context)
                fsm.SetCurrentState(fsm.GetState(MovementStatus.Running));
            else
                fsm.SetCurrentState(fsm.GetState(MovementStatus.Walking));
        }
        else
        {
            if (context)
                tryRun = true;
            else
                tryRun = false;
        }
    }
    public void SetAMove(bool context)
    {
        if (fsm.GetCurrentState().ID == MovementStatus.Walking)
            fsm.SetCurrentState(fsm.GetState(MovementStatus.Hurdle));
        else if (context && fsm.GetCurrentState().ID == MovementStatus.Running)
        {
            fsm.SetCurrentState(fsm.GetState(MovementStatus.Hurdle));
            tryRun = true;
        }
    }
    public void SetFight(bool context)
    {
        if (context)
            fsm.SetCurrentState(fsm.GetState(MovementStatus.Fighting));
        else
            fsm.SetCurrentState(fsm.GetState(MovementStatus.Walking));
    }
}
