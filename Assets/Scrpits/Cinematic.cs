using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Cinemachine;
using UnityEngine;

public class Cinematic : MonoBehaviour
{
    [SerializeField] private Animator m_UI;
    [SerializeField] private CinemachineVirtualCamera m_camera;
    [SerializeField] private List<String> m_sequence;

    private int m_currentAction = 0;
    
    public void Play()
    {
        GameManager.instance.TakeControl();
        m_camera.Priority = 100;
        
        ReadAction(m_sequence[m_currentAction]);
        
    }
    
    public void End()
    {
        GameManager.currentCheckpoint.Activate(false);
        GameManager.instance.GiveControl();
        m_camera.Priority = 0;
    }
    
    private void ReadNextAction()
    {
        ++m_currentAction;
        if (m_currentAction < m_sequence.Count) ReadAction(m_sequence[m_currentAction]);
        else End();
    }

    void ReadAction(String _action)
    {
        Debug.Log(_action);
        String[] splitActions = _action.Split(new char[]{'[',']',','}, StringSplitOptions.RemoveEmptyEntries );
        foreach (String action in splitActions)
        {
            String[] splitAction = action.Split('=');
            if (splitAction.Length != 2)
            {
                Debug.LogError("action invalid : " + action);    
                continue;
            }
            
            string type = splitAction[0];
            switch (type)
            {
                case "TeleportPlayer":
                    Vector2 teleportPos = ReadPosition(splitAction[1]);
                    TeleportPlayer(teleportPos);
                    break;
                case "ReachPlayer":
                    Vector2 reachPos = ReadPosition(splitAction[1]);
                    ReachPlayer(reachPos);
                    break;
                case "Wait":
                    float duration;
                    Debug.Log("Wait for " + splitAction[1]);
                    if (float.TryParse(splitAction[1], NumberStyles.Any, CultureInfo.InvariantCulture, out duration)) 
                        StartCoroutine(Wait(duration));
                    break;
            }
        }
    }


    private void TeleportPlayer(Vector2 _pos)
    {
        GameManager.character.transform.position = _pos;
        ReadNextAction();
    }

    private void ReachPlayer(Vector2 _pos)
    {
        GameManager.character.bot.Reach(_pos, ReachCallBack);
    }
    
    void ReachCallBack()
    {
        ReadNextAction();
    }

    private IEnumerator Wait(float _duration)
    {
        yield return new WaitForSeconds(_duration);
        ReadNextAction();
    }

    private Vector2 ReadPosition(string _position)
    {
        Debug.Log("Try to read coord : "+ _position);
        String[] coordonate = _position.Split(':');
        float x,y;

        if (coordonate.Length == 2 && float.TryParse(coordonate[0], NumberStyles.Any, CultureInfo.InvariantCulture, out x) 
                                   && float.TryParse(coordonate[1], NumberStyles.Any, CultureInfo.InvariantCulture, out y))
        {
            return new Vector2(x, y);
        }

        Debug.LogError("invalid coordonate : " + _position);
            
        return Vector2.zero;
    }
}
