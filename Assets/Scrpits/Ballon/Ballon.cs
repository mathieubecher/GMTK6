using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Ballon : MonoBehaviour
{
    [SerializeField] private Transform m_head;
    [SerializeField] private float m_size = 2.0f;
    [SerializeField] private List<Vector2> m_possibleInflateDir;
    [SerializeField] private int m_defaultDir;
    [SerializeField] private float m_inflateBlendForce = 0.1f;
    private LineRenderer m_line;
  
    private Vector2 m_currentInflateDir;
    private Vector2 m_headObjectivePos;
    private bool m_collided;
    
    void Awake()
    {
        m_line = GetComponent<LineRenderer>();
        ResetBalloon();
    }
    void OnEnable()
    {
        Controller.OnElbowDropPress += DebugInflate;
    }

    void OnDisable()
    {
        Controller.OnElbowDropPress -= DebugInflate;
        
    }

    void FixedUpdate()
    {
        Vector2 delta = (m_headObjectivePos - (Vector2)m_head.localPosition) * m_inflateBlendForce;
        if (delta.magnitude > 0.01f && !m_collided)
        {
            Vector2 desiredPos = CheckCollision(m_head.localPosition, delta);
        
            m_head.localPosition = desiredPos;
        }
        m_line.SetPosition(m_line.positionCount - 1, m_head.position);
    }

    private Vector2 CheckCollision(Vector2 _currentPos, Vector2 _delta)
    {
        //Disable current actor to ignore it
        gameObject.SetActive(false);
        
        RaycastHit2D hit = Physics2D.BoxCast(
            m_head.position, 
                Vector2.one * (m_size * 0.99f), 
                0.0f, 
                _delta.normalized, 
                _delta.magnitude
            );

        Vector2 delta = _delta;
        
        if (hit.collider != null)
        {
            delta = _delta.normalized * hit.distance;
            m_collided = true;
        }

        gameObject.SetActive(true);
        return _currentPos + delta;
    }

    private void ResetBalloon()
    {
        m_currentInflateDir = m_possibleInflateDir[m_defaultDir];
        
        m_head.localScale = Vector3.one * m_size;
        
        m_headObjectivePos = new Vector3(0.0f, m_size/2.0f, 0.0f);
        m_head.localPosition = m_headObjectivePos;
        
        m_line.positionCount = 2;
        m_line.SetPositions(new Vector3[]{transform.position, m_headObjectivePos });
        m_line.widthMultiplier = m_size;
    }

    private void DebugInflate()
    {
        Inflate();
    }

    public void Inflate(float _value = 1.0f)
    {
        m_headObjectivePos += m_currentInflateDir * _value;
    }
}
