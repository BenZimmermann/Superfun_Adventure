using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class InputController : MonoBehaviour
{
    public static PlayerInput PlayerInput;

    public static Vector2 Movement;
    public static bool JumpWasPressed;
    public static bool JumpWasHeld;
    public static bool JumpWasReleased;
    public static bool RunIsHeld;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _runAction;

    public InputController() { return; }
    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();

        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Jump"];
        _runAction = PlayerInput.actions["Run"];
    }

    private void Update()
    {
        Movement = _moveAction.ReadValue<Vector2>();

        JumpWasPressed = _jumpAction.WasPressedThisFrame();
        JumpWasHeld = _jumpAction.IsPressed();
        JumpWasReleased = _jumpAction.WasReleasedThisFrame();

        RunIsHeld = _runAction.IsPressed();
    }
}
