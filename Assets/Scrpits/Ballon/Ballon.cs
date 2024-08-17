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
  
    [Header("Debug")]
    [SerializeField] private Vector2 m_currentInflateDir;
    [SerializeField] private Vector2 m_headObjectivePos;
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
        Vector2 desiredPos = Vector2.Lerp(m_head.localPosition, m_headObjectivePos, m_inflateBlendForce);
        m_head.localPosition = desiredPos;
        m_line.SetPosition(m_line.positionCount - 1, m_head.position);
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
