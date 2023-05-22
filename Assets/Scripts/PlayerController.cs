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
            rig.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = .5f + magnitude / maxRunningSpeed;

            if (fsm.GetCurrentState().ID == MovementStatus.Walking)
                rig.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = .5f;
            else
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

    void ApplyRootMotion(bool clamp)
    {

        if (rootmotion)
        {
            var posX = currentRootMotionData.curvePosX;
            var posY = currentRootMotionData.curvePosY;
            var posZ = currentRootMotionData.curvePosZ;

            var rotX = currentRootMotionData.curveRotX;
            var rotY = currentRootMotionData.curveRotY;
            var rotZ = currentRootMotionData.curveRotZ;
            var rotW = currentRootMotionData.curveRotW;

            float currentTime;

            Debug.Log("PlayerController, request clip = " + Animator.StringToHash(currentRootMotionData.clipName));
            Debug.Log("PlayerController, current = " + animator.GetCurrentAnimatorStateInfo(0).shortNameHash);
            Debug.Log("PlayerController, next = " + animator.GetNextAnimatorStateInfo(0).shortNameHash);

            if (animator.GetCurrentAnimatorStateInfo(0).IsName(currentRootMotionData.clipName))
                currentTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            else if (animator.GetNextAnimatorStateInfo(0).IsName(currentRootMotionData.clipName))
                currentTime = animator.GetNextAnimatorStateInfo(0).normalizedTime;
            else 
                return;

            //Debug.Log("PlayerController, currentTime = " + currentTime);

            var xPosDelta = (posX.Evaluate(currentTime * currentRootMotionData.length + Time.fixedDeltaTime) - posX.Evaluate((currentTime * currentRootMotionData.length))) / Time.fixedDeltaTime;
            var yPosDelta = (posY.Evaluate(currentTime * currentRootMotionData.length + Time.fixedDeltaTime) - posY.Evaluate((currentTime * currentRootMotionData.length))) / Time.fixedDeltaTime;
            var zPosDelta = (posZ.Evaluate(currentTime * currentRootMotionData.length + Time.fixedDeltaTime) - posZ.Evaluate((currentTime * currentRootMotionData.length))) / Time.fixedDeltaTime;

            var xRotDelta = rotX.Evaluate(currentTime * currentRootMotionData.length) * Time.fixedDeltaTime;
            var yRotDelta = rotY.Evaluate(currentTime * currentRootMotionData.length) * Time.fixedDeltaTime;
            var zRotDelta = rotZ.Evaluate(currentTime * currentRootMotionData.length) * Time.fixedDeltaTime;
            var wRotDelta = rotW.Evaluate(currentTime * currentRootMotionData.length) * Time.fixedDeltaTime;

            var quat = new Quaternion(xRotDelta, yRotDelta, zRotDelta, wRotDelta);
            var eulerAngle = quat.eulerAngles;

            var x = currentRootMotionData.speed *(body.transform.right * xPosDelta);
            var y = currentRootMotionData.speed *(body.transform.up * yPosDelta);
            y = Vector3.zero;

            var originRot = new Quaternion(currentRootMotionData.curveRotX.Evaluate(0), currentRootMotionData.curveRotZ.Evaluate(0), currentRootMotionData.curveRotZ.Evaluate(0), currentRootMotionData.curveRotW.Evaluate(0));
            var originY = Quaternion.Euler(0, differenceAngle, 0);

            var unclampedZ = currentRootMotionData.speed * (body.transform.forward * zPosDelta);
            //var unclampedZ = currentRootMotionData.speed * (body.transform.forward * zPosDelta) + differenceAngle * Vector3.up;
            var clampedZ = Vector3.MoveTowards(
                new Vector3(0f, 0f, body.velocity.z),
                unclampedZ, 
                .02f);

            var xRotFinal = rotX.Evaluate(currentRootMotionData.length) * Time.fixedDeltaTime;
            var yRotFinal = rotY.Evaluate(currentRootMotionData.length) * Time.fixedDeltaTime;
            var zRotFinal = rotZ.Evaluate(currentRootMotionData.length) * Time.fixedDeltaTime;
            var wRotFinal = rotW.Evaluate(currentRootMotionData.length) * Time.fixedDeltaTime;


            var finalQuat = new Quaternion(xRotFinal, yRotFinal, zRotFinal, wRotFinal);
            var finalEulerAngle = finalQuat.eulerAngles;
            finalEulerAngle.x = 0f;
            finalEulerAngle.z = 0f;

            Debug.DrawRay(transform.position + Vector3.up * 1.7f, (unclampedZ.magnitude > clampedZ.magnitude || !clamp ? unclampedZ + y + x : clampedZ + y + x) * 0.3f, Color.blue);
            Debug.DrawRay(transform.position + Vector3.up * 1f, originY * (unclampedZ.magnitude > clampedZ.magnitude || !clamp ? unclampedZ + y + x : clampedZ + y + x), Color.red);
            //Debug.DrawRay(transform.position + Vector3.up * 2f, unclampedZ.magnitude > clampedZ.magnitude || !clamp ? unclampedZ + y + x : clampedZ + y + x, Color.red);

            body.velocity = originY * (unclampedZ.magnitude > clampedZ.magnitude || !clamp ? unclampedZ + y + x: clampedZ + y + x);

            body.transform.rotation = Quaternion.Slerp(baseRotation, originY * baseRotation * Quaternion.Euler(finalEulerAngle).normalized, currentTime);
            /*body.transform.rotation = Quaternion.RotateTowards(body.transform.rotation, 
                baseRotation * Quaternion.Euler(finalEulerAngle).normalized, 
                (Time.fixedDeltaTime * currentRootMotionData.length) * 10f * currentRootMotionData.speed);*/

            body.transform.GetChild(0).eulerAngles = originY * (baseVisualRotation + new Vector3(0f, eulerAngle.y, 0f));
        }
    }

    internal void SetRootMotion(bool value, AnimationClipRootMotionData data = null)
    {
        Debug.Log("PlayerController, SetRootMotion : value = " + value + ", keepRecenteringDisable = " + keepRecenteringDisable);
        if (value && !rootmotion)
        {
            rootmotion = value;
            currentRootMotionData = data;
            animationHasBegun = false;
            baseVisualRotation = body.transform.GetChild(0).eulerAngles;
            baseRotation = body.transform.rotation;
            //baseRotation = body.transform.GetChild(0).rotation;
            differenceAngle = 0f;
        }
        else if (!keepRecenteringDisable)
        {
            rootmotion = value;
            currentRootMotionData = data;
            body.rotation = body.transform.GetChild(0).rotation;
            body.transform.GetChild(0).localRotation = Quaternion.identity;
            baseVisualRotation = body.transform.GetChild(0).eulerAngles;
            baseRotation = Quaternion.identity;
            differenceAngle = 0f;
        }
        else
            keepRecenteringDisable = false;


    }
    internal void SetKeepRootMotion(bool value, AnimationClipRootMotionData animationClipRootMotionData)
    {
        if (value)
        {
            keepRecenteringDisable = value;
            currentRootMotionData = animationClipRootMotionData;
            baseVisualRotation = body.transform.GetChild(0).eulerAngles;
            baseRotation = body.transform.rotation;
            //baseRotation = body.transform.GetChild(0).rotation;
            differenceAngle = Quaternion.Angle(transform.rotation, body.transform.GetChild(0).rotation);
        }
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
        if(fsm.GetCurrentState().TransitionAllowedList.Contains(MovementStatus.Running) 
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
