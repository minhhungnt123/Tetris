using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("Input Action References")]
    public InputActionReference moveAction;
    public InputActionReference rotateAction;
    public InputActionReference hardDropAction;
    public InputActionReference holdAction;
    public InputActionReference softDropAction;
    public InputActionReference pauseAction;

    public event Action<Vector2> OnMove;
    public event Action OnRotate;
    public event Action OnHardDrop;
    public event Action OnHold;
    public event Action OnSoftDropStart;
    public event Action OnSoftDropCancel;
    public event Action OnPause;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.action.performed += MovePerformed;
            moveAction.action.Enable();
        }
        if (rotateAction != null)
        {
            rotateAction.action.performed += RotatePerformed;
            rotateAction.action.Enable();
        }
        if (hardDropAction != null)
        {
            hardDropAction.action.performed += HardDropPerformed;
            hardDropAction.action.Enable();
        }
        if (holdAction != null)
        {
            holdAction.action.performed += HoldPerformed;
            holdAction.action.Enable();
        }
        if (softDropAction != null)
        {
            softDropAction.action.performed += SoftDropPerformed;
            softDropAction.action.canceled += SoftDropCanceled;
            softDropAction.action.Enable();
        }
        if (pauseAction != null)
        {
            pauseAction.action.performed += PausePerformed;
            pauseAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.action.performed -= MovePerformed;
            moveAction.action.Disable();
        }
        if (rotateAction != null)
        {
            rotateAction.action.performed -= RotatePerformed;
            rotateAction.action.Disable();
        }
        if (hardDropAction != null)
        {
            hardDropAction.action.performed -= HardDropPerformed;
            hardDropAction.action.Disable();
        }
        if (holdAction != null)
        {
            holdAction.action.performed -= HoldPerformed;
            holdAction.action.Disable();
        }
        if (softDropAction != null)
        {
            softDropAction.action.performed -= SoftDropPerformed;
            softDropAction.action.canceled -= SoftDropCanceled;
            softDropAction.action.Disable();
        }
        if (pauseAction != null)
        {
            pauseAction.action.performed -= PausePerformed;
            pauseAction.action.Disable();
        }
    }

    private void MovePerformed(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
        Debug.Log("InputManager: MovePerformed -> " + value);
        OnMove?.Invoke(value);
    }

    private void RotatePerformed(InputAction.CallbackContext context)
    {
        Debug.Log("InputManager: RotatePerformed");
        OnRotate?.Invoke();
    }

    private void HardDropPerformed(InputAction.CallbackContext context)
    {
        OnHardDrop?.Invoke();
    }

    private void HoldPerformed(InputAction.CallbackContext context)
    {
        OnHold?.Invoke();
    }

    private void SoftDropPerformed(InputAction.CallbackContext context)
    {
        OnSoftDropStart?.Invoke();
    }

    private void SoftDropCanceled(InputAction.CallbackContext context)
    {
        OnSoftDropCancel?.Invoke();
    }

    private void PausePerformed(InputAction.CallbackContext context)
    {
        OnPause?.Invoke();
    }
}
