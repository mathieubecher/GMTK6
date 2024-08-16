using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
