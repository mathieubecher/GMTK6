using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform m_spawnPoint;
    [SerializeField] private List<Balloon> m_balloons;
    public Vector2 spawnPos => m_spawnPoint.position;

    public void Reset()
    {
        foreach (Balloon balloon in m_balloons)
        {
            balloon.Reset();
        }
    }
}
