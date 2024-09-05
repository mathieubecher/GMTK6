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

    [Header("Piment")]
    [SerializeField] private Cinematic m_gotPimentTuto;
    [SerializeField] private bool m_hasPiment;

    [SerializeField] public GameObject ui_Piment;


    private Rigidbody2D m_rigidbody;
    private Animator m_locomotion;
    private Bot m_bot;
    private Vector3 m_originalScale;
    private bool m_hasControl;
    
    #region getter and setter
    public bool isOnGround => m_detectGround.isOnGround;
    public float elbowDropHeight => m_locomotion.GetFloat("elbowDropHeight");

    public Bot bot => m_bot;
    public Animator locomotion => m_locomotion;

    public float gravityScale
    {
        set => m_rigidbody.gravityScale = value;
    }
    
    public Vector2 velocity
    {
        get => m_rigidbody.velocity;
        set => m_rigidbody.velocity = value;
    }
    public Rigidbody2D rigidbody => m_rigidbody;

    public bool hasControl
    {
        get => m_hasControl;
        set => m_hasControl = value;
    }
    
    #endregion

    void Awake()
    {
        m_originalScale = transform.lossyScale;
        m_locomotion = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_bot = GetComponent<Bot>();
        m_detectGround.SetCharacter(this);
    }
    void OnEnable()
    {
        Controller.OnJumpPress += JumpPress;
        Controller.OnJumpRelease += JumpRelease;
        Controller.OnElbowDropPress += ElbowDropPress;
        Controller.OnElbowDropRelease += ElbowDropRelease;
        Controller.OnPimentPress += PimentPress;
        Controller.OnPimentRelease += PimentRelease;
        Controller.OnKickPress += KickPress;
    }

    void OnDisable()
    {
        Controller.OnJumpPress -= JumpPress;
        Controller.OnJumpRelease -= JumpRelease;
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
        if(m_hasControl) m_locomotion.SetFloat("tilt", Controller.instance.tilt);
        m_locomotion.SetBool("inAir", !isOnGround);
    }

    private void JumpPress()
    {
        StartCoroutine(TryPlayAction("Jump", m_jumpBuffer));
        if(m_hasControl) m_locomotion.SetBool("holdJump", true);
    }

    private void JumpRelease()
    {
        if(m_hasControl) m_locomotion.SetBool("holdJump", false);
    }

    private void ElbowDropPress()
    {
        if(!m_detectGround.isOnGround)
            StartCoroutine(TryPlayAction("ElbowDrop", m_jelbowDropBuffer));

        if(m_hasControl) RequestCharacterDown();
    }

    private void ElbowDropRelease()
    {
        if(m_hasControl) FinishCharacterDown();
    }


    private void PimentPress()
    {
        if(m_hasControl) m_locomotion.SetBool("piment", m_hasPiment);
    }
    
    private void PimentRelease()
    {
        if(m_hasControl) m_locomotion.SetBool("piment", false);
    }
    
    private void KickPress()
    {
        StartCoroutine(TryPlayAction("Kick", m_kickBuffer));
    }
    
    private IEnumerator TryPlayAction(string _name, float _buffer)
    {
        if(m_hasControl) m_locomotion.SetTrigger(_name);
        yield return new WaitForSeconds(_buffer);
        if(m_hasControl) m_locomotion.ResetTrigger(_name);
    }

    private void OnTriggerEnter2D(Collider2D _collider)
    {
        if (GameManager.IsPiment(_collider.gameObject.layer))
        {
            Destroy(_collider.gameObject);
            m_gotPimentTuto.Play();
            m_hasPiment = true;
            ui_Piment.SetActive(true);
        }
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

    public void SetParent(Transform _parent)
    {
        if(_parent.gameObject.activeInHierarchy)
            transform.SetParent(_parent);
    }

    public void ResetParent()
    {
        if(transform.parent != null && transform.parent.gameObject.activeInHierarchy) 
            transform.SetParent(null);
    }

    private int m_characterDownRequest;
    public void RequestCharacterDown()
    {
        ++m_characterDownRequest;
        
        gameObject.layer = LayerMask.NameToLayer("CharacterDown");
        m_detectGround.gameObject.layer = LayerMask.NameToLayer("CharacterDown");
    }
    public void FinishCharacterDown()
    {
        --m_characterDownRequest;
        if (m_characterDownRequest > 0) return;
        
        gameObject.layer = LayerMask.NameToLayer("Character");
        m_detectGround.gameObject.layer = LayerMask.NameToLayer("Character");
    }
    public void ForceFinishCharacterDown()
    {
        m_characterDownRequest = 0;
        gameObject.layer = LayerMask.NameToLayer("Character");
        m_detectGround.gameObject.layer = LayerMask.NameToLayer("Character");
    }
}
