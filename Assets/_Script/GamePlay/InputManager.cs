using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static Vector2 Movement;
    public static Vector2 Face;
    public static float shooting;

    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _faceAction;
    private InputAction _shooting;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();

        _moveAction = _playerInput.actions["Move"];
        _faceAction = _playerInput.actions["Facing"];
        _shooting = _playerInput.actions["Shoot"];
    }

    private void Update()
    {
        Movement = _moveAction.ReadValue<Vector2>();
        Face = _faceAction.ReadValue<Vector2>();
        Debug.Log(_shooting.ReadValue<float>());
        shooting = _shooting.ReadValue<float>();
    }
}
