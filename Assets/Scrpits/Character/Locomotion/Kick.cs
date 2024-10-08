using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Kick : StateMachineBehaviour
{
    [SerializeField] private Vector2 m_kickOffset;
    [SerializeField] private float m_kickDist = 1.5f;
    [SerializeField] private float m_kickTime = 0.3f;
    [SerializeField] private float m_castRadius = 0.5f;
    [SerializeField] private LayerMask m_layermask;
    
    private Character m_character;
    private float m_timer;
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_character = animator.GetComponent<Character>();
        m_timer = 0.0f;
        animator.SetFloat("currentTimer", 0.0f);
            
        m_character.gravityScale = 0.0f;
        m_character.velocity = Vector2.zero;
        
        if(math.abs(Controller.instance.tilt) > 0.01f)
            m_character.animation.transform.localScale = new Vector3(math.sign(Controller.instance.tilt), 1.0f, 1.0f);
        m_character.animation.SetTrigger("Kick");
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_kickTime - m_timer > 0.0f && m_kickTime - m_timer <= Time.deltaTime)
        {
            if(math.abs(Controller.instance.tilt) > 0.01f)
                m_character.animation.transform.localScale = new Vector3(math.sign(Controller.instance.tilt), 1.0f, 1.0f);
            
            float orientation = m_character.animation.transform.localScale.x;
            Vector2 origin = (Vector2)animator.transform.position + new Vector2(m_kickOffset.x * math.sign(orientation), m_kickOffset.y);
            Vector2 dir = m_character.animation.transform.localScale.x > 0.0f ? Vector2.right : Vector2.left;
            RaycastHit2D hit = Physics2D.CircleCast(
                origin,
                m_castRadius,
                dir, 
                m_kickDist,
                m_layermask
            );
            
            //Color color = Color.red;
            if (hit.collider != null)
            {
                if (GameManager.IsBalloonHead(hit.collider.gameObject.layer) 
                    && Vector2.Dot(dir,hit.collider.transform.position - m_character.transform.position) > 0.0f)
                {
                    //color = Color.green;
                    if (hit.collider.transform.parent.TryGetComponent(out Balloon ballon))
                    {
                        ballon.Hit(dir);
                    }
                }
            }
            
            //Debug.DrawLine(Vector2.up * m_castRadius + origin, Vector2.up * m_castRadius + origin + dir * m_kickDist, color, 1.0f);
            //Debug.DrawLine(Vector2.down * m_castRadius + origin, Vector2.down * m_castRadius + origin + dir * m_kickDist, color, 1.0f);
            //Debug.DrawLine(origin - dir * m_castRadius, origin + dir * (m_kickDist + m_castRadius), color, 1.0f);
        }
        m_timer += Time.deltaTime;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
            
    }
}
