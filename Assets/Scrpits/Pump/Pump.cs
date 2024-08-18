using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Pump : MonoBehaviour
{
    [SerializeField] private Balloon m_connectedBalloon;
    [SerializeField] private float m_maxPression = 10.0f;
    private float m_offsetValue = 0.0f;
    private float m_starPos;
    private bool m_pressed;
    private float m_pressTimer;
    private float m_releaseTimer;

    private void Awake()
    {
        m_starPos = transform.position.y;
    }

    private void Update()
    {
        Vector3 pos = transform.position;
        float pressDuration = GameManager.offsetValueToPressDuration.Evaluate(m_offsetValue);
        
        if (m_pressed && m_pressTimer < pressDuration)
        {
            m_pressTimer += Time.deltaTime;
            pos.y = m_starPos - (1.0f - GameManager.pumpPressOverTime.Evaluate(m_pressTimer / pressDuration)) * m_offsetValue;
        }
        else if (!m_pressed && m_releaseTimer < m_pressTimer)
        {
            m_releaseTimer += Time.deltaTime;
            float releaseDuration = m_pressTimer;
            pos.y = m_starPos - (1.0f - GameManager.pumpReleaseOverTime.Evaluate(m_releaseTimer / releaseDuration)) * m_offsetValue;
        }
        transform.position = pos;
    }
    
    public void Press(float _elbowDropHeight)
    {
        float force = GameManager.elbowForceToPressure(_elbowDropHeight);
        force = math.min(m_maxPression - m_connectedBalloon.length - m_connectedBalloon.pressure, force);
        if(force > 0.0f)
            m_connectedBalloon.Inflate(force);
        m_offsetValue = GameManager.pressureToOffsetValue.Evaluate(force);
        m_pressTimer = 0.0f;
        m_pressed = true;
    }

    public void Release()
    {
        m_releaseTimer = 0.0f;
        m_pressed = false;
    }
}
