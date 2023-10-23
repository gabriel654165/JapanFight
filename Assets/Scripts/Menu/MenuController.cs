using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MenuController : MonoBehaviour
{
    // @note: Players
    [Header("Players")]
    [SerializeField] private Transform m_posPlayer1;
    [SerializeField] private Transform m_posPlayer2;
    // @note: didn't took the time to create my own editor to creation tools functions to add my varibales in editor
    [SerializeField] private string[] m_playerNameArray;
    [SerializeField] private GameObject[] m_playerPrefabArray;
    private Dictionary<string, GameObject> m_playerPrefabDictionnary = new Dictionary<string, GameObject>();

    // @note: Maps
    [Header("Maps")]
    // @note: didn't took the time to create my own editor to creation tools functions to add my varibales in editor
    [SerializeField] private string[] m_mapNameArray;
    [SerializeField] private GameObject[] m_mapPrefabArray;
    private Dictionary<string, GameObject> m_mapPrefabDictionnary = new Dictionary<string, GameObject>();


    // @note: canvas items
    [Header("UI")]
    [SerializeField] private Canvas m_canvas;
    [SerializeField] private GameObject m_containerButtonPlayer;
    [SerializeField] private GameObject m_containerButtonMap;
    [SerializeField] private GameObject m_buttonPrefab;
    [SerializeField] private Vector3 m_initialCenterPosButton = new Vector3(0, 100, 400);
    [SerializeField] private Vector3 m_offsetCenterButton = new Vector3(0, -40, 0);
    [SerializeField] private Vector3 m_offsetSideButton = new Vector3(100, 0, 0);


    void Start()
    {
        InitPlayerUI();
        InitMapUI();
    }

    private void InitPlayerUI()
    {
        Vector3 lastPos = m_initialCenterPosButton;
        int index = 0;

        // @note: init player buttons
        foreach (var playerName in m_playerNameArray) {
            GameObject newPlayer1Button = Instantiate(m_buttonPrefab);
            GameObject newPlayer2Button = Instantiate(m_buttonPrefab);
            
            if (m_playerPrefabArray[index] != null)
                m_playerPrefabDictionnary.Add(playerName, m_playerPrefabArray[index]);
            index++;
            newPlayer1Button.GetComponent<ButtonMenu>().SetText(playerName);
            newPlayer2Button.GetComponent<ButtonMenu>().SetText(playerName);
            newPlayer1Button.transform.parent = m_containerButtonPlayer.transform;
            newPlayer2Button.transform.parent = m_containerButtonPlayer.transform;
            newPlayer1Button.transform.position = lastPos - m_offsetSideButton;
            newPlayer2Button.transform.position = lastPos + m_offsetSideButton;
            lastPos = new Vector3(m_initialCenterPosButton.x + m_offsetCenterButton.x, lastPos.y + m_offsetCenterButton.y, m_initialCenterPosButton.z + m_offsetCenterButton.z);
            lastPos = new Vector3(m_initialCenterPosButton.x + m_offsetCenterButton.x, lastPos.y + m_offsetCenterButton.y, m_initialCenterPosButton.z + m_offsetCenterButton.z);
        }
    }

    private void InitMapUI()
    {
        Vector3 lastPos = m_initialCenterPosButton;
        int index = 0;

        // @note: init map buttons
        foreach (var mapName in m_mapNameArray) {
            GameObject newMapButton = Instantiate(m_buttonPrefab);

            if (m_mapPrefabArray[index] != null)
                m_mapPrefabDictionnary.Add(mapName, m_mapPrefabArray[index]);
            index++;
            newMapButton.GetComponent<ButtonMenu>().SetText(mapName);
            newMapButton.transform.parent = m_containerButtonMap.transform;
            newMapButton.transform.position = lastPos;
            lastPos = new Vector3(m_initialCenterPosButton.x, lastPos.y + m_offsetCenterButton.y * 2, m_initialCenterPosButton.z);
        }
    }

    public void SetPlayer(string playerName)
    {
        foreach (var key in m_playerPrefabDictionnary.Keys)
        {
            // @todo: instanciate le player sur le transform du P1 ou P2
            //if (key == playerName)
            //    m_playerPrefabDictionnary[key].SetActive(true);
            //else
            //    m_playerPrefabDictionnary[key].SetActive(false);
        }
    }

    public void SelectPlayer()
    {

    }

    public void SetMap(string mapName)
    {
        foreach (var key in m_mapPrefabDictionnary.Keys)
        {
            if (key == mapName)
                m_mapPrefabDictionnary[key].SetActive(true);
            else
                m_mapPrefabDictionnary[key].SetActive(false);
        }
    }

    public void SelectMap()
    {

    }
}