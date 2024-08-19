using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    #region Singleton
    private static Controller m_instance;
    public static Controller instance
    {
        get
        {
            if (!m_instance)
            {
                m_instance = FindObjectOfType<Controller>();
            }
            return m_instance;
        }
    }
    #endregion

    [SerializeField] private AnimationCurve m_deadzone;
    [HideInInspector] public float tilt;

    public delegate void SimpleEvent();
    public static event SimpleEvent OnJumpPress;
    public static event SimpleEvent OnJumpRelease;
    
    public static event SimpleEvent OnElbowDropPress;
    public static event SimpleEvent OnElbowDropRelease;
    
    public static event SimpleEvent OnKickPress;
    public static event SimpleEvent OnKickRelease;
    
    public static event SimpleEvent OnPimentPress;
    public static event SimpleEvent OnPimentRelease;
    
    public static event SimpleEvent OnPausePress;
    public static event SimpleEvent OnContinuePress;
    
    public static event SimpleEvent OnReset;
    
    public void ReadMoveInput(InputAction.CallbackContext _context)
    {
        float input = _context.ReadValue<float>();
        tilt = m_deadzone.Evaluate(math.abs(input)) * math.sign(input); 
    }
    
    public void ReadJumpInput(InputAction.CallbackContext _context)
    {
        if (_context.performed)
        {
            OnJumpPress?.Invoke();
        }
        else if (_context.canceled)
        {
            OnJumpRelease?.Invoke();
        }
    }
    
    public void ReadElbowDropInput(InputAction.CallbackContext _context)
    {
        if (_context.performed)
        {
            OnElbowDropPress?.Invoke(); 
        }
        else if (_context.canceled)
        {
            OnElbowDropRelease?.Invoke();
        }
    }
    
    public void ReadKickInput(InputAction.CallbackContext _context)
    {
        if (_context.performed)
        {
            OnKickPress?.Invoke(); 
        }
        else if (_context.canceled)
        {
            OnKickRelease?.Invoke();
        }
    }
    
    public void ReadPimentInput(InputAction.CallbackContext _context)
    {
        if (_context.performed)
        {
            OnPimentPress?.Invoke(); 
        }
        else if (_context.canceled)
        {
            OnPimentRelease?.Invoke();
        }
    }

    public void ReadResetInput(InputAction.CallbackContext _context)
    {
        if (_context.performed)
        {
            OnReset?.Invoke();
        }
    }
    
    public void ReadContinueInput(InputAction.CallbackContext _context)
    {
        if (_context.performed)
        {
            OnContinuePress?.Invoke();
        }
    }
    
    public void ReadPauseInput(InputAction.CallbackContext _context)
    {
        if (_context.performed)
        {
            OnPausePress?.Invoke();
        }
    }
    
}
