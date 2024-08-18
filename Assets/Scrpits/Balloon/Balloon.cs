using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Balloon : MonoBehaviour
{
    const float TOLERANCE = 1e-6f;
    
    [SerializeField] private Transform m_head;
    [SerializeField] private float m_size = 2.0f;
    [SerializeField] private Vector2 m_defaultDir;
    [SerializeField] private bool m_resetAtExplode;
    [SerializeField] private Color m_color;
    private LineRenderer m_line;
  
    private Vector2 m_currentInflateDir;
    private List<SpriteRenderer> m_balloonBodies;
    private bool m_collided;
    private bool m_explode;
    
    private int m_saveBodyId;
    private int m_saveLineId;
    private Vector2 m_saveDir;
    private Vector2 m_savePos;
    private float m_previousLength = 0.0f;
    private float m_currentLength = 0.0f;
    
    private float m_pressure = 0.0f;

    public float length => m_previousLength + m_currentLength;
    public float pressure => m_pressure;
    
    void Awake()
    {
        Init();
    }
    public void Init()
    {
        m_previousLength = 0.0f;
        m_currentLength = 0.0f;
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
        m_explode = false;
        m_currentInflateDir = m_saveDir;
        m_line.positionCount = m_saveLineId;
        m_pressure = 0.0f; 
        for (int i = m_balloonBodies.Count; i > m_saveBodyId; --i)
        {
            Destroy(m_balloonBodies[i - 1].gameObject);
            m_balloonBodies.RemoveAt(i - 1);
        }
        
        m_head.position = m_savePos;
        m_head.GetChild(0).localRotation = Quaternion.Euler(0.0f, 0.0f, Vector2.SignedAngle(Vector2.up, m_currentInflateDir));
        
        UpdateDirection(m_currentInflateDir, m_head.position);
        ActiveCollider(true, 1000);
    }
    
    void FixedUpdate()
    {
        if (m_explode || m_pressure < TOLERANCE) return;
        
        float delta = m_pressure * GameManager.inflateBlendForce;
        Vector2 currentDir = m_currentInflateDir;
        delta = math.max(0.0f, CheckCollision(m_head.position, currentDir, delta));
        m_head.position += (Vector3)currentDir * delta;
        Debug.Log(m_currentInflateDir + " pressure : " + m_pressure + " - force " + GameManager.inflateBlendForce + " - result " + delta);
        m_pressure -= delta;
        m_currentLength += delta;
        
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
        
        Vector2 size = new Vector2(
            math.abs(pointA.x - pointB.x) + (isUp ? m_size : 0.0f),
            math.abs(pointA.y - pointB.y) + m_size
        );
        m_balloonBodies[^1].size = size;
        m_balloonBodies[^1].transform.GetChild(0).localScale = size;
    }

    public void Inflate(float _value = 1.0f)
    {
        m_pressure = math.min(GameManager.maxPressure, m_pressure + _value);
    }
    
    public void Hit(Vector2 _dir)
    {
        if (m_collided)
        {
            UpdateDirection(_dir, m_head.position);
        }
    }
    
    private float CheckCollision(Vector2 _currentPos, Vector2 _dir, float _delta)
    {
        //Disable current actor to ignore it
        ActiveCollider(false, 2);
        
        RaycastHit2D hit = Physics2D.Raycast(
            m_head.position, 
            _dir, 
                m_size/2.0f + _delta,
            GameManager.obstacleLayermask
            );

        float delta = _delta;
        
        if (hit.collider != null)
        {
            if (GameManager.IsCactus(hit.collider.gameObject.layer))
            {
                Explode();
                return 0.0f;
            }
            else
            {
                Debug.Log("Collide : " + hit.collider.gameObject + " dist : " + (hit.point - _currentPos));
                delta = (hit.distance - m_size / 2.0f);
                m_collided = true;
            
                CheckPossiblePath(_currentPos + _dir * delta);
            }
        }
    
        // Enable current actor at the end of raycast
        ActiveCollider(true, 3);
        
        return delta;
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
        else if(nbPathFound == 0 && m_currentInflateDir.y == 0.0f)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                _currentPos,
                GameManager.downInflateDir, 
                m_size / 2.0f + 0.5f,
                GameManager.obstacleLayermask
            );
            
            if (hit.collider == null)
            {
                UpdateDirection(GameManager.downInflateDir, _currentPos);
            }
        }
    }

    private void UpdateDirection(Vector2 _dir, Vector2 _currentPos)
    {
        Debug.LogWarning("Update direction");
        m_line.SetPosition(m_line.positionCount - 1, _currentPos);
        if(m_line.positionCount > 1 && m_balloonBodies.Count > 0) UpdateBodyPosition();
        
        m_currentInflateDir = _dir;
        ++m_line.positionCount;
        m_line.SetPosition(m_line.positionCount - 1, _currentPos);
        AddBody();
        UpdateBodyPosition();
        
        m_collided = false;
        m_head.GetChild(0).localRotation = Quaternion.Euler(0.0f, 0.0f, Vector2.SignedAngle(Vector2.up, m_currentInflateDir));
    }

    private void Explode()
    {
        Debug.Log("Explode");
        ActiveCollider(false, 1000);
        m_explode = true;
        
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
        
        m_previousLength = m_currentLength;
        m_currentLength = 0.0f;
        
        UpdateDirection(m_currentInflateDir, m_head.position);

    }
}
