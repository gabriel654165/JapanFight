using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PunchProperty {
    
    public string animationName { get; set; }
    public float powerToAdd { get; set; }

    public PunchProperty(string animName, float pwr) {
        animationName = animName;
        powerToAdd = pwr;
    }
}

public class HandlePunch : MonoBehaviour
{
    [SerializeField] private Animator m_animator;
    [SerializeField] private Collider[] m_boxToExclude;
    private Power m_power;
    private bool m_hasLandPunch = false;
    private PlayerInputController m_playerInputController;
    private GameManager m_gameManagerInstance;

    private List<PunchProperty> m_PunchPropertyList = new List<PunchProperty> {
        new PunchProperty("Punch", 5f),
        new PunchProperty("ZombiePunch", 7f),
        new PunchProperty("BarbareKick", 5f),
        new PunchProperty("HighKick", 7f),
    };

    void Start()
    {
        m_power = transform.root.GetComponent<Power>();
        m_playerInputController = transform.root.GetComponent<PlayerInputController>();
        m_gameManagerInstance = (GameManager)FindObjectOfType<GameManager>().GetComponent<GameManager>();
    }

    private void HandlePunchLogic(int indexPunchProperty, Collider collision, bool specialPower = false)
    {
        if (collision?.GetComponent<HandleHit>().GetAnimator()?.GetCurrentAnimatorClipInfo(0)[0].clip.name == "GuardIdle" && !specialPower) {
            return;
        }
        
        if (!m_hasLandPunch) {
            m_power.AddPowerCharge(m_PunchPropertyList[indexPunchProperty].powerToAdd);
            m_hasLandPunch = true;
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("HitBox") || collision.gameObject.layer == LayerMask.NameToLayer("HitBox"))
        {
            foreach (var box in m_boxToExclude)
                if (collision == box || collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
                    return;

            switch (m_playerInputController.GetAnimator().GetCurrentAnimatorClipInfo(0)[0].clip.name)
            {
                case "Punch":
                    HandlePunchLogic(0, collision);
                    break;
                case "ZombiePunch":
                    HandlePunchLogic(1, collision);
                    break;
                case "BarbareKick":
                    HandlePunchLogic(2, collision);
                    break;
                case "HighKick":
                    HandlePunchLogic(3, collision);
                    break;
                case "Flying Kick":
                    // @todo: callback special power canvas (effect, idk which)
                    break;
                case "Armada":
                    // @todo: callback special power canvas (effect, idk which)
                    break;
                case "Uppercut Big":
                    // @todo: callback special power canvas (effect, idk which)
                    break;
                case "Kamehameha":
                    // @todo: callback special power canvas (effect, idk which)
                    break;
                case "Sword Swoosh Insane":
                    // @todo: callback special power canvas (effect, idk which)
                    break;
                default:
                    break;
            }

            if (m_hasLandPunch) {
                m_gameManagerInstance.HandlePunchCallBack();
                m_hasLandPunch = false;
            }
        }
    }
}