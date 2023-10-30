using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private GameManager m_gameManagerInstance;

    void Start()
    {
        m_health = transform.root.GetComponent<Health>();
        m_gameManagerInstance = (GameManager)FindObjectOfType<GameManager>().GetComponent<GameManager>();
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hand") || collision.gameObject.layer == LayerMask.NameToLayer("Foot"))
        {
            foreach (var box in m_boxToExclude)
                if (collision == box)
                    return;

            m_gameManagerInstance.HandleHitCallBack();
            switch (collision.transform.root.GetComponent<PlayerInputController>().GetAnimator().GetCurrentAnimatorClipInfo(0)[0].clip.name)
            {
                case "Punch":
                    if (!m_health.isDead()) {
                        var currentAnim = m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

                        //Debug.Log("Current animation = " + currentAnim);

                        if (!(currentAnim == "HighHit" || currentAnim == "LowHit")) {
                            m_health.Hit(10f);
                            if (m_health.isDead()) {
                                ChooseDeathAnim(currentAnim);
                            } else {
                                m_animator.SetTrigger(m_part == Part.HIGH ? "HighHit" : "LowHit");
                            }
                        }
                    }
                    break;
                case "ZombiePunch":
                    if (!m_health.isDead()) {
                        var currentAnim = m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

                        if (!(currentAnim == "HighHit" || currentAnim == "LowHit")) {
                            m_health.Hit(10f);
                            if (m_health.isDead()) {
                                ChooseDeathAnim(currentAnim);
                            } else {
                                m_animator.SetTrigger(m_part == Part.HIGH ? "HighHit" : "LowHit");
                            }
                        }
                    }
                    break;
                case "BarbareKick":
                    if (!m_health.isDead()) {
                        var currentAnim = m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

                        if (!(currentAnim == "HighHit" || currentAnim == "LowHit")) {
                            m_health.Hit(7f);
                            if (m_health.isDead()) {
                                ChooseDeathAnim(currentAnim);
                            } else {
                                m_animator.SetTrigger(m_part == Part.HIGH ? "HighHit" : "LowHit");
                            }
                        }
                    }
                    break;
                case "HighKick":
                    if (!m_health.isDead()) {
                        var currentAnim = m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

                        if (!(currentAnim == "HighHit" || currentAnim == "LowHit")) {
                            m_health.Hit(12f);
                            if (m_health.isDead()) {
                                ChooseDeathAnim(currentAnim);
                            } else {
                                m_animator.SetTrigger(m_part == Part.HIGH ? "HighHit" : "LowHit");
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            if (m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "GuardIdle") {
                m_animator.SetTrigger("BlockWithGuard");
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
