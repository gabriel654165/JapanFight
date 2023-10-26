using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float m_maxHealth = 200f;
    private float m_health = 0;
    private bool m_isDead = false;

    void Start()
    {
        m_health = m_maxHealth;
    }

    // @todo: mettre un coolDown avant de prendre un prochain hit ?
    public void Hit(float damages)
    {
        if (m_health - damages < 0)
            m_health = 0;
        else
            m_health -= damages;

        if (m_health <= 0) {
            Die();
        }
    }

    private void Die()
    {
        m_isDead = true;
        return;
    }

    public float GetPrctLeftHealth()
    {
        return (m_health / m_maxHealth) * 100;
    }

    public bool isDead()
    {
        return m_isDead;
    }
}