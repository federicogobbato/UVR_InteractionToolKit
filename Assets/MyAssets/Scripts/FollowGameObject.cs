using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowGameObject : MonoBehaviour
{
    [SerializeField] Transform m_GameObjectToFollow;

    void Start()
    {
        transform.parent = m_GameObjectToFollow;
    }
}