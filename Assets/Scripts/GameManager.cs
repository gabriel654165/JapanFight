using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager m_instance;

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

    // @note: Rules
    private float m_gameDuration = 60 * 3;
    private float m_timer = 0f;
    private int m_nbRoundsToWin = 2;
    private int m_playerOneWinRate = 0;
    private int m_playerTwoWinRate = 0;
    // @note: Singleton obj
    private GameObject m_camera;
    private GameObject m_playerOne;
    private GameObject m_playerTwo;
    [SerializeField] private Vector3 spawnPositionP1 = new Vector3(5, 1, 0);
    [SerializeField] private Vector3 spawnPositionP2 = new Vector3(-5, 1, 0);
    [SerializeField] private Vector3 spawnPositionCam = new Vector3(0, 0, 10);
    // @note: prefabs
    public GameObject cameraPrefab;
    public GameObject playerPrefab;


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
        m_timer += Time.deltaTime;
        if (m_timer > m_gameDuration) {
            Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetKeyDown("space")) {
            Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void Load()
    {
        if (!PlayerPrefs.HasKey("playerOneWinRate") || !PlayerPrefs.HasKey("playerTwoWinRate")) {
            Debug.Log("Value does not exist");
            m_playerOneWinRate = 0;
            m_playerTwoWinRate = 0;
            return;
        }
        m_playerOneWinRate = PlayerPrefs.GetInt("playerOneWinRate");
        m_playerTwoWinRate = PlayerPrefs.GetInt("playerTwoWinRate");

        if (m_playerOneWinRate > m_nbRoundsToWin || m_playerTwoWinRate > m_nbRoundsToWin) {
            m_playerOneWinRate = 0;
            m_playerTwoWinRate = 0;
        }

        //Debug.Log("WinRate One = " + m_playerOneWinRate.ToString());
        //Debug.Log("WinRate One = " + m_playerTwoWinRate.ToString());
    }

    private void Save()
    {
        //m_playerOneWinRate++;
        //m_playerTwoWinRate++;
        PlayerPrefs.SetInt("playerOneWinRate", m_playerOneWinRate);
        PlayerPrefs.SetInt("playerTwoWinRate", m_playerTwoWinRate);
        PlayerPrefs.Save();
    }
    

    private void InitRound()
    {
        m_camera = Instantiate(cameraPrefab);
        m_playerOne = Instantiate(playerPrefab);
        m_playerTwo = Instantiate(playerPrefab);

        m_camera.transform.position = spawnPositionCam;
        m_playerOne.transform.position = spawnPositionP1;
        m_playerTwo.transform.position = spawnPositionP2;
        var targets = new List<Transform>{m_playerOne.transform, m_playerTwo.transform};
        m_camera.GetComponent<CameraController>().SetTargets(targets);
        m_camera.GetComponent<CameraController>().m_isFollowingTargets = true;

        if (m_playerOneWinRate == 0 && m_playerTwoWinRate == 0) {
            StartCoroutine(StartNewGameCoroutine(targets));
        } else {
            StartCoroutine(StartNewRoundCoroutine());
        }
    } 

    private IEnumerator StartNewGameCoroutine(List<Transform> targets)
    {
        yield return new WaitForSeconds(4);
        m_camera.GetComponent<CameraController>().TranslateToTarget(targets[0], 20f);
        yield return new WaitForSeconds(4);
        m_camera.GetComponent<CameraController>().TranslateToTarget(targets[1], 20f);
        yield return new WaitForSeconds(4);
        m_camera.GetComponent<CameraController>().m_isFollowingTargets = true;
    }

    private IEnumerator StartNewRoundCoroutine()
    {
        yield return null;
    }
}
