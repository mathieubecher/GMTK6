using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectCheckpoint : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (GameManager.IsCheckpoint(_other.gameObject.layer))
        {
            _other.transform.GetComponentInParent<Checkpoint>().Activate();
        }
    }
}
