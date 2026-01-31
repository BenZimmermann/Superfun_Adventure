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
    public static bool EscIsUsed;
    public static bool ShootWasPressed;
    public static bool ShootWasHeld;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _runAction;
    private InputAction _pauseAction;
    private InputAction _shootAction;

    public InputController() { return; }
    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();

        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Jump"];
        _runAction = PlayerInput.actions["Run"];
        _pauseAction = PlayerInput.actions["Pause"];
        _shootAction = PlayerInput.actions["Shoot"];
    }
    public void JumpAction(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            Debug.Log("Recognizing jump");
            JumpWasPressed = true;
            JumpWasReleased = false;
        }
        else if (context.canceled)
        {
            JumpWasPressed = false;
            JumpWasReleased = true;
        }
    }
    private void Update()
    {
        Movement = _moveAction.ReadValue<Vector2>();

        JumpWasPressed = _jumpAction.WasPressedThisFrame();
        JumpWasHeld = _jumpAction.IsPressed();
        JumpWasReleased = _jumpAction.WasReleasedThisFrame();

        RunIsHeld = _runAction.IsPressed();
        EscIsUsed = _pauseAction.WasPressedThisFrame();
        ShootWasPressed = _shootAction.WasPressedThisFrame();
        ShootWasHeld = _shootAction.IsPressed();
    }
}
