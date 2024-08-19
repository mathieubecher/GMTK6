using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Cinemachine;
using UnityEngine;

public class Cinematic : MonoBehaviour
{
    [SerializeField] private Animator m_UI;
    [SerializeField] private List<CinemachineVirtualCamera> m_cameras;
    [SerializeField] private List<String> m_sequence;

    private int request = 0;
    private int m_currentAction = 0;
    
    public void Play()
    {
        GameManager.instance.TakeControl();
        
        ReadAction(m_sequence[m_currentAction]);
        
    }

    public void End()
    {
        GameManager.currentCheckpoint.Activate(false);
        GameManager.instance.GiveControl();
        foreach (var camera in m_cameras)
        {
            camera.Priority = 0;
        }
    }

    private void Update()
    {
        if (m_currentAction < m_sequence.Count) ReadNextAction();
    }

    private void EndRequest()
    {
        --request;
        Debug.Log("End request " + request);
    }
    private void ReadNextAction()
    {
        if (request > 0) return;
        
        ++m_currentAction;
        request = 0;
        if (m_currentAction < m_sequence.Count) ReadAction(m_sequence[m_currentAction]);
        else End();
    }

    void ReadAction(String _action)
    {
        Debug.Log(_action);
        String[] splitActions = _action.Split(new char[]{'[',']','|'}, StringSplitOptions.RemoveEmptyEntries );
        foreach (String action in splitActions)
        {
            String[] splitAction = action.Split('=');
            if (splitAction.Length != 2)
            {
                Debug.LogError("action invalid : " + action);    
                continue;
            }
            
            string type = splitAction[0];
            Debug.Log(action);
            switch (type)
            {
                case "ActivateCamera":
                    int cameraId;
                    if (int.TryParse(splitAction[1], out cameraId)) 
                        ActivateCamera(cameraId);
                    break;
                case "WaitIsOnGround":
                    WaitIsOnGround();
                    break;
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
                    if (float.TryParse(splitAction[1], NumberStyles.Any, CultureInfo.InvariantCulture, out duration)) 
                        StartCoroutine(Wait(duration));
                    break;
            }
        }

    }
    
    private Vector2 ReadPosition(string _position)
    {
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
    
    public void ActivateCamera(int _i)
    {
        ++request;
        m_cameras[_i].Priority = 100;
        EndRequest();
    }

    public void WaitIsOnGround()
    {
        ++request;
        Debug.Log(request + "-> Wait is on ground");
        GameManager.character.bot.WaitIsOnGround(EndRequest);
    }
    
    private void TeleportPlayer(Vector2 _pos)
    {
        ++request;
        Debug.Log(request + "-> Teleport " + _pos);
        GameManager.character.transform.position = _pos;
        EndRequest();
    }

    private void ReachPlayer(Vector2 _pos)
    {
        ++request;
        Debug.Log(request + "-> Reach pos " + _pos);
        GameManager.character.bot.Reach(_pos, EndRequest);
    }
    
    private IEnumerator Wait(float _duration)
    {
        ++request;
        Debug.Log(request + "-> Wait for " + _duration);
        yield return new WaitForSeconds(_duration);
        EndRequest();
    }

}
