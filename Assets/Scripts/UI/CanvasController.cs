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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimer();
        UpdatePlayerPrct();
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
}
