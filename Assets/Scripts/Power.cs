using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Power : MonoBehaviour
{
    [SerializeField] private float m_maxCharge = 100f;
    private float m_charge = 0;
    private bool m_isCharged = false;

    public void AddPowerCharge(float powerToAdd)
    {
        if (m_charge + powerToAdd >= m_maxCharge)
        {
            m_charge = m_maxCharge;
            m_isCharged = true;
        } else {
            m_charge += powerToAdd;
        }
    }

    public void ResetPowerCharge()
    {
        m_charge = 0f;
        m_isCharged = false;
    }

    // @note: return power charge out of 1 (not 100)
    public float GetPowerCharge()
    {
        return (m_charge / m_maxCharge);
    }

    public bool isCharged()
    {
        return m_isCharged;
    }
}