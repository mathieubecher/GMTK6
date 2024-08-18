using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Breakable : Interactive
{
    [SerializeField] private float m_lateralStrenght = 3.0f;
    [SerializeField] private float m_verticalStrenght = 3.0f;
    
    private Animator m_animator;
    private Collider2D m_collider;

    void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_collider = GetComponent<Collider2D>();
    }
    public bool TryToBreak(Vector2 _dir, float _force)
    {
        if (CanBreak(_dir, _force))
        {
            Break();
            return true;
        }

        return false;
    }

    private bool CanBreak(Vector2 _dir, float _force)
    {
        
        if (math.abs(Vector2.Dot(_dir, Vector2.up)) > 0.0f)
        {
            return _force >= m_verticalStrenght;
        }
        else return _force >= m_lateralStrenght;
    }


    public override void Reset()
    {
        m_animator.ResetTrigger("Break");
        m_animator.Play("Idle");
        m_collider.enabled = true;
    }
    
    private void Break()
    {
        m_animator.SetTrigger("Break");
        m_collider.enabled = false;
    }
}
