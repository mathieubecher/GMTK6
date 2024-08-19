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
        [SerializeField] private float m_jumpHeight = 10.0f;
        [SerializeField] private Vector2 m_counterJumpForce;
        [SerializeField] private float m_minHeldDuration = 0.1f;
        bool jumpKeyHeld = false;
        
        private Character m_character;
        private float m_timer;
        
        public static float CalculateJumpForce(float gravityStrength, float jumpHeight)
        {
            //h = v^2/2g
            //2gh = v^2
            //sqrt(2gh) = v
            return math.sqrt(2 * gravityStrength * jumpHeight);
        }

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_character = animator.GetComponent<Character>();
            m_timer = 0.0f;
            animator.SetFloat("currentTimer", 0.0f);
            jumpKeyHeld = animator.GetBool("holdJump");
            
            float jumpForce = CalculateJumpForce(Physics2D.gravity.magnitude, m_jumpHeight);
            m_character.rigidbody.AddForce(Vector2.up * jumpForce * m_character.rigidbody.mass, ForceMode2D.Impulse);
            m_character.velocity = new Vector2(m_maxSpeed * animator.GetFloat("tilt"), m_character.velocity.y);
            
            m_character.animation.SetTrigger("Jump");
            m_character.animation.SetBool("inAir", true);
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_timer += Time.deltaTime;
            animator.SetFloat("currentTimer", m_timer);
            jumpKeyHeld &= animator.GetBool("holdJump");

            
            Vector2 currentSpeed = m_character.velocity;
            
            float desiredSpeed = m_maxSpeed * animator.GetFloat("tilt");
            float horizontalSpeed = ComputeAirControl(m_accel, currentSpeed.x, desiredSpeed);
            
            
            if(!jumpKeyHeld && currentSpeed.y > 0 && m_timer > m_minHeldDuration)
            {
                m_character.rigidbody.AddForce(m_counterJumpForce * m_character.rigidbody.mass);
            }
            m_character.velocity = new Vector2(horizontalSpeed, m_character.velocity.y);
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