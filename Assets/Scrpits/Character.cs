using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private float m_jumpBuffer = 0.2f;
    [SerializeField] private float m_kickBuffer = 0.2f;
    [SerializeField] private float m_jelbowDropBuffer = 0.2f;
    
    [Header("System")]
    [SerializeField] private DetectGround m_detectGround;
    [SerializeField] public Animator animation;
    private Rigidbody2D m_rigidbody;
    private Animator m_locomotion;
    
    #region getter and setter
    public bool onGround => m_detectGround.OnGround();
    public float elbowDropHeight => m_locomotion.GetFloat("elbowDropHeight");

    public float gravityScale
    {
        set => m_rigidbody.gravityScale = value * 5.0f;
    }
    
    public Vector2 velocity
    {
        get => m_rigidbody.velocity;
        set => m_rigidbody.velocity = value;
    }
    #endregion

    void Awake()
    {
        m_locomotion = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();
    }
    void OnEnable()
    {
        Controller.OnJumpPress += JumpPress;
        Controller.OnElbowDropPress += ElbowDropPress;
        Controller.OnKickPress += KickPress;
    }

    void OnDisable()
    {
        Controller.OnJumpPress -= JumpPress;
        Controller.OnElbowDropPress -= ElbowDropPress;
        Controller.OnKickPress -= KickPress;
        
    }

    void Update()
    {
        if(math.abs(velocity.x) > 0.01f)
            animation.transform.localScale = new Vector3(math.sign(velocity.x), 1.0f, 1.0f);
    }
    void FixedUpdate()
    {
        m_locomotion.SetFloat("tilt", Controller.instance.tilt);
        m_locomotion.SetBool("inAir", !onGround);

    }

    private void JumpPress()
    {
        StartCoroutine(TryPlayAction("Jump", m_jumpBuffer));

    }

    private void ElbowDropPress()
    {
        StartCoroutine(TryPlayAction("ElbowDrop", m_jelbowDropBuffer));
    }
    
    private void KickPress()
    {
        StartCoroutine(TryPlayAction("Kick", m_kickBuffer));
    }
    
    private IEnumerator TryPlayAction(string _name, float _buffer)
    {
        m_locomotion.SetTrigger(_name);
        yield return new WaitForSeconds(_buffer);
        m_locomotion.ResetTrigger(_name);
    }

    private void OnCollisionEnter2D(Collision2D _collision)
    {
        foreach (var contact in _collision.contacts)
        {
            Debug.Log(contact.normal);
            if (Vector2.Dot(contact.normal, Vector2.up) < 0.0f)
            {
                Debug.Log("Ceil");
                StartCoroutine(TryPlayAction("CeilBump", 0.033f));
                break;
            }
        }
        //col.contacts[0].normal
    }

    public void Reset(Vector2 _checkpoint)
    {
        transform.position = _checkpoint;
        m_locomotion.Play("Ground");
        
        m_locomotion.ResetTrigger("Jump");
        m_locomotion.ResetTrigger("ElbowDrop");
        m_locomotion.ResetTrigger("Kick");
        m_locomotion.ResetTrigger("CeilBump");
        m_locomotion.SetBool("inAir", false);
        m_locomotion.SetBool("dead", false);
        m_locomotion.SetFloat("ElbowDropHeight", 0.0f);
        
        animation.Play("Idle");
    }
}
