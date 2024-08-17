using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DetectGround : MonoBehaviour
{
    [SerializeField] private List<Collider2D> m_contacts;

    private Character m_character;
    public bool OnGround(){return (m_contacts != null && m_contacts.Count > 0);}

    void Awake()
    {
        m_character = transform.parent.GetComponent<Character>();
        m_contacts = new List<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger) return;
        
        m_contacts.Add(other);
        
        if(!GameManager.IsBalloon(other.gameObject.layer))
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
        
        if(transform.parent.parent == other.transform )
        {
            transform.parent.parent = null;
        }
        
        m_contacts.Remove(other);
    }

    public void ForceAir()
    {
        m_contacts = new List<Collider2D>();
        transform.parent.parent = null;
        
    }
}