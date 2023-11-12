using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System.Reflection;
using UnityEngine.Experimental.Rendering.Universal;
using Cyan;
using System.Linq;

[RequireComponent(typeof(MapMetaData))]
public class GameManager : MonoBehaviour
{
    private static GameManager m_instance;

    // @note: Rules
    [SerializeField] private float m_gameDuration = 180;
    [SerializeField] private bool m_roundIsFinished = false;
    [SerializeField] private bool m_roundAsStarted = false;
    [SerializeField] private bool m_suddenDeathOn = false;
    private float m_timer = 0f;
    private int m_nbRoundsToWin = 3;
    private int m_playerOneWinRate = 0;
    private int m_playerTwoWinRate = 0;

    // @note: Singleton obj
    private GameObject m_camera;
    private Canvas m_canvas;
    private GameObject m_playerOne;
    private GameObject m_playerTwo;

    // @note: map property
    [SerializeField] private int m_indexPlace = 0;
    private static MapMetaData m_mapMetaData;
    private int m_indexPlayer1 = 0;
    private int m_indexPlayer2 = 0;
    [SerializeField] private GameObject[] m_mapPrefabArray;

    // @note: prefabs
    public GameObject cameraPrefab;
    public Canvas canvasPrefab;
    public PlayerInputManager inputManager;
    public VolumeProfile profileVolume;
    private GameObject m_playerPrefab1;
    private GameObject m_playerPrefab2;

    //@note: rotation players
    private Quaternion m_leftPlayerRotation;
    private Quaternion m_rightPlayerRotation;

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

        m_mapMetaData = GetComponent<MapMetaData>();
        // @note: prefabs init
        m_camera = Instantiate(cameraPrefab);
        m_canvas = Instantiate(canvasPrefab);

        InitPlayers();
        InitCamera();
        InitCanvas();

        // @note: reset screen shader
        SetVoronoiScreenShader(0);

        // @note: launch round coroutines
        var targets = new List<Transform>{m_playerOne.transform, m_playerTwo.transform};
        if (m_playerOneWinRate == 0 && m_playerTwoWinRate == 0) {
            StartCoroutine(StartNewGameCoroutine(targets));
        } else {
            StartCoroutine(StartNewRoundCoroutine(targets));
        }
    }

    #region INIT COMPONENTS

    private void InitCamera()
    {
        m_camera.transform.position = m_mapMetaData.GetSpawnPosCam(m_indexPlace);

        var targets = new List<Transform>{m_playerOne.transform, m_playerTwo.transform};
        m_camera.GetComponent<CameraController>().SetTargets(targets);
        m_camera.GetComponent<CameraController>().m_isFollowingTargets = true;
        ColorAdjustments colorAdjustments;
        if (profileVolume.TryGet<ColorAdjustments>(out colorAdjustments)) {
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.saturation.value = 100f;
        }
    }

    private void InitPlayers()
    {
        InputSystem.settings.SetInternalFeatureFlag("DISABLE_SHORTCUT_SUPPORT", true);

        m_playerOne = Instantiate(m_mapPrefabArray[m_indexPlayer1]);
        m_playerTwo = Instantiate(m_mapPrefabArray[m_indexPlayer2]);
        /*PlayerInputManager.instance.playerPrefab = m_mapPrefabArray[m_indexPlayer1];
        var inputPlayer1 = PlayerInputManager.instance.JoinPlayer(0, default, default, InputSystem.devices.ToArray()[2]);
        PlayerInputManager.instance.playerPrefab = m_mapPrefabArray[m_indexPlayer2];
        var inputPlayer2 = PlayerInputManager.instance.JoinPlayer(1, default, default, InputSystem.devices.ToArray()[3]);

        if (inputPlayer1 != null && inputPlayer2 != null) {
            m_playerOne = inputPlayer1.gameObject;
            m_playerTwo = inputPlayer2.gameObject;
        }*/

        // @note: Random on differents map places for the round
        m_indexPlace = UnityEngine.Random.Range(0, m_mapMetaData.GetNbSpawnPos());
        m_playerOne.transform.position = m_mapMetaData.GetSpawnPosP1(m_indexPlace);
        m_playerTwo.transform.position = m_mapMetaData.GetSpawnPosP2(m_indexPlace);

        // @note: rotation, make players face each others
        m_leftPlayerRotation = Quaternion.LookRotation(m_playerTwo.transform.position - m_playerOne.transform.position);
        m_rightPlayerRotation = Quaternion.LookRotation(m_playerOne.transform.position - m_playerTwo.transform.position);
        m_playerOne.transform.rotation = m_leftPlayerRotation;
        m_playerTwo.transform.rotation = m_rightPlayerRotation;

        // @debug: uncomment this line
        //m_playerTwo.GetComponent<PlayerInputController>().InvertX(true);

        if (m_playerOne.transform.position.x < m_playerTwo.transform.position.x) {
            m_playerOne.GetComponent<PlayerInputController>().Init(m_playerTwo, true);
            m_playerTwo.GetComponent<PlayerInputController>().Init(m_playerOne);
        } else {
            m_playerOne.GetComponent<PlayerInputController>().Init(m_playerTwo);
            m_playerTwo.GetComponent<PlayerInputController>().Init(m_playerOne, true);
        }
    }

    private void InitCanvas()
    {
        // @note: init canvas
        Camera cameraOverlayUI = null;

        m_canvas.renderMode = RenderMode.ScreenSpaceCamera;
        // @note: assign the overlay camera rendering the UI for this canvas
        foreach (var overlayCamera in m_camera.GetComponent<UniversalAdditionalCameraData>().cameraStack)
            if (overlayCamera.gameObject.name == "OverlayCameraUI")
                cameraOverlayUI = overlayCamera.GetComponent<Camera>();
        m_canvas.worldCamera = cameraOverlayUI;
        m_canvas.sortingOrder = -100;
        m_canvas.GetComponent<CanvasController>().Init(this);
        m_canvas.GetComponent<CanvasController>().SetRound(GetCurrentRound());
        m_canvas.GetComponent<CanvasController>().SetWinRateBars(m_playerOneWinRate, m_playerTwoWinRate, m_nbRoundsToWin);
    }

    #endregion
    
    #region GET/SET
    public List<int> GetPlayerIndexes()
    {
        return new List<int> {m_indexPlayer1, m_indexPlayer2};
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

    private void SetVoronoiScreenShader(float value)
    {
        var pipeline = ((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset);
        FieldInfo propertyInfo = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic );
        var scriptableRendererData = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[0];
        var renderObject = scriptableRendererData.rendererFeatures.OfType<Blit>().FirstOrDefault();
        Material material = renderObject.settings.blitMaterial;

        if (material != null)
            material.SetFloat("_FullScreenIntensity", value);
    }

    // @todo: should return the ref ?
    public Camera GetCamera()
    {
        return m_camera.GetComponent<Camera>();
    }
    #endregion

    #region PLAYERPREF
    
    private void Load()
    {
        if (!PlayerPrefs.HasKey("playerOneWinRate") || !PlayerPrefs.HasKey("playerTwoWinRate")) {
            Debug.Log("WinRates values does not exist in PlayerPrefs");
            m_playerOneWinRate = 0;
            m_playerTwoWinRate = 0;
            return;
        }
        
        if (!PlayerPrefs.HasKey("indexPlayer1") || !PlayerPrefs.HasKey("indexPlayer2")) {
            Debug.Log("Players Index values does not exist in PlayerPrefs");
            m_indexPlayer1 = 0;
            m_indexPlayer2 = 0;
            return;
        }

        m_playerOneWinRate = PlayerPrefs.GetInt("playerOneWinRate");
        m_playerTwoWinRate = PlayerPrefs.GetInt("playerTwoWinRate");
        m_indexPlayer1 = PlayerPrefs.GetInt("indexPlayer1");
        m_indexPlayer2 = PlayerPrefs.GetInt("indexPlayer2");

        if (m_playerOneWinRate >= m_nbRoundsToWin || m_playerTwoWinRate >= m_nbRoundsToWin) {
            m_playerOneWinRate = 0;
            m_playerTwoWinRate = 0;
        }
        if (m_mapPrefabArray.Length <= m_indexPlayer2) {
            m_indexPlayer2 = 0;
        }
        if (m_mapPrefabArray.Length <= m_indexPlayer1) {
            m_indexPlayer1 = 0;
        }

        // @debug: to remove
        m_playerOneWinRate = 2;
        m_playerTwoWinRate = 2;
    }

    private void Save()
    {
        PlayerPrefs.SetInt("playerOneWinRate", m_playerOneWinRate);
        PlayerPrefs.SetInt("playerTwoWinRate", m_playerTwoWinRate);
        PlayerPrefs.Save();
    }
    #endregion

    void Update()
    {
        HandlePlayerDirections();

        if (m_roundIsFinished || (!m_roundAsStarted && !m_suddenDeathOn))
            return;
        bool someoneIsDead = m_playerOne.GetComponent<Health>().isDead() || m_playerTwo.GetComponent<Health>().isDead();
        
        m_timer += Time.deltaTime;
        if (m_timer > m_gameDuration && !m_suddenDeathOn) {
            if (!someoneIsDead) {
                StartCoroutine(SuddenDeathRoundCoroutine(new List<Transform>{m_playerOne.transform, m_playerTwo.transform}));
            } else {
                Save();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        if (someoneIsDead) {
            if (m_playerOne.GetComponent<Health>().isDead() && m_playerTwo.GetComponent<Health>().isDead()) {
                StartCoroutine(SuddenDeathRoundCoroutine(new List<Transform>{m_playerOne.transform, m_playerTwo.transform}));
            } else {
                StartCoroutine(EndRoundCoroutine(
                    m_playerOne.GetComponent<Health>().isDead() ? m_playerTwo.transform : m_playerOne.transform,
                    m_playerOne.GetComponent<Health>().isDead() ? m_playerOne.transform : m_playerTwo.transform
                ));
            }
        }
    }

    //c le contraire des fois le player de gauche est le p2
    private void HandlePlayerDirections()
    {
        if (m_playerOne == null || m_playerTwo == null)
            return;
        
        bool PlayerOneIsLeft = m_playerOne.GetComponent<PlayerInputController>().IsLeft();
        bool PlayerTwoIsLeft = m_playerTwo.GetComponent<PlayerInputController>().IsLeft();

        if (m_playerOne.transform.position.x > m_playerTwo.transform.position.x && PlayerOneIsLeft) 
        {
            m_playerOne.GetComponent<PlayerInputController>().SetIsLeft(!PlayerOneIsLeft);
            m_playerTwo.GetComponent<PlayerInputController>().SetIsLeft(!PlayerTwoIsLeft);
            m_playerOne.transform.rotation = m_rightPlayerRotation;
            m_playerTwo.transform.rotation = m_leftPlayerRotation;
            m_playerOne.GetComponent<PlayerInputController>().InvertX(PlayerOneIsLeft);
            m_playerTwo.GetComponent<PlayerInputController>().InvertX(PlayerTwoIsLeft);
            m_playerOne.GetComponent<PlayerInputController>().SaveRotation();
            m_playerTwo.GetComponent<PlayerInputController>().SaveRotation();
        } 
        else if (m_playerOne.transform.position.x < m_playerTwo.transform.position.x && !PlayerOneIsLeft) 
        {
            m_playerOne.GetComponent<PlayerInputController>().SetIsLeft(!PlayerOneIsLeft);
            m_playerTwo.GetComponent<PlayerInputController>().SetIsLeft(!PlayerTwoIsLeft);

            m_playerOne.transform.rotation = m_leftPlayerRotation;
            m_playerTwo.transform.rotation = m_rightPlayerRotation;
            m_playerOne.GetComponent<PlayerInputController>().InvertX(PlayerOneIsLeft);
            m_playerTwo.GetComponent<PlayerInputController>().InvertX(PlayerTwoIsLeft);

            m_playerOne.GetComponent<PlayerInputController>().SaveRotation();
            m_playerTwo.GetComponent<PlayerInputController>().SaveRotation();
        }
    }

    #region CALLBACKS

    public void HandleHitCallBack()
    {
        StartCoroutine(Rumble(0.25f));
        m_canvas.GetComponent<CanvasController>().UpdatePlayerPrct();
    }

    public void HandlePunchCallBack()
    {
        m_canvas.GetComponent<CanvasController>().UpdatePlayerPowerCharge();
    }

    public void HandleUsePowerCallBack()
    {
        m_canvas.GetComponent<CanvasController>().UpdatePlayerPowerCharge(true);
    }

    private int cptPlayerAction = 0;

    public void Pause()
    {
        if (Time.timeScale != 0 && cptPlayerAction < 2) {
            cptPlayerAction++;
            if (cptPlayerAction >= 2) {
                Time.timeScale = 0;
                cptPlayerAction = 0;
            }
        } else if (Time.timeScale == 0 && cptPlayerAction < 2) {
            cptPlayerAction++;
            if (cptPlayerAction >= 2) {
                Time.timeScale = 1;
                cptPlayerAction = 0;
            }
        }
    }

    public void Restart()
    {
        if (cptPlayerAction < 2)
        {
            cptPlayerAction++;
            if (cptPlayerAction >= 2) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                cptPlayerAction = 0;
            }
        }
    }

    public void Quit()
    {
        if (cptPlayerAction < 2)
        {
            cptPlayerAction++;
            if (cptPlayerAction >= 2) {
                cptPlayerAction = 0;
                SceneManager.LoadScene("Menu");
            }
        }
    }

    #endregion

    #region COROUTINES

    private IEnumerator StartNewGameCoroutine(List<Transform> targets)
    {
        foreach (var target in targets)
            target.GetComponent<PlayerInputController>().Lock();
        
        Vector3 intiScale = new Vector3(0.5f, 0.5f, 0.5f); 
        Vector3 destScale = new Vector3(2f, 2f, 2f);
        Vector2 offsetPopUp = new Vector2(-150, -150);
        float duration = 3f;
        var startingPos = new Vector3(m_mapMetaData.GetCamoffset(m_indexPlace).x, 200, m_mapMetaData.GetCamoffset(m_indexPlace).z);
        
        m_camera.GetComponent<CameraController>().SetOffset(startingPos);
        yield return new WaitForSeconds(1);
        m_camera.GetComponent<CameraController>().SetSmooth(1f);//try with 0.75
        m_camera.GetComponent<CameraController>().SetSpeed(1f);
        yield return new WaitForSeconds(1);
        m_camera.GetComponent<CameraController>().SetOffset(m_mapMetaData.GetCamoffset(m_indexPlace));
        yield return new WaitForSeconds(4);
        m_camera.GetComponent<CameraController>().SetSmooth(0.05f);
        m_camera.GetComponent<CameraController>().SetSpeed(20);
        
        Transform posTarget = new GameObject().transform;
        posTarget.position =  new Vector3(targets[0].position.x, targets[0].position.y + 0.5f, targets[0].position.z);
        m_camera.GetComponent<CameraController>().TranslateToTarget(posTarget, -0.5f, 7f);
        yield return new WaitForSeconds(2f);
        // @todo: write the name of the player ?
        m_canvas.GetComponent<CanvasController>().SpawnTextPopUp(intiScale, destScale, "player 1", offsetPopUp, duration, false);
        var randomIndex = UnityEngine.Random.Range(0, 9);
        targets[0].gameObject.GetComponent<Animator>().SetInteger("Celebrate", randomIndex);
        targets[0].gameObject.GetComponent<Animator>().SetTrigger("TriggerCelebrate");
        yield return new WaitForSeconds(duration);

        posTarget.position =  new Vector3(targets[1].position.x, targets[1].position.y + 0.5f, targets[1].position.z);
        m_camera.GetComponent<CameraController>().TranslateToTarget(posTarget, -0.5f, 3f);//creer un autre transform et move dessus
        yield return new WaitForSeconds(1.5f);
        // @todo: write the name of the player ?
        m_canvas.GetComponent<CanvasController>().SpawnTextPopUp(intiScale, destScale, "player 2", offsetPopUp, duration, false);
        randomIndex = UnityEngine.Random.Range(0, 9);
        targets[1].gameObject.GetComponent<Animator>().SetInteger("Celebrate", randomIndex);
        targets[1].gameObject.GetComponent<Animator>().SetTrigger("TriggerCelebrate");
        yield return new WaitForSeconds(duration);
        m_camera.GetComponent<CameraController>().m_isFollowingTargets = true;

        yield return StartCoroutine(PreRoundCoroutine(targets, new List<string> {"READY", "FIGHT"}));
    }

    private IEnumerator StartNewRoundCoroutine(List<Transform> targets)
    {
        foreach (var target in targets)
            target.GetComponent<PlayerInputController>().Lock();
        
        m_camera.transform.position = m_mapMetaData.GetSpawnPosCam(m_indexPlace);
        m_camera.GetComponent<CameraController>().SetOffset(m_mapMetaData.GetCamoffset(m_indexPlace));
        yield return new WaitForSeconds(1);
        
        yield return StartCoroutine(PreRoundCoroutine(targets, new List<string> {"READY", "FIGHT"}));
    }

    private IEnumerator PreRoundCoroutine(List<Transform> targets, List<string> textToDisplay)
    {
        Vector3 intiScale = new Vector3(0.5f, 0.5f, 0.5f); 
        Vector3 destScale = new Vector3(2f, 2f, 2f);
        var duration = 1f;

        foreach (var text in textToDisplay) {
            m_canvas.GetComponent<CanvasController>().SpawnTextPopUp(intiScale, destScale, text, Vector2.zero, duration, true);
            yield return new WaitForSeconds(duration);
        }

        foreach (var target in targets)
            target.GetComponent<PlayerInputController>().Unlock();
        m_roundAsStarted = true;
    }

    // @todo: ecrire quel player a win
    private IEnumerator EndRoundCoroutine(Transform winner, Transform loser)
    {
        m_roundIsFinished = true;

        // @note: add score to alive player
        bool playerOneWon = m_playerTwo.GetComponent<Health>().isDead() ? true : false;
        bool playerTwoWon = m_playerOne.GetComponent<Health>().isDead() ? true : false;
        
        m_playerOneWinRate += playerOneWon ? 1 : 0;
        m_playerTwoWinRate += playerTwoWon ? 1 : 0;

        // @note: save and reload
        Save();
        
        // @note: b&w background
        ColorAdjustments colorAdjustments;
        if (profileVolume.TryGet<ColorAdjustments>(out colorAdjustments)) {
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.saturation.value = -100f;
        }
        // @note: slowmotion
        Time.timeScale = 0.25f;
        yield return new WaitForSeconds(1.5f);
        Time.timeScale = 1f;

        yield return StartCoroutine(m_canvas.GetComponent<CanvasController>().AddWinRateBar(playerOneWon, playerTwoWon, m_playerOneWinRate, m_playerTwoWinRate));
        yield return new WaitForSeconds(2);

        if (m_playerOneWinRate >= m_nbRoundsToWin || m_playerTwoWinRate >= m_nbRoundsToWin) {
            yield return StartCoroutine(WinnerCoroutine(winner, loser));

            // @todo: display les player save dans le menu 
            SceneManager.LoadScene("Menu");
        } else {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private IEnumerator WinnerCoroutine(Transform winner, Transform loser)
    {
        float duration = 5;
        var randomIndex = UnityEngine.Random.Range(0, 9);   
        Vector3 intiScale = new Vector3(0.5f, 0.5f, 0.5f); 
        Vector3 destScale = new Vector3(2.25f, 2.25f, 2.25f);
        Vector2 offsetPopUp = new Vector2(0, -120);

        // @note: color background
        ColorAdjustments colorAdjustments;
        if (profileVolume.TryGet<ColorAdjustments>(out colorAdjustments)) {
            //yield return StartCoroutine(EndGameCoroutine());
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.saturation.value = 100f;
        }

        // @note: Lock player inputs
        winner.GetComponent<PlayerInputController>().Lock();
        loser.GetComponent<PlayerInputController>().Lock();

        // @note: translate cam to dead player
        m_camera.GetComponent<CameraController>().TranslateToTarget(loser, 2f, 10f);
        yield return new WaitForSeconds(4);
        // @note: translate camera to alive player
        m_camera.GetComponent<CameraController>().TranslateToTarget(winner, 0f, 0f);
        winner.gameObject.GetComponent<Animator>().SetInteger("Celebrate", randomIndex);
        winner.gameObject.GetComponent<Animator>().SetTrigger("TriggerCelebrate");

        yield return new WaitForSeconds(1);
        m_canvas.GetComponent<CanvasController>().SpawnTextPopUp(intiScale, destScale, "WINNER", offsetPopUp, duration, false);
        yield return new WaitForSeconds(duration);

        //@todo: mettre un recap des coups, des pouvoirs speciaux etc
    }

    private IEnumerator SuddenDeathRoundCoroutine(List<Transform> targets) 
    {
        m_suddenDeathOn = true;

        foreach (var target in targets)
            target.GetComponent<PlayerInputController>().Lock();
        
        // @note: reset player transforms
        m_playerOne.transform.position = m_mapMetaData.GetSpawnPosP1(m_indexPlace);
        m_playerTwo.transform.position = m_mapMetaData.GetSpawnPosP2(m_indexPlace);
        m_playerOne.transform.rotation = Quaternion.LookRotation(m_playerTwo.transform.position - m_playerOne.transform.position);
        m_playerTwo.transform.rotation = Quaternion.LookRotation(m_playerOne.transform.position - m_playerTwo.transform.position);

        // @note: update camera
        m_camera.transform.position = m_mapMetaData.GetSpawnPosCam(m_indexPlace);
        m_camera.GetComponent<CameraController>().SetOffset(m_mapMetaData.GetCamoffset(m_indexPlace));
        
        // @note: reset player properties
        foreach (var target in targets)
        {
            target.GetComponent<Health>().SetHealth(2);
            target.GetComponent<Power>().ResetPowerCharge();
            // @todo: cancel callbacks or animations
            target.GetComponent<Animator>().SetBool("Idle", true);
            target.GetComponent<Animator>().Play("Idle");
        }

        // @note: update canvas
        m_canvas.GetComponent<CanvasController>().SetRoundFromString("Sudden Death");
        m_canvas.GetComponent<CanvasController>().EnableTimer(false);
        m_canvas.GetComponent<CanvasController>().UpdatePlayerPrct(true);
        m_canvas.GetComponent<CanvasController>().UpdatePlayerPowerCharge(true);

        SetVoronoiScreenShader(0.15f);

        yield return StartCoroutine(PreRoundCoroutine(targets, new List<string> {"SUDDEN DEATH", "READY", "FIGHT"}));
    }

    private IEnumerator Rumble(float time, float forceX = 0.350f, float forceY = 0.350f)
    {
        // @bug: not always the same gamepad
        if (Gamepad.current != null)
            Gamepad.current.SetMotorSpeeds(forceX, forceY);

        yield return new WaitForSeconds(time);

        if (Gamepad.current != null)
            Gamepad.current.SetMotorSpeeds(0, 0);
    }

    #endregion

}
