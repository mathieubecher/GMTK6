using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameFlow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_pedroDialog;
    [SerializeField] private TextMeshProUGUI m_diegoDialog;
    [SerializeField] private Image m_ctrlsKeyboard;
    [SerializeField] private Image m_ctrlsGamepad;
    [SerializeField] private Sprite m_spriteKeyboardPiment;
    [SerializeField] private Sprite m_spriteGamepadPiment;
    [SerializeField] private GameObject m_character;

    private Animator m_animator;
    private bool m_canCancelDialog;
        
    public delegate void SimpleCallback();
    private SimpleCallback m_dialogCallback;

    private static GameManager gameManager;

    void Awake()
    {
        m_animator = GetComponent<Animator>();
        gameManager = FindObjectOfType<GameManager>();
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

    public void PauseGame()
    {
        Time.timeScale = 0;
        if (m_character.GetComponent<Character>().m_hasPiment == true)
        {
            m_ctrlsKeyboard.sprite = m_spriteKeyboardPiment;
            m_ctrlsGamepad.sprite = m_spriteGamepadPiment;
        }
        m_animator.ResetTrigger("CancelDialog");
        m_canCancelDialog = false;
        m_animator.ResetTrigger("Controls");
        gameManager.GetComponent<GameManager>().TakeControl();

    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        m_animator.ResetTrigger("Controls");
        gameManager.GetComponent<GameManager>().GiveControl();

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

    public void SendTrigger(String _trigger, SimpleCallback _callback)
    {
        m_animator.ResetTrigger("CancelDialog");
        m_animator.SetTrigger(_trigger);
        m_canCancelDialog = true;
        m_dialogCallback = _callback;
    }

    public void SetCameraRes(string _res)
    {
        m_animator.SetBool("cinema", _res == "Cinema");
    }
}
