using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleHit : MonoBehaviour
{
    [SerializeField] private Part m_part;
    [SerializeField] private Animator m_animator;
    [SerializeField] private Collider[] m_boxToExclude;
    [SerializeField] private Collider m_colliderToMaintain;
    private Health m_health;
    private HandleHitManager m_handleHitManager; 
    private GameManager m_gameManagerInstance;
    [SerializeField] private List<HitProperty> m_hitPropertyList;
    
    void Awake()
    {
        m_handleHitManager = transform.parent.GetComponent<HandleHitManager>();
    }

    void Start()
    {
        m_health = transform.root.GetComponent<Health>();
        m_gameManagerInstance = (GameManager)FindObjectOfType<GameManager>().GetComponent<GameManager>();

        // @note: init hitproperties hardly
        m_hitPropertyList = new List<HitProperty> {
            new HitProperty("Punch", 10f, 1f),
            new HitProperty("ZombiePunch", 10f, 1f),
            new HitProperty("BarbareKick", 7f, 1f),
            new HitProperty("HighKick", 12f, 1f),
            new HitProperty("Flying Kick", 30f, 1f),
            new HitProperty("Armada", 20f, 1f),
            new HitProperty("Uppercut Big", 30f, 1f),
            new HitProperty("Kamehameha", 50f, 1f),
            new HitProperty("Sword Swoosh Insane", 30f, 1f),
        };
    }

    private void HandleHitLogic(int indexHitProperty, bool specialPower = false)
    {
        if (!m_health.isDead()) {
            var currentAnim = m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            
            if (!(currentAnim == "HighHit" || currentAnim == "LowHit" || currentAnim == "Sweep Fall")) {
                m_handleHitManager.SetCoolDownTime(m_hitPropertyList[indexHitProperty].coolDownUntilNextHit);
                m_handleHitManager.Hited();
                m_health.Hit(m_hitPropertyList[indexHitProperty].damage);
                if (m_health.isDead()) {
                    ChooseDeathAnim(currentAnim);
                } else {
                    if (specialPower)
                        m_animator.SetTrigger("SpecialHit");
                    else
                        m_animator.SetTrigger(m_part == Part.HIGH ? "HighHit" : "LowHit");
                }
            }
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        // @note: cooldown between hits
        if (m_handleHitManager.HasBeenHited()) {
            return;
        }
        int collisionLayer = collision.gameObject.layer;

        if (collisionLayer == LayerMask.NameToLayer("Hand") || collisionLayer == LayerMask.NameToLayer("Foot") || collisionLayer == LayerMask.NameToLayer("SpecialAttack")) {
            foreach (var box in m_boxToExclude)
                if (collision == box || collisionLayer == LayerMask.NameToLayer("Wall"))
                    return;

            switch (collision.transform.root.GetComponent<PlayerInputController>().GetAnimator().GetCurrentAnimatorClipInfo(0)[0].clip.name)
            {
                case "Punch":
                    HandleHitLogic(0);
                    break;
                case "ZombiePunch":
                    HandleHitLogic(1);
                    break;
                case "BarbareKick":
                    HandleHitLogic(2);
                    break;
                case "HighKick":
                    HandleHitLogic(3);
                    break;
                case "Flying Kick":
                    HandleHitLogic(4, true);
                    break;
                case "Armada":
                    HandleHitLogic(5, true);
                    break;
                case "Uppercut Big":
                    HandleHitLogic(6, true);
                    break;
                case "Kamehameha":
                    // @note: for special powers hit with a external obj (not hand or foot)
                    if (collisionLayer != LayerMask.NameToLayer("SpecialAttack"))
                        return;
                    HandleHitLogic(7, true);
                    break;
                case "Sword Swoosh Insane":
                    // @note: for special powers hit with a external obj (not hand or foot)
                    if (collisionLayer != LayerMask.NameToLayer("SpecialAttack"))
                        return;
                    HandleHitLogic(8, true);
                    break;
                default:
                    break;
            }
            if (m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "GuardIdle") {
                m_animator.SetTrigger("BlockWithGuard");
            }
            if (m_handleHitManager.HasBeenHited()) {
                m_gameManagerInstance.HandleHitCallBack();
            }
        }
    }

    private void ChooseDeathAnim(string currentAnimName)
    {
        if (currentAnimName != "Brutal Assassination" && currentAnimName != "Death") {
            // @note: to make the chracter able to toutch the ground, the collider goes up
            Vector3 localPos = m_colliderToMaintain.gameObject.transform.localPosition;
            
            m_colliderToMaintain.gameObject.transform.localPosition = new Vector3(localPos.x, localPos.y + 1, localPos.z);
            // @note: random anim between 2 animations of death
            var randomIndex = Random.Range(0, 4);
            m_animator.SetInteger("Die", randomIndex);
        }
    }
}
