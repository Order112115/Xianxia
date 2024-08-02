using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cemeraController : MonoBehaviour
{
    [SerializeField] int sens;
    [SerializeField] int lockVertMin, lockVertMax;
    [SerializeField] bool invertY;

    float rotx;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseY = Input.GetAxis("Mouse Y") * sens * Time.deltaTime;
        float mouseX = Input.GetAxis("Mouse X") * sens * Time.deltaTime;

        if (invertY)
            rotx += mouseY;
        else
            rotx -= mouseY;

        rotx = Mathf.Clamp(rotx, lockVertMin, lockVertMax);

        transform.localRotation = Quaternion.Euler(rotx, 0, 0);

        transform.parent.Rotate(Vector3.up * mouseX);
    }
}
