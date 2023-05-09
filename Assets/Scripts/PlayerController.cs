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

    public bool Running { get => fsm.GetCurrentState().ID == MovementStatus.Running;}
    public bool Walking { get => fsm.GetCurrentState().ID == MovementStatus.Walking; }
    public bool Fighting { get => fsm.GetCurrentState().ID == MovementStatus.Fighting; }

    enum MovementStatus
    {
        Walking,
        Running,
        Fighting
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

        fsm.Add(walk);
        fsm.Add(run);
        fsm.Add(fight);

        fsm.SetCurrentState(walk);
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
        Debug.Log("MovementStatus = " + fsm.GetCurrentState().ID);
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

    public void SetMove(InputAction.CallbackContext context)
    {
        this.move = context.ReadValue<Vector2>();
    }
    public void SetRun(InputAction.CallbackContext context)
    {
        if (context.performed)
            fsm.SetCurrentState(fsm.GetState(MovementStatus.Running));
        else
            fsm.SetCurrentState(fsm.GetState(MovementStatus.Walking));
    }
    public void SetFight(InputAction.CallbackContext context)
    {
        if (context.performed)
            fsm.SetCurrentState(fsm.GetState(MovementStatus.Fighting));
        else
            fsm.SetCurrentState(fsm.GetState(MovementStatus.Walking));
    }
}
