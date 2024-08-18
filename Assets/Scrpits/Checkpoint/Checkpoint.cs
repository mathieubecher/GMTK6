using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform m_spawnPoint;
    [SerializeField] private List<Interactive> m_interactives;
    private Animator m_animator;
    private bool m_activated;
    public Vector2 spawnPos => m_spawnPoint.position;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    public void Activate(bool _saveState = true)
    {
        if (m_activated) return;

        m_activated = true;
        m_animator.SetTrigger("Activate");
        GameManager.currentCheckpoint = this;
        if(_saveState) GameManager.mainBalloon.SaveState();
    }
    
    public void Reset()
    {
        foreach (Interactive interactive in m_interactives)
        {
            interactive.Reset();
        }
    }
}
