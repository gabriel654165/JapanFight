using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MonoBehaviour
{
    [SerializeField] private Rigidbody m_rigidbody;
    [SerializeField] private float m_jumpSpeed;

    public void ComputeJump()
    {
        //m_footCollider.isTrigger = true;
        m_jumpSpeed = 6.5f;
        m_rigidbody.AddForce(new Vector3(0, m_jumpSpeed, 0), ForceMode.Impulse);
    }

    public void ResetJump()
    {
        GetComponent<Animator>().ResetTrigger("Jump");
    }
}


// comme on a un collider au pied droit
// quand on fait une animation ou le pied droit se souleve le player rentre dans le sol
// TODO : 
//  Soit mettre un collider sous son cul qui bouge pas en fonction des animations
//   -> soucis -> des fois les qui isGrounded sera trop haut et il groundera jamais
//   -> SOLUTION mettre un collider istrigger un peu plus bas du collider qui collide et utiliser les deux : un pour collide et un pour grounded
//  Soit utiliser un raycast
