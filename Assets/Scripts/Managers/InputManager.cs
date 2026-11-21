using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Quản lý tín hiệu đầu vào từ Unity Input System và phát sự kiện cho các Controller (SRP).
/// </summary>
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
    public InputActionReference activateSkillAction;

    public event Action<Vector2> OnMove;
    public event Action<Vector2> OnMoveCanceled;
    public event Action OnRotate;
    public event Action OnHardDrop;
    public event Action OnHold;
    public event Action OnSoftDropStart;
    public event Action OnSoftDropCancel;
    public event Action OnPause;
    public event Action OnActivateSkill;

    private InputAction _resolvedActivateSkillAction;

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
            moveAction.action.canceled += MoveCanceled;
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

        _resolvedActivateSkillAction = ResolveActivateSkillAction();
        if (_resolvedActivateSkillAction != null)
        {
            _resolvedActivateSkillAction.performed += ActivateSkillPerformed;
            _resolvedActivateSkillAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.action.performed -= MovePerformed;
            moveAction.action.canceled -= MoveCanceled;
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
        if (_resolvedActivateSkillAction != null)
        {
            _resolvedActivateSkillAction.performed -= ActivateSkillPerformed;
            _resolvedActivateSkillAction.Disable();
            _resolvedActivateSkillAction = null;
        }
    }

    private InputAction ResolveActivateSkillAction()
    {
        if (activateSkillAction != null && activateSkillAction.action != null)
        {
            return activateSkillAction.action;
        }

        // Tự động tìm action "Input/ActivateSkill" từ cùng InputActionAsset nếu chưa kéo trong Inspector
        if (moveAction != null && moveAction.asset != null)
        {
            return moveAction.asset.FindAction("Input/ActivateSkill", throwIfNotFound: false);
        }

        return null;
    }

    private void MovePerformed(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
        OnMove?.Invoke(value);
    }

    private void MoveCanceled(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
        OnMoveCanceled?.Invoke(value);
    }

    private void RotatePerformed(InputAction.CallbackContext context)
    {
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

    private void ActivateSkillPerformed(InputAction.CallbackContext context)
    {
        OnActivateSkill?.Invoke();
    }
}
