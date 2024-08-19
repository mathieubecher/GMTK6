using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SpeakingBalloon : MonoBehaviour
{
    [SerializeField] private GameObject m_dialogPrefab;
    [SerializeField] private Transform m_head;
    [SerializeField] private Sprite m_normalHead;
    [SerializeField] private Sprite m_angryHead;
    [SerializeField] private Sprite m_stressHead;
    [SerializeField] private Sprite m_inflatedHead;

    private Dialog m_currentDialog;
    
    private bool m_canTalk = true;
    
    public void OnInflate(float _value)
    {
        Talk("Hummm, yeeeeees, mooooooore. \n I feel it growing inside.");
    }

    public void OnStuck()
    {
        m_head.GetComponentInChildren<SpriteRenderer>().sprite = m_inflatedHead;
        Talk("Stepbro! I'm stuck!", true);
    }

    public void OnChangedDirection()
    {
        m_head.GetComponentInChildren<SpriteRenderer>().sprite = m_normalHead;
    }
    
    private bool Talk(String _text, bool _forceTalk = false)
    {
        if (!m_canTalk && !_forceTalk) return false;
        
        if (m_currentDialog != null)
        {
            Destroy(m_currentDialog.gameObject);
            m_currentDialog = null;
        }
        
        m_canTalk = false;
        GameObject instance = Instantiate(m_dialogPrefab, m_head);
        m_currentDialog = instance.GetComponent<Dialog>();
        m_currentDialog.Init(m_head.position, _text);
        StartCoroutine(DestroyAtTime(3.0f));
        return true;
        
    }

    private IEnumerator DestroyAtTime(float _duration)
    {
        yield return new WaitForSeconds(_duration);
        
        if (m_currentDialog)
        {
            Destroy(m_currentDialog.gameObject);
            m_currentDialog = null;
            m_canTalk = true;
        }
    }
}
