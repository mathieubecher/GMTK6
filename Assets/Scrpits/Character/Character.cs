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
    private Vector3 m_originalScale;
    
    #region getter and setter
    public bool isOnGround => m_detectGround.isOnGround;
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
        m_originalScale = transform.lossyScale;
        m_locomotion = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_detectGround.SetCharacter(this);
    }
    void OnEnable()
    {
        Controller.OnJumpPress += JumpPress;
        Controller.OnElbowDropPress += ElbowDropPress;
        Controller.OnElbowDropRelease += ElbowDropRelease;
        Controller.OnPimentPress += PimentPress;
        Controller.OnPimentRelease += PimentRelease;
        Controller.OnKickPress += KickPress;
    }

    void OnDisable()
    {
        Controller.OnJumpPress -= JumpPress;
        Controller.OnElbowDropPress -= ElbowDropPress;
        Controller.OnElbowDropRelease += ElbowDropRelease;
        Controller.OnPimentPress -= PimentPress;
        Controller.OnPimentRelease += PimentRelease;
        Controller.OnKickPress -= KickPress;
        
    }

    void Update()
    {
        Transform parent = transform.parent;
        if(transform.parent) 
            transform.localScale = new Vector3(m_originalScale.x/parent.lossyScale.x, m_originalScale.y/parent.lossyScale.y, 1.0f);
        
        if(math.abs(velocity.x) > 0.01f)
            animation.transform.localScale = new Vector3(math.sign(velocity.x), 1.0f, 1.0f);
    }
    void FixedUpdate()
    {
        m_locomotion.SetFloat("tilt", Controller.instance.tilt);
        m_locomotion.SetBool("inAir", !isOnGround);
    }

    private void JumpPress()
    {
        StartCoroutine(TryPlayAction("Jump", m_jumpBuffer));

    }

    private void ElbowDropPress()
    {
        if(!m_detectGround.isOnGround)
            StartCoroutine(TryPlayAction("ElbowDrop", m_jelbowDropBuffer));
        gameObject.layer = LayerMask.NameToLayer("CharacterDown");
        m_detectGround.gameObject.layer = LayerMask.NameToLayer("CharacterDown");
    }
    
    private void ElbowDropRelease()
    {
        gameObject.layer = LayerMask.NameToLayer("Character");
        m_detectGround.gameObject.layer = LayerMask.NameToLayer("Character");
    }

    private void PimentPress()
    {
        m_locomotion.SetBool("piment", true);
    }
    
    private void PimentRelease()
    {
        m_locomotion.SetBool("piment", false);
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
            if(GameManager.IsCactus(contact.collider.gameObject.layer))
            {
                m_locomotion.SetBool("dead", true);
                GameManager.instance.Reset();
            }
            if (!contact.collider.usedByEffector && Vector2.Dot(contact.normal, Vector2.up) < 0.0f)
            {
                StartCoroutine(TryPlayAction("CeilBump", 0.033f));
                break;
            }
        }
    }

    public void Reset(Vector2 _checkpoint)
    {
        transform.position = _checkpoint;
        m_locomotion.Play("Ground");
        
        m_locomotion.ResetTrigger("Jump");
        m_locomotion.ResetTrigger("ElbowDrop");
        m_locomotion.ResetTrigger("Kick");
        m_locomotion.ResetTrigger("CeilBump");
        m_locomotion.SetBool("piment", false);
        m_locomotion.SetBool("inAir", false);
        m_locomotion.SetBool("dead", false);
        m_locomotion.SetFloat("elbowDropHeight", 0.0f);
        
        animation.Play("Idle");
    }
}
