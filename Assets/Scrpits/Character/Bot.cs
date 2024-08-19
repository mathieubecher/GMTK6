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

    public delegate void ReachCallback();
    private ReachCallback m_reachFunction;		
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
    }

    public void Reach(Vector2 _pos, ReachCallback _func)
    {
        m_reachFunction = _func;
        m_hasObjective = true;
        m_objective = _pos;
    }
}
