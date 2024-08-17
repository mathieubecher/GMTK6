using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Locomotion
{
    public class ElbowDropHitGround : StateMachineBehaviour
    {
        private Character m_character;
        
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_character = animator.GetComponent<Character>();
            animator.SetFloat("currentTimer", 0.0f);
            m_character.gravityScale = 1.0f;
            
            m_character.animation.SetBool("inAir", false);
            m_character.animation.SetFloat("speed", 0.0f);
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetFloat("elbowDropHeight", 0.0f);
        }
    }
}