using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform m_spawnPoint;
    [SerializeField] private List<Balloon> m_balloons;
    private Animator m_animator;
    private bool m_activated;
    public Vector2 spawnPos => m_spawnPoint.position;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    public void Activate()
    {
        if (m_activated) return;

        m_activated = true;
        m_animator.SetTrigger("Activate");
        GameManager.currentCheckpoint = this;
        GameManager.mainBalloon.SaveState();
    }
    
    public void Reset()
    {
        foreach (Balloon balloon in m_balloons)
        {
            balloon.Reset();
        }
    }
}
