using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine;
using TMPro;
using System;

[System.Serializable]
public class PlayerGUIProperties {
    public Sprite avatar;
    public Sprite avatarBg;
    public Color backgroundColor;
}

[RequireComponent(typeof(Canvas))]
public class CanvasController : MonoBehaviour
{
    private GameManager m_refGameManager;

    [Header("Health")]
    [SerializeField] private TextMeshProUGUI m_textPrctP1;
    [SerializeField] private TextMeshProUGUI m_textPrctP2;
    [SerializeField] private float m_durationSubstractHealth = 1f;
    private float m_lastHealthValueP1 = 0;
    private float m_lastHealthValueP2 = 0;
    private Animator m_animatorTextP1;
    private Animator m_animatorTextP2;

    [Header("Avatar")]
    [SerializeField] private Image m_imgAvatarP1;
    [SerializeField] private Image m_imgAvatarP2;
    [SerializeField] private Image m_imgAvatarBgP1;
    [SerializeField] private Image m_imgAvatarBgP2;

    [Header("Background")]
    [SerializeField] private Image m_imgBgCircleP1;
    [SerializeField] private Image m_imgBgCircleP2;

    [Header("Power")]
    [SerializeField] private Image m_imgOutlineCircleP1;
    [SerializeField] private Image m_imgOutlineCircleP2;
    [SerializeField] private float m_durationFillPower = 10f;
    private float m_lastPowerValueP1 = 0;
    private float m_lastPowerValueP2 = 0;

    [Header("Round Indicators")]
    [SerializeField] private TextMeshProUGUI m_textTimer;
    [SerializeField] private TextMeshProUGUI m_textRound;

    [Header("Prefabs")]
    [SerializeField] private GameObject m_popUpPrefab;
    [SerializeField] private GameObject m_popUpNoGlowPrefab;

    [SerializeField] private List<PlayerGUIProperties> m_avatarSpritesList;
    
    void Start()
    {
        m_animatorTextP1 = m_textPrctP1.GetComponent<Animator>();
        m_animatorTextP2 = m_textPrctP2.GetComponent<Animator>();
    }

    public void Init(GameManager gameManager)
    {
        SetGameManager(gameManager);
        // @note: spawn right avatar
        var playersIndexes = m_refGameManager.GetPlayerIndexes();
        List<GameObject> playerList = m_refGameManager.GetPlayerList();

        m_imgAvatarP1.sprite = m_avatarSpritesList[playersIndexes[0]].avatar;
        m_imgAvatarP2.sprite = m_avatarSpritesList[playersIndexes[1]].avatar;
        m_imgAvatarBgP1.sprite = m_avatarSpritesList[playersIndexes[0]].avatarBg;
        m_imgAvatarBgP2.sprite = m_avatarSpritesList[playersIndexes[1]].avatarBg;
        m_imgBgCircleP1.color = m_avatarSpritesList[playersIndexes[0]].backgroundColor;
        m_imgBgCircleP2.color = m_avatarSpritesList[playersIndexes[1]].backgroundColor;

        m_lastHealthValueP1 = playerList[0].GetComponent<Health>().GetHealth();
        m_lastHealthValueP2 = playerList[1].GetComponent<Health>().GetHealth();
        m_lastPowerValueP1 = playerList[0].GetComponent<Power>().GetPowerCharge();
        m_lastPowerValueP2 = playerList[1].GetComponent<Power>().GetPowerCharge();

        UpdatePlayerPrct();
    }

    void Update()
    {
        UpdateTimer();
        UpdateRound();
    }

    public void SpawnTextPopUp(Vector3 initialScale, Vector3 destScale, string textContent, Vector2 offset, float duration, bool shouldGlow = false)
    {
        PopUpExtendText currentPopUp = Instantiate(shouldGlow ? m_popUpPrefab : m_popUpNoGlowPrefab).GetComponent<PopUpExtendText>();
        Canvas currentCanvasPopUp = currentPopUp.GetComponent<Canvas>();
        Camera cameraOverlayUI = null;

        // @note: assign the overlay camera rendering the UI for this canvas
        foreach (var overlayCamera in m_refGameManager.GetCamera().GetComponent<UniversalAdditionalCameraData>().cameraStack)
            if (overlayCamera.gameObject.name == "OverlayCameraUI")
                cameraOverlayUI = overlayCamera.GetComponent<Camera>();
        currentCanvasPopUp.worldCamera = cameraOverlayUI;
        currentCanvasPopUp.sortingOrder = -100;
        currentCanvasPopUp.planeDistance = 1f;

        currentPopUp.InitPopUp(destScale, duration);
        currentPopUp.SetScale(initialScale);
        currentPopUp.SetText(textContent);
        currentPopUp.PopText(offset.x, offset.y);
    }

    private void SetGameManager(GameManager gameManager)
    {
        m_refGameManager = gameManager;
    }

    private void UpdateTimer() 
    {
        double rTime = (double)m_refGameManager.GetRemainingTime();
        var rTimeSpan = TimeSpan.FromSeconds(rTime);
        
        m_textTimer.text = rTimeSpan.ToString(@"mm\:ss");
    }

    private void UpdateRound()
    {
        // @note: add 1 to GetCurrentRound to never display the round 0
        m_textRound.text = "Round " + (m_refGameManager.GetCurrentRound() + 1).ToString();
    }

    public void UpdatePlayerPowerCharge(bool forceUpdate = false)
    {
        List<GameObject> playerList = m_refGameManager.GetPlayerList();
        float currentPowerP1 = playerList[0].GetComponent<Power>().GetPowerCharge();
        float currentPowerP2 = playerList[1].GetComponent<Power>().GetPowerCharge();

        if (m_lastPowerValueP1 != currentPowerP1 || forceUpdate) {
            StartCoroutine(LerpAndDebounceOverTime(
                m_durationFillPower, 
                m_lastPowerValueP1, 
                currentPowerP1, 
                (value) => { m_imgOutlineCircleP1.fillAmount = value; return null; },
                () => { m_imgOutlineCircleP1.fillAmount = currentPowerP1; m_lastPowerValueP1 = currentPowerP1; })
            );
        }
        if (m_lastPowerValueP2 != currentPowerP2 || forceUpdate) {
            StartCoroutine(LerpAndDebounceOverTime(
                m_durationFillPower, 
                m_lastPowerValueP2, 
                currentPowerP2, 
                (value) => { m_imgOutlineCircleP2.fillAmount = value; return null; },
                () => { m_imgOutlineCircleP2.fillAmount = currentPowerP2; m_lastPowerValueP2 = currentPowerP2; })
            );
        }
    }

    // @bug: both texts are shaking when the trigger is called
    // @todo : gradiant color from white to red (more the life prct is low)
    public void UpdatePlayerPrct()
    {
        List<GameObject> playerList = m_refGameManager.GetPlayerList();
        float currentHealthP1 = playerList[0].GetComponent<Health>().GetHealth();
        float currentHealthP2 = playerList[1].GetComponent<Health>().GetHealth();

        if (m_lastHealthValueP1 != currentHealthP1) {
            float healthP1 = playerList[0].GetComponent<Health>().GetPrctLeftHealth();
            
            ShakeText(m_textPrctP1, m_animatorTextP1, 3f);
            StartCoroutine(LerpAndDebounceOverTime(
                m_durationSubstractHealth, 
                m_lastHealthValueP1, 
                healthP1, 
                (float value) => { m_textPrctP1.text = ((int)value).ToString() + "%"; return null; },
                () => { m_textPrctP1.text = healthP1.ToString() + "%"; m_lastHealthValueP1 = healthP1; })
            );
        }

        if (m_lastHealthValueP2 != currentHealthP2) {
            float healthP2 = playerList[1].GetComponent<Health>().GetPrctLeftHealth();
            
            ShakeText(m_textPrctP2, m_animatorTextP2, 3f);
            StartCoroutine(LerpAndDebounceOverTime(
                m_durationSubstractHealth, 
                m_lastHealthValueP2, 
                healthP2, 
                (float value) => { m_textPrctP2.text = ((int)value).ToString() + "%"; return null; },
                () => { m_textPrctP2.text = healthP2.ToString() + "%"; m_lastHealthValueP2 = healthP2; })
            );
        }
    }

    public void ShakeText(TextMeshProUGUI text, Animator animator, float duration)
    {
        animator.SetTrigger("Shake");
    }

    #region COROUTINES

    private IEnumerator LerpAndDebounceOverTime(float duration, float srcVal, float destVal, Func<float, GameObject> debounceLogic, Action debounceFunction)
    {
        float elapsedTime = 0f;

        while (elapsedTime <= duration) {   
            debounceLogic(Mathf.Lerp(srcVal, destVal, (elapsedTime / duration)));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        debounceFunction();
    }

    #endregion
}
