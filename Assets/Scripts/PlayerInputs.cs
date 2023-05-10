using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerInputs : MonoBehaviour
{
    private PlayerController motor;


    public void Start()
    {
        /*Debug.Log("Network Informations : IsOwner " + IsOwner);
        if (!IsOwner) return;*/

        motor = GetComponent<PlayerController>();

        InputsManager.Controls.InGameActions.Move.performed += ctx => OnMove(ctx.ReadValue<Vector2>());
        InputsManager.Controls.InGameActions.Move.canceled += _ => OnMove(Vector2.zero);

        InputsManager.Controls.InGameActions.MoveStick.performed += ctx => OnMove(ctx.ReadValue<Vector2>());
        InputsManager.Controls.InGameActions.MoveStick.canceled += _ => OnMove(Vector2.zero);

        InputsManager.Controls.InGameActions.Fight.performed += _ => OnFightStance(true);
        InputsManager.Controls.InGameActions.Move.canceled += _ => OnFightStance(false);

        InputsManager.Controls.InGameActions.Run.performed += _ => OnSprint(true);
        InputsManager.Controls.InGameActions.Run.canceled += _ => OnSprint(false);

        InputsManager.Controls.InGameActions.A.performed += _ => OnA(true);
        InputsManager.Controls.InGameActions.A.canceled += _ => OnA(false);
    }

    private void OnFightStance(bool v)
    {
        motor.SetFight(v);
    }

    public void OnMove(Vector2 context)
    {
        motor.SetMove(context);
    }
    public void OnSprint(bool context)
    {
        motor.SetRun(context);
    }
    public void OnA(bool v)
    {
        motor.SetAMove(v);
    }
}