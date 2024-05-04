using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTransform : MonoBehaviour
{
    [SerializeField] private Transform tf;
    [SerializeField] private Rigidbody rb;

    // Update is called once per frame
    void Update()
    {
        tf.position = rb.transform.position;
        tf.rotation = rb.transform.rotation;
    }
}
