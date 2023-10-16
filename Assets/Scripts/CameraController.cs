using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private List<Transform> m_targets;

    //@note: regular movements
    [SerializeField] private Vector3 m_offset = new Vector3(-25, 2.5f, -25);//0, 2.5f, -30f
    [SerializeField] private float m_smoothTime = 5f;
    [SerializeField] private float m_minZoom = 40f;
    [SerializeField] private float m_maxZoom = 10f;
    [SerializeField] private float m_zoomLimit = 50f;

    //@note: camera special moves
    [SerializeField] public float m_speedMovement = 20f;
    [SerializeField] private Vector3 m_offsetAbsoluteLookAt = new Vector3(5, 0, 0);
    private Transform m_currentTarget = null;

    private Camera m_camera;
    private Camera m_overlayCameraPlayer;
    private Camera m_overlayCameraUI;
    private Vector3 m_velocity;

    public bool m_isFollowingTargets = false;
    

    void Start()
    {
        m_camera = GetComponent<Camera>();
        m_overlayCameraPlayer = m_camera.transform.Find("OverlayCameraPlayer").GetComponent<Camera>();
        m_overlayCameraUI = m_camera.transform.Find("OverlayCameraUI").GetComponent<Camera>();
    }


    void LateUpdate()
    {
        if (m_targets.Count == 0) {
            return;
        }
        if (m_isFollowingTargets) {
            // @note: regular behavior of the camera
            Move();
            Zoom();
        } else {
            // @note: for special camera moves
            float newZoom = Mathf.Lerp(m_camera.fieldOfView, m_minZoom, 0.5f);
            float time = Time.deltaTime;

            m_camera.fieldOfView = Mathf.Lerp(m_camera.fieldOfView, newZoom, time);
            m_overlayCameraPlayer.fieldOfView = Mathf.Lerp(m_camera.fieldOfView, newZoom, time);
            m_overlayCameraUI.fieldOfView = Mathf.Lerp(m_camera.fieldOfView, newZoom, time);
        }
    }


    public void SetTargets(List<Transform> targets)
    {
        m_targets = targets;
    }

    public void SetOffset(Vector3 offset)
    {
        m_offset = offset;
    }


    private void Zoom()
    {
        float newZoom = Mathf.Lerp(m_maxZoom, m_minZoom, GetGreatestDistance() / m_zoomLimit);
        float time = Time.deltaTime;
        m_camera.fieldOfView = Mathf.Lerp(m_camera.fieldOfView, newZoom, time);
        m_overlayCameraPlayer.fieldOfView = Mathf.Lerp(m_camera.fieldOfView, newZoom, time);
        m_overlayCameraUI.fieldOfView = Mathf.Lerp(m_camera.fieldOfView, newZoom, time);
    }

    private void Move()
    {
        var centerPoint = GetCenterPoint();
        var newPosition = centerPoint + m_offset;

        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref m_velocity, m_smoothTime);
        //@todo : move code from this func, quick fix
        m_camera.transform.LookAt(centerPoint + new Vector3(0, 2, 0));
    }


    public void TranslateToTarget(Transform target, float offsetToAddY, float speedMovement)
    {
        m_isFollowingTargets = false;
        m_currentTarget = target;

        // @note : Ternaire to switch between right and left player, for beeing in front of the player
        bool isLeft = target.transform.position.x < transform.position.x;
        var offsetX = isLeft ? m_offsetAbsoluteLookAt.x : -m_offsetAbsoluteLookAt.x;
        // @note: add values to offsetToAddY if you want the camera to be upper
        var offsetY = m_offsetAbsoluteLookAt.y + offsetToAddY;
        var offsetTranslatePosition = new Vector3(offsetX, offsetY, m_offsetAbsoluteLookAt.z);

        StartCoroutine(MoveAndRotateTo(target.transform, offsetTranslatePosition, speedMovement));
    }


    private float GetGreatestDistance()
    {
        var bounds = new Bounds(m_targets[0].position, Vector3.zero);

        for (int i = 0; i < m_targets.Count; ++i)
        {
            bounds.Encapsulate(m_targets[i].position);
        }
        // @note: return width of the distance between targets
        return bounds.size.x;
    }


    private Vector3 GetCenterPoint()
    {
        if (m_targets.Count == 1)
        {
            return m_targets[0].position;
        }

        var bounds = new Bounds(m_targets[0].position, Vector3.zero);
        for (int i = 0; i < m_targets.Count; ++i) {
            bounds.Encapsulate(m_targets[i].position);
        }
        // @note: return the center of all the targets encapsulated in the same box
        return bounds.center;
    }


    private IEnumerator MoveAndRotateTo(Transform target, Vector3 offsetTarget, float speed)
    {
        // @note : find a node in the center of the target
        var targetCenter = target;
        //var targetCenter = target.Find("Berserker").Find("Motion").Find("B_Pelvis");
        var currentPos = transform.position;
        var targetPos = target.position + offsetTarget;
        var distance = Vector3.Distance(currentPos, targetPos);
        // @todo: make sure speed is always > 0
        var duration = distance / speed;

        var timePassed = 0f;
        while(timePassed < duration)
        {
            // @note: always a factor between 0 and 1
            var factor = timePassed / duration;

            transform.position = Vector3.Lerp(currentPos, targetPos, factor);
            // @note: increase timePassed with Mathf.Min to avoid overshooting
            timePassed += Mathf.Min(Time.deltaTime, duration - timePassed);
            transform.LookAt(targetCenter);
            yield return null;
        }
        transform.position = targetPos;
        transform.LookAt(targetCenter);
    }
}
