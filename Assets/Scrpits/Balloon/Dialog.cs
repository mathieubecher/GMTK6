using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Dialog : MonoBehaviour
{

    public delegate void SimpleCallback();
    private SimpleCallback m_dialogCallback;	
    
    [SerializeField] private TextMeshPro m_textMesh;
    [SerializeField] private Transform m_triangle;
    [SerializeField, TextArea] private String m_text;
    [SerializeField] private Vector2 m_border;
    [SerializeField] private float m_margin;
    private SpriteRenderer m_box;
    
    void Awake()
    {
        m_box = GetComponent<SpriteRenderer>();
    }

    public void Init(Vector2 _anchor, String _text)
    {
        if (transform.parent)
            transform.localScale = new Vector3(1.0f / transform.parent.lossyScale.x,
                1.0f / transform.parent.lossyScale.y, 1.0f);
        m_text = _text;
        m_textMesh.text = m_text;
        Vector2 size = m_textMesh.GetPreferredValues(m_text);
        m_box.size = size + m_border;
        ((RectTransform)m_textMesh.transform).sizeDelta = size;

        transform.position = _anchor + Vector2.up * (m_box.size.y / 2.0f + m_margin);
    }
}
