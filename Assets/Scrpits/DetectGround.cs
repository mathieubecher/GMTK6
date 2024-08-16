using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectGround : MonoBehaviour
{
    [SerializeField] private float coyoteeTime = 1f;
    [SerializeField] private List<Collider2D> _contacts;
    private float _coyoteeTimer = 0.0f;
    public bool OnGround(){return (_contacts != null && _contacts.Count > 0) || _coyoteeTimer < coyoteeTime;}

    void Awake()
    {
        _contacts = new List<Collider2D>();
        _coyoteeTimer = coyoteeTime;
    }

    void Update()
    {
        if (_contacts.Count == 0 && _coyoteeTimer < coyoteeTime)
            _coyoteeTimer += Time.deltaTime;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger) return;
        _contacts.Add(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.isTrigger) return;
        _contacts.Remove(other);
        if (_contacts.Count == 0)
            _coyoteeTimer = 0.0f;
    }
}