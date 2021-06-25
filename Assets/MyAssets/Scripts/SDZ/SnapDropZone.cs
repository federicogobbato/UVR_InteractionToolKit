using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace UVR
{
    [RequireComponent(typeof(Collider))]
    [DisallowMultipleComponent]
    public class SnapDropZone : MonoBehaviour
    {
        public enum Type
        {
            CLONE,
            DROP,
            DROP_AND_CLONE,
            DROP_AND_DISAPPEAR, 
            DROP_EVERYTHING 
        }

        public enum Connection
        {
            IS_KINEMATIC,
            JOINT,
            NONE
        }

        #region EVENTS

        public GlobalVariables.UVR_BaseEvent OnEnter;
        public GlobalVariables.UVR_BaseEvent OnExit;
        public GlobalVariables.UVR_BaseEvent OnDrop;

        #endregion

        #region FIELDS

        [Tooltip("The object is visible in editor and the BaseDropMaterial is applied")]
        [SerializeField] bool m_ShowInEditor = false;
        [Tooltip("Make visible the sphere collider used to detect OnTriggerEnter and Exit from the SDZ")]
        [SerializeField] bool m_ShowSphereCollider = false;

        [SerializeField] SnapDropZoneTag m_SampleObject;
        [SerializeField] Type m_Type = Type.DROP;
        [SerializeField] bool m_ShowBaseMeshMaterial = true;
        [SerializeField] bool m_ShowValidMeshMaterial = true;
        [Tooltip("Physic behaviout after drop")]
        [SerializeField] Connection m_Connection = Connection.IS_KINEMATIC;

        [Tooltip("Define if the drop is valid also when the object in thrown and not grabbed")]
        [SerializeField] bool m_DroppableOnThrow = false;
        [SerializeField] bool m_GrabbableAfterDrop = false;
        [SerializeField] float m_DelayBeforeDisappear = 1.0f;
        [SerializeField] Rigidbody m_RigidbodyToConnect;

        [SerializeField] Material m_BaseDropMaterial;
        [SerializeField] Material m_ValidDropMaterial;
        [SerializeField] Material m_SphereColliderMaterial;

        [SerializeField, HideInInspector] GameObject m_SampleEditor;
        [SerializeField, HideInInspector] GameObject m_SphereColliderMesh;


        Collider m_DropZoneCollider;
        GameObject m_ObjectToDrop;
        GameObject m_ObjectToClone;
        GameObject m_ClonedObject;
        GameObject m_DroppedObject;

        bool m_WaitBeforeDetectCollision = false;   
        bool m_DroppedGenericObject = false;

        #endregion


        #region EDITOR METHODS

        /// <summary>
        /// Look for the SampleEditor in the children array of the SDZ 
        /// </summary>
        public void LookForSampleObject()
        {
            if(m_Type == Type.DROP_EVERYTHING)
            {
                m_SampleObject = null;
                m_Connection = Connection.IS_KINEMATIC;
                m_GrabbableAfterDrop = true;
                return;
            }

            Transform[] childrens = gameObject.GetComponentsInChildren<Transform>();

            if (!m_SampleEditor && m_SampleObject)
            {
                foreach (Transform child in childrens)
                {
                    if (child.gameObject.name == m_SampleObject.Label + " EditorSample")
                    {
                        m_SampleEditor = child.gameObject;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Look for the SphereCollider in the children array of the SDZ and  
        /// </summary>
        public void LookForSphereCollider()
        {
            Transform[] childrens = gameObject.GetComponentsInChildren<Transform>();

            if (!m_SphereColliderMesh)
            {
                bool found = false;
                foreach (Transform child in childrens)
                {
                    if (child.gameObject.name == "SphereCollider")
                    {
                        if (!found)
                        {
                            m_SphereColliderMesh = child.gameObject;
                            found = true;
                        }
                        else
                        {
                            DestroyImmediate(child.gameObject);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Destroy SampleObject if a copy is found in the scene, so if a prefab is not used 
        /// </summary>
        public bool CheckThisIsPrefab()
        {
            var clones = GameObject.FindObjectsOfType<GameObject>().Where(obj => obj.GetInstanceID() == m_SampleObject.gameObject.GetInstanceID());
            if (clones.Count() > 0)
            {
                m_SampleObject = null;
                DestroySample();
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Generate the sample object, visible in editor and destroyed when the scene start
        /// </summary>
        public void GenerateSampleEditorObject()
        {
            LookForSampleObject();

            if (!m_SampleEditor && m_SampleObject)
            {
                m_SampleEditor = Instantiate(m_SampleObject.gameObject, transform.position, transform.rotation, transform);

                if (m_Type != Type.CLONE)
                {
                    Material[] newMaterials = new Material[1];
                    newMaterials[0] = m_BaseDropMaterial;

                    ChangeMeshMaterials(m_SampleEditor, newMaterials);
                }

                //Disable the collider so when the scene start no collisons are detected before the sample is destroyed
                Collider[] sampleColliders = m_SampleEditor.GetComponentsInChildren<Collider>();
                foreach (var coll in sampleColliders)
                {
                    coll.enabled = false;
                }

                m_SampleEditor.name = m_SampleObject.Label + " EditorSample";
            }
        }

        /// <summary>
        /// Generate a gameObject with a mesh renderer and mesh filter that represent the SnapDropZone collider
        /// </summary>
        public void GenerateSphereCollider()
        {
            LookForSphereCollider();

            //size of the of mesh 
            float radius = ((SphereCollider)GetComponent<Collider>()).radius * 2;
            //position of the collider
            Vector3 offset = ((SphereCollider)GetComponent<Collider>()).center;

            if (!m_SphereColliderMesh)
            {
                m_SphereColliderMesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                m_SphereColliderMesh.name = "SphereCollider";

                DestroyImmediate(m_SphereColliderMesh.GetComponent<Collider>());

                m_SphereColliderMesh.GetComponent<MeshRenderer>().sharedMaterial = m_SphereColliderMaterial;

                m_SphereColliderMesh.transform.localScale = new Vector3(radius, radius, radius);

                m_SphereColliderMesh.transform.SetPositionAndRotation(transform.position, transform.rotation);
                m_SphereColliderMesh.transform.parent = transform;

                m_SphereColliderMesh.transform.localPosition += offset;
            }
            else if (Math.Abs(m_SphereColliderMesh.transform.localScale.x - radius) > 0.001 ||
                     Math.Abs(m_SphereColliderMesh.transform.localPosition.x - offset.x) > 0.001 ||
                     Math.Abs(m_SphereColliderMesh.transform.localPosition.y - offset.y) > 0.001 ||
                     Math.Abs(m_SphereColliderMesh.transform.localPosition.z - offset.z) > 0.001)
            {
                DestroyImmediate(m_SphereColliderMesh);
            }
        }


        /// <summary>
        /// Destroy the sample object visible in editor
        /// </summary>
        public void DestroySample()
        {
            DestroyImmediate(m_SampleEditor);
        }


        /// <summary>
        /// Destroy the SphereCollider representation
        /// </summary>
        public void DestroySphereCollider()
        {
            DestroyImmediate(m_SphereColliderMesh);
        }

        /// <summary>
        /// Follow the SnapDropZone when its position change 
        /// </summary>
        public void FollowSDZ()
        {
            if (m_SampleEditor)
            {
                m_SampleEditor.transform.position = transform.position;
                m_SampleEditor.transform.rotation = transform.rotation;
            }
        }

        /// <summary>
        /// Called when an inspector variable change
        /// </summary>
        //void OnValidate()
        //}

        #endregion


        #region START & AWAKE

        void Awake()
        {
            LookForSampleObject();
            LookForSphereCollider();
            Destroy(m_SampleEditor);
        }


        void Start()
        {
            m_DropZoneCollider = GetComponent<Collider>();
            m_DropZoneCollider.isTrigger = true;

            // If the sample object is NULL every object can be dropped;
            if (m_SampleObject)
            {
                if (m_Type == Type.CLONE)
                {
                    GenerateClonableObject();
                }
                else
                {
                    GenerateObjectToDrop();
                }
            }
            else
            {
                m_Type = Type.DROP_EVERYTHING;
                m_Connection = Connection.IS_KINEMATIC;
                m_GrabbableAfterDrop = true;
                m_DroppableOnThrow = false;
            }

            if (m_Connection == Connection.JOINT && !m_RigidbodyToConnect)
            {
                var parentRB = transform.parent.GetComponent<Rigidbody>();
                m_RigidbodyToConnect = parentRB ? parentRB : GetComponent<Rigidbody>();
            }
        }

        #endregion


        #region GENERATE_OBJECT

        /// <summary>
        /// Generate the object that represent the object that can dropped inside the SDZ
        /// </summary>
        private void GenerateObjectToDrop()
        {
            m_ObjectToDrop = GenerateObject();

            // Change the mesh material of the ObjectToDrop
            ShowBaseObjectToDropMaterials();

            Destroy(m_ObjectToDrop.GetComponent<XRGrabInteractable>());
        }

        /// <summary>
        /// Generate the first clone grabbable
        /// </summary>
        private void GenerateClonableObject()
        {
            m_ObjectToClone = GenerateObject();

            m_ObjectToClone.GetComponent<XRGrabInteractable>().onSelectEntered.AddListener(new UnityAction<XRBaseInteractor>(CloneGrabbed));
            m_ObjectToClone.GetComponent<XRGrabInteractable>().onSelectExited.AddListener(new UnityAction<XRBaseInteractor>(CloneUngrabbed));
        }


        private GameObject GenerateObject()
        {
            GameObject obj = Instantiate(m_SampleObject.gameObject, transform.position, transform.rotation, transform);
            obj.name = m_SampleObject.GetComponentInChildren<SnapDropZoneTag>().Label + " Clone";

            if (obj.GetComponent<Rigidbody>())
            {
                obj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            }

            SetTriggerColliders(obj, true);

            return obj;
        }

        /// <summary>
        /// Set the trigget of all colliders on the current object and its children
        /// </summary>
        private void SetTriggerColliders(GameObject obj, bool state)
        {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            foreach (Collider c in colliders)
            {
                c.isTrigger = state;
            }
        }

        #endregion


        #region COLLISION DETECTION


        /// <summary>
        ///  When an object with the same SnapDropZoneDataTag of the SampleObject enter inside the collider of the SDZ something happen......
        /// </summary>
        private void OnTriggerEnter(Collider collider)
        {
            SnapDropZoneTag snapDropZoneTag = collider.gameObject.GetComponentInParent<SnapDropZoneTag>();

            if (snapDropZoneTag == null)
                return;

            // Checks if the collide.gameobjct is grabbed
            XRGrabInteractable xrGrabInteractable = collider.gameObject.GetComponentInParent<XRGrabInteractable>();

            if ((!xrGrabInteractable || !xrGrabInteractable.selectingInteractor) &&
                !m_DroppableOnThrow)
                return;

            if (m_Type != Type.CLONE && m_ObjectToDrop && m_DroppedObject == null &&
                collider.gameObject != m_ObjectToDrop &&
                collider.gameObject != m_SampleObject &&
                collider.gameObject.GetComponentInParent<SnapDropZoneTag>().Label == m_ObjectToDrop.GetComponentInChildren<SnapDropZoneTag>().Label)
            {
                // Change the mesh material of the ObjectToDrop
                ShowValidObjectToDropMaterials();

                // Register the object that is inside the collider 
                m_DroppedObject = snapDropZoneTag.Root;

                if (xrGrabInteractable && xrGrabInteractable.selectingInteractor)
                {
                    xrGrabInteractable.onSelectExited.AddListener(new UnityAction<XRBaseInteractor>(ObjectDropped));
                }
                else if (m_DroppableOnThrow) // if the DroppedObject is not grabbed can be thrown
                {
                    Debug.Log("THROWN");
                    ObjectThrownInside();
                }

                OnEnter?.Invoke();
            }
            else if (m_Type == Type.DROP_EVERYTHING && m_DroppedObject == null &&
                    collider.gameObject.GetComponentInParent<SnapDropZoneTag>().Label == "all")
            {
                //...we register the object that is inside the collider 
                m_DroppedObject = snapDropZoneTag.Root;

                if (xrGrabInteractable && xrGrabInteractable.selectingInteractor)
                {
                    xrGrabInteractable.onSelectExited.AddListener(new UnityAction<XRBaseInteractor>(ObjectDropped));
                }

                OnEnter?.Invoke();
            }
        }


        /// <summary>
        /// If the clone or the droppedObject go out from the SDZ they are not controlled anymore by the SDZ
        /// </summary>
        private void OnTriggerExit(Collider collider)
        {
            if (m_WaitBeforeDetectCollision)
            {
                m_WaitBeforeDetectCollision = false;
                return;
            }

            SnapDropZoneTag snapDropZoneDataTag = collider.gameObject.GetComponentInParent<SnapDropZoneTag>();

            if (snapDropZoneDataTag == null)
                return;

            if (snapDropZoneDataTag.Root == m_ClonedObject)
            {
                m_ClonedObject.GetComponentInParent<XRGrabInteractable>().onSelectEntered.RemoveListener(CloneGrabbed);
                m_ClonedObject.GetComponentInParent<XRGrabInteractable>().onSelectExited.RemoveListener(CloneUngrabbed);

                SetTriggerColliders(m_ClonedObject, false);

                m_ClonedObject = null;

                OnExit?.Invoke();
            }
            else if (snapDropZoneDataTag.Root == m_DroppedObject && !m_DroppedGenericObject)
            {
                ShowBaseObjectToDropMaterials();

                SetTriggerColliders(m_DroppedObject, false);

                m_DroppedObject.GetComponentInParent<XRGrabInteractable>().onSelectExited.RemoveListener(ObjectDropped);
                
                m_DroppedObject = null;

                OnExit?.Invoke();
            }
        }

        #endregion

        #region LISTENERS

        /// <summary>
        /// Called when the object is ungrabbed inside the SnapDropZone
        /// </summary>
        private void ObjectDropped(XRBaseInteractor arg0)
        {
            m_DroppedObject.GetComponent<XRGrabInteractable>().onSelectExited.RemoveListener(ObjectDropped);

            if (m_Type == Type.DROP_EVERYTHING && m_DroppedObject)
            {
                m_DroppedGenericObject = true;
            }
            else if (!m_GrabbableAfterDrop)
            {
                Destroy(m_ObjectToDrop);
                DestroyAndUngrabDroppingObject(m_DroppedObject);
            }
            else if (m_ObjectToDrop)
            {
                m_ObjectToDrop.SetActive(false);
                DestroyAndUngrabDroppingObject(m_DroppedObject);
            }

            LateDrop();

            OnDrop?.Invoke();
        }

        /// <summary>
        /// Called when the dropped object is grabbed
        /// </summary>
        private void DroppedObjectGrabbed(XRBaseInteractor arg0)
        {
            m_DroppedGenericObject = false;

            m_DroppedObject.GetComponent<XRGrabInteractable>().onSelectEntered.RemoveListener(DroppedObjectGrabbed);

            ObjectInsideSnapDropZoneGrabbed(m_DroppedObject);

            // Renable the SDZ collider and the ObjecToDrop for next object drops
            if(m_ObjectToDrop) m_ObjectToDrop.SetActive(true);

            m_DroppedObject = null;
        }


        /// <summary>
        /// When a clonable object is grabbed a new clone is generated
        /// </summary>
        private void CloneGrabbed(XRBaseInteractor arg0)
        {
            m_ClonedObject = m_ObjectToClone;

            ObjectInsideSnapDropZoneGrabbed(m_ClonedObject);

            GenerateClonableObject();
        }


        /// <summary>
        /// If the clone is ungrabbed inside the SDZ, it's destoyed 
        /// </summary>
        private void CloneUngrabbed(XRBaseInteractor arg0)
        {
            m_ClonedObject.GetComponent<XRGrabInteractable>().onSelectEntered.RemoveListener(CloneGrabbed);
            m_ClonedObject.GetComponent<XRGrabInteractable>().onSelectExited.RemoveListener(CloneUngrabbed);

            DestroyAndUngrabDroppingObject(m_ClonedObject);
        }


        private void ObjectInsideSnapDropZoneGrabbed(GameObject obj)
        {
            obj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

            obj.transform.parent = null;

            //if I dont set this variable OnTriggerExit is detected before the object is grabbed 
            m_WaitBeforeDetectCollision = true;
        }

        #endregion


        /// <summary>
        /// What happen after drop? ........
        /// </summary>
        private void LateDrop()
        {
            if (m_Type == Type.DROP_EVERYTHING)
            {
                m_DroppedObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                SetTriggerColliders(m_DroppedObject, true);
                m_DroppedObject.transform.parent = gameObject.transform;
                m_DroppedObject.transform.localPosition = Vector3.zero;
                m_DroppedObject.transform.localRotation = Quaternion.identity;
                m_DroppedObject.GetComponent<XRGrabInteractable>().onSelectEntered.AddListener(DroppedObjectGrabbed);
            }
            else if (m_Type == Type.DROP_AND_CLONE)
            {
                m_Type = Type.CLONE;
                //...the dropped object became clonable
                GenerateClonableObject();
            }
            else
            {
                //...1 Instantiate a new object with the original material applyed
                m_DroppedObject = GenerateObject();
                //...2 Disable the SDZ collider to prevent other object drops  
                m_DropZoneCollider.enabled = m_GrabbableAfterDrop;

                XRGrabInteractable xrGrabInteractable = m_DroppedObject.GetComponent<XRGrabInteractable>();

                if (m_Type == Type.DROP_AND_DISAPPEAR)
                {
                    //...3 the object became ungrabbable 
                    Destroy(xrGrabInteractable);
                    //...4 a new object can be dropped inside the SDZ after a delay
                    StartCoroutine(DisappearAfterDrop());
                }
                else
                {
                    if (!m_GrabbableAfterDrop)
                    {
                        //...3 the object is not grabbable afer drop
                        Destroy(xrGrabInteractable);
                    }

                    if (m_Connection == Connection.IS_KINEMATIC)
                    {
                        if (m_GrabbableAfterDrop)
                        {
                            //...3 the object is grabbable after drop
                            xrGrabInteractable.onSelectEntered.AddListener(DroppedObjectGrabbed);
                        }
                    }
                    else if (m_Connection == Connection.NONE)
                    {
                        //...4 is not kinematic after drop
                        if (m_DroppedObject.GetComponent<Rigidbody>())
                        {
                            m_DroppedObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                        }
                        m_DroppedObject.transform.parent = null;
                    }
                    else if (m_Connection == Connection.JOINT &&
                            m_RigidbodyToConnect &&
                            m_DroppedObject.GetComponent<Joint>())
                    {
                        //...4 is connected with the rigidbody on the parent of the SDZ or on the SDZ itself                       
                        m_DroppedObject.GetComponent<Joint>().connectedBody = m_RigidbodyToConnect;

                        StartCoroutine(LateJointAfterDrop());
                    }
                }
            }
        }


        private IEnumerator LateJointAfterDrop()
        {
            yield return new WaitForEndOfFrame();

            m_DroppedObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            SetTriggerColliders(m_DroppedObject, false);
            m_DroppedObject.transform.parent = m_RigidbodyToConnect.transform;
        }


        /// <summary>
        /// The ObjectToDrop get its original material and the DroppedObject is destroyed
        /// </summary>
        private void ObjectThrownInside()
        {
            Destroy(m_DroppedObject);

            m_ObjectToDrop.SetActive(false);

            LateDrop();
        }


        /// <summary>
        /// Destroy the actual DroppingObject and generate a new one 
        /// </summary>
        private IEnumerator DisappearAfterDrop()
        {
            yield return new WaitForSeconds(m_DelayBeforeDisappear);

            Destroy(m_DroppedObject);
            //Renable the SDZ collider 
            m_DropZoneCollider.enabled = true;

            m_ObjectToDrop.SetActive(true);

            ShowBaseObjectToDropMaterials();
        }


        /// <summary>
        /// Destroy an object ungrabbed inside the snapdropzone, force the untouch first
        /// </summary>
        private void DestroyAndUngrabDroppingObject(GameObject objectToDestroy)
        {
            m_WaitBeforeDetectCollision = true;

            objectToDestroy.GetComponent<XRBaseInteractable>().colliders.Clear();
            
            Destroy(objectToDestroy);
        }


        #region CHANGE MATERIALS

        private void ShowBaseObjectToDropMaterials()
        {
            if (!m_ObjectToDrop) return;

            if (m_ShowBaseMeshMaterial)
            {
                Material[] newMaterials = new Material[1];
                newMaterials[0] = m_BaseDropMaterial;
                ChangeMeshMaterials(m_ObjectToDrop, newMaterials);
            }
            else
            {
                MeshRenderer[] meshRends = m_ObjectToDrop.GetComponentsInChildren<MeshRenderer>();
                foreach (var mr in meshRends)
                {
                    mr.enabled = false;
                }
            }
        }


        private void ShowValidObjectToDropMaterials()
        {
            if (!m_ObjectToDrop) return;

            if (m_ShowValidMeshMaterial)
            {
                //....check the mesh renderer is enabled
                MeshRenderer[] meshRends = m_ObjectToDrop.GetComponentsInChildren<MeshRenderer>();
                foreach (var mr in meshRends)
                {
                    mr.enabled = true;
                }

                //......the ObjectToDrop change color
                Material[] newMaterials = new Material[1];
                newMaterials[0] = m_ValidDropMaterial;

                ChangeMeshMaterials(m_ObjectToDrop, newMaterials);
            }
        }


        /// <summary>
        /// Change the materials of the object passed and its children
        /// </summary>
        private void ChangeMeshMaterials(GameObject obj, Material[] newMaterials)
        {
            if (obj.GetComponent<MeshRenderer>())
            {
                obj.GetComponent<MeshRenderer>().materials = newMaterials;
            }

            //Change the material of the children 
            MeshRenderer[] meshInChildren = obj.GetComponentsInChildren<MeshRenderer>();

            if (meshInChildren.Length > 0)
            {
                foreach (MeshRenderer m in meshInChildren)
                {
                    if (m_SphereColliderMesh &&
                        m == m_SphereColliderMesh.GetComponent<MeshRenderer>())
                        continue;

                    m.materials = newMaterials;
                }
            }
        }

        #endregion
    }
}
