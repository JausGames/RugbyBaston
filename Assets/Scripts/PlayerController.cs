using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class PlayerController : MonoBehaviour
{
    [Header("Running")]
    [SerializeField] float maxRunningSpeed = 15f;
    [SerializeField] CinemachineFreeLook camera;
    [SerializeField] VisualEffect vfx;


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

    private bool tryRun;

    [SerializeField] private bool keepRecenteringDisable;
    private BiasOffset biasOffset;

    [SerializeField] PlayerSoundManager soundManager;
    [SerializeField] Transform rootBone;
    [SerializeField] private float desiredRootBoneRotation;

    public bool Running { get => fsm.GetCurrentState().ID == MovementStatus.Running;}


    public bool Walking { get => fsm.GetCurrentState().ID == MovementStatus.Walking; }
    public bool Fighting { get => fsm.GetCurrentState().ID == MovementStatus.Fighting; }
    public bool CanCancel { get => canCancel; set => canCancel = value; }
    public bool TryRun { get => tryRun; }
    public PlayerSoundManager SoundManager { get => soundManager; }

    public enum MovementStatus
    {
        Walking,
        Running,
        Fighting,
        Hurdle,
        Spin,
        Juke
    }
    internal void SetBiasOffset(float finalBias, float duration)
    {
        biasOffset = new BiasOffset(finalBias, duration, camera.m_Heading.m_Bias);
    }
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
                UpdateVfx();
            },
            delegate {
                    RotateRootBone(animator.GetFloat("SpeedX"));
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
                UpdateCamera(.1f, 2.2f, 1.9f);
            },
            delegate {
                //ApplyRootMotion(true);
                RotatePlayer();
                //ChangeVelocity(maxRunningSpeed, runningVelocity, runningReDirectionForce, runningDirBrakePow);
                SetCinemachineNoiseIntensity(body.velocity.magnitude);
                PlayAnimation(move.x, 1f);
                UpdateVfx();
            },
            delegate {
                RotateRootBone(animator.GetFloat("SpeedX"));
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
            },
            delegate {
                if(desiredRootBoneRotation != 0f)
                    RotateRootBone(0f);
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
                UpdateVfx();
            },
            delegate {
                //ApplyRootMotion(true);
                SetCinemachineNoiseIntensity(body.velocity.magnitude);
            },
            delegate {
                if (desiredRootBoneRotation != 0f)
                    RotateRootBone(0f);
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
                UpdateVfx();
            },
            delegate {
                //ApplyRootMotion(false);
                SetCinemachineNoiseIntensity(body.velocity.magnitude);
            },
            delegate {
                if (desiredRootBoneRotation != 0f)
                    RotateRootBone(0f);
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
                UpdateVfx();
            },
            delegate {
                //ApplyRootMotion(false);
                SetCinemachineNoiseIntensity(body.velocity.magnitude);
            },
            delegate {
                if (desiredRootBoneRotation != 0f)
                    RotateRootBone(0f);
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


    private void UpdateVfx()
    {
        var minSpeed = 4.4f;
        var maxSpeed = 8f;

        var value = Mathf.Max(0f, (-minSpeed + body.velocity.magnitude) / (-minSpeed + maxSpeed));

        if(Running)
        {
            vfx.SetFloat("SpawnRate", Mathf.MoveTowards(vfx.GetFloat("SpawnRate"), value * 110f, 90f * Time.deltaTime));
            vfx.SetVector2("YScaleRange", Vector2.MoveTowards(vfx.GetVector2("YScaleRange"), new Vector2(value * .3f + .05f, value * .6f + .1f), 0.65f * Time.deltaTime));
        }
        else
        {
            vfx.SetFloat("SpawnRate", Mathf.MoveTowards(vfx.GetFloat("SpawnRate"), 0f, 250f * Time.deltaTime));
            vfx.SetVector2("YScaleRange", Vector2.MoveTowards(vfx.GetVector2("YScaleRange"), new Vector2(.05f, .1f), 1.75f * Time.deltaTime));
        }

        //Debug.Log("Value = " + value);
        //vfx.SetVector2("YScaleRange", Vector2.MoveTowards(vfx.GetVector2("YScaleRange"), new Vector2(value * .3f + .1f, value * .6f + .2f), .5f * Time.deltaTime));
        //vfx.SetFloat("Radius", Mathf.Min(1.6f, ((-minSpeed + maxSpeed) / (-minSpeed + body.velocity.magnitude)) * 1.1f));
    }

    private void UpdateCamera(float horizontalDamping, float cameraDistance = 4.2f, float cameraHeight = 2.6f)
    {
        //var composer = camera.GetCinemachineComponent<CinemachineComposer>();
        //var thirdPersonFollow = camera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        //composer.m_HorizontalDamping = Mathf.MoveTowards(composer.m_HorizontalDamping, horizontalDamping, 50f * Time.deltaTime);
        //thirdPersonFollow.CameraDistance = Mathf.MoveTowards(thirdPersonFollow.CameraDistance, cameraDistance, 4f * Time.deltaTime);

        camera.m_Orbits[1].m_Height = Mathf.MoveTowards(camera.m_Orbits[1].m_Height, cameraHeight, 1.5f * Time.deltaTime);
        camera.m_Orbits[1].m_Radius = Mathf.MoveTowards(camera.m_Orbits[1].m_Radius, cameraDistance, 4f * Time.deltaTime);

        /*for (int i = 0; i < 3; i++)
        {
            var orbit = camera.m_Orbits[i];

            orbit.m_Height = cameraHeight;
            orbit.m_Radius = cameraDistance;
        }*/
    }

    private void SetCinemachineNoiseIntensity(float magnitude)
    {
        for(int i = 0; i < 3; i++)
        {
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

        if (biasOffset == null) return;
        if (biasOffset.startTime + biasOffset.duration >= Time.time)
        {
            camera.m_XAxis.Value += ((biasOffset.offset - biasOffset.startingBias) / biasOffset.duration) * Time.deltaTime;
            //camera.m_Heading.m_Bias =;
        }
        /*else if (biasOffset.startTime + biasOffset.duration + 0.1f >= Time.time)
        {
            camera.m_Heading.m_Bias = Mathf.Lerp(biasOffset.offset, 0f, (Time.time - biasOffset.startTime) / (biasOffset.duration + 0.1f));
        }*/
    }

    private void RotatePlayer()
    {
        Debug.Log("running speed = " + body.velocity.magnitude);
        float rotation = Running ? move.x * (rotationSpeed * rotationBySpeed.Evaluate(body.velocity.magnitude / maxRunningSpeed)) : move.x * rotationSpeed;
        body.transform.rotation *= Quaternion.Euler(0f, rotation, 0f);
    }

    /*private void ChangeVelocity(float maxSpeed, float force, float reDirectionForce, float dirBrakePow = 0f)
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
    }*/
    void PlayAnimation(float speedX, float speedY)
    {
        animator.SetFloat("Speed", Mathf.MoveTowards(animator.GetFloat("Speed"), speedY, .02f));
        animator.SetFloat("SpeedX", Mathf.MoveTowards(animator.GetFloat("SpeedX"), speedX, .08f));
    }
    private void LateUpdate()
    {
        fsm.LateUpdate();
    }

    private void RotateRootBone(float target)
    {
        desiredRootBoneRotation = Mathf.MoveTowards(desiredRootBoneRotation, target, .08f);
        rootBone.localEulerAngles = new Vector3(rootBone.localEulerAngles.x, desiredRootBoneRotation * 10f, rootBone.localEulerAngles.z);
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
    [System.Serializable]
    class BiasOffset
    {
        public float offset = 0f;
        public float duration = 0f;
        public float startTime = 0f;
        public float startingBias = 0f;

        public BiasOffset(float offset, float duration, float currentBias)
        {
            this.offset = offset;
            this.duration = duration;
            this.startTime = Time.time;
            this.startingBias = currentBias;
        }
    }
}
[System.Serializable]
public class PlayerSoundManager
{
    [SerializeField] private AudioClip[] walkClips;
    [SerializeField] private AudioClip[] runClips;

    [SerializeField] private AudioSource audioSource;


    internal void PlayFootstep()
    {
        audioSource.clip = walkClips[UnityEngine.Random.Range(0, walkClips.Length)];
        audioSource.Play();

        /*audioSource.Stop();
        audioSource.PlayOneShot(walkClips[UnityEngine.Random.Range(0, walkClips.Length)]);*/
    }
}