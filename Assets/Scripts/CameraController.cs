using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private List<Transform> m_targets;

    //@note: regular movements
    [SerializeField] private Vector3 m_offset = new Vector3(0, 0, -50);
    [SerializeField] private float m_smoothTime = 5f;
    [SerializeField] private float m_minZoom = 40f;
    [SerializeField] private float m_maxZoom = 10f;
    [SerializeField] private float m_zoomLimit = 50f;

    //@note: camera special moves
    [SerializeField] private float m_speedMovement = 20f;
    [SerializeField] private Vector3 m_offsetAbsoluteLookAt = new Vector3(5, 0, 0);
    private Transform m_currentTarget = null;

    private Camera m_camera;
    private Vector3 m_velocity;
    private Quaternion m_initialRotation;
    [SerializeField]//@debug
    private bool m_isFollowingTargets = true;
    

    void Start()
    {
        m_camera = GetComponent<Camera>();
        m_initialRotation = transform.transform.rotation;
    }

    //@debug
    /*bool ok = false;
    void Update()
    {
        if (Input.GetKeyDown("space")) {
            if (!ok) {
                TranslateToTarget(m_targets[0], m_speedMovement);
                ok = true;
            } else {
                TranslateToTarget(m_targets[1], m_speedMovement);
            }
        }
    }*/

    void LateUpdate()
    {
        if (m_targets.Count == 0)
        {
            return;
        }
        if (m_isFollowingTargets) {
            // @note: to reset camera special moves
            m_camera.transform.rotation = Quaternion.Lerp(m_camera.transform.rotation, m_initialRotation, 0.5f);
            Move();
            Zoom();
        } else {
            // @note: for special camera moves
            float newZoom = Mathf.Lerp(m_camera.fieldOfView, m_minZoom, 0.5f);
            m_camera.fieldOfView = Mathf.Lerp(m_camera.fieldOfView, newZoom, Time.deltaTime);
        }
    }


    void Zoom()
    {
        float newZoom = Mathf.Lerp(m_maxZoom, m_minZoom, GetGreatestDistance() / m_zoomLimit);
        m_camera.fieldOfView = Mathf.Lerp(m_camera.fieldOfView, newZoom, Time.deltaTime);
    }

    void Move()
    {
        var centerPoint = GetCenterPoint();
        var newPosition = centerPoint + m_offset;

        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref m_velocity, m_smoothTime);
    }


    void TranslateToTarget(Transform target, float speedMovement)
    {
        m_isFollowingTargets = false;
        m_currentTarget = target;

        bool isLeft = target.transform.position.x < transform.position.x;
        var offsetTranslatePosition = new Vector3(isLeft ? m_offsetAbsoluteLookAt.x : -m_offsetAbsoluteLookAt.x, m_offsetAbsoluteLookAt.y, m_offsetAbsoluteLookAt.z);//switch en fonction de devant le player

        StartCoroutine(MoveAndRotateTo(target.transform, offsetTranslatePosition, speedMovement));
    }


    float GetGreatestDistance()
    {
        var bounds = new Bounds(m_targets[0].position, Vector3.zero);

        for (int i = 0; i < m_targets.Count; ++i)
        {
            bounds.Encapsulate(m_targets[i].position);
        }
        // @note: return width of the distance between targets
        return bounds.size.x;
    }


    Vector3 GetCenterPoint()
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
            transform.LookAt(target);
            yield return null;
        }
        transform.position = targetPos;
        transform.LookAt(target);
    }
}
