using System.Collections;
using System.Collections.Generic;
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

    [HideInInspector] public float tilt;

    public delegate void SimpleEvent();
    public static event SimpleEvent OnJumpPress;
    public static event SimpleEvent OnJumpRelease;
    
    public static event SimpleEvent OnElbowDropPress;
    public static event SimpleEvent OnElbowDropRelease;
    
    public static event SimpleEvent OnKickPress;
    public static event SimpleEvent OnKickRelease;
    
    public void ReadMoveInput(InputAction.CallbackContext _context)
    {
        float input = _context.ReadValue<float>();
        tilt = input; 
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
    
    
}
