using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReticleManager : MonoBehaviour
{
    [SerializeField] Vector3 m_Offset = Vector3.zero;
    [SerializeField] LineRenderer m_LineRenderer;

    Material m_Material;

    void Start()
    {
        m_LineRenderer = GetComponentInParent<LineRenderer>();
        m_Material = GetComponent<MeshRenderer>().material;
    }

    void LateUpdate()
    {
        if (m_LineRenderer && m_Material)
        {
            m_Material.color = m_LineRenderer.endColor;
        }

        transform.position += m_Offset;
    }
}
