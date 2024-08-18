using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Balloon : Interactive
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
    private Breakable m_blockByBreakable;
    
    private int m_saveBodyId;
    private int m_saveLineId;
    private Vector2 m_saveDir;
    private Vector2 m_savePos;
    private float m_previousLength = 0.0f;
    private float m_currentLength = 0.0f;
    
    private float m_pressure = 0.0f;

    [SerializeField] public float length => m_previousLength + m_currentLength;
    [SerializeField] public float pressure => m_pressure;
    
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
        
        Reset();
    }
    
    public override void Reset()
    {
        m_collided = false;
        m_blockByBreakable = null;
        m_explode = false;
        m_currentInflateDir = m_saveDir;
        m_line.positionCount = m_saveLineId;
        m_pressure = 0.0f;
        m_currentLength = 0.0f;
        for (int i = m_balloonBodies.Count; i > m_saveBodyId; --i)
        {
            DestroyBalloon(m_balloonBodies[i - 1]);
        }
        
        m_head.position = m_savePos;
        m_head.GetChild(0).localRotation = Quaternion.Euler(0.0f, 0.0f, Vector2.SignedAngle(Vector2.up, m_currentInflateDir));
        
        UpdateDirection(m_currentInflateDir, m_head.position);
        ActiveCollider(true, 1000);
    }

    void FixedUpdate()
    {
        if(m_explode) UpdateExplode();
        else UpdateInflate();
    }

    private void UpdateExplode()
    {
        if (m_line.positionCount <= m_saveLineId) return;
        
        Vector2 currentDir = -m_currentInflateDir;
        float delta = GameManager.explodeSpeed * Time.deltaTime;
        
        m_head.gameObject.SetActive(false);
        m_head.position += (Vector3)currentDir * delta;
        if (Vector3.Distance(m_head.position, m_line.GetPosition(m_line.positionCount - 2)) < delta)
        {
            --m_line.positionCount;
            DestroyBalloon(m_balloonBodies[^1]);

            if (m_line.positionCount <= 1) return; 
            
            m_currentInflateDir = (m_line.GetPosition(m_line.positionCount - 1) - m_line.GetPosition(m_line.positionCount - 2)).normalized;
            m_head.GetChild(0).localRotation = Quaternion.Euler(0.0f, 0.0f, Vector2.SignedAngle(Vector2.up, m_currentInflateDir));
        }
        
        m_line.SetPosition(m_line.positionCount - 1, m_head.position);
        UpdateBodyPosition();
    }

    private void UpdateInflate()
    {
        if (m_pressure < TOLERANCE) return;
        float delta = m_pressure * GameManager.inflateBlendForce;
        Vector2 currentDir = m_currentInflateDir;
        delta = math.max(0.0f, CheckCollision(m_head.position, currentDir, delta));
        m_head.position += (Vector3)currentDir * delta;
        m_pressure -= delta;
        m_currentLength += delta;
        
        m_line.SetPosition(m_line.positionCount - 1, m_head.position);
        UpdateBodyPosition();
    }

    private void AddBody()
    {
        GameObject instance = Instantiate(GameManager.balloonBody, transform);
        var spriteRenderer = instance.GetComponent<SpriteRenderer>();
        spriteRenderer.color = m_color;
        m_balloonBodies.Add(spriteRenderer);
        ActiveCollider(false, 1);
    }
    private void UpdateBodyPosition()
    {
        bool isUp = (math.abs(m_currentInflateDir.y) > 0.0f);
        Vector2 pointB = m_line.GetPosition(m_line.positionCount - 1);
        Vector2 pointA = m_line.GetPosition(m_line.positionCount - 2);
        m_balloonBodies[^1].transform.position = (pointA + pointB)/ 2.0f;
        Vector2 size = new Vector2(
            math.abs(pointA.x - pointB.x) + m_size,
            math.abs(pointA.y - pointB.y) + m_size
        );
        m_balloonBodies[^1].size = size;

        Vector2 colSize = new Vector2(size.x - (isUp ? 0.0f : m_size), size.y);

        m_balloonBodies[^1].transform.GetChild(0).localScale = colSize;
        
        m_balloonBodies[^1].transform.GetChild(0).localPosition = 
            isUp ? Vector2.zero : new Vector2(-math.sign(m_currentInflateDir.x) * m_size / 2.0f, 0.0f);
    }

    public void Inflate(float _value = 1.0f)
    {
        if (m_blockByBreakable && m_blockByBreakable.TryToBreak(m_currentInflateDir, _value))  
            m_pressure = _value;
        else
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
            }
            else if(GameManager.IsBreakable(hit.collider.gameObject.layer) && hit.collider.TryGetComponent(out Breakable breakable))
            {
                delta = 0.0f;
                m_blockByBreakable = breakable;
            }
            else
            {
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
        m_head.GetComponentInChildren<BoxCollider2D>().enabled = _active;
        
        for (int i = m_balloonBodies.Count - 1; i >= m_balloonBodies.Count - _nb && i >= 0; --i)
        {
            var collider = m_balloonBodies[i].GetComponentInChildren<BoxCollider2D>();
            if(collider) collider.enabled = _active;
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
            if (hit.collider == null || GameManager.IsBreakable(hit.collider.gameObject.layer))
            {
                ++nbPathFound;
                bestPathFound = bestPathFound.y > 0.0f? bestPathFound : path;
            }
        }

        if (nbPathFound == 1 || bestPathFound.y > 0.0f)
        {
            UpdateDirection(bestPathFound, _currentPos);
        }
        /*
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
        }*/
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
        
        m_collided = false;
        m_head.GetChild(0).localRotation = Quaternion.Euler(0.0f, 0.0f, Vector2.SignedAngle(Vector2.up, m_currentInflateDir));
    }

    private void Explode()
    {
        Debug.Log("Explode");
        m_explode = true;
        
        if (m_resetAtExplode)
        {
            GameManager.instance.Reset();
        }
    }
    
    private void DestroyBalloon(SpriteRenderer _balloon)
    {
        var character = _balloon.gameObject.GetComponentInChildren<Character>();
        if (character) character.transform.parent = null;
        m_balloonBodies.Remove(_balloon);
        Destroy(_balloon.gameObject);
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
