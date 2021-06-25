using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class KeyboardManager : MonoBehaviour
{
    [SerializeField] Button m_SpaceButton;
    [SerializeField] Button m_UppercaseButton;
    [SerializeField] Button m_DeleteButton;
    [SerializeField] Button m_DoneButton;

    Dictionary<Button, Text> m_ListLetterButtons = new Dictionary<Button, Text>();

    InputField m_CurrentInputField;

    bool m_UppercaseEnabled = false;

    // Start is called before the first frame update
    void Start()
    {
        List<Button> buttonsInScene = GameObject.FindObjectsOfType<Button>().ToList();

        foreach(Button button in buttonsInScene)
        {
            if(button.gameObject.name == "Key Button")
            {
                Text text = button.transform.GetComponentInChildren<Text>();
                Image image = button.transform.GetComponentInChildren<Image>();

                if (text)
                {
                    button.onClick.AddListener(() => OnKeyButtonClicked(text.text));
                }
   
                m_ListLetterButtons.Add(button, text);
            }
        }

        if (m_SpaceButton)
            m_SpaceButton.onClick.AddListener(() => OnSpaceClicked());

        if (m_UppercaseButton)
            m_UppercaseButton.onClick.AddListener(() => OnUppercaseClicked());

        if (m_DeleteButton)
            m_DeleteButton.onClick.AddListener(() => OnDeleteClicked());

        if (m_DoneButton)
            m_DoneButton.onClick.AddListener(() => DisableKeyboard());

        gameObject.SetActive(false);
    }


    public void EnableKeyboard(InputField inputField)
    {
        gameObject.SetActive(true);
        m_CurrentInputField = inputField;
    }


    public void DisableKeyboard()
    {
        gameObject.SetActive(false);
        m_CurrentInputField = null;

        DisablePreviousKey();
    }


    public void OnKeyButtonClicked(string key)
    {
        Debug.Log("key: " + key);
        if (m_CurrentInputField)
        {
            if (m_UppercaseEnabled)
            {
                key = key.ToUpper();
            }
            m_CurrentInputField.text += key;
        }

        DisablePreviousKey();
    }


    public void OnSpaceClicked()
    {
        if (m_CurrentInputField)
        {
            m_CurrentInputField.text = m_CurrentInputField.text.PadRight(m_CurrentInputField.text.Length + 1);
        }

        DisablePreviousKey();
    }


    private void OnDeleteClicked()
    {
        if (m_CurrentInputField)
        {
            m_CurrentInputField.text = m_CurrentInputField.text.Remove(m_CurrentInputField.text.Length - 1);
        }

        DisablePreviousKey();
    }


    public void OnUppercaseClicked()
    {
        m_UppercaseEnabled = !m_UppercaseEnabled;

        if (m_UppercaseEnabled)
        {
            foreach (var letter in m_ListLetterButtons)
            {
                letter.Value.text = letter.Value.text.ToUpper();
            }
        }
        else
        {
            foreach (var letter in m_ListLetterButtons)
            {
                letter.Value.text = letter.Value.text.ToLower();
            }
        }

        DisablePreviousKey();
    }

    void DisablePreviousKey()
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        if(currentSelected.tag == "Keyboard")
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
