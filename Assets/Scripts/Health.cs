using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float m_maxHealth = 200f;
    private float m_health = 0;
    private bool m_isDead = false;

    void Awake()
    {
        m_health = m_maxHealth;
    }

    public void SetHealth(float health)
    {
        m_health = health;
    }

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

    public float GetHealth()
    {
        return m_health;
    }

    public float GetMaxHealth()
    {
        return m_maxHealth;
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