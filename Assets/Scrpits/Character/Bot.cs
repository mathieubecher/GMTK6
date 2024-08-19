using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Bot : MonoBehaviour
{
    [SerializeField] private float m_tolerance = 0.5f;
    
    private Character m_character;
    private bool m_hasObjective;
    private Vector2 m_objective;

    public delegate void SimpleCallback();
    private SimpleCallback m_reachFunction;		
    private SimpleCallback m_groundFunction;		
    void Awake()
    {
        m_character = GetComponent<Character>();
    }
    // Update is called once per frame
    void Update()
    {
        if (m_character.hasControl) return;
        
        if (m_hasObjective)
        {
            if (Vector2.Distance(m_objective,  transform.position) < m_tolerance)
            {
                m_hasObjective = false;
                m_character.locomotion.SetFloat("tilt", 0.0f);
                m_reachFunction?.Invoke();
                m_reachFunction = null;
            }
            else
            {
                m_character.locomotion.SetFloat("tilt", math.sign(Vector2.Dot(Vector2.right, m_objective - (Vector2)transform.position)));
            }
        }
        if (m_groundFunction != null && !m_character.locomotion.GetBool("inAir"))
        {
            m_groundFunction();
            m_groundFunction = null;
        }
    }

    public void Reach(Vector2 _pos, SimpleCallback _func)
    {
        m_reachFunction = _func;
        m_hasObjective = true;
        m_objective = _pos;
    }

    public void WaitIsOnGround(SimpleCallback _func)
    {
        m_groundFunction = _func;

    }
}
