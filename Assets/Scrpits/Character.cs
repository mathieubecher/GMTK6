using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private float m_jumpBuffer = 0.2f;
    [SerializeField] private float m_jelbowDropBuffer = 0.2f;
    
    [Header("System")]
    [SerializeField] private DetectGround m_detectGround;
    [SerializeField] public Animator animation;
    private Rigidbody2D m_rigidbody;
    private Animator m_locomotion;
    
    #region getter and setter
    public bool onGround {get => m_detectGround.OnGround(); }
    public void IgnoreKoyoteeTime() { m_detectGround.IgnoreKoyoteeTime(); }

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
    }

    void OnDisable()
    {
        Controller.OnJumpPress -= JumpPress;
        Controller.OnElbowDropPress -= ElbowDropPress;
        
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
