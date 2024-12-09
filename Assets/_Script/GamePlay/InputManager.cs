using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static Vector2 Movement;
    public static Vector2 Face;
    public static Vector2 Aim;
    public static float shooting;

    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _faceAction;
    private InputAction _aimAction;
    private InputAction _shooting;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();

        _moveAction = _playerInput.actions["Move"];
        _faceAction = _playerInput.actions["Facing"];
        _shooting = _playerInput.actions["Shoot"];
        _aimAction = _playerInput.actions["MouseAim"];
    }

    private void Update()
    {
        Movement = _moveAction.ReadValue<Vector2>();
        Face = _faceAction.ReadValue<Vector2>();
        Aim = _aimAction.ReadValue<Vector2>();
        shooting = _shooting.ReadValue<float>();

    }
}
