using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class Balloon : MonoBehaviour
{
    [SerializeField] private Transform m_head;
    [SerializeField] private float m_size = 2.0f;
    [SerializeField] private Vector2 m_defaultDir;
    private LineRenderer m_line;
  
    private Vector2 m_currentInflateDir;
    private Vector2 m_headObjectivePos;
    private bool m_collided;
    
    private float m_pressure => (m_headObjectivePos - (Vector2)m_head.position).magnitude;
    
    void Awake()
    {
        m_line = GetComponent<LineRenderer>();
        ResetBalloon();
    }
    
    private void ResetBalloon()
    {
        m_currentInflateDir = m_defaultDir;
        m_collided = false;
        
        m_head.localScale = Vector3.one * m_size;
        m_head.localRotation = Quaternion.Euler(0.0f, 0.0f, Vector2.SignedAngle(Vector2.up, m_currentInflateDir));
        
        m_headObjectivePos = transform.position + new Vector3(0.0f, m_size / 2.0f, 0.0f);
        m_head.position = m_headObjectivePos;
        
        m_line.positionCount = 2;
        m_line.SetPositions(new Vector3[]{transform.position, m_headObjectivePos });
        m_line.widthMultiplier = m_size;
    }
    
    void FixedUpdate()
    {
        Vector2 delta = (m_headObjectivePos - (Vector2)m_head.position) * GameManager.inflateBlendForce;
        Vector2 desiredPos = CheckCollision(m_head.position, delta);
        m_head.position = desiredPos;
        m_line.SetPosition(m_line.positionCount - 1, m_head.position);
    }

    
    private void Explode()
    {
        ResetBalloon();
    }
    

    public void Inflate(float _value = 1.0f)
    {
        m_headObjectivePos += m_currentInflateDir * (m_collided ? math.min(_value, GameManager.maxPressure - m_pressure) : _value);
    }

    public void Hit(Vector2 _dir)
    {
        if (m_collided)
        {
            UpdateDirection(_dir, m_head.position);
        }
    }
    
    private Vector2 CheckCollision(Vector2 _currentPos, Vector2 _delta)
    {
        //Disable current actor to ignore it
        gameObject.SetActive(false);
        RaycastHit2D hit = Physics2D.Raycast(
            m_head.position, 
                _delta.normalized, 
                m_size/2.0f + _delta.magnitude,
            GameManager.obstacleLayermask
            );

        Vector2 desiredPosition = _currentPos;
        
        if (hit.collider != null)
        {
            if (GameManager.IsCactus(hit.collider.gameObject.layer))
            {
                Explode();
                desiredPosition = m_head.position;
            }
            else
            {
                desiredPosition += _delta.normalized * (Vector2.Dot(_delta.normalized, hit.point - _currentPos) - m_size / 2.0f);
                m_collided = true;
            
                CheckPossiblePath(desiredPosition);
            }
        }
        else
        {
            desiredPosition += _delta;
        }
    
        // Enable current actor at the end of raycast
        gameObject.SetActive(true);
        
        return desiredPosition;
    }


    private void CheckPossiblePath(Vector2 _currentPos)
    {
        Vector2 bestPathFound = Vector2.zero;
        int nbPathFound = 0;
        
        foreach (var path in GameManager.possibleInflateDir)
        {
            if (Vector2.Dot(m_currentInflateDir, path) < 0.0f) continue;
            
            gameObject.SetActive(false);
            RaycastHit2D hit = Physics2D.Raycast(
                _currentPos,
                path, 
                m_size / 2.0f + 0.5f,
                GameManager.obstacleLayermask
            );
            if (hit.collider == null)
            {
                ++nbPathFound;
                bestPathFound = bestPathFound.y > 0.0f? bestPathFound : path;
            }
        }

        if (nbPathFound == 1 || bestPathFound.y > 0.0f)
        {
            UpdateDirection(bestPathFound, _currentPos);
        }
    }

    private void UpdateDirection(Vector2 _dir, Vector2 _currentPos)
    {
        m_currentInflateDir = _dir;
        m_line.SetPosition(m_line.positionCount - 1, _currentPos);
        ++m_line.positionCount;
        m_line.SetPosition(m_line.positionCount - 1, _currentPos);
        m_headObjectivePos = _currentPos + m_currentInflateDir * (m_headObjectivePos - _currentPos).magnitude;
        m_collided = false;
        m_head.localRotation = Quaternion.Euler(0.0f, 0.0f, Vector2.SignedAngle(Vector2.up, m_currentInflateDir));
    }
}
