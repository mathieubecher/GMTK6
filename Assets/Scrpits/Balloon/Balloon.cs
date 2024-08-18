using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Balloon : MonoBehaviour
{
    [SerializeField] private Transform m_head;
    [SerializeField] private float m_size = 2.0f;
    [SerializeField] private Vector2 m_defaultDir;
    [SerializeField] private bool m_resetAtExplode;
    [SerializeField] private Color m_color;
    private LineRenderer m_line;
  
    private Vector2 m_currentInflateDir;
    private Vector2 m_headObjectivePos;
    private List<SpriteRenderer> m_balloonBodies;
    private bool m_collided;
    
    private int m_saveBodyId;
    private int m_saveLineId;
    private Vector2 m_saveDir;
    private Vector2 m_savePos;
    
    private float m_pressure => (m_headObjectivePos - (Vector2)m_head.position).magnitude;
    
    void Awake()
    {
        Init();
    }
    public void Init()
    {
        m_saveLineId = 1;
        m_saveBodyId = 0;
        m_saveDir = m_defaultDir;
        m_savePos = transform.position + new Vector3(0.0f, m_size / 2.0f, 0.0f);
        
        m_balloonBodies = new List<SpriteRenderer>();
        m_line = GetComponent<LineRenderer>();
        m_line.positionCount = m_saveLineId;
        m_line.SetPosition(0, transform.position);
        
        m_head.localScale = Vector3.one * m_size;
        m_head.GetChild(0).GetComponent<SpriteRenderer>().color = m_color;
        
        Reset();
    }
    
    public void Reset()
    {
        m_collided = false;
        m_currentInflateDir = m_saveDir;
        m_line.positionCount = m_saveLineId;
        
        for (int i = m_balloonBodies.Count; i > m_saveBodyId; --i)
        {
            Destroy(m_balloonBodies[i - 1].gameObject);
            m_balloonBodies.RemoveAt(i - 1);
        }
        
        m_headObjectivePos = m_savePos;
        m_head.position = m_headObjectivePos;
        m_head.GetChild(0).localRotation = Quaternion.Euler(0.0f, 0.0f, Vector2.SignedAngle(Vector2.up, m_currentInflateDir));
        
        UpdateDirection(m_currentInflateDir, m_headObjectivePos);
    }
    
    void FixedUpdate()
    {
        Vector2 delta = (m_headObjectivePos - (Vector2)m_head.position) * GameManager.inflateBlendForce;
        Vector2 desiredPos = CheckCollision(m_head.position, delta);
        m_head.position = desiredPos;
        
        m_line.SetPosition(m_line.positionCount - 1, m_head.position);
        UpdateBodyPosition();
    }

    private void AddBody()
    {
        GameObject instance = Instantiate(GameManager.balloonBody, Vector3.zero, Quaternion.identity);
        var spriteRenderer = instance.GetComponent<SpriteRenderer>();
        spriteRenderer.color = m_color;
        m_balloonBodies.Add(spriteRenderer);
        instance.SetActive(false);
    }
    private void UpdateBodyPosition()
    {
        bool isUp = (math.abs(m_currentInflateDir.y) > 0.0f);
        Vector2 pointB = m_line.GetPosition(m_line.positionCount - 1);
        Vector2 pointA = m_line.GetPosition(m_line.positionCount - 2);
        m_balloonBodies[^1].transform.position = (pointA + pointB)/ 2.0f - (isUp ? Vector2.zero : m_currentInflateDir * m_size / 2.0f);
        m_balloonBodies[^1].size = new Vector2(
            math.abs(pointA.x - pointB.x) + (isUp ? m_size : 0.0f),
            math.abs(pointA.y - pointB.y) + m_size
            );
        m_balloonBodies[^1].transform.GetChild(0).localScale = m_balloonBodies[^1].size;
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
        ActiveCollider(false, 3);
        
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
        ActiveCollider(true, 4);
        
        return desiredPosition;
    }

    private void ActiveCollider(bool _active, int _nb = 3)
    {
        gameObject.SetActive(_active);
        for (int i = m_balloonBodies.Count - 1; i >= m_balloonBodies.Count - _nb && i >= 0; --i)
        {
            m_balloonBodies[i].gameObject.SetActive(_active);
        }
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
        m_line.SetPosition(m_line.positionCount - 1, _currentPos);
        if(m_line.positionCount > 1 && m_balloonBodies.Count > 0) UpdateBodyPosition();
        
        m_currentInflateDir = _dir;
        ++m_line.positionCount;
        m_line.SetPosition(m_line.positionCount - 1, _currentPos);
        AddBody();
        UpdateBodyPosition();
        
        m_headObjectivePos = _currentPos + m_currentInflateDir * (m_headObjectivePos - _currentPos).magnitude;
        m_collided = false;
        m_head.GetChild(0).localRotation = Quaternion.Euler(0.0f, 0.0f, Vector2.SignedAngle(Vector2.up, m_currentInflateDir));
    }

    private void Explode()
    {
        if (m_resetAtExplode)
        {
            GameManager.instance.Reset();
        }
    }

    public void SaveState()
    {
        m_saveBodyId = m_balloonBodies.Count;
        m_saveLineId = m_line.positionCount;
        m_saveDir = m_currentInflateDir;
        m_savePos = m_head.position;
        UpdateDirection(m_currentInflateDir, m_head.position);

    }
}
