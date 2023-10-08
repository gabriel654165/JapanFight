using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MonoBehaviour
{
    [SerializeField] private Rigidbody m_rigidbody;
    [SerializeField] private float m_jumpSpeed;
    [SerializeField] private BoxCollider m_footCollider;

    public void ComputeJump()
    {
        m_footCollider.isTrigger = true;
        m_rigidbody.AddForce(new Vector3(0, m_jumpSpeed * Time.deltaTime, 0), ForceMode.Impulse);
    }
}
