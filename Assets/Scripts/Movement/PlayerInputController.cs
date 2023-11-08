using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Power))]
public class PlayerInputController : MonoBehaviour
{
    private Power m_power;
    private GameObject m_effect;

    [Header("Movements")]
    [SerializeField] private float m_speed;
    private Rigidbody m_rigidbody;
    private bool m_invertX = false;
    private bool m_lock = false;

    [Header("Animations")]
    [SerializeField] private Animator m_animator;
    [SerializeField] private Collider m_colliderToMaintain;
    private Quaternion m_initialRotation;

    [Header("Special Effect")]
    [SerializeField] private GameObject m_ballEffectPrefab;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float m_hueEffectSaturation = 0;
    [SerializeField] private float m_effectDuration = 0.5f;
    [SerializeField] private int m_specialPowerIdx = 0;
    private GameManager m_gameManagerInstance;
    private GameObject m_enemy;
    public bool m_isLeft = false;

    // private InputMaster m_inputMaster;

    public Animator GetAnimator() {return m_animator;}

    private void Start()
    {
        m_gameManagerInstance = (GameManager)FindObjectOfType<GameManager>();
        m_initialRotation = transform.rotation;
    }

    public void Init(GameObject enemy, bool isLeft = false)
    {
        m_enemy = enemy;
        m_isLeft = isLeft;
    }

    private void Awake()
    {
//        m_inputMaster = new InputMaster();

        m_rigidbody = GetComponent<Rigidbody>();
        m_power = GetComponent<Power>();

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

    public bool IsLeft()
    {
        return m_isLeft;
    }

    public void SetIsLeft(bool isLeft)
    {
        m_isLeft = isLeft;
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
        if (context.performed)
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
        if (context.performed)
        {
            m_animator.SetTrigger("Dodge");
        }
    }

    public void SpecialAttack(InputAction.CallbackContext context)
    {
        if (m_lock)
            return;
        if (context.performed && m_power.isCharged())
        {
            m_animator.SetInteger("SpecialAttackIdx", m_specialPowerIdx);
            m_animator.SetTrigger("SpecialAttack");
            m_power.ResetPowerCharge();
            m_gameManagerInstance.HandleUsePowerCallBack();
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
            m_animator.SetTrigger("Kick_1");
        }
    }

    public void Foot_attack_2(InputAction.CallbackContext context)
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
        var currentAnimName = m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        if (currentAnimName == "WalkForward" || currentAnimName == "WalkBackward"
           || currentAnimName == "JumpUp" || currentAnimName == "JumpDown")
        {
            Vector2 move = GetComponent<PlayerInput>().actions["Movement"].ReadValue<Vector2>();
            transform.Translate(0, 0, ((m_invertX ? -move.x : move.x) * m_speed) * Time.deltaTime);
        }
    }

    public void InvertX(bool shouldInvert)
    {
        m_invertX = shouldInvert;
    }

    #region ANIMATION EFFECTS

    // @todo: rename with CamelCase
    public void spawnEffect(float posToAddY)
    {
        if (m_ballEffectPrefab == null)
            return;
        m_effect = Instantiate(m_ballEffectPrefab);
        m_effect.transform.SetParent(transform);
        // @todo: if the enemy is behind, the player is gonna launch the effect behind him
        m_effect.transform.rotation = Quaternion.LookRotation(m_enemy.transform.position - transform.position);
        m_effect.transform.position = new Vector3(transform.position.x, transform.position.y + posToAddY, transform.position.z);
        m_effect.transform.GetComponent<RedHollowControl>().hue = m_hueEffectSaturation;
        m_effect.transform.GetComponent<Rigidbody>().isKinematic = true;
        m_effect.transform.transform.GetChild(0).GetComponent<Animator>().Play("Red Hollow - Charging");

    }
    public void ActivateEffect()
    {
        if (m_effect == null)
            return;
        // @todo: if the enemy is behind, the player is gonna launch the effect behind him
        m_effect.transform.rotation = Quaternion.LookRotation(m_enemy.transform.position - transform.position);
        m_effect.transform.position = new Vector3(m_effect.transform.position.x + (m_isLeft ? 1f : -1f), m_effect.transform.position.y + 0.5f, m_effect.transform.position.z);
        m_effect.transform.transform.GetChild(0).GetComponent<Animator>().Play("Red Hollow - Charged");
        m_effect.transform.transform.GetChild(0).GetComponent<Animator>().Play("Red Hollow - Burst");
        StartCoroutine(DestroyEffectTimer(m_effect, m_effectDuration));
    }

    //BUG
    public void ResetAnimationEffects()
    {
        if (m_effect != null) {
            Destroy(m_effect);
        }
    }

    private IEnumerator DestroyEffectTimer(GameObject objToDestroy, float time)
    {
        if (objToDestroy == null)
            yield return null;
        yield return new WaitForSeconds(time);
        objToDestroy.transform.GetChild(0).GetComponent<Animator>().Play("Red Hollow - Dead");
        yield return new WaitForSeconds(1f);
        Destroy(objToDestroy);
    }
    #endregion

    #region MOVE ANIMATION CHEAT

    public void SaveRotation()
    {
        m_initialRotation = transform.rotation;
    }
    // @note: set rotation once
    public void SetRotation(float rotationToAddY)
    {
        transform.Rotate(transform.rotation.x, transform.rotation.y + rotationToAddY, transform.rotation.z);
    }
    public void ResetInitialRotation()
    {
        transform.rotation = m_initialRotation;
    }

    // @note: move y position over time // @todo: rename
    public void SetPositionY(float posToAddY)
    {
        float duration = 0.25f;
        StartCoroutine(MovePositionOverTimeY(transform.localPosition.y, transform.localPosition.y + posToAddY, duration));
    }
    // @note: move x position over time // @todo: rename
    public void SetPositionX(float posToAddX)
    {
        float duration = 0.5f;
        StartCoroutine(MovePositionOverTimeX(transform.localPosition.x, transform.localPosition.x + (m_isLeft ? posToAddX : -posToAddX), duration));
    }

    private IEnumerator MovePositionOverTimeX(float srcPosX, float destPosX, float duration)
    {
        float elapsedTime = 0;

        while (elapsedTime <= duration)
        {
            float newPosX = Mathf.Lerp(srcPosX, destPosX, (elapsedTime / duration));
            
            transform.localPosition = new Vector3(newPosX, transform.localPosition.y, transform.localPosition.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    private IEnumerator MovePositionOverTimeY(float srcPosY, float destPosY, float duration)
    {
        float elapsedTime = 0;

        while (elapsedTime <= duration)
        {
            float newPosY = Mathf.Lerp(srcPosY, destPosY, (elapsedTime / duration));
            
            transform.localPosition = new Vector3(transform.localPosition.x, newPosY, transform.localPosition.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    #endregion

    #region REGULAR CALLBACK ANIMATIONS

    public void SetColliderToGround()
    {
        if (m_colliderToMaintain == null)
            return;
        
        Vector3 localPos = m_colliderToMaintain.gameObject.transform.localPosition;
        
        m_colliderToMaintain.gameObject.transform.localPosition = new Vector3(localPos.x, localPos.y + 1, localPos.z);
    }

    // BUG 
    public void ResetColliderToNormal()
    {
        if (m_colliderToMaintain == null)
            return;
        
        Vector3 localPos = m_colliderToMaintain.gameObject.transform.localPosition;
        
        m_colliderToMaintain.gameObject.transform.localPosition = new Vector3(localPos.x, localPos.y - 1, localPos.z);
    }

    #endregion

}
