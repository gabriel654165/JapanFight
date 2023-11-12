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
    [SerializeField] private TextMeshProUGUI m_textControlSpecialPowerP1;
    [SerializeField] private TextMeshProUGUI m_textControlSpecialPowerP2;
    [SerializeField] private float m_durationFillPower = 10f;
    private float m_lastPowerValueP1 = 0;
    private float m_lastPowerValueP2 = 0;
    private bool m_playerOneIsCharged = false;
    private bool m_playerTwoIsCharged = false;

    [Header("Round Indicators")]
    [SerializeField] private TextMeshProUGUI m_textTimer;
    [SerializeField] private TextMeshProUGUI m_textRound;
    [SerializeField] private TextMeshProUGUI m_textRoundNumber;

    [Header("Win Rate")]
    [SerializeField] private GameObject m_containerBarP1;
    [SerializeField] private GameObject m_containerBarP2;
    [SerializeField] private Vector3 m_offsetBetweenBars;
    [SerializeField] private Vector3 m_InitialPosP1;
    [SerializeField] private Vector3 m_InitialPosP2;

    [Header("Prefabs")]
    [SerializeField] private GameObject m_popUpPrefab;
    [SerializeField] private GameObject m_popUpNoGlowPrefab;
    [SerializeField] private GameObject m_winRoundBar;
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

    #region WIN RATE BARS

    private IEnumerator AddWinRateBarByPlayer(GameObject containerBar, Vector3 initialPos, Vector3 srcScale, Vector3 destScale, float duration)
    {
        float elapsedTime = 0f;
        GameObject bar = Instantiate(m_winRoundBar);

        bar.GetComponent<RawImage>().color = Color.yellow;
        bar.transform.SetParent(containerBar.transform);
        bar.GetComponent<RectTransform>().localPosition = initialPos;
        bar.GetComponent<RectTransform>().localScale = srcScale;

        while (elapsedTime <= duration) 
        {
            bar.GetComponent<RectTransform>().localScale = Vector3.Lerp(srcScale, destScale, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public IEnumerator AddWinRateBar(bool playerOneWon, bool playerTwoWon, int playerOneWinRate, int playerTwoWinRate)
    {
        float duration = 2f;
        Vector3 srcScale = new Vector3(0.01f, 0.01f, 0.01f);
        Vector3 destScale = new Vector3(1f, 1f, 1f);

        if (playerOneWon)
            yield return AddWinRateBarByPlayer(m_containerBarP1, new Vector3(m_InitialPosP1.x + (((playerOneWinRate - 1) * m_offsetBetweenBars.x) * -1), 0, 0), srcScale, destScale, duration);
        if (playerTwoWon)
            yield return AddWinRateBarByPlayer(m_containerBarP2, new Vector3(m_InitialPosP2.x + ((playerTwoWinRate - 1) * m_offsetBetweenBars.x), 0, 0), srcScale, destScale, duration);
    }

    public void SetWinRateBars(int playerOneWinRate, int playerTwoWinRate, int nbRoundsToWin)
    {
        for (int i = 0; i < nbRoundsToWin; ++i)
        {
            GameObject bar = Instantiate(m_winRoundBar);

            if (i < playerOneWinRate)
                bar.GetComponent<RawImage>().color = Color.white;
            else
                bar.GetComponent<RawImage>().color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            bar.transform.SetParent(m_containerBarP1.transform);
            bar.GetComponent<RectTransform>().localPosition = new Vector3(m_InitialPosP1.x + ((i * m_offsetBetweenBars.x) * -1), 0, 0);
            bar.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        }

        for (int i = 0; i < nbRoundsToWin; ++i)
        {
            GameObject bar = Instantiate(m_winRoundBar);
            if (i < playerTwoWinRate)
                bar.GetComponent<RawImage>().color = Color.white;
            else
                bar.GetComponent<RawImage>().color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            bar.transform.SetParent(m_containerBarP2.transform);
            bar.GetComponent<RectTransform>().localPosition = new Vector3(m_InitialPosP2.x + (i * m_offsetBetweenBars.x), 0, 0);
            bar.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        }
    }

    #endregion

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

        UpdatePowerValues(currentPowerP1, currentPowerP2, playerList, forceUpdate);
        UpdatePowerMaterial(currentPowerP1, currentPowerP2, playerList, forceUpdate);
        PowerBeatWhenFilled(currentPowerP1, currentPowerP2, playerList);
    }

    private void UpdatePowerValues(float currentPowerP1, float currentPowerP2, List<GameObject> playerList, bool forceUpdate = false)
    {
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

    private void UpdatePowerMaterial(float currentPowerP1, float currentPowerP2, List<GameObject> playerList, bool forceUpdate = false)
    {
        float maxPower = 3;
        float maxSpeed = 30;

        if (m_lastPowerValueP1 != currentPowerP1 || forceUpdate) {
            StartCoroutine(LerpAndDebounceOverTime(
                m_durationFillPower,
                m_imgOutlineCircleP1.material.GetFloat("_DisolvePower"),
                maxPower * currentPowerP1,
                (value) => { m_imgOutlineCircleP1.material.SetFloat("_DisolvePower", value); return null; },
                () => { m_imgOutlineCircleP1.material.SetFloat("_DisolvePower", maxPower * currentPowerP1); }
            ));
            StartCoroutine(LerpAndDebounceOverTime(
                m_durationFillPower,
                m_imgOutlineCircleP1.material.GetFloat("_DisolveSpeed"),
                maxSpeed * currentPowerP1,
                (value) => { m_imgOutlineCircleP1.material.SetFloat("_DisolveSpeed", value); return null; },
                () => { m_imgOutlineCircleP1.material.SetFloat("_DisolveSpeed", maxSpeed * currentPowerP1); }
            ));
        }

        if (m_lastPowerValueP2 != currentPowerP2 || forceUpdate) {
            StartCoroutine(LerpAndDebounceOverTime(
                m_durationFillPower,
                m_imgOutlineCircleP2.material.GetFloat("_DisolvePower"),
                maxPower * currentPowerP2,
                (value) => { m_imgOutlineCircleP2.material.SetFloat("_DisolvePower", value); return null; },
                () => { m_imgOutlineCircleP2.material.SetFloat("_DisolvePower", maxPower * currentPowerP2); }
            ));
            StartCoroutine(LerpAndDebounceOverTime(
                m_durationFillPower,
                m_imgOutlineCircleP2.material.GetFloat("_DisolveSpeed"),
                maxSpeed * currentPowerP2,
                (value) => { m_imgOutlineCircleP2.material.SetFloat("_DisolveSpeed", value); return null; },
                () => { m_imgOutlineCircleP2.material.SetFloat("_DisolveSpeed", maxSpeed * currentPowerP2); }
            ));
        }

    }

    //idea : apply the fire material to the shadow of player
    private void PowerBeatWhenFilled(float currentPowerP1, float currentPowerP2, List<GameObject> playerList)
    {
        if (currentPowerP1 >= 1) {
            if (!m_playerOneIsCharged) {
                m_playerOneIsCharged = true;
                m_textControlSpecialPowerP1.gameObject.SetActive(true);
                StartCoroutine(ScaleTextUntilCondition(m_imgOutlineCircleP1.GetComponent<RectTransform>(), 20f, 0.25f, 0.1f, (value) => { return (playerList[0].GetComponent<Power>().GetPowerCharge() >= 1); }));
                StartCoroutine(ScaleTextUntilCondition(m_imgBgCircleP1.GetComponent<RectTransform>(), 20f, 0.25f, 0.1f, (value) => { return (playerList[0].GetComponent<Power>().GetPowerCharge() >= 1); }));
                StartCoroutine(ScaleTextUntilCondition(m_textControlSpecialPowerP1.GetComponent<RectTransform>(), 20f, 0.1f, 0.1f, (value) => { return (playerList[0].GetComponent<Power>().GetPowerCharge() >= 1); }));
                //StartCoroutine(ShadeImage(m_imgOutlineCircleP1));
            }
        } else {
            m_textControlSpecialPowerP1.gameObject.SetActive(false);
            m_playerOneIsCharged = false;
        }
        if (currentPowerP2 >= 1) {
            if (!m_playerTwoIsCharged) {
                m_playerTwoIsCharged = true;
                m_textControlSpecialPowerP2.gameObject.SetActive(true);
                StartCoroutine(ScaleTextUntilCondition(m_imgOutlineCircleP2.GetComponent<RectTransform>(), 20f, 0.25f, 0.1f, (value) => { return (playerList[1].GetComponent<Power>().GetPowerCharge() >= 1); }));
                StartCoroutine(ScaleTextUntilCondition(m_imgBgCircleP2.GetComponent<RectTransform>(), 20f, 0.25f, 0.1f, (value) => { return (playerList[1].GetComponent<Power>().GetPowerCharge() >= 1); }));
                StartCoroutine(ScaleTextUntilCondition(m_textControlSpecialPowerP2.GetComponent<RectTransform>(), 20f, 0.25f, 0.1f, (value) => { return (playerList[1].GetComponent<Power>().GetPowerCharge() >= 1); }));
                //StartCoroutine(ShadeImage(m_imgOutlineCircleP2));
            }
        } else {
            m_textControlSpecialPowerP2.gameObject.SetActive(false);
            m_playerTwoIsCharged = false;
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

    private IEnumerator ScaleTextUntilCondition(RectTransform textRect, float scaleToAdd, float timeToExtand, float timeToRetract, Func<float, bool> condition)
    {
        while (condition(0))
        {
            float srcSizeX = textRect.sizeDelta.x;
            float destSizeX = textRect.sizeDelta.x + scaleToAdd;
            float srcSizeY = textRect.sizeDelta.y;
            float destSizeY = textRect.sizeDelta.y + scaleToAdd;

            yield return ExtandText(textRect, srcSizeX, destSizeX, srcSizeY, destSizeY, timeToExtand);
            yield return RetractText(textRect, srcSizeX, destSizeX, srcSizeY, destSizeY, timeToRetract);
        }
    }

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
