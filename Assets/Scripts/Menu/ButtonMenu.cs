using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

public class ButtonMenu : MonoBehaviour
{
    [SerializeField] private GameObject m_textChild;
    private Button m_button;

    void Start()
    {
        m_button = GetComponent<Button>();
    }

    public void SetText(string text)
    {
        m_textChild.GetComponent<Text>().text = text;
    }

    public void SetEventCallBack(Action action)
    {
        if (m_button == null) {
            m_button = GetComponent<Button>();
        }
        UnityAction unityAction = new UnityAction(action);
        m_button.onClick.AddListener(unityAction);
    }

}