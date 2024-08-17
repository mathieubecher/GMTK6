using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Locomotion
{
    public class Jump : StateMachineBehaviour
    {
        [Header("Air Control")]
        [SerializeField] private float m_accel = 10.0f;
        [SerializeField] private float m_maxSpeed = 8.0f;
        
        [Header("Jump dynamic")]
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
            
            Vector2 currentSpeed = m_character.velocity;
            m_character.velocity = new Vector2(m_maxSpeed * Controller.instance.tilt, currentSpeed.y);
            
            m_character.animation.SetTrigger("Jump");
            m_character.animation.SetBool("inAir", true);
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_timer += Time.deltaTime;
            animator.SetFloat("currentTimer", m_timer);
            
            Vector2 currentSpeed = m_character.velocity;
            float desiredPos = m_jumpPositionOverTime.Evaluate(m_timer) * m_jumpHeight;

            float verticalSpeed = Time.deltaTime > 0.0f? (desiredPos - m_lastPos) / Time.deltaTime : 0.0f;
            m_lastPos = desiredPos;
            
            float desiredSpeed = m_maxSpeed * Controller.instance.tilt;
            float horizontalSpeed = ComputeAirControl(m_accel, currentSpeed.x, desiredSpeed);
            
            m_character.velocity = new Vector2(horizontalSpeed, verticalSpeed);
        }

        private float ComputeAirControl(float _accel, float _currentSpeed, float _desiredSpeed)
        {
            
            float speed = _currentSpeed;
            
            float accel = math.sign(_desiredSpeed - _currentSpeed) * m_accel * Time.deltaTime;
            if (math.abs(_desiredSpeed - _currentSpeed) < math.abs(accel))
            {
                speed = _desiredSpeed;
            }
            else
            {
                speed += accel;
            }

            return speed;
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_character.gravityScale = 1.0f;
            
            m_character.animation.ResetTrigger("Jump");
        }
    }
}