using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UVR
{
    [RequireComponent(typeof(XRController))]
    public class UV_DetectCollisionsWith : MonoBehaviour
    {
        [SerializeField] XRController m_Controller;
        [SerializeField] LayerMask m_Mask;
        [SerializeField] bool m_DisableTeleportOnCollision = true;

        GlobalVariables.UVR_BaseEvent eTriggerEnter;
        GlobalVariables.UVR_BaseEvent eTriggerExit;


        void Start()
        {
            if (m_Controller == null) GetComponent<XRController>();

            gameObject.AddComponent<Rigidbody>().isKinematic = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            LayerMask maskOther = 1 << other.gameObject.layer;

            if (m_Mask == maskOther)
            {
                if (m_DisableTeleportOnCollision)
                    UV_MasterController.Instance.SetTeleport(m_Controller, false);

                eTriggerEnter?.Invoke();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            LayerMask maskOther = 1 << other.gameObject.layer;

            if (m_Mask == maskOther)
            {
                if (m_DisableTeleportOnCollision)
                    UV_MasterController.Instance.SetTeleport(m_Controller, true);

                eTriggerExit?.Invoke();
            }
        }

    }
}


