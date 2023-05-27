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
    public bool TryRun { get => tryRun; set => tryRun = value; }
    public PlayerSoundManager SoundManager { get => soundManager; }
    public Vector2 Move { get => move; set => move = value; }
    public float DesiredRootBoneRotation { get => desiredRootBoneRotation; set => desiredRootBoneRotation = value; }
    public FiniteStateMachine<MovementStatus> Fsm { get => fsm; set => fsm = value; }
    public Animator Animator { get => animator; set => animator = value; }

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
    void Awake()
    {
        body = GetComponentInChildren<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        fsm = FsmSetter.SetUpControllerFsm(this, body, animator);
    }


    public void UpdateVfx()
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

    public void UpdateCamera(float horizontalDamping, float cameraDistance = 4.2f, float cameraHeight = 2.6f)
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

    public void SetCinemachineNoiseIntensity(float magnitude)
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

    public void SetAnimationLayer(MovementStatus state)
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

    public void RotatePlayer()
    {
        Debug.Log("running speed = " + body.velocity.magnitude);
        float rotation = Running ? move.x * (rotationSpeed * rotationBySpeed.Evaluate(body.velocity.magnitude / maxRunningSpeed)) : move.x * rotationSpeed;
        body.transform.rotation *= Quaternion.Euler(0f, rotation, 0f);
    }
    public void PlayAnimation(float speedX, float speedY)
    {
        animator.SetFloat("Speed", Mathf.MoveTowards(animator.GetFloat("Speed"), speedY, .02f));
        animator.SetFloat("SpeedX", Mathf.MoveTowards(animator.GetFloat("SpeedX"), speedX, .08f));
    }
    private void LateUpdate()
    {
        fsm.LateUpdate();
    }

    public void RotateRootBone(float target)
    {
        desiredRootBoneRotation = Mathf.MoveTowards(desiredRootBoneRotation, target, .08f);
        rootBone.localEulerAngles = new Vector3(rootBone.localEulerAngles.x, desiredRootBoneRotation * 10f, rootBone.localEulerAngles.z);
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
