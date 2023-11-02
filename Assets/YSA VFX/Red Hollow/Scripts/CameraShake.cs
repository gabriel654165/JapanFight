using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    Transform point;

    public AnimationCurve smooth;

    void Start()
    {
         point = new GameObject("Camera - PointInterested").transform;
    }

    public void Shake(float duartion, float magnitude){
        StartCoroutine(Shaker(duartion, magnitude));
    }
    IEnumerator Shaker(float duartion, float magnitude) {

        point.position = transform.position + transform.forward;

        Vector3 oldPos = point.position;

        float elapsed = 0.0f;

        while (elapsed < duartion) {
            float x = Random.Range(-1f, 1) * (magnitude/40);
            float y = Random.Range(-1f, 1) * (magnitude/40);
            float z = Random.Range(-1f, 1) * (magnitude/40);

            Vector3 shaked = new Vector3(x, y, z);
            float sm = smooth.Evaluate(elapsed / duartion);
            point.position = oldPos + (shaked * sm);

            transform.LookAt(point);

            elapsed += Time.deltaTime;

            yield return null;
        }

        point.position = oldPos;
    }
}
