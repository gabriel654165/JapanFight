using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float m_maxHealth = 200f;
    /*private*/public float m_health = 0;
    private bool m_isDead = false;

    void Start()
    {
        m_health = m_maxHealth;
    }

    public void Hit(float damages)
    {
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

    public bool isDead()
    {
        return m_isDead;
    }
}