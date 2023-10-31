using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HitProperty {
    
    public string animationName { get; set; }
    public float damage { get; set; }
    public float coolDownUntilNextHit { get; set; }

    public HitProperty(string animName, float dmg, float cd) {
        animationName = animName;
        damage = dmg;
        coolDownUntilNextHit = cd;
    }
}

//implem cooldown
public class HandleHit : MonoBehaviour
{
    public enum Part {
        HIGH,
        LOW
    }
    [SerializeField] private Part m_part;
    [SerializeField] private Animator m_animator;
    [SerializeField] private Collider[] m_boxToExclude;
    [SerializeField] private Collider m_colliderToMaintain;
    private Health m_health;
    private bool m_hasBeenHited = false;
    private GameManager m_gameManagerInstance;
    
    private List<HitProperty> m_hitPropertyList = new List<HitProperty> {
        new HitProperty("Punch", 10f, 1f),
        new HitProperty("ZombiePunch", 10f, 1f),
        new HitProperty("BarbareKick", 7f, 1f),
        new HitProperty("HighKick", 12f, 1f),
    };
    
    void Start()
    {
        m_health = transform.root.GetComponent<Health>();
        m_gameManagerInstance = (GameManager)FindObjectOfType<GameManager>().GetComponent<GameManager>();
    }

    private void HandleHitLogic(int indexHitProperty)
    {
        m_hasBeenHited = true;
        if (!m_health.isDead()) {
            var currentAnim = m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            //Debug.Log("Current animation = " + currentAnim);
            if (!(currentAnim == "HighHit" || currentAnim == "LowHit")) {
                m_health.Hit(m_hitPropertyList[indexHitProperty].damage);
                if (m_health.isDead()) {
                    ChooseDeathAnim(currentAnim);
                } else {
                    m_animator.SetTrigger(m_part == Part.HIGH ? "HighHit" : "LowHit");
                }
            }
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hand") || collision.gameObject.layer == LayerMask.NameToLayer("Foot"))
        {
            foreach (var box in m_boxToExclude)
                if (collision == box)
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
                default:
                    break;
            }
            if (m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "GuardIdle") {
                m_animator.SetTrigger("BlockWithGuard");
            }
            if (m_hasBeenHited) {
                m_gameManagerInstance.HandleHitCallBack();
                m_hasBeenHited = false;
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
            var randomIndex = Random.Range(0, 2);
            m_animator.SetInteger("Die", randomIndex);
        }
    }
}
