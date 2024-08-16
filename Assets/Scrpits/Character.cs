using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private float m_jumpBuffer = 0.2f;
    [SerializeField] private float m_jelbowDropBuffer = 0.2f;
    
    [Header("System")]
    [SerializeField] private DetectGround m_detectGround;
    private Animator m_locomotion;
    
    public bool OnGround() {return m_detectGround.OnGround(); }

    void Awake()
    {
        m_locomotion = GetComponent<Animator>();
    }
    void OnEnable()
    {
        Controller.OnJumpPress += JumpPress;
        Controller.OnElbowDropPress += ElbowDropPress;
    }

    void OnDisable()
    {
        Controller.OnJumpPress -= JumpPress;
        Controller.OnElbowDropPress -= ElbowDropPress;
        
    }

    void FixedUpdate()
    {
        m_locomotion.SetFloat("tilt", Controller.instance.tilt);
        m_locomotion.SetBool("inAir", !OnGround());

    }

    private void JumpPress()
    {
        Debug.Log("Jump");
        StartCoroutine(TryPlayAction("Jump", m_jumpBuffer));

    }

    private void ElbowDropPress()
    {
        Debug.Log("ElbowDrop");
        StartCoroutine(TryPlayAction("ElbowDrop", m_jelbowDropBuffer));
    }
    
    private IEnumerator TryPlayAction(string _name, float _buffer)
    {
        m_locomotion.SetTrigger(_name);
        yield return new WaitForSeconds(_buffer);
        m_locomotion.ResetTrigger(_name);
    }

}
