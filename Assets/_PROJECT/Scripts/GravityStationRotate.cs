using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityStationRotate : MonoBehaviour
{
    [SerializeField] float dump = 1;

    float rotat = 0;
    // Update is called once per frame
    void Update()
    {
        Vector3 rot = transform.rotation.eulerAngles;

        rot.z = rotat * Time.deltaTime * dump;

        //if (rot.z > 360)
        //    rot.z = 0;

        //transform.eulerAngles = rot;
        transform.Rotate(Vector3.forward,/* rotat **/ Time.deltaTime * dump);
    }
}
