using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HitProperty {
    public HitProperty(string animName, float dmg, float cd) {
        animationName = animName;
        damage = dmg;
        coolDownUntilNextHit = cd;
    }
    public string animationName { get; set; }
    public float damage { get; set; }
    public float coolDownUntilNextHit { get; set; }
}

public enum Part {
    HIGH,
    LOW
}

public class HandleHitManager : MonoBehaviour
{
    private bool m_hasBeenHited = false;
    private float m_coolDownTime = 1f;
    private float m_timer = 0f;

    void Update()
    {
        if (m_timer >= m_coolDownTime) {
            m_hasBeenHited = false;
            m_timer = 0f;
        }
        if (m_hasBeenHited) {
            m_timer += Time.deltaTime;
        }
    }

    public void SetCoolDownTime(float time)
    {
        m_coolDownTime = time;
    }

    public void Hited()
    {
        m_hasBeenHited = true;
    }

    public bool HasBeenHited()
    {
        return m_hasBeenHited;
    }

}