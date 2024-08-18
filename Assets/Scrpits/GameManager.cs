using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager m_instance;
    public static GameManager instance
    {
        get
        {
            if (!m_instance)
            {
                m_instance = FindObjectOfType<GameManager>();
            }
            return m_instance;
        }
    }

    public static List<Vector2> possibleInflateDir => instance.m_possibleInflateDir;
    public static Vector2 downInflateDir => instance.m_downInflateDir;
    public static GameObject balloonBody => instance.m_ballonBody;
    public static float inflateBlendForce => instance.m_inflateBlendForce;
    public static float maxPressure => instance.m_maxPressure;
    public static AnimationCurve pumpPressOverTime => instance.m_pressOverTime;
    public static AnimationCurve pumpReleaseOverTime => instance.m_releaseOverTime;
    public static AnimationCurve pressureToOffsetValue => instance.m_pressureToOffsetValue;
    public static AnimationCurve offsetValueToPressDuration => instance.m_offsetValueToPressDuration;
    public static float elbowForceToPressure(float elbowDropHeight) { return instance.m_elbowForceToPressure.Evaluate(elbowDropHeight); }
    public static Checkpoint currentCheckpoint
    {
        get => instance.m_currentCheckpoint;
        set => instance.m_currentCheckpoint = value;
    }
    
    public static Character character => instance.m_character;
    public static Balloon mainBalloon => instance.m_mainBalloon;
    
    public static LayerMask obstacleLayermask => instance.m_obstacleLayermask;
    public static LayerMask deadLayermask => instance.m_deadLayermask;
    public static LayerMask balloonHeadLayermask => instance.m_balloonHeadLayermask;
    public static LayerMask movableLayermask => instance.m_movableLayermask;
    public static LayerMask pumpLayermask => instance.m_pumpLayermask;
    public static LayerMask checkpointLayermask => instance.m_checkpointLayermask;
    public static bool IsBalloonHead(int _layer){ return balloonHeadLayermask == (balloonHeadLayermask | (1 << _layer));}
    public static bool IsCactus(int _layer){ return deadLayermask == (deadLayermask | (1 << _layer));}
    public static bool IsPump(int _layer){ return pumpLayermask == (pumpLayermask | (1 << _layer));}
    public static bool IsMovablePlatform(int _layer){ return movableLayermask == (movableLayermask | (1 << _layer));}
    public static bool IsCheckpoint(int _layer){ return checkpointLayermask == (checkpointLayermask | (1 << _layer));}
    

    
    #endregion
    
    [Header("Layer")]
    [SerializeField] private LayerMask m_obstacleLayermask;
    [SerializeField] private LayerMask m_deadLayermask;
    [SerializeField] private LayerMask m_balloonHeadLayermask;
    [SerializeField] private LayerMask m_movableLayermask;
    [SerializeField] private LayerMask m_pumpLayermask;
    [SerializeField] private LayerMask m_checkpointLayermask;
    
    [Header("Balloon")]
    [SerializeField] private List<Vector2> m_possibleInflateDir;
    [SerializeField] private Vector2 m_downInflateDir;
    [SerializeField] private float m_inflateBlendForce = 0.1f;
    [SerializeField] private float m_maxPressure = 3.0f;
    [SerializeField] private GameObject m_ballonBody;

    [Header("Pump")] 
    [SerializeField] private AnimationCurve m_elbowForceToPressure;
    [SerializeField] private AnimationCurve m_pressOverTime;
    [SerializeField] private AnimationCurve m_releaseOverTime;
    [SerializeField] private AnimationCurve m_pressureToOffsetValue;
    [SerializeField] private AnimationCurve m_offsetValueToPressDuration;

    [Header("Runtime")]
    [SerializeField] private Checkpoint m_currentCheckpoint;
    [SerializeField] private Character m_character;
    [SerializeField] private Balloon m_mainBalloon;
    
    void OnEnable()
    {
        Controller.OnReset += Reset;
    }

    void OnDisable()
    {
        Controller.OnReset -= Reset;
    }

    void Start()
    {
        m_currentCheckpoint.Activate(false);
    }
    
    public void Reset()
    {
        m_currentCheckpoint.Reset();
        m_character.Reset(m_currentCheckpoint.spawnPos);
        m_mainBalloon.Reset();
    }
    
    public static float TimeFromValue(AnimationCurve c, float value, float precision = 1e-6f)
    {
        float minTime = c.keys[0].time;
        float maxTime = c.keys[c.keys.Length-1].time;
        float best = (maxTime + minTime) / 2;
        float bestVal = c.Evaluate(best);
        int it=0;
        const int maxIt = 1000;
        float sign = Mathf.Sign(c.keys[c.keys.Length-1].value -c.keys[0].value);
        while(it < maxIt && Mathf.Abs(minTime - maxTime) > precision) {
            if((bestVal - value) * sign > 0) {
                maxTime = best;
            } else {
                minTime = best;
            }
            best = (maxTime + minTime) / 2;
            bestVal = c.Evaluate(best);
            it++;
        }
        return best;
    }
}
