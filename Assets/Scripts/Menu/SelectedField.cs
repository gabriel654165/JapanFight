using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

public class SelectedField : MonoBehaviour
{
    [SerializeField] private GameObject m_textChild;
    [SerializeField] private Image m_backgroundImage;
    private Button m_button;
    public bool isDone = false;

    void Start()
    {
        m_button = GetComponent<Button>();
        setState(isDone);
    }

    private void SetText(string text)
    {
        m_textChild.GetComponent<Text>().text = text;
    }

    private void SetColor(Color color)
    {
        m_backgroundImage.color = color;
    }

    public void setState(bool state)
    {
        isDone = state;

        if (isDone) {
            SetColor(Color.green);
            SetText("Done");
        } else {
            SetColor(Color.grey);
            SetText("Choosing");
        }
    }

}