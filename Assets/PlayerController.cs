using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Vector2 move = new Vector2();
    Rigidbody body;
    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (move != Vector2.zero)
            body.velocity += new Vector3(move.x, 0f, move.y);
        else if (body.velocity.magnitude > .2f)
            body.velocity /= 1.1f;
        else
            body.velocity = Vector3.zero;
    }

    public void SetMove(InputAction.CallbackContext context)
    {
        this.move = context.ReadValue<Vector2>();
    }
}
