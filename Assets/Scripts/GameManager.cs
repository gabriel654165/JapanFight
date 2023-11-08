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
    private int m_nbRoundsToWin = 2;
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
        PlayerInputManager.instance.playerPrefab = m_mapPrefabArray[m_indexPlayer1];
        var inputPlayer1 = PlayerInputManager.instance.JoinPlayer(0, default, default, InputSystem.devices.ToArray()[2]);
        PlayerInputManager.instance.playerPrefab = m_mapPrefabArray[m_indexPlayer2];
        var inputPlayer2 = PlayerInputManager.instance.JoinPlayer(1, default, default, InputSystem.devices.ToArray()[3]);

        if (inputPlayer1 != null && inputPlayer2 != null) {
            m_playerOne = inputPlayer1.gameObject;
            m_playerTwo = inputPlayer2.gameObject;
        }

        // @note: 2 map pos so random from 0 to 1 to set position by mapMetaData
        m_indexPlace = UnityEngine.Random.Range(0, 2);
        m_playerOne.transform.position = m_mapMetaData.GetSpawnPosP1(m_indexPlace);
        m_playerTwo.transform.position = m_mapMetaData.GetSpawnPosP2(m_indexPlace);

        // @note: rotation, make players face each others
        m_leftPlayerRotation = Quaternion.LookRotation(m_playerTwo.transform.position - m_playerOne.transform.position);
        m_rightPlayerRotation = Quaternion.LookRotation(m_playerOne.transform.position - m_playerTwo.transform.position);
        m_playerOne.transform.rotation = m_leftPlayerRotation;
        m_playerTwo.transform.rotation = m_rightPlayerRotation;

        m_playerTwo.GetComponent<PlayerInputController>().InvertX(true);

        m_playerOne.GetComponent<PlayerInputController>().Init(m_playerTwo);
        m_playerTwo.GetComponent<PlayerInputController>().Init(m_playerOne, true);
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
                var targets = new List<Transform>{m_playerOne.transform, m_playerTwo.transform};
                StartCoroutine(SuddenDeathRoundCoroutine(targets));
            } else {
                Save();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        if (someoneIsDead) {
            StartCoroutine(EndRoundCoroutine(m_playerOne.GetComponent<Health>().isDead() ? m_playerOne.transform : m_playerTwo.transform));
        }
    }

    private void HandlePlayerDirections()
    {
        if (m_playerOne == null || m_playerTwo == null)
            return;

        if (m_playerOne.transform.position.x > m_playerTwo.transform.position.x) {
            m_playerOne.transform.rotation = m_rightPlayerRotation;
            m_playerTwo.transform.rotation = m_leftPlayerRotation;
            
            m_playerOne.GetComponent<PlayerInputController>().InvertX(true);
            m_playerTwo.GetComponent<PlayerInputController>().InvertX(false);
        } else {
            m_playerOne.transform.rotation = m_leftPlayerRotation;
            m_playerTwo.transform.rotation = m_rightPlayerRotation;

            m_playerOne.GetComponent<PlayerInputController>().InvertX(false);
            m_playerTwo.GetComponent<PlayerInputController>().InvertX(true);
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

    #endregion

    #region COROUTINES
    private IEnumerator Rumble(float time, float forceX = 0.123f, float forceY = 0.234f)
    {
        if (Gamepad.current != null)
            Gamepad.current.SetMotorSpeeds(forceX, forceY);

        yield return new WaitForSeconds(time);

        if (Gamepad.current != null)
            Gamepad.current.SetMotorSpeeds(0, 0);
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

    private IEnumerator DeadPlayerCoroutine()
    {
        Vector3 intiScale = new Vector3(0.5f, 0.5f, 0.5f); 
        Vector3 destScale = new Vector3(2f, 2f, 2f);
        Vector2 offsetPopUp = new Vector2(0, -120);
        var duration = 2f;
        m_canvas.GetComponent<CanvasController>().SpawnTextPopUp(intiScale, destScale, "DEAD", offsetPopUp, duration, true);
        yield return new WaitForSeconds(duration);
    }

    private IEnumerator StartNewGameCoroutine(List<Transform> targets)
    {
        foreach (var target in targets)
            target.GetComponent<PlayerInputController>().Lock();
        
        Vector3 intiScale = new Vector3(0.5f, 0.5f, 0.5f); 
        Vector3 destScale = new Vector3(2f, 2f, 2f);
        Vector2 offsetPopUp = new Vector2(-150, -150);
        float duration = 3f;
        //var startingPos = new Vector3(m_arrayCameraOffset[m_indexPlace].x, 200, m_arrayCameraOffset[m_indexPlace].z);
        var startingPos = new Vector3(m_mapMetaData.GetCamoffset(m_indexPlace).x, 200, m_mapMetaData.GetCamoffset(m_indexPlace).z);
        
        m_camera.GetComponent<CameraController>().SetOffset(startingPos);
        yield return new WaitForSeconds(1);
        //m_camera.GetComponent<CameraController>().SetOffset(m_arrayCameraOffset);
        m_camera.GetComponent<CameraController>().SetOffset(m_mapMetaData.GetCamoffset(m_indexPlace));
        yield return new WaitForSeconds(4);
        
        Transform posTarget = new GameObject().transform;
        posTarget.position =  new Vector3(targets[0].position.x, targets[0].position.y + 0.5f, targets[0].position.z);
        m_camera.GetComponent<CameraController>().TranslateToTarget(posTarget, -0.5f, 7f);
        yield return new WaitForSeconds(2f);
        m_canvas.GetComponent<CanvasController>().SpawnTextPopUp(intiScale, destScale, "player 1", offsetPopUp, duration, false);
        var randomIndex = UnityEngine.Random.Range(0, 9);
        targets[0].gameObject.GetComponent<Animator>().SetInteger("Celebrate", randomIndex);
        targets[0].gameObject.GetComponent<Animator>().SetTrigger("TriggerCelebrate");
        yield return new WaitForSeconds(duration);

        posTarget.position =  new Vector3(targets[1].position.x, targets[1].position.y + 0.5f, targets[1].position.z);
        m_camera.GetComponent<CameraController>().TranslateToTarget(posTarget, -0.5f, 3f);//creer un autre transform et move dessus
        yield return new WaitForSeconds(1.5f);
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
        
        //m_camera.transform.position = m_arraySpawnPosCam[m_indexPlace];
        m_camera.transform.position = m_mapMetaData.GetSpawnPosCam(m_indexPlace);
        //m_camera.GetComponent<CameraController>().SetOffset(m_arrayCameraOffset[m_indexPlace]);
        m_camera.GetComponent<CameraController>().SetOffset(m_mapMetaData.GetCamoffset(m_indexPlace));
        yield return new WaitForSeconds(1);
        
        yield return StartCoroutine(PreRoundCoroutine(targets, new List<string> {"READY", "FIGHT"}));
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

        SetVoronoiScreenShader(0.25f);

        yield return StartCoroutine(PreRoundCoroutine(targets, new List<string> {"SUDDEN DEATH", "READY", "FIGHT"}));
    }

    private IEnumerator EndRoundCoroutine(Transform deadPlayer)
    {
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
        yield return new WaitForSeconds(6);

        yield return StartCoroutine(DeadPlayerCoroutine());

        // @note: add score to alive player
        m_playerOneWinRate += m_playerOne.GetComponent<Health>().isDead() ? 0 : 1;
        m_playerTwoWinRate += m_playerTwo.GetComponent<Health>().isDead() ? 0 : 1;

        // @note: save and reload
        Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    #endregion

}
