using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public partial class PlayerController : MonoBehaviour
{
    [Header("Running")]
    [SerializeField] float maxRunningSpeed = 15f;
    [Header("Rotation")]
    [SerializeField] float rotationSpeed = 8f;
    [SerializeField] AnimationCurve rotationBySpeed;
    [SerializeField] AnimationCurve accelerationCurve;
    [Header("Camera & VFX")]
    [SerializeField] PlayerCameraController camera;
    [SerializeField] VisualEffect vfx;

    // Inputs
    Vector2 move = new Vector2();
    private bool canCancel;
    private bool tryRun;
    [Header("Component")]
    [SerializeField] private FiniteStateMachine<MovementStatus> fsm;
    Rigidbody body;
    PlayerAnimatorController animator;


    [SerializeField] PlayerSoundManager soundManager;
    [Header("Bones manual update")]
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
    public PlayerAnimatorController Animator { get => animator; set => animator = value; }
    public float MaxRunningSpeed { get => maxRunningSpeed; set => maxRunningSpeed = value; }

    public enum MovementStatus
    {
        Walking,
        Running,
        Fighting,
        Hurdle,
        Spin,
        Juke
    }
    void Awake()
    {
        body = GetComponentInChildren<Rigidbody>();
        camera = GetComponentInChildren<PlayerCameraController>();
        animator = new PlayerAnimatorController(GetComponentInChildren<Animator>());

    }
    private void Start()
    {

        fsm = FsmSetter.SetUpControllerFsm(this, body, animator, camera);
    }

    #region FSM : state & updates
    internal void SetState(MovementStatus state) { fsm.SetCurrentState(fsm.GetState(state)); }
    internal MovementStatus GetState() { return fsm.GetCurrentState().ID; }
    void FixedUpdate() { fsm.FixedUpdate(); }
    void Update() { fsm.Update(); }
    private void LateUpdate() { fsm.LateUpdate(); }
    #endregion

    public void UpdateVfx()
    {
        var minSpeed = 4.4f;
        var maxSpeed = 8f;

        var value = Mathf.Max(0f, (-minSpeed + body.velocity.magnitude) / (-minSpeed + maxSpeed));

        if (Running)
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

    public void RotatePlayer()
    {
        Debug.Log("running speed = " + body.velocity.magnitude);
        float rotation = Running ? move.x * (rotationSpeed * rotationBySpeed.Evaluate(body.velocity.magnitude / maxRunningSpeed)) : move.x * rotationSpeed;
        body.transform.rotation *= Quaternion.Euler(0f, rotation, 0f);
    }

    public void RotateRootBone(float target)
    {
        desiredRootBoneRotation = Mathf.MoveTowards(desiredRootBoneRotation, target, .08f);
        rootBone.localEulerAngles = new Vector3(rootBone.localEulerAngles.x, desiredRootBoneRotation * 10f, rootBone.localEulerAngles.z);
    }

}
