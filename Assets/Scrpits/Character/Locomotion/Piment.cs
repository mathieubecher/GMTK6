using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class Piment : StateMachineBehaviour
{
    [SerializeField] private float m_castRadius = 4.0f;
    [SerializeField] private Vector2 m_castOffset;
    [SerializeField] private float m_inflateSpeed = 1.0f;
    [SerializeField] private LayerMask m_layermask;
    
    private Character m_character;
    private float m_timer;
        
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_character = animator.GetComponent<Character>();
        m_timer = 0.0f;
        m_character.animation.SetBool("piment", true);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_timer += Time.deltaTime;
        
        float orientation = m_character.animation.transform.localScale.x;
        Vector2 origin = (Vector2)animator.transform.position + new Vector2(m_castOffset.x * math.sign(orientation), m_castOffset.y);
        
        Vector2 dir = m_character.animation.transform.localScale.x > 0.0f ? Vector2.right : Vector2.left;
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            origin,
            m_castRadius,
            Vector2.zero,
            0.0f,
            m_layermask
        );
        foreach (var hit in hits)
        {
            var balloon = hit.transform.GetComponentInParent<Balloon>();
            balloon.Inflate(m_inflateSpeed * Time.deltaTime, 1000000.0f);
        }
        

    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_character.animation.SetBool("piment", false);
    }
}
