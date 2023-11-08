using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MonoBehaviour
{
    [SerializeField] private Rigidbody m_rigidbody;
    [SerializeField] private float m_jumpSpeed;

    public void ComputeJump()
    {
        m_jumpSpeed = 6.5f;
        // @note: cannot move with velocity beacause the player is moving with positions
        m_rigidbody.AddForce(new Vector3(0, m_jumpSpeed, 0), ForceMode.Impulse);
    }

    public void ResetJump()
    {
        GetComponent<Animator>().ResetTrigger("Jump");
    }
}
