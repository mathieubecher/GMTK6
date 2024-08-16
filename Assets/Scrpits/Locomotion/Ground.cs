using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Locomotion
{
    public class Ground : StateMachineBehaviour
    {
        const float TOLERANCE = 1e-6f;
        
        [SerializeField] private float m_runSpeed = 5.0f;
        [SerializeField] private AnimationCurve m_accel;
        [SerializeField] private AnimationCurve m_decel;
        
        private Character m_character;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_character = animator.GetComponent<Character>();
            animator.SetFloat("currentTimer", 0.0f);
            
            m_character.gravityScale = 1.0f;
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            float currentSpeed = m_character.velocity.x;
            float desiredSpeed = m_runSpeed * Controller.instance.tilt;
            float speed = currentSpeed;

            if (math.abs(desiredSpeed) > 0.01f && math.abs(math.sign(currentSpeed) - math.sign(desiredSpeed)) > TOLERANCE)
            {
                currentSpeed = 0.0f;
            }

            if (math.abs(math.abs(desiredSpeed) - math.abs(currentSpeed)) >= TOLERANCE)
            {
                if(math.abs(desiredSpeed) > math.abs(currentSpeed))
                {
                    float timeInCurve = GameManager.TimeFromValue(m_accel, math.abs(currentSpeed / m_runSpeed));
                    speed = m_accel.Evaluate(timeInCurve + Time.deltaTime) * m_runSpeed * math.sign(desiredSpeed);
                    if (math.abs(speed) >= math.abs(desiredSpeed)) 
                        speed = desiredSpeed;
                
                    //Debug.Log("Accel -> current : " + currentSpeed + ", desired : " + desiredSpeed + ", time : " + timeInCurve + ", final : " + speed);
                }
                else
                {
                    float timeInCurve = GameManager.TimeFromValue(m_decel, math.abs(currentSpeed / m_runSpeed));
                    speed = m_decel.Evaluate(timeInCurve - Time.deltaTime) * m_runSpeed * math.sign(currentSpeed);
                
                    if (math.abs(speed) <= math.abs(desiredSpeed)) 
                        speed = desiredSpeed;
                
                    //Debug.Log("Decel -> current : " + currentSpeed + ", desired : " + desiredSpeed + ", time : " + timeInCurve + ", final : " + speed);
                }

            }
            
            m_character.velocity = new Vector2(speed, m_character.velocity.y);
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            
        }
    }
}