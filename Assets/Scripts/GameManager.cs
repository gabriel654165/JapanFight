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
    [SerializeField] private bool m_roundIsFinished = false;
    // @note: Singleton obj
    private GameObject m_camera;
    private GameObject m_playerOne;
    private GameObject m_playerTwo;
    [SerializeField] private Vector3 m_spawnPositionP1 = new Vector3(5, 1, 0);
    [SerializeField] private Vector3 m_spawnPositionP2 = new Vector3(-5, 1, 0);
    [SerializeField] private Vector3 m_spawnPositionCam = new Vector3(0, 0, 10);
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

        //if (Input.GetKeyDown("space")) {
        //    Save();
        //    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //}
        
        //if (Input.GetKeyDown("space")) {
        //    m_playerOne.GetComponent<Health>().Hit(100.0f);
        //    Debug.Log("Health player one = " + m_playerOne.GetComponent<Health>().m_health.ToString());
        //}
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

        m_camera.transform.position = m_spawnPositionCam;
        m_playerOne.transform.position = m_spawnPositionP1;
        m_playerTwo.transform.position = m_spawnPositionP2;
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
        Debug.Log("New game coroutine");
        yield return new WaitForSeconds(4);
        m_camera.GetComponent<CameraController>().TranslateToTarget(targets[0], 10f);
        yield return new WaitForSeconds(4);
        m_camera.GetComponent<CameraController>().TranslateToTarget(targets[1], 5f);
        yield return new WaitForSeconds(4);
        m_camera.GetComponent<CameraController>().m_isFollowingTargets = true;
    }

    private IEnumerator StartNewRoundCoroutine()
    {
        Debug.Log("New round coroutine");
        yield return null;
    }

    private IEnumerator EndRoundCoroutine(Transform deadPlayer)
    {
        Debug.Log("End coroutine");
        m_roundIsFinished = true;
        // @note: slowmotion during KO
        yield return new WaitForSeconds(1);
        Time.timeScale = 0.5f;
        yield return new WaitForSeconds(2);
        Time.timeScale = 1f;
        yield return new WaitForSeconds(2);
        m_camera.GetComponent<CameraController>().TranslateToTarget(deadPlayer, 10f);
        yield return new WaitForSeconds(6);
        Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
