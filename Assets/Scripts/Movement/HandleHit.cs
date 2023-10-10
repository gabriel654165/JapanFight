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
    private Health m_health;

    void Start()
    {
        m_health = transform.parent.parent.parent.GetComponent<Health>();
    }

    void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hand") || collision.gameObject.layer == LayerMask.NameToLayer("Foot"))
        {
            foreach (var box in m_boxToExclude)
                if (collision == box)
                    return;

            switch (collision.transform.root.GetComponent<PlayerInputController>().GetAnimator().GetCurrentAnimatorClipInfo(0)[0].clip.name)
            {
                case "Punch":
                    if (!m_health.isDead()) {
                        var currentAnim = m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

                        if (currentAnim != "HighHit" && currentAnim != "LowHit") {
                            m_health.Hit(20f);
                            if (m_health.isDead()) {
                                Debug.Log("Is DEAD");
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

                        if (currentAnim != "HighHit" && currentAnim != "LowHit") {
                            m_health.Hit(40f);
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

                        if (currentAnim != "HighHit" && currentAnim != "LowHit") {
                            m_health.Hit(40f);
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
            // @note: random anim between 2 animations of death
            var randomIndex = Random.Range(0, 2);
            m_animator.SetInteger("Die", randomIndex);
        }
    }
}
