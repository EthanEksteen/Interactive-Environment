using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpController : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] Transform holdArea;
    private GameObject heldObj;
    private Rigidbody heldObjRB;

    [Header("Physics Parameters")]
    [SerializeField] private float pickUpRange = 5.0f;
    [SerializeField] private float pickUpForce = 150.0f;
    [SerializeField] private float pickUpMass = 100.0f;

    // [SerializeField] Animator animator;
    Animator animator;

    // Use this for initialization
    void Start()
    {
       
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0)) 
        {
            if(heldObj == null) 
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickUpRange))
                {
                    // Pick up object
                    PickUpObject(hit.transform.gameObject);
                }
            }
            else 
            {
                // Drop object
                DropObject();
            }
        }
        if(heldObj != null) 
        {
            // Move object
            MoveObject();
        }
    }

    void PickUpObject(GameObject pickObj)
    {
        if (pickObj.gameObject.tag == "CanOpen")
        {
            if (pickObj.transform.parent.parent.parent.GetComponent<Animator>())
            {
                animator = pickObj.transform.parent.parent.parent.GetComponent<Animator>();
                if (animator.GetBool("open"))
                {
                    animator.SetBool("open", false);
                }
                else
                {
                    animator.SetBool("open", true);
                }
            }
        }

        if (pickObj.GetComponent<Rigidbody>())
        {
            if(pickObj.GetComponent <Rigidbody>().mass <= pickUpMass) 
            {
                heldObjRB = pickObj.GetComponent<Rigidbody>();
                heldObjRB.useGravity = false;
                heldObjRB.drag = 10;
                heldObjRB.angularDrag = 4;
                // heldObjRB.constraints = RigidbodyConstraints.FreezeRotation;

                heldObjRB.transform.parent = holdArea;
                heldObj = pickObj;
            }
        }
    }

    void DropObject()
    {
        heldObjRB.useGravity = true;
        heldObjRB.drag = 1;
        heldObjRB.angularDrag = 0.05f;
        // heldObjRB.constraints = RigidbodyConstraints.None;

        heldObjRB.transform.parent = null;
        heldObj = null;
    }

    void MoveObject()
    {
        if (Vector3.Distance(heldObj.transform.position, holdArea.position) > 0.1f)
        {
            Vector3 moveDirection = (holdArea.position - heldObj.transform.position);
            heldObjRB.AddForce(moveDirection * pickUpForce);
        }
    }

}
