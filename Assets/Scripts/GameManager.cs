using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class GameManager : MonoBehaviour
{
    private static GameManager m_instance;

    // @note: Rules
    private float m_gameDuration = 60 * 3;
    private float m_timer = 0f;
    private int m_nbRoundsToWin = 2;
    private int m_playerOneWinRate = 0;
    private int m_playerTwoWinRate = 0;
    [SerializeField] private bool m_roundIsFinished = false;

    // @note: Singleton obj
    private GameObject m_camera;
    private Canvas m_canvas;
    private GameObject m_playerOne;
    private GameObject m_playerTwo;

    // @note: map properties
    [SerializeField] private int m_indexMap = 0;
    private Vector3[] m_arraySpawnPosP1 = {new Vector3(-5, 1, 0), new Vector3(-40, 1, 50)};
    private Vector3[] m_arraySpawnPosP2 = {new Vector3(5, 1, 0), new Vector3(-35, 1, 45)};
    private Vector3[] m_arraySpawnPosCam = {new Vector3(0, 0, 10), new Vector3(-37.5f, 0, 47.5f)};
    private Vector3[] m_arrayCameraOffset = {new Vector3(0, 2.5f, -30), new Vector3(-20, 2.5f, -20)};
    
    // @note: prefabs
    public GameObject cameraPrefab;
    public Canvas canvasPrefab;
    public GameObject playerPrefab;
    public PlayerInputManager inputManager;
    public VolumeProfile profileVolume;

    public static GameManager Instance {
        get {
            if (m_instance == null) {
                m_instance = FindObjectOfType<GameManager>();
                if (m_instance == null) {
                    m_instance = new GameObject().AddComponent<GameManager>();
                }
            }
            return m_instance;
        }
    }

    void Awake()
    {
        if (m_instance != null && m_instance != this) {
            Destroy(this);
        }
        else {
            m_instance = this;
        }
    }

    void Start()
    {
        Load();
        InitRound();
    }

    void Update()
    {
        if (m_roundIsFinished)
            return;
        m_timer += Time.deltaTime;
        if (m_timer > m_gameDuration) {
            Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (m_playerOne.GetComponent<Health>().isDead() || m_playerTwo.GetComponent<Health>().isDead()) {
            StartCoroutine(EndRoundCoroutine(m_playerOne.GetComponent<Health>().isDead() ? m_playerOne.transform : m_playerTwo.transform));
        }
    }

    private void Load()
    {
        if (!PlayerPrefs.HasKey("playerOneWinRate") || !PlayerPrefs.HasKey("playerTwoWinRate")) {
            Debug.Log("Value does not exist in PlayerPrefs");
            m_playerOneWinRate = 0;
            m_playerTwoWinRate = 0;
            return;
        }
        m_playerOneWinRate = PlayerPrefs.GetInt("playerOneWinRate");
        m_playerTwoWinRate = PlayerPrefs.GetInt("playerTwoWinRate");

        if (m_playerOneWinRate >= m_nbRoundsToWin || m_playerTwoWinRate >= m_nbRoundsToWin) {
            m_playerOneWinRate = 0;
            m_playerTwoWinRate = 0;
        }
    }

    private void Save()
    {
        PlayerPrefs.SetInt("playerOneWinRate", m_playerOneWinRate);
        PlayerPrefs.SetInt("playerTwoWinRate", m_playerTwoWinRate);
        PlayerPrefs.Save();
    }
    

    private void InitRound()
    {
        // @note: prefabs init
        m_camera = Instantiate(cameraPrefab);
        m_canvas = Instantiate(canvasPrefab);

        // @note: init canvas
        m_canvas.renderMode = RenderMode.ScreenSpaceCamera;
        m_canvas.worldCamera = m_camera.GetComponent<Camera>();
        m_canvas.sortingOrder = -100;
        m_canvas.GetComponent<CanvasController>().SetGameManager(this);

        // @debug purpose
        m_playerOne = Instantiate(playerPrefab);
        m_playerTwo = Instantiate(playerPrefab);
        // @note: controls binding to players
        /*try {
            inputManager.playerPrefab = playerPrefab;
            inputManager.JoinPlayer();
            inputManager.JoinPlayer();
            Player[] players = FindObjectsOfType<Player>();
            //If one controller, the game crash
            m_playerOne = players[0].gameObject;
            m_playerTwo = players[1].gameObject;
        } catch (Exception er) {
            Debug.Log(er.ToString());
        }*/
        // @note: invert the x axis for the player on the right
        m_playerTwo.GetComponent<PlayerInputController>().InvertX(true);

        // @note: 2 map pos so random from 0 to 1
        m_indexMap = UnityEngine.Random.Range(0, 2);
        // @note: transfrom init
        m_camera.transform.position = m_arraySpawnPosCam[m_indexMap];
        m_playerOne.transform.position = m_arraySpawnPosP1[m_indexMap];
        m_playerTwo.transform.position = m_arraySpawnPosP2[m_indexMap];
        // @note: make player face each other
        m_playerOne.transform.rotation = Quaternion.LookRotation(m_playerTwo.transform.position - m_playerOne.transform.position);
        m_playerTwo.transform.rotation = Quaternion.LookRotation(m_playerOne.transform.position - m_playerTwo.transform.position);

        // @note: camera init 
        var targets = new List<Transform>{m_playerOne.transform, m_playerTwo.transform};
        m_camera.GetComponent<CameraController>().SetTargets(targets);
        m_camera.GetComponent<CameraController>().m_isFollowingTargets = true;
        ColorAdjustments colorAdjustments;
        if (profileVolume.TryGet<ColorAdjustments>(out colorAdjustments)) {
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.saturation.value = 100f;
        }

        if (m_playerOneWinRate == 0 && m_playerTwoWinRate == 0) {
            StartCoroutine(StartNewGameCoroutine(targets));
        } else {
            StartCoroutine(StartNewRoundCoroutine());
        }
    }

    public List<GameObject> GetPlayerList()
    {
        return new List<GameObject>() {m_playerOne, m_playerTwo};
    }

    public float GetRemainingTime()
    {
        return m_timer - m_gameDuration;
    }

    public int GetCurrentRound()
    {
        return m_playerOneWinRate + m_playerTwoWinRate;
    }

    // @todo: should return the ref ?
    public Camera GetCamera()
    {
        return m_camera.GetComponent<Camera>();
    }


    private IEnumerator StartNewGameCoroutine(List<Transform> targets)
    {
        Debug.Log("New game coroutine");
        var startingPos = new Vector3(m_arrayCameraOffset[m_indexMap].x, 200, m_arrayCameraOffset[m_indexMap].z);
        m_camera.GetComponent<CameraController>().SetOffset(startingPos);
        yield return new WaitForSeconds(1);
        m_camera.GetComponent<CameraController>().SetOffset(m_arrayCameraOffset[m_indexMap]);
        yield return new WaitForSeconds(4);
        m_camera.GetComponent<CameraController>().TranslateToTarget(targets[0], 0f, 10f);
        yield return new WaitForSeconds(4);
        m_camera.GetComponent<CameraController>().TranslateToTarget(targets[1], 0f, 5f);
        yield return new WaitForSeconds(4);
        m_camera.GetComponent<CameraController>().m_isFollowingTargets = true;
    }


    private IEnumerator StartNewRoundCoroutine()
    {
        Debug.Log("New round coroutine");
        m_camera.transform.position = m_arraySpawnPosCam[m_indexMap];
        m_camera.GetComponent<CameraController>().SetOffset(m_arrayCameraOffset[m_indexMap]);
        yield return new WaitForSeconds(1);
        
        Vector3 intiScale = new Vector3(0.5f, 0.5f, 0.5f); 
        Vector3 destScale = new Vector3(2f, 2f, 2f);
        float duration = 1f;

        m_canvas.GetComponent<CanvasController>().SpawnTextPopUp(intiScale, destScale, "ready", duration);
        yield return new WaitForSeconds(duration);
        
        m_canvas.GetComponent<CanvasController>().SpawnTextPopUp(intiScale, destScale, "fight", duration);
        yield return new WaitForSeconds(duration);
    }


    private IEnumerator EndRoundCoroutine(Transform deadPlayer)
    {
        Debug.Log("End coroutine");
        m_roundIsFinished = true;
        
        // @note: b&w background
        ColorAdjustments colorAdjustments;
        if (profileVolume.TryGet<ColorAdjustments>(out colorAdjustments)) {
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.saturation.value = -100f;
        }
        // @note: slowmotion
        Time.timeScale = 0.25f;
        yield return new WaitForSeconds(1);

        Time.timeScale = 1f;
        m_camera.GetComponent<CameraController>().TranslateToTarget(deadPlayer, 2f, 10f);
         if (profileVolume.TryGet<ColorAdjustments>(out colorAdjustments)) {
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.saturation.value = 100f;
        }
        yield return new WaitForSeconds(10);

        // @note: add score to alive player
        m_playerOneWinRate += m_playerOne.GetComponent<Health>().isDead() ? 0 : 1;
        m_playerTwoWinRate += m_playerTwo.GetComponent<Health>().isDead() ? 0 : 1;

        // @note: save and reload
        Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
