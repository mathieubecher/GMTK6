using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Locomotion
{
    public class Jump : StateMachineBehaviour
    {
        [SerializeField] private AnimationCurve m_jumpPositionOverTime;
        [SerializeField] private float m_jumpHeight = 10.0f;
        
        private Character m_character;
        private float m_timer;
        private float m_lastPos;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_character = animator.GetComponent<Character>();
            m_timer = 0.0f;
            animator.SetFloat("currentTimer", 0.0f);

            m_character.IgnoreKoyoteeTime();
            m_character.gravityScale = 0.0f;
            m_lastPos = 0.0f;
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_timer += Time.deltaTime;
            animator.SetFloat("currentTimer", m_timer);
            
            float desiredPos = m_jumpPositionOverTime.Evaluate(m_timer) * m_jumpHeight;

            float desiredVerticalSpeed = Time.deltaTime > 0.0f? (desiredPos - m_lastPos) / Time.deltaTime : 0.0f;
            m_lastPos = desiredPos;
            m_character.velocity = new Vector2(m_character.velocity.x, desiredVerticalSpeed);
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_character.gravityScale = 1.0f;
        }
    }
}