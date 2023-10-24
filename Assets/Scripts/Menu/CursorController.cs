using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class CursorController : MonoBehaviour
{
    private MenuController m_menuController;
    public bool hasSelected = false;

    public void Init(MenuController menuController)
    {
        m_menuController = menuController;
    }

    public void OnSelect(InputAction.CallbackContext context) 
    {
        if (context.performed) {
            hasSelected = !hasSelected;
            m_menuController.Select(hasSelected);
        }
    }
}