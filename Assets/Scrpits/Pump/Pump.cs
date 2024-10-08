using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Pump : MonoBehaviour
{
    [SerializeField] private Balloon m_connectedBalloon;
    [SerializeField] private Transform m_pressureSensor;
    [SerializeField] private float m_maxPression = 10.0f;
    [SerializeField] private float m_minPression = 0.0f;
    [SerializeField] private float m_minPressureAngle = -70.0f;
    [SerializeField] private float m_maxPressureAngle = 70.0f;
    [SerializeField] private Sprite m_emptyPump;
    private float m_offsetValue = 0.0f;
    private float m_starPos;
    private bool m_pressed;
    private float m_pressTimer;
    private float m_releaseTimer;
    private float m_cooldown = 0.0f;
    private bool m_isEmpty = false;

    private void Awake()
    {
        m_starPos = transform.position.y;
    }

    private void Update()
    {
        float pression = m_maxPression - m_minPression;
        float pressureRatio = pression > 0.0f? (m_connectedBalloon.length + m_connectedBalloon.pressure - m_minPression) / pression : 1.0f;
        m_pressureSensor.localRotation = Quaternion.Euler(0.0f,0.0f,m_minPressureAngle + (m_maxPressureAngle - m_minPressureAngle) * math.clamp(pressureRatio, 0.0f, 1.0f));
        
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


        if (pressureRatio >= 1.0f && m_isEmpty == false)
        {
            GetComponent<SpriteRenderer>().sprite = m_emptyPump;
            m_isEmpty = true;
        }

        m_cooldown -= Time.deltaTime;
    }
    
    public void Press(float _elbowDropHeight)
    {
        if (m_cooldown > 0.0f) return;
        float rawForce = GameManager.elbowForceToPressure(_elbowDropHeight);
        float force = math.min(m_maxPression - m_connectedBalloon.length, rawForce);
        //Debug.Log("Height : " + _elbowDropHeight + "Force : " + rawForce + ", Clamp force : " + force + " -> MaxPression : " + m_maxPression + ", Balloon length : " + m_connectedBalloon.length + ", Balloon pressure : " + m_connectedBalloon.pressure);
        m_connectedBalloon.Inflate(force, m_maxPression);
        m_offsetValue = GameManager.pressureToOffsetValue.Evaluate(force);
        m_pressTimer = 0.0f;
        m_pressed = true;
        m_cooldown = 0.3f;
    }

    public void Release()
    {
        m_releaseTimer = 0.0f;
        m_pressed = false;
        
    }
}
