using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameFlow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_pedroDialog;
    [SerializeField] private TextMeshProUGUI m_diegoDialog;
    
    private Animator m_animator;
    private bool m_canCancelDialog;
        
    public delegate void SimpleCallback();
    private SimpleCallback m_dialogCallback;	

    void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        Controller.OnContinuePress += CancelDialog;
    }

    private void OnDisable()
    {
        Controller.OnContinuePress -= CancelDialog;
    }

    private void CancelDialog()
    {
        if(m_canCancelDialog)
            m_animator.SetTrigger("CancelDialog");
    }

    public void CanCancelDialog()
    {
        m_canCancelDialog = true;
    }
    
    public void ContinueCinematic()
    {
        if (m_dialogCallback != null)
        {
            m_dialogCallback();
            m_dialogCallback = null;
        }
    }
    
    public void Reset()
    {
        m_animator.SetTrigger("Defeat");
    }

    public void DrawDialogPedro(String _text, SimpleCallback _callback)
    {
        m_pedroDialog.text = _text;
        m_animator.ResetTrigger("CancelDialog");
        m_animator.SetTrigger("Pedro");
        m_canCancelDialog = false;
        
        m_dialogCallback = _callback;
    }
    public void DrawDialogDiego(String _text, SimpleCallback _callback)
    {
        m_diegoDialog.text = _text;
        m_animator.ResetTrigger("CancelDialog");
        m_animator.SetTrigger("Diego");
        m_canCancelDialog = false;
        
        m_dialogCallback = _callback;
    }

    public void SetCameraRes(string _res)
    {
        m_animator.SetBool("cinema", _res == "Cinema");
    }
}
