using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;

[RequireComponent(typeof(Canvas))]
public class CanvasController : MonoBehaviour
{
    private GameManager m_refGameManager;

    [SerializeField] private TextMeshProUGUI m_textPrctP1;
    [SerializeField] private TextMeshProUGUI m_textPrctP2;

    [SerializeField] private Image m_imgAvatarP1;
    [SerializeField] private Image m_imgAvatarP2;
    
    [SerializeField] private TextMeshProUGUI m_textTimer;
    [SerializeField] private TextMeshProUGUI m_textRound;

    [SerializeField] private GameObject m_popUpPrefab;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimer();
        UpdatePlayerPrct();
        UpdateRound();
    }

    public void SpawnTextPopUp(Vector3 initialScale, Vector3 destScale, string textContent, float duration)
    {
        PopUpExtendText currentPopUp = Instantiate(m_popUpPrefab).GetComponent<PopUpExtendText>();
        Canvas currentCanvasPopUp = currentPopUp.GetComponent<Canvas>();

        currentCanvasPopUp.worldCamera = m_refGameManager.GetCamera();
        currentCanvasPopUp.sortingOrder = -100;
        currentCanvasPopUp.planeDistance = 1;

        currentPopUp.InitPopUp(destScale, duration);
        currentPopUp.SetScale(initialScale);
        currentPopUp.SetText(textContent);
        currentPopUp.PopText(0, 0, 0);
    }

    public void SetGameManager(GameManager gameManager)
    {
        m_refGameManager = gameManager;
    }

    private void UpdateTimer() 
    {
        double rTime = (double)m_refGameManager.GetRemainingTime();
        var rTimeSpan = TimeSpan.FromSeconds(rTime);
        
        m_textTimer.text = rTimeSpan.ToString(@"mm\:ss");
    }

    private void UpdatePlayerPrct()
    {
        List<GameObject> playerList = m_refGameManager.GetPlayerList();

        m_textPrctP1.text = playerList[0].GetComponent<Health>().GetPrctLeftHealth().ToString() + "%";
        m_textPrctP2.text = playerList[1].GetComponent<Health>().GetPrctLeftHealth().ToString() + "%";
    }

    private void UpdateRound()
    {
        // @note: add 1 to GetCurrentRound to never display the round 0
        m_textRound.text = "Round " + (m_refGameManager.GetCurrentRound() + 1).ToString();
    }
}
