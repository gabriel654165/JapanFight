using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine;
using TMPro;
using System;

[System.Serializable]
public class PlayerGUISprites {
    public Sprite avatar;
    public Sprite avatarBg;
}

[RequireComponent(typeof(Canvas))]
public class CanvasController : MonoBehaviour
{
    private GameManager m_refGameManager;

    [SerializeField] private TextMeshProUGUI m_textPrctP1;
    [SerializeField] private TextMeshProUGUI m_textPrctP2;
    private float m_lastHealthValueP1 = 0;
    private float m_lastHealthValueP2 = 0;
    private Animator m_animatorTextP1;
    private Animator m_animatorTextP2;

    [SerializeField] private Image m_imgAvatarP1;
    [SerializeField] private Image m_imgAvatarP2;
    [SerializeField] private Image m_imgAvatarBgP1;
    [SerializeField] private Image m_imgAvatarBgP2;
    
    [SerializeField] private TextMeshProUGUI m_textTimer;
    [SerializeField] private TextMeshProUGUI m_textRound;

    [SerializeField] private GameObject m_popUpPrefab;
    [SerializeField] private GameObject m_popUpNoGlowPrefab;

    [SerializeField] private List<PlayerGUISprites> m_avatarSpritesList;
    
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

        m_lastHealthValueP1 = playerList[0].GetComponent<Health>().GetHealth();
        m_lastHealthValueP2 = playerList[1].GetComponent<Health>().GetHealth();
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

    public void UpdatePlayerPrct()
    {
        List<GameObject> playerList = m_refGameManager.GetPlayerList();
        float currentHealthP1 = playerList[0].GetComponent<Health>().GetHealth();
        float currentHealthP2 = playerList[1].GetComponent<Health>().GetHealth();

        if (m_lastHealthValueP1 != currentHealthP1) {        
            ShakeText(m_textPrctP1, m_animatorTextP1, 3f);
            m_textPrctP1.text = playerList[0].GetComponent<Health>().GetPrctLeftHealth().ToString() + "%";
            m_lastHealthValueP1 = currentHealthP1;
        }

        if (m_lastHealthValueP2 != currentHealthP2) {
            ShakeText(m_textPrctP2, m_animatorTextP2, 3f);
            m_textPrctP2.text = playerList[1].GetComponent<Health>().GetPrctLeftHealth().ToString() + "%";
            m_lastHealthValueP2 = currentHealthP2;
        }
    }

    public void ShakeText(TextMeshProUGUI text, Animator animator, float duration)
    {
        animator.SetTrigger("Shake");
    }

    private void UpdateRound()
    {
        // @note: add 1 to GetCurrentRound to never display the round 0
        m_textRound.text = "Round " + (m_refGameManager.GetCurrentRound() + 1).ToString();
    }
}
