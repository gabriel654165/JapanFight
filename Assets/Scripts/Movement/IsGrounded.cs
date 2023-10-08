using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsGrounded : MonoBehaviour
{
    [SerializeField] private Animator m_animator;
    [SerializeField] private Rigidbody m_rigidbody;
    [SerializeField] private BoxCollider m_footCollider;

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            m_animator.SetBool("Grounded", true);
            m_rigidbody.velocity = new Vector3(m_rigidbody.velocity.x, 0, m_rigidbody.velocity.z);
        }
    }

    void OnTriggerExit(Collider collision)
    {   
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // test (useless for the moment)
            m_footCollider.isTrigger = false;
            m_animator.SetBool("Grounded", false);
        }
    }
}
