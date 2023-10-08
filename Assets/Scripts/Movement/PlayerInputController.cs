using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    private InputMaster m_inputMaster;
    private Rigidbody m_rigidbody;
    [SerializeField] private Animator m_animator;
    [SerializeField] private float m_speed;

    private void Awake()
    {
        m_inputMaster = new InputMaster();
        m_rigidbody = GetComponent<Rigidbody>();

        m_inputMaster.Enable();
        m_inputMaster.Player.Fist_attack_1.performed += Fist_attack_1;
        m_inputMaster.Player.Fist_attack_2.performed += Fist_attack_2;
        m_inputMaster.Player.Foot_attack_1.performed += Foot_attack_1;
        m_inputMaster.Player.Jump.performed += TriggerJump;
        m_inputMaster.Player.Special_attack.performed += SpecialAttack;
        m_inputMaster.Player.Dodge.performed += Dodge;
        m_inputMaster.Player.High_block.performed += Guard;
        m_inputMaster.Player.High_block.canceled += Guard;
        m_inputMaster.Player.Movement.performed += Walk;
        m_inputMaster.Player.Movement.canceled += Walk;
    }

    public void Walk(InputAction.CallbackContext context)
    {
        Vector2 analog = m_inputMaster.Player.Movement.ReadValue<Vector2>();

        if(context.performed && analog.x != 0)
        {
            m_animator.SetBool(analog.x > 0 ? "WalkForward" : "WalkBackward", true);
            m_animator.SetBool(analog.x > 0 ? "WalkBackward" : "WalkForward", false);
        }
        else if (context.canceled || analog.x == 0.0f)
        {
            m_animator.SetBool("WalkForward", false);
            m_animator.SetBool("WalkBackward", false);
        }
    }

    public void Guard(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            m_animator.SetBool("GuardIdle", true);
        }
        else if (context.canceled)
        {
            m_animator.SetBool("GuardIdle", false);
        }
    }

    public void Dodge(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            m_animator.SetTrigger("Dodge");
        }
    }

    public void SpecialAttack(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            m_animator.SetTrigger("SpecialAttack");
        }
    }

    public void Fist_attack_2(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            m_animator.SetTrigger("Punch_1");
        }
    }

    public void Fist_attack_1(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            m_animator.SetTrigger("Punch_2");
        }
    }

    public void Foot_attack_1(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            m_animator.SetTrigger("Kick_1");
        }
    }

    public void TriggerJump(InputAction.CallbackContext context)
    {
        if (m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "WalkForward"
            || m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "WalkBackward"
            || m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Idle")
        {
            m_animator.SetTrigger("Jump");
        }
    }

    private void Update()
    {
        if (m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "WalkForward" || m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "WalkBackward")
        {
            Vector2 move = m_inputMaster.Player.Movement.ReadValue<Vector2>();

            transform.Translate(0, 0, (move.x * m_speed) * Time.deltaTime);
        }
    }
}
