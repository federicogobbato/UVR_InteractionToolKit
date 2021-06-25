using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace UVR.NetworkPhoton
{

    public class PUN_PlayerManagerVR : PUN_BasePlayerManager, IPunObservable
    {
        // GameObjects that must be shown to others client
        [SerializeField] Transform m_RemoteHead;
        [SerializeField] Transform m_RemoteHandLeft;
        [SerializeField] Transform m_RemoteHandRight;

        Animator m_AnimatorRightHand;
        Animator m_AnimatorLeftHand;

        // GameObjects used to track position and rotation of the remote ones
        Transform m_VRCamera;
        Transform m_LeftHand;
        Transform m_RightHand;

        bool m_JoinedRoom = false;

        public void Start()
        {
            if(m_RemoteHandRight)
                m_AnimatorRightHand = m_RemoteHandRight.GetComponent<Animator>();
            if(m_RemoteHandLeft)
                m_AnimatorLeftHand = m_RemoteHandLeft.GetComponent<Animator>();

            if (photonView.AmOwner)
            {
                Debug.Log("Player VR joined room");

                UV_MasterController masterController = UV_MasterController.Instance;

                masterController.OnPlayPointAnimation += PlayHandAnimation;
                masterController.OnPlayGrabAnimation += PlayTriggerHandAnimation;

                m_VRCamera = masterController.MainCamera.transform;
                m_LeftHand = masterController.LeftDirectInteractor.gameObject.transform;
                m_RightHand = masterController.RightDirectInteractor.gameObject.transform;

                m_RemoteHandLeft.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
                m_RemoteHandRight.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;

                m_JoinedRoom = true;
            }
        }


        public new void OnDisable()
        {
            if (photonView.AmOwner)
            {
                UV_MasterController.Instance.OnPlayPointAnimation -= PlayHandAnimation;
                UV_MasterController.Instance.OnPlayGrabAnimation -= PlayTriggerHandAnimation;
            }

            base.OnDisable();
        }


        ///<summary>
        /// Update transform of local hands and head
        ///</summary>
        void Update()
        {
            if (photonView.AmOwner)
            {
                if(m_RemoteHead)
                {
                    m_RemoteHead.position = m_VRCamera.position;
                    m_RemoteHead.rotation = m_VRCamera.rotation;
                }

                if (m_RemoteHandRight)
                {
                    m_RemoteHandRight.position = m_RightHand.position;
                    m_RemoteHandRight.rotation = m_RightHand.rotation;
                }

                if (m_RemoteHandLeft)
                {
                    m_RemoteHandLeft.position = m_LeftHand.position;
                    m_RemoteHandLeft.rotation = m_LeftHand.rotation;
                }
            }
        }

        /// <summary>
        /// Allow to play other animations not controllled by XRController, as the Pointing animation before a teleport 
        /// </summary>
        /// <param name="handNode"></param>
        /// <param name="transiction"></param>
        /// <param name="state"></param>
        public void PlayHandAnimation(XRNode handNode, string transiction, bool state)
        {
            if (m_AnimatorRightHand && handNode == XRNode.RightHand)
                m_AnimatorRightHand.SetBool(transiction, state);
            else if (m_AnimatorLeftHand && handNode == XRNode.LeftHand)
                m_AnimatorLeftHand.SetBool(transiction, state);
        }

        private void PlayTriggerHandAnimation(XRNode handNode, string transiction, bool reset)
        {
            if (handNode == XRNode.RightHand)
            {
                if (!m_AnimatorRightHand) return;
                    
                if (!reset)
                    m_AnimatorRightHand.SetTrigger(transiction);
                else
                    m_AnimatorRightHand.ResetTrigger(transiction);
            }
            else if (handNode == XRNode.LeftHand)
            {
                if (!m_AnimatorLeftHand) return;

                if (!reset)
                    m_AnimatorLeftHand.SetTrigger(transiction);
                else
                    m_AnimatorLeftHand.ResetTrigger(transiction);
            }

            // Save the current animator transition info. Later that info are sent to the remote players 
            if (photonView.AmOwner)
            {
                lastTriggers.Add(new LastTriggetAnimationPlayed() { node = handNode, transictionName = transiction, reset = reset });
            }
        }


        #region IPunObservable implementation

        private List<LastTriggetAnimationPlayed> lastTriggers = new List<LastTriggetAnimationPlayed>();

        public class LastTriggetAnimationPlayed
        {
            public XRNode node;
            public string transictionName;
            public bool reset;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                if (lastTriggers.Count > 0)
                {
                    // We own this player: send the others our data
                    // Send first the numbers of trigger transitions saved
                    stream.SendNext(lastTriggers.Count);

                    foreach (var trigger in lastTriggers)
                    {
                        stream.SendNext(trigger.node);
                        stream.SendNext(trigger.transictionName);
                        stream.SendNext(trigger.reset);
                    }
                    lastTriggers.Clear();
                }
            }
            else
            {
                // Network player, receive data
                int triggersCount = (int)stream.ReceiveNext();

                // Extract each animator transiction info sent by the remote player and set the triggers on the local Animator 
                for (int i = 0; i < triggersCount; i++)
                {
                    LastTriggetAnimationPlayed remoteTrigger = new LastTriggetAnimationPlayed()
                    {
                        node = (XRNode)stream.ReceiveNext(),
                        transictionName = (string)stream.ReceiveNext(),
                        reset = (bool)stream.ReceiveNext()
                    };

                    PlayTriggerHandAnimation(remoteTrigger.node, remoteTrigger.transictionName, remoteTrigger.reset);
                }

                Debug.Log("Lag: " + Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime)));
            }
        }

        #endregion
    }
}
