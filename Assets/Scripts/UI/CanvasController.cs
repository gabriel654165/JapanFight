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
    [SerializeField] private Color m_colorLifeMax = Color.white;
    [SerializeField] private Color m_colorLifeLow = Color.red;
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
    [SerializeField] private TextMeshProUGUI m_textRoundNumber;

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

        m_lastHealthValueP1 = playerList[0].GetComponent<Health>().GetPrctLeftHealth();
        m_lastHealthValueP2 = playerList[1].GetComponent<Health>().GetPrctLeftHealth();
        m_lastPowerValueP1 = playerList[0].GetComponent<Power>().GetPowerCharge();
        m_lastPowerValueP2 = playerList[1].GetComponent<Power>().GetPowerCharge();
        UpdatePlayerPrct(true);
    }

    void Update()
    {
        UpdateTimer();
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

    #region ROUNDS

    public void SetRoundFromString(string roundName)
    {
        m_textRound.text = roundName;
        m_textRound.GetComponent<RectTransform>().localPosition = new Vector3(
            0,
            m_textRound.GetComponent<RectTransform>().localPosition.y,
            m_textRound.GetComponent<RectTransform>().localPosition.z
        );
        m_textRoundNumber.gameObject.SetActive(false);
    }

    public void SetRound(int round)
    {
        m_textRound.text = "Round ";
        // @note: add 1 to GetCurrentRound to never display the round 0
        m_textRoundNumber.text = (round + 1).ToString();
    }
    #endregion

    #region TIMER

    // @todo: remove these lines
    Coroutine task1 = null;
    Coroutine task2 = null;

    private void UpdateTimer() 
    {
        float rTime = (float)m_refGameManager.GetRemainingTime();
        var rTimeSpan = TimeSpan.FromSeconds((double)rTime);

        float rTimeClamp = (int)Mathf.Abs(rTime);
        //Debug.Log("rTime = " + rTimeClamp.ToString());

        if (rTimeClamp == 60 && task1 == null) {
            m_textTimer.color = new Color(1.0f, 0.64f, 0.0f);;
            task1 = StartCoroutine(ScaleTextOnce(m_textTimer.GetComponent<RectTransform>(), 20f, 0.25f, 0.1f));
        }

        if (rTimeClamp == 30 && task2 == null) {
            m_textTimer.color = Color.red;
            task2 = StartCoroutine(ScaleTextOnce(m_textTimer.GetComponent<RectTransform>(), 20f, 0.25f, 0.1f));
        }

        if (rTimeClamp == 10 && rTimeClamp > 0) {
            StartCoroutine(ScaleTextOnce(m_textTimer.GetComponent<RectTransform>(), 10f, 0.15f, 0.15f));
        }
        
        m_textTimer.text = rTimeSpan.ToString(@"mm\:ss");
    }

    public void EnableTimer(bool state)
    {
        m_textTimer.gameObject.SetActive(state);
    }

    #endregion

    #region POWER

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

    #endregion

    #region HEALTH

    public void UpdatePlayerPrct(bool forceUpdate = false)
    {
        List<GameObject> playerList = m_refGameManager.GetPlayerList();
        float currentHealthP1 = playerList[0].GetComponent<Health>().GetPrctLeftHealth();
        float currentHealthP2 = playerList[1].GetComponent<Health>().GetPrctLeftHealth();

        if (m_lastHealthValueP1 != currentHealthP1 || forceUpdate) {
            if (!forceUpdate)
                ShakeText(m_textPrctP1, m_animatorTextP1, 3f);
            StartCoroutine(LerpAndDebounceOverTime(
                m_durationSubstractHealth, 
                m_lastHealthValueP1, 
                currentHealthP1, 
                (float value) => { UpdateHealthText(m_textPrctP1, value); return null; },
                () => { UpdateHealthText(m_textPrctP1, currentHealthP1); m_lastHealthValueP1 = currentHealthP1; })
            );
        }

        if (m_lastHealthValueP2 != currentHealthP2 || forceUpdate) {
            if (!forceUpdate)
                ShakeText(m_textPrctP2, m_animatorTextP2, 3f);
            StartCoroutine(LerpAndDebounceOverTime(
                m_durationSubstractHealth, 
                m_lastHealthValueP2, 
                currentHealthP2, 
                (float value) => { UpdateHealthText(m_textPrctP2, value); return null; },
                () => { UpdateHealthText(m_textPrctP2, currentHealthP2); m_lastHealthValueP2 = currentHealthP2; })
            );
        }
    }

    private void UpdateHealthText(TextMeshProUGUI text, float value)
    {
        VertexGradient colorGradient = new VertexGradient();
        float colorValue = value/100;

        if (colorValue >= 0 && colorValue <= 0.5f)
        {
            colorGradient.topLeft = m_colorLifeMax * (2 * colorValue);
            colorGradient.topRight = m_colorLifeMax * (2 * colorValue);
            colorGradient.bottomLeft = m_colorLifeLow;
            colorGradient.bottomRight = m_colorLifeLow;
        }
        else if (colorValue > 0.5f && colorValue <= 1)
        {
            colorGradient.topLeft = m_colorLifeMax;
            colorGradient.topRight = m_colorLifeMax;
            colorGradient.bottomLeft = m_colorLifeLow + m_colorLifeMax * (2 * colorValue - 1);
            colorGradient.bottomRight = m_colorLifeLow + m_colorLifeMax * (2 * colorValue - 1);
        }
        
        // @note: Ensure alpha is 1
        colorGradient.topLeft.a = 1;
        colorGradient.topRight.a = 1;
        colorGradient.bottomLeft.a = 1;
        colorGradient.bottomRight.a = 1;

        text.enableVertexGradient = true;
        text.colorGradient = colorGradient;

        text.text = ((int)value).ToString() + "%";
    }

    #endregion

    public void ShakeText(TextMeshProUGUI text, Animator animator, float duration)
    {
        animator.SetTrigger("Shake");
    }

    #region COROUTINES

    private IEnumerator ScaleTextOnce(RectTransform textRect, float scaleToAdd, float timeToExtand, float timeToRetract)
    {
        float srcSizeX = textRect.sizeDelta.x;
        float destSizeX = textRect.sizeDelta.x + scaleToAdd;
        float srcSizeY = textRect.sizeDelta.y;
        float destSizeY = textRect.sizeDelta.y + scaleToAdd;

        yield return ExtandText(textRect, srcSizeX, destSizeX, srcSizeY, destSizeY, timeToExtand);
        yield return RetractText(textRect, srcSizeX, destSizeX, srcSizeY, destSizeY, timeToRetract);
        yield return ExtandText(textRect, srcSizeX, destSizeX, srcSizeY, destSizeY, timeToExtand);
        yield return RetractText(textRect, srcSizeX, destSizeX, srcSizeY, destSizeY, timeToRetract);
    }

    private IEnumerator ExtandText(RectTransform textRect, float srcSizeX, float destSizeX, float srcSizeY, float destSizeY, float duration)
    {
        float elapsedTime = 0f;

        while (textRect.sizeDelta.x < destSizeX || textRect.sizeDelta.y < destSizeY) 
        {
            float currentSizeX = Mathf.Lerp(srcSizeX, destSizeX, (elapsedTime / duration));
            float currentSizeY = Mathf.Lerp(srcSizeY, destSizeY, (elapsedTime / duration));
            textRect.sizeDelta = new Vector2(currentSizeX, currentSizeY);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        textRect.sizeDelta = new Vector2(destSizeX, destSizeY);
    }

    private IEnumerator RetractText(RectTransform textRect, float srcSizeX, float destSizeX, float srcSizeY, float destSizeY, float duration)
    {
        float elapsedTime = 0f;

        while (textRect.sizeDelta.x > srcSizeX || textRect.sizeDelta.y > srcSizeY) 
        {
            float currentSizeX = Mathf.Lerp(destSizeX, srcSizeX, (elapsedTime / duration));
            float currentSizeY = Mathf.Lerp(destSizeY, srcSizeY, (elapsedTime / duration));
            textRect.sizeDelta = new Vector2(currentSizeX, currentSizeY);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        textRect.sizeDelta = new Vector2(srcSizeX, srcSizeY);
    }

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
