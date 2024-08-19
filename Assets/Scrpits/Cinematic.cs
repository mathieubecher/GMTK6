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

    private bool m_active = false;
    private int m_request = 0;
    private int m_currentAction = 0;
    
    public void Play()
    {
        m_active = true;
        GameManager.instance.TakeControl();
        
        ReadAction(m_sequence[m_currentAction]);
        
    }

    public void End()
    {
        m_active = false;
        GameManager.currentCheckpoint.Activate(false);
        GameManager.instance.GiveControl();
        GameManager.character.locomotion.enabled = true;
        GameManager.character.gravityScale = 1.0f;
        foreach (var camera in m_cameras)
        {
            camera.Priority = 0;
        }
        GameManager.gameFlow.SetCameraRes("");
    }

    private void Update()
    {
        if (m_active && m_currentAction < m_sequence.Count) ReadNextAction();
    }

    private void EndRequest()
    {
        --m_request;
        Debug.Log("End request " + m_request);
    }
    private void ReadNextAction()
    {
        if (m_request > 0) return;
        
        ++m_currentAction;
        m_request = 0;
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
                case "CameraRes":
                        ActivateCameraRes(splitAction[1]);
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
                case "PedroTalk":
                    ++m_request;
                    GameManager.gameFlow.DrawDialogPedro(splitAction[1], EndRequest);
                    break;
                case "DiegoTalk":
                    ++m_request;
                    GameManager.gameFlow.DrawDialogDiego(splitAction[1], EndRequest);
                    break;
                case "GameFlowTrigger":
                    ++m_request;
                    GameManager.gameFlow.SendTrigger(splitAction[1], EndRequest);
                    break;
                case "StopPlayer":
                    StopPlayer();
                    break;
            }
        }

    }

    private void ActivateCameraRes(string _res)
    {
        GameManager.gameFlow.SetCameraRes(_res);
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
        ++m_request;
        m_cameras[_i].Priority = 100;
        EndRequest();
    }

    public void WaitIsOnGround()
    {
        ++m_request;
        Debug.Log(m_request + "-> Wait is on ground");
        GameManager.character.bot.WaitIsOnGround(EndRequest);
    }
    
    private void TeleportPlayer(Vector2 _pos)
    {
        ++m_request;
        Debug.Log(m_request + "-> Teleport " + _pos);
        GameManager.character.transform.position = _pos;
        EndRequest();
    }

    private void ReachPlayer(Vector2 _pos)
    {
        ++m_request;
        Debug.Log(m_request + "-> Reach pos " + _pos);
        GameManager.character.bot.Reach(_pos, EndRequest);
    }
    
    private void StopPlayer()
    {
        ++m_request;
        Debug.Log(m_request + "-> Stop player");
        GameManager.character.locomotion.enabled = false;
        GameManager.character.gravityScale = 0.0f;
        GameManager.character.rigidbody.velocity = Vector2.zero;
        EndRequest();
    }

    private IEnumerator Wait(float _duration)
    {
        ++m_request;
        Debug.Log(m_request + "-> Wait for " + _duration);
        yield return new WaitForSeconds(_duration);
        EndRequest();
    }

}
