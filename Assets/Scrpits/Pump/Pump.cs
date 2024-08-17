using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pump : MonoBehaviour
{
    [SerializeField] private Balloon m_connectedBalloon;
    public void Press(float _elbowDropHeight)
    {
        float force = GameManager.elbowForceToPressure(_elbowDropHeight);
        if(force > 0.0f)
            m_connectedBalloon.Inflate(force);
    }

    public void Release()
    {
        
    }
}
