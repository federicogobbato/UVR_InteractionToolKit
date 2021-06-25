using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class ButtonInteractable : MonoBehaviour
{
    [System.Serializable]
    public class ButtonPressedEvent : UnityEvent { }
    [System.Serializable]
    public class ButtonReleasedEvent : UnityEvent { }

    public Vector3 Axis = new Vector3(0,-1,0 );
    public float MaxDistance;
    public float ReturnSpeed = 10.0f;

    public AudioClip ButtonPressAudioClip;
    public AudioClip ButtonReleaseAudioClip;
    
    public ButtonPressedEvent OnButtonPressed;
    public ButtonReleasedEvent OnButtonReleased;

    public bool LockAfterPress = false;

    Vector3 m_StartPosition;
    Rigidbody m_Rigidbody;
    Collider m_Collider;

    bool m_Pressed = false;
    bool m_Locked = false;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Collider = GetComponentInChildren<Collider>();
        m_StartPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (m_Locked) return;

        Vector3 worldAxis = transform.TransformDirection(Axis);
        Vector3 end = transform.position + worldAxis * MaxDistance;
        
        float currentDistance = (transform.position - m_StartPosition).magnitude;
        RaycastHit info;

        float move = 0.0f;

        if (m_Rigidbody.SweepTest(-worldAxis, out info, ReturnSpeed * Time.deltaTime + 0.005f))
        {
            //hitting something, if the contact is < mean we are pressed, move downward
            move = (ReturnSpeed * Time.deltaTime) - info.distance;
        }
        else
        {
            move -= ReturnSpeed * Time.deltaTime;
        }

        float newDistance = Mathf.Clamp(currentDistance + move, 0, MaxDistance);

        m_Rigidbody.position = m_StartPosition + worldAxis * newDistance;

        bool maxDistanceReached = Mathf.Approximately(newDistance, MaxDistance);

        if (!m_Pressed && maxDistanceReached)
        {
            //was just pressed
            m_Pressed = true;
            SFXPlayer.Instance.PlaySFX(ButtonPressAudioClip, transform.position, new SFXPlayer.PlayParameters()
            {
                Pitch = Random.Range(0.9f, 1.1f),
                SourceID = -1,
                Volume = 1.0f
            }, 0.0f);
            OnButtonPressed.Invoke();
        }
        else if (m_Pressed && !maxDistanceReached)
        {
            //was just released
            m_Pressed = false;
            SFXPlayer.Instance.PlaySFX(ButtonReleaseAudioClip, transform.position, new SFXPlayer.PlayParameters()
            {
                Pitch = Random.Range(0.9f, 1.1f),
                SourceID = -1,
                Volume = 1.0f
            }, 0.0f);
            OnButtonReleased.Invoke();
        }

        if (LockAfterPress && maxDistanceReached)
        {
            m_Locked = true;
            Debug.Log("Locked");
            return;
        }
    }


#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Handles.DrawLine(transform.position, transform.position + transform.TransformDirection(Axis).normalized * MaxDistance);
    }
#endif
}
