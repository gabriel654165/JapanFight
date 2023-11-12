using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private Transform m_posPlayer1;
    [SerializeField] private Transform m_posPlayer2;
    // @note: didn't took the time to create my own editor to creation tools functions to add my varibales in editor
    [SerializeField] private string[] m_playerNameArray;
    [SerializeField] private GameObject[] m_playerPrefabArray;
    private Dictionary<string, GameObject> m_playerPrefabDictionary = new Dictionary<string, GameObject>();
    private string m_playerName1;
    private string m_playerName2;
    private int m_indexPlayer1 = 0;
    private int m_indexPlayer2 = 0;
    // @note: players on screen
    private GameObject m_playerOneObj;
    private GameObject m_playerTwoObj;
    // @note: dirty bool
    private bool playerHasBeenChoosen = false;

    [Header("Maps")]
    // @note: didn't took the time to create my own editor to creation tools functions to add my varibales in editor
    [SerializeField] private string[] m_mapNameArray;
    [SerializeField] private GameObject[] m_mapPrefabArray;
    [SerializeField] private string[] m_mapScenesNameArray;
    private Dictionary<string, GameObject> m_mapPrefabDictionary = new Dictionary<string, GameObject>();
    private Dictionary<int, string> m_scenesDictionnary = new Dictionary<int, string>();
    private int m_mapIndex = -1;

    // @note: canvas items
    [Header("UI")]
    [SerializeField] private Canvas m_canvas;
    // @note: contrainers
    [SerializeField] private GameObject m_containerButtonPlayer;
    [SerializeField] private GameObject m_containerButtonMap;
    [SerializeField] private GameObject m_containerSelect;
    // @note: buttons
    [SerializeField] private GameObject m_buttonPrefab;
    [SerializeField] private Vector3 m_initialCenterPosButton = new Vector3(0, 100, 400);
    [SerializeField] private Vector3 m_offsetCenterButton = new Vector3(0, -40, 0);
    [SerializeField] private Vector3 m_offsetSideButton = new Vector3(100, 0, 0);
    // @note: select fields
    [SerializeField] private GameObject m_selectedFieldPrefab;
    private GameObject m_selectedFieldP1;
    private GameObject m_selectedFieldP2;
    [SerializeField] private Vector3 m_centerPosSelectFields = new Vector3(0, 0, 400);
    [SerializeField] private Vector3 m_offsetSelectField = new Vector3(300, 0, 0);
    // @note: cursors
    [SerializeField] private GameObject m_cursorPlayerPrefab;
    private GameObject m_cursorP1;
    private GameObject m_cursorP2;

    [Header("InputManager")]
    [SerializeField] public PlayerInputManager inputManager;
    private int m_cptSelect = 0;


    void Start()
    {
        InitChoosePlayerUI();
        InitSelectPlayerUI();
        InitMapUI();
        InitPlayerInputs();
    }

    private void InitPlayerInputs()
    {
        PlayerInputManager.instance.playerPrefab = m_cursorPlayerPrefab;
        // @note: warning if the two controllers are not the 2 and 3 crash
        // var inputPlayer1 = PlayerInputManager.instance.JoinPlayer(0, default, default, InputSystem.devices.ToArray()[2]);
        // var inputPlayer2 = PlayerInputManager.instance.JoinPlayer(1, default, default, InputSystem.devices.ToArray()[3]);
        
        // if (inputPlayer1 != null && inputPlayer2 != null) {
        //     m_cursorP1 = inputPlayer1.gameObject;
        //     m_cursorP2 = inputPlayer2.gameObject;
        // }

        // @debug: remove this
        m_cursorP1 = Instantiate(m_cursorPlayerPrefab);
        m_cursorP2 = Instantiate(m_cursorPlayerPrefab);
        // !debug

        m_cursorP1.transform.SetParent(m_canvas.transform);
        m_cursorP2.transform.SetParent(m_canvas.transform);
        m_cursorP1.gameObject.GetComponent<CursorController>().Init(this);
        m_cursorP2.gameObject.GetComponent<CursorController>().Init(this);
    }

    private void InitSelectPlayerUI()
    {
        m_selectedFieldP1 = Instantiate(m_selectedFieldPrefab);
        m_selectedFieldP2 = Instantiate(m_selectedFieldPrefab);
        m_selectedFieldP1.transform.SetParent(m_containerSelect.transform);
        m_selectedFieldP2.transform.SetParent(m_containerSelect.transform);
        m_selectedFieldP1.transform.localPosition = new Vector3(m_centerPosSelectFields.x - m_offsetSelectField.x, m_centerPosSelectFields.y + m_offsetSelectField.y, m_centerPosSelectFields.z + m_offsetSelectField.z);
        m_selectedFieldP2.transform.localPosition = new Vector3(m_centerPosSelectFields.x + m_offsetSelectField.x, m_centerPosSelectFields.y + m_offsetSelectField.y, m_centerPosSelectFields.z + m_offsetSelectField.z);
        m_selectedFieldP1.transform.localScale = new Vector3(1, 1, 1);
        m_selectedFieldP2.transform.localScale = new Vector3(1, 1, 1);
    }

    private void InitChoosePlayerUI()
    {
        Vector3 lastPos = m_initialCenterPosButton;
        int index = 0;

        // @note: init player dictionnary
        foreach (var playerName in m_playerNameArray) {
            if (m_playerPrefabArray[index] != null)
                m_playerPrefabDictionary.Add(playerName, m_playerPrefabArray[index]);
            index++;
        }

        // @note: init player buttons
        index = 0;
        foreach (var playerName in m_playerNameArray) {
            int newIndex = index;
            GameObject newButtonP1 = Instantiate(m_buttonPrefab);
            GameObject newButtonP2 = Instantiate(m_buttonPrefab);
            
            newButtonP1.GetComponent<ButtonMenu>().SetText(playerName);
            newButtonP2.GetComponent<ButtonMenu>().SetText(playerName);
            newButtonP1.GetComponent<ButtonMenu>().SetEventCallBack(() => { SetPlayerOne(m_posPlayer1, playerName, newIndex, m_playerPrefabDictionary[playerName]); });
            newButtonP2.GetComponent<ButtonMenu>().SetEventCallBack(() => { SetPlayerTwo(m_posPlayer2, playerName, newIndex, m_playerPrefabDictionary[playerName]); });
            newButtonP1.transform.SetParent(m_containerButtonPlayer.transform);
            newButtonP2.transform.SetParent(m_containerButtonPlayer.transform);
            newButtonP1.transform.localPosition = lastPos - m_offsetSideButton;
            newButtonP2.transform.localPosition = lastPos + m_offsetSideButton;
            newButtonP1.transform.localScale = new Vector3(1, 1, 1);
            newButtonP2.transform.localScale = new Vector3(1, 1, 1);
            lastPos = new Vector3(m_initialCenterPosButton.x + m_offsetCenterButton.x, lastPos.y + m_offsetCenterButton.y, m_initialCenterPosButton.z + m_offsetCenterButton.z);
            lastPos = new Vector3(m_initialCenterPosButton.x + m_offsetCenterButton.x, lastPos.y + m_offsetCenterButton.y, m_initialCenterPosButton.z + m_offsetCenterButton.z);
            index++;
        }
    }

    private void InitMapUI()
    {
        Vector3 lastPos = m_initialCenterPosButton;
        int index = 0;

        // @note: init dictionnary
        foreach (var mapName in m_mapNameArray) {
            if (m_mapPrefabArray[index] != null)
                m_mapPrefabDictionary.Add(mapName, m_mapPrefabArray[index]);
                m_scenesDictionnary.Add(index, m_mapScenesNameArray[index]);
            index++;
        }

        // @note: init map buttons
        foreach (var mapName in m_mapNameArray) {
            GameObject newMapButton = Instantiate(m_buttonPrefab);
            newMapButton.GetComponent<ButtonMenu>().SetText(mapName);
            newMapButton.GetComponent<ButtonMenu>().SetEventCallBack(() => { SetMap(mapName, m_mapPrefabDictionary); });
            newMapButton.transform.SetParent(m_containerButtonMap.transform);
            newMapButton.transform.localPosition = lastPos;
            newMapButton.transform.localScale = new Vector3(1, 1, 1);
            lastPos = new Vector3(m_initialCenterPosButton.x, lastPos.y + m_offsetCenterButton.y * 2, m_initialCenterPosButton.z);
        }
    }

    public void SetPlayerOne(Transform posSpawn, string playerName, int indexPlayer, GameObject playerToInstantiate)
    {
        m_indexPlayer1 = indexPlayer;
        m_playerName1 = playerName;
        if (m_playerOneObj != null) {
            Destroy(m_playerOneObj);
        }
        GameObject newPlayer = Instantiate(playerToInstantiate);

        newPlayer.transform.position = posSpawn.position;
        newPlayer.transform.rotation = posSpawn.rotation;
        // @note: special transform values for left player (invert of the right one)
        newPlayer.transform.localScale = new Vector3(-newPlayer.transform.localScale.x, newPlayer.transform.localScale.y, newPlayer.transform.localScale.z);
        m_playerOneObj = newPlayer;
    }

    public void SetPlayerTwo(Transform posSpawn, string playerName, int indexPlayer, GameObject playerToInstantiate)
    {
        m_indexPlayer2 = indexPlayer;
        m_playerName2 = playerName;
        if (m_playerTwoObj != null) {
            Destroy(m_playerTwoObj);
        }
        GameObject newPlayer = Instantiate(playerToInstantiate);

        newPlayer.transform.position = posSpawn.position;
        newPlayer.transform.rotation = posSpawn.rotation;
        m_playerTwoObj = newPlayer;
    }

    public void SetMap(string mapName, Dictionary<string, GameObject> mapPrefabDictionnary)
    {
        int index = 0;

        foreach (var key in mapPrefabDictionnary.Keys)
        {
            if (key == mapName) {
                m_mapIndex = index;
                mapPrefabDictionnary[key].SetActive(true);
            } else {
                mapPrefabDictionnary[key].SetActive(false);
            }
            index += 1;
        }
    }

    public void Select(bool state)
    {
        // @todo: mettre un ecran avant, play, quit, settings, credits
        if (!playerHasBeenChoosen) {
            SelectPlayer(state);
        } else {
            SelectMap(state);
        }
    }

    private void SelectPlayer(bool state)
    {
        if (string.IsNullOrEmpty(m_playerName1) && m_cursorP1.GetComponent<CursorController>().hasSelected) {
            m_cursorP1.GetComponent<CursorController>().hasSelected = false;
        } else if (string.IsNullOrEmpty(m_playerName2) && m_cursorP2.GetComponent<CursorController>().hasSelected) {
            m_cursorP2.GetComponent<CursorController>().hasSelected = false;
        } else {
            m_cptSelect = state ? m_cptSelect + 1 : m_cptSelect - 1;
        }
        m_selectedFieldP1.GetComponent<SelectedField>().setState(m_cursorP1.GetComponent<CursorController>().hasSelected);
        m_selectedFieldP2.GetComponent<SelectedField>().setState(m_cursorP2.GetComponent<CursorController>().hasSelected);
        if (m_cptSelect >= 2) {
            NextStep();
        }
    }

    private void SelectMap(bool state)
    {
        if (m_mapIndex < 0 && m_cursorP1.GetComponent<CursorController>().hasSelected) {
            m_cursorP1.GetComponent<CursorController>().hasSelected = false;
        } else if (m_mapIndex < 0 && m_cursorP2.GetComponent<CursorController>().hasSelected) {
            m_cursorP2.GetComponent<CursorController>().hasSelected = false;
        } else {
            m_cptSelect = state ? m_cptSelect + 1 : m_cptSelect - 1;
        }

        m_selectedFieldP1.GetComponent<SelectedField>().setState(m_cursorP1.GetComponent<CursorController>().hasSelected);
        m_selectedFieldP2.GetComponent<SelectedField>().setState(m_cursorP2.GetComponent<CursorController>().hasSelected);
        if (m_cptSelect >= 2) {
            NextStep();
        }
    }

    private void NextStep()
    {
        // @todo: mettre un ecran avant, play, quit, settings, credits
        if (m_cptSelect >= 2 && m_containerButtonPlayer.activeSelf && !playerHasBeenChoosen) {
            playerHasBeenChoosen = true;
            StartCoroutine(NextStepCoroutine());
        } else if (m_cptSelect >= 2 && m_containerButtonMap.activeSelf && playerHasBeenChoosen) {
            StartCoroutine(LoadSceneCoroutine());
        }
    }

    private IEnumerator LoadSceneCoroutine() {
        Save();

        foreach (var index in m_scenesDictionnary.Keys)
        {
            if (index == m_mapIndex) {
                SceneManager.LoadScene(m_scenesDictionnary[index]);
            }
        }
        yield return null;
    }

    private IEnumerator NextStepCoroutine()
    {
        yield return new WaitForSeconds(1);
        m_containerButtonPlayer.SetActive(false);
        m_containerButtonMap.SetActive(true);
        m_cursorP1.GetComponent<CursorController>().hasSelected = false;
        m_cursorP2.GetComponent<CursorController>().hasSelected = false;
        m_selectedFieldP1.GetComponent<SelectedField>().setState(false);
        m_selectedFieldP2.GetComponent<SelectedField>().setState(false);
        m_cptSelect = 0;
    }

    private void Save()
    {
        PlayerPrefs.SetInt("indexPlayer1", m_indexPlayer1);
        PlayerPrefs.SetInt("indexPlayer2", m_indexPlayer2);
        PlayerPrefs.Save();
    }
}