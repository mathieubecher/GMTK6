using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DetectGround : MonoBehaviour
{
    [SerializeField] private float m_coyoteeTime = 0.033f;
    [SerializeField] private List<Collider2D> m_contacts;
    private float m_coyoteeTimer = 0.0f;

    private Character m_character;
    public bool isOnGround => (m_contacts != null && m_contacts.Count > 0) || m_coyoteeTimer < m_coyoteeTime;

    void Awake()
    {
        m_contacts = new List<Collider2D>();
        m_coyoteeTimer = m_coyoteeTime;
    }

    void Update()
    {
        if (m_contacts.Count == 0 && m_coyoteeTimer < m_coyoteeTime)
            m_coyoteeTimer += Time.deltaTime;
    }
    public void SetCharacter(Character _character)
    {
        m_character = _character;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger) return;
        
        m_contacts.Add(other);
        
        if(GameManager.IsMovablePlatform(other.gameObject.layer))
        {
            transform.parent.parent = other.transform;
        }
        if (GameManager.IsPump(other.gameObject.layer) && other.TryGetComponent(out Pump _pump))
        {
            _pump.Press(m_character.elbowDropHeight);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.isTrigger || m_contacts.Count == 0) return;
        
        if (GameManager.IsPump(other.gameObject.layer) && other.TryGetComponent(out Pump _pump))
        {
            _pump.Release();
        }
        
        if(transform.parent != null && transform.parent.parent != null && transform.parent.parent == other.transform )
        {
            transform.parent.parent = null;
        }
        if (m_contacts.Count == 0)
            m_coyoteeTimer = 0.0f;
        
        m_contacts.Remove(other);
    }

}