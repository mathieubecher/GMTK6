using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Locomotion
{
    public class Jump : StateMachineBehaviour
    {
        private Character m_character;
        private float m_timer;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_character = animator.GetComponent<Character>();
            m_timer = 0.0f;
            animator.SetFloat("currentTimer", 0.0f);
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_timer += Time.deltaTime;
            animator.SetFloat("currentTimer", m_timer);
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            
        }
    }
}