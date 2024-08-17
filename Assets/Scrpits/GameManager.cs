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
    public static float inflateBlendForce => instance.m_inflateBlendForce;
    public static float maxPressure => instance.m_maxPressure;
    public static LayerMask obstacleLayermask => instance.m_obstacleLayermask;
    public static LayerMask deadLayermask => instance.m_deadLayermask;
    
    #endregion
    
    [Header("Balloon")]
    [SerializeField] private List<Vector2> m_possibleInflateDir;
    [SerializeField] private float m_inflateBlendForce = 0.1f;
    [SerializeField] private float m_maxPressure = 3.0f;
    [SerializeField] private LayerMask m_obstacleLayermask;
    [SerializeField] private LayerMask m_deadLayermask;
    
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
