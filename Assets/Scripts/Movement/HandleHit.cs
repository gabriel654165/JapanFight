using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleHit : MonoBehaviour
{
    [SerializeField] private Part m_part;
    [SerializeField] private Animator m_animator;
    [SerializeField] private Collider[] m_boxToExclude;
    [SerializeField] private Collider m_colliderToMaintain;
    private Health m_health;
    private HandleHitManager m_handleHitManager; 
    private GameManager m_gameManagerInstance;
    [SerializeField] private List<HitProperty> m_hitPropertyList;
    
    void Awake()
    {
        m_handleHitManager = transform.parent.GetComponent<HandleHitManager>();
    }

    void Start()
    {
        m_health = transform.root.GetComponent<Health>();
        m_gameManagerInstance = (GameManager)FindObjectOfType<GameManager>().GetComponent<GameManager>();

        // @note: init hitproperties hardly
        m_hitPropertyList = new List<HitProperty> {
            new HitProperty("Punch", 10f, 1.5f),
            new HitProperty("ZombiePunch", 10f, 1.5f),
            new HitProperty("BarbareKick", 7f, 1.5f),
            new HitProperty("HighKick", 12f, 1.5f),
            new HitProperty("Flying Kick", 30f, 1.5f),
            new HitProperty("Armada", 20f, 1.5f),
            new HitProperty("Uppercut Big", 30f, 1.5f),
            new HitProperty("Kamehameha", 50f, 1.5f),
            new HitProperty("Sword Swoosh Insane", 30f, 1.5f),
        };
    }

    public Animator GetAnimator() { return m_animator; }

    private void HandleHitLogic(int indexHitProperty, bool specialPower = false)
    {
        var clipName = m_animator?.GetCurrentAnimatorClipInfo(0)[0].clip.name;

        if (clipName == "GuardIdle" && !specialPower) {
            m_animator.SetTrigger("BlockWithGuard");
            return;
        }

        if (clipName == "Dodge" || clipName == "BlockWithGuard")
            return;

        if (!m_health.isDead()) {
            var currentAnim = m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            
            if (!(currentAnim == "HighHit" || currentAnim == "LowHit" || currentAnim == "Sweep Fall" || currentAnim == "Standing Up")) {
                m_handleHitManager.SetCoolDownTime(m_hitPropertyList[indexHitProperty].coolDownUntilNextHit);
                m_handleHitManager.Hited();
                m_health.Hit(m_hitPropertyList[indexHitProperty].damage);
                
                if (m_health.isDead()) {
                    ChooseDeathAnim(currentAnim);
                } else {
                    if (specialPower) {
                        StartCoroutine(LerpColorMaterial(1f));
                        m_animator.SetTrigger("SpecialHit");
                    } else {
                        StartCoroutine(LerpColorMaterial(0.5f));
                        m_animator.SetTrigger(m_part == Part.HIGH ? "HighHit" : "LowHit");
                    }
                }
            }
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        // @note: cooldown between hits
        if (m_handleHitManager.HasBeenHited()) {
            return;
        }
        int collisionLayer = collision.gameObject.layer;

        if (collisionLayer == LayerMask.NameToLayer("Hand") || collisionLayer == LayerMask.NameToLayer("Foot") || collisionLayer == LayerMask.NameToLayer("SpecialAttack")) {
            foreach (var box in m_boxToExclude)
                if (collision == box || collisionLayer == LayerMask.NameToLayer("Wall"))
                    return;

            switch (collision.transform.root.GetComponent<PlayerInputController>().GetAnimator().GetCurrentAnimatorClipInfo(0)[0].clip.name)
            {
                case "Punch":
                    HandleHitLogic(0);
                    break;
                case "ZombiePunch":
                    HandleHitLogic(1);
                    break;
                case "BarbareKick":
                    HandleHitLogic(2);
                    break;
                case "HighKick":
                    HandleHitLogic(3);
                    break;
                case "Flying Kick":
                    HandleHitLogic(4, true);
                    break;
                case "Armada":
                    HandleHitLogic(5, true);
                    break;
                case "Uppercut Big":
                    HandleHitLogic(6, true);
                    break;
                case "Kamehameha":
                    // @note: for special powers hit with a external obj (not hand or foot)
                    if (collisionLayer != LayerMask.NameToLayer("SpecialAttack"))
                        return;
                    HandleHitLogic(7, true);
                    break;
                case "Sword Swoosh Insane":
                    // @note: for special powers hit with a external obj (not hand or foot)
                    if (collisionLayer != LayerMask.NameToLayer("SpecialAttack"))
                        return;
                    HandleHitLogic(8, true);
                    break;
                default:
                    break;
            }
            if (m_handleHitManager.HasBeenHited()) {
                m_gameManagerInstance.HandleHitCallBack();
            }
        }
    }

    private void ChooseDeathAnim(string currentAnimName)
    {
        if (currentAnimName != "Brutal Assassination" && currentAnimName != "Death") {
            // @note: to make the chracter able to toutch the ground, the collider goes up
            Vector3 localPos = m_colliderToMaintain.gameObject.transform.localPosition;
            
            m_colliderToMaintain.gameObject.transform.localPosition = new Vector3(localPos.x, localPos.y + 1, localPos.z);
            // @note: random anim between 2 animations of death
            var randomIndex = Random.Range(0, 2);
            m_animator.SetInteger("Die", randomIndex);
        }
    }

    private IEnumerator LerpColorMaterial(float duration)
    {
        Renderer[] m_spriteRenderers = transform.root.GetComponentsInChildren<Renderer>();
        Material[] m_materials;
        
        // @note: find materials
        int nbMaterials = 0;
        for (int i = 0; i < m_spriteRenderers.Length; i++) {
            foreach (var material in m_spriteRenderers[i].materials)
                nbMaterials++;
        }

        m_materials = new Material[nbMaterials];
        for (int i = 0, j = 0; j < nbMaterials; i++) {
            foreach (var material in m_spriteRenderers[i].materials) {
                m_materials[j] = material;
                j++;
            }
        }

        // @note: lerp material color
        float elapsedTime = 0f;

        for (int i = 0; i < m_materials.Length; i++) 
            m_materials[i].color = Color.red;

        while (elapsedTime < duration) {
            Color lerpedColor = Color.Lerp(Color.red, Color.white, (elapsedTime / duration));

            elapsedTime += Time.deltaTime;
            for (int i = 0; i < m_materials.Length; i++) 
                m_materials[i].color = lerpedColor;
            yield return null;
        }
        for (int i = 0; i < m_materials.Length; i++) 
            m_materials[i].color = Color.white;
    }
}
