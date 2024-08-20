using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Locomotion
{
    public class ElbowDrop : StateMachineBehaviour
    {
        [SerializeField] private AnimationCurve m_speedOverTime;
        [SerializeField] private float m_elbowDropFallSpeed = 10.0f;
        [SerializeField] private float m_friction = 0.2f;
        
        private Character m_character;
        private float m_startPos;
        private float m_timer;
        
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_character = animator.GetComponent<Character>();
            animator.SetFloat("currentTimer", 0.0f);

            m_timer = 0.0f;
            m_character.gravityScale = 0.0f;
            m_startPos = animator.transform.position.y;
            
            m_character.animation.SetTrigger("ElbowDrop");
            m_character.RequestCharacterDown();
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_timer += Time.deltaTime;
            float desiredVerticalSpeed = m_speedOverTime.Evaluate(m_timer) * m_elbowDropFallSpeed;
            m_character.velocity = new Vector2(m_character.velocity.x * m_friction, desiredVerticalSpeed);
            
            float currentPos = animator.transform.position.y;
            animator.SetFloat("elbowDropHeight", m_startPos - currentPos);
        }
        
        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_character.gravityScale = 1.0f;
            
            m_character.animation.ResetTrigger("ElbowDrop");
            m_character.FinishCharacterDown();
        }
    }
}