using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PopUpExtendText : MonoBehaviour
{
    [SerializeField] private GameObject m_popUpText;

    private Vector3 m_destScale = new Vector3(2f, 2f, 2f);
    private float m_duration = 1.5f;
    private float m_timer = 0f;
    private string m_textContent = "";
    private bool m_isActive = false;

    public void InitPopUp(Vector3 destScale, float duration)
    {
        m_destScale = destScale;
        m_duration = duration;
    }

    public void SetScale(Vector3 scale)
    {
        m_popUpText.transform.localScale = scale;
    }

    public void SetText(string text)
    {
        m_textContent = text;
        m_popUpText.GetComponent<TextMeshProUGUI>().text = text;
    }

    public void PopText(float positionX = 0, float positionY = 0)
    {
        m_popUpText.transform.localPosition = new Vector3(positionX, positionY, m_popUpText.transform.localPosition.z);
        m_isActive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_isActive) {
            m_timer += Time.deltaTime;

            if (m_timer >= m_duration) {
                Destroy(gameObject);
            }

            Vector3 currentDestScale = Vector3.zero;

            currentDestScale.x = Mathf.Lerp(m_popUpText.transform.localScale.x, m_destScale.x, 0.5f);
            currentDestScale.y = Mathf.Lerp(m_popUpText.transform.localScale.y, m_destScale.y, 0.5f);
            currentDestScale.z = Mathf.Lerp(m_popUpText.transform.localScale.z, m_destScale.z, 0.5f);
            m_popUpText.transform.localScale = currentDestScale;
        }
    }
}
