using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    private Rigidbody m_rigidbody;
    private bool m_invertX = false;

    [SerializeField] private Animator m_animator;
    [SerializeField] private float m_speed;
    //@debug
    [SerializeField] private bool m_lock = false;
    // private InputMaster m_inputMaster;

    public Animator GetAnimator() {return m_animator;}

    private void Awake()
    {
//        m_inputMaster = new InputMaster();

        m_rigidbody = GetComponent<Rigidbody>();

        // m_inputMaster.Enable();
        // m_inputMaster.Player.Fist_attack_1.performed += Fist_attack_1;
        // m_inputMaster.Player.Fist_attack_2.performed += Fist_attack_2;
        // m_inputMaster.Player.Foot_attack_1.performed += Foot_attack_1;
        // m_inputMaster.Player.Jump.performed += TriggerJump;
        // m_inputMaster.Player.Special_attack.performed += SpecialAttack;
        // m_inputMaster.Player.Dodge.performed += Dodge;
        // m_inputMaster.Player.High_block.performed += Guard;
        // m_inputMaster.Player.High_block.canceled += Guard;
        // m_inputMaster.Player.Movement.performed += Walk;
        // m_inputMaster.Player.Movement.canceled += Walk;
    }

    public void Lock()
    {
        m_lock = true;
    }
    
    public void Unlock()
    {
        m_lock = false;
    }

    public void Walk(InputAction.CallbackContext context)
    {
        if (m_lock)
            return;
        Vector2 analog = context.ReadValue<Vector2>();

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
        if (m_lock)
            return;
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
        if (m_lock)
            return;
        if(context.performed)
        {
            m_animator.SetTrigger("Dodge");
        }
    }

    public void SpecialAttack(InputAction.CallbackContext context)
    {
        if (m_lock)
            return;
        if(context.performed)
        {
            m_animator.SetTrigger("SpecialAttack");
        }
    }

    public void Fist_attack_2(InputAction.CallbackContext context)
    {
        if (m_lock)
            return;
        if (context.performed)
        {
            m_animator.SetTrigger("Punch_1");
        }
    }

    public void Fist_attack_1(InputAction.CallbackContext context)
    {
        if (m_lock)
            return;
        if (context.performed)
        {
            m_animator.SetTrigger("Punch_2");
        }
    }

    public void Foot_attack_1(InputAction.CallbackContext context)
    {
        if (m_lock)
            return;
        if (context.performed)
        {
            m_animator.SetTrigger("Kick_2");
        }
    }

    public void TriggerJump(InputAction.CallbackContext context)
    {
        if (m_lock)
            return;
        if (m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "WalkForward"
            || m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "WalkBackward"
            || m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Idle")
        {
            m_animator.SetTrigger("Jump");
        }
    }

    private void Update()
    {
        if (m_lock)
            return;
        if (m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "WalkForward" || m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "WalkBackward")
        {
            Vector2 move = GetComponent<PlayerInput>().actions["Movement"].ReadValue<Vector2>();
            transform.Translate(0, 0, ((m_invertX ? -move.x : move.x) * m_speed) * Time.deltaTime);
        }
    }

    public void InvertX(bool shouldInvert)
    {
        m_invertX = shouldInvert;
    }
}
