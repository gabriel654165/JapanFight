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

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hand") || collision.gameObject.layer == LayerMask.NameToLayer("Foot"))
        {
            foreach (var box in m_boxToExclude)
                if (collision == box)
                    return;

            switch (collision.transform.root.GetComponent<PlayerInputController>().GetAnimator().GetCurrentAnimatorClipInfo(0)[0].clip.name)
            {
                case "Punch_1":
                    // enlever x points de vie
                    break;
                case "Punch_2":
                    // enlever x points de vie
                    break;
                case "Kick_2":
                    // enlever x points de vie
                    break;
                default:
                    break;
            }
            if (m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "GuardIdle")
                m_animator.SetTrigger("BlockWithGuard");
            else
                m_animator.SetTrigger(m_part == Part.HIGH ? "HighHit" : "LowHit");
        }
    }
}
