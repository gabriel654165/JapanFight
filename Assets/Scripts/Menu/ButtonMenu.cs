using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ButtonMenu : MonoBehaviour
{
    [SerializeField] private GameObject m_textChild;

    public void SetText(string text)
    {
        m_textChild.GetComponent<Text>().text = text;
    }
}