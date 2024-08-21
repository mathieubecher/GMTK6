using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;


public class Balloon : Interactive
{
    const float TOLERANCE = 1e-6f;
    
    [SerializeField] private Transform m_head;
    [SerializeField] private Transform m_exploseHead;
    [SerializeField] private float m_size = 2.0f;
    [SerializeField] private bool m_resetAtExplode;
    [SerializeField] private Color m_color;
    
    [SerializeField] private UnityEvent<float> m_InflateRequestEvent;
    [SerializeField] private UnityEvent<Vector2> m_changeDirectionEvent;
    [SerializeField] private UnityEvent m_ExplodeEvent;
    [SerializeField] private UnityEvent m_StuckEvent;
    [SerializeField] private UnityEvent m_StuckByBreakableEvent;
    [SerializeField] private UnityEvent m_BreakBreakableEvent;
    
    private LineRenderer m_line;
    private Animator m_animator;
  
    private Vector2 m_currentInflateDir;
    private List<SpriteRenderer> m_balloonBodies;
    [SerializeField] private bool m_collided;
    private bool m_explode;
    private bool m_deflate;
    private Breakable m_blockByBreakable;
    
    private int m_saveBodyId;
    private int m_saveLineId;
    private Vector2 m_saveDir;
    private Vector2 m_savePos;
    private float m_previousLength = 0.0f;
    private float m_currentLength = 0.0f;
    
    private float m_pressure = 0.0f;

    public float length => m_previousLength + m_currentLength;
    public float pressure => m_pressure;

    public void StartDeflate()
    {
        m_deflate = true;
        GameManager.effects.ScreenShake(5.0f, 0.1f, 0.1f);
    }
    
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
        m_saveDir = transform.rotation * Vector2.up;
        transform.rotation = quaternion.identity;
        m_savePos = transform.position + (Vector3)m_saveDir * m_size / 2.0f;
        
        m_balloonBodies = new List<SpriteRenderer>();
        m_line = GetComponent<LineRenderer>();
        m_line.positionCount = m_saveLineId;
        m_line.SetPosition(0, transform.position);
        
        m_animator = GetComponent<Animator>();
        
        m_head.localScale = Vector3.one * m_size;
        m_exploseHead.localScale = m_head.localScale;
        m_exploseHead.GetComponent<SpriteRenderer>().color = m_color;

        Reset();
    }
    
    public override void Reset()
    {
        m_collided = false;
        m_blockByBreakable = null;
        m_explode = false;
        m_deflate = false;
        m_currentInflateDir = m_saveDir;
        m_line.positionCount = m_saveLineId;
        m_pressure = 0.0f;
        m_currentLength = 0.0f;
        for (int i = m_balloonBodies.Count; i > m_saveBodyId; --i)
        {
            DestroyBalloon(m_balloonBodies[i - 1]);
        }
        
        m_head.position = m_savePos;
        m_exploseHead.position = m_savePos;
        Quaternion spriteHeadRotation = Quaternion.Euler(0.0f, 0.0f, Vector2.SignedAngle(Vector2.up, m_currentInflateDir));
        m_head.GetChild(0).localRotation = spriteHeadRotation;
        m_exploseHead.localRotation = spriteHeadRotation;
        
        UpdateDirection(m_currentInflateDir, m_head.position);

        ActiveCollider(true, 1000);
    }

    void FixedUpdate()
    {
        m_animator.SetBool("explode", m_explode);
        m_animator.SetBool("blocked", m_collided);
        
        if(m_explode) UpdateExplode();
        else UpdateInflate();
    }

    private void UpdateExplode()
    {
        if (m_line.positionCount <= m_saveLineId || !m_deflate) return;
        
        Vector2 currentDir = -m_currentInflateDir;
        float delta = GameManager.explodeSpeed * Time.deltaTime;
        Quaternion spriteHeadRotation = Quaternion.Euler(0.0f, 0.0f, Vector2.SignedAngle(Vector2.up, m_currentInflateDir));
        m_head.GetChild(0).localRotation = spriteHeadRotation;
        m_exploseHead.localRotation = spriteHeadRotation;
        
        var character = m_head.gameObject.GetComponentInChildren<Character>();
        if (character) character.ResetParent();
        
        m_head.position += (Vector3)currentDir * delta;
        m_exploseHead.position = m_head.position;
        
        
        if (Vector3.Distance(m_head.position, m_line.GetPosition(m_line.positionCount - 2)) < delta)
        {
            --m_line.positionCount;
            DestroyBalloon(m_balloonBodies[^1]);

            if (m_line.positionCount <= 1) return; 
            
            m_currentInflateDir = (m_line.GetPosition(m_line.positionCount - 1) - m_line.GetPosition(m_line.positionCount - 2)).normalized;
            m_head.GetChild(0).localRotation = Quaternion.Euler(0.0f, 0.0f, Vector2.SignedAngle(Vector2.up, m_currentInflateDir));
        }
        
        if(m_line.positionCount > 1) m_line.SetPosition(m_line.positionCount - 1, m_head.position);
        UpdateBodyPosition();
    }

    private void UpdateInflate()
    {
        if (m_pressure < TOLERANCE) return;
        float delta = m_pressure * GameManager.inflateBlendForce;
        Vector2 currentDir = m_currentInflateDir;
        delta = math.max(0.0f, CheckCollision(m_head.position, currentDir, delta));
        m_head.position += (Vector3)currentDir * delta;
        m_exploseHead.position = m_head.position;
        m_pressure -= delta;
        m_currentLength += delta;
        
        if(m_line.positionCount > 1) m_line.SetPosition(m_line.positionCount - 1, m_head.position);
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

    public void Inflate(float _value, float _maxPressure)
    {
        m_InflateRequestEvent?.Invoke(_value);
        if (m_blockByBreakable && m_blockByBreakable.TryToBreak(m_currentInflateDir, _value))
        {
            m_BreakBreakableEvent?.Invoke();
            m_pressure = _value;
        }
        else
        {
            m_pressure += _value;
            m_pressure = math.min(m_pressure, _maxPressure - length);
        }
        if (m_collided) m_pressure = math.min(GameManager.maxPressure, m_pressure);
        if(_value > 0.0f) StartCoroutine(TryPlayAction("Inflate", 0.1f));
    }
    
    public void Hit(Vector2 _dir)
    {
        m_animator.SetTrigger("Hit");
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
                m_StuckByBreakableEvent?.Invoke();
                delta = (hit.distance - m_size / 2.0f);
                m_blockByBreakable = breakable;
            }
            else
            {
                //Debug.Log("Collide with " + hit.collider.gameObject);
                delta = (hit.distance - m_size / 2.0f);
            
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
            var colliders = m_balloonBodies[i].GetComponentsInChildren<BoxCollider2D>();
            foreach(var collider in colliders)
            {
                if (collider && !GameManager.IsCharacter(collider.gameObject.layer)) 
                    collider.enabled = _active;
            }
        }
    }
    
    private void CheckPossiblePath(Vector2 _currentPos)
    {
        Vector2 bestPathFound = Vector2.zero;
        int nbPathFound = 0;
        
        foreach (var path in GameManager.possibleInflateDir)
        {
            if (Vector2.Dot(m_currentInflateDir, path) < -0.5f) continue;
            
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
        else if(!m_collided)
        {
            m_collided = true;
            m_pressure = math.min(GameManager.maxPressure, m_pressure);
            m_StuckEvent?.Invoke();
        }
    }

    private void UpdateDirection(Vector2 _dir, Vector2 _currentPos)
    {
        m_changeDirectionEvent?.Invoke(_dir);
        if(m_line.positionCount > 1)  m_line.SetPosition(m_line.positionCount - 1, _currentPos);
        if(m_line.positionCount > 1 && m_balloonBodies.Count > 0) UpdateBodyPosition();
        
        m_currentInflateDir = _dir;
        ++m_line.positionCount;
        if(m_line.positionCount > 1) m_line.SetPosition(m_line.positionCount - 1, _currentPos);
        AddBody();
        UpdateBodyPosition();
        
        m_collided = false;
        Quaternion spriteHeadRotation = Quaternion.Euler(0.0f, 0.0f, Vector2.SignedAngle(Vector2.up, m_currentInflateDir));
        m_head.GetChild(0).localRotation = spriteHeadRotation;
        m_exploseHead.localRotation = spriteHeadRotation;
    }

    public void Explode(bool _reset = true)
    {
        m_explode = true;
        m_ExplodeEvent?.Invoke();
        GameManager.character.ResetParent();
        
        if (m_resetAtExplode && _reset)
        {
            GameManager.instance.Reset();
        }
    }
    
    private void DestroyBalloon(SpriteRenderer _balloon)
    {
        var character = _balloon.gameObject.GetComponentInChildren<Character>();
        if (character) character.ResetParent();
        m_balloonBodies.Remove(_balloon);
        Destroy(_balloon.gameObject);
    }

    public void SaveState()
    {
        m_saveBodyId = m_balloonBodies.Count;
        m_saveLineId = m_line.positionCount;
        m_saveDir = m_currentInflateDir;
        m_savePos = m_head.position;
        
        //Debug.Log(m_previousLength + " " + m_currentLength);
        m_previousLength += m_currentLength;
        m_currentLength = 0.0f;
        
        UpdateDirection(m_currentInflateDir, m_head.position);
    }
    
    private IEnumerator TryPlayAction(string _name, float _buffer)
    {
        m_animator.SetTrigger(_name);
        yield return new WaitForSeconds(_buffer);
        m_animator.ResetTrigger(_name);
    }
}
