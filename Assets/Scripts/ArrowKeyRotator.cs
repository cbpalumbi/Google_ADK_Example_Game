using UnityEngine;

public class ArrowKeyRotator : MonoBehaviour
{
    public float rotationSpeed = 100f;

    void Update()
    {
        if (Input.GetKey(KeyCode.J))
        {
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime, Space.Self);
        }

        if (Input.GetKey(KeyCode.L))
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        }

        if (Input.GetKey(KeyCode.I))
        {
            transform.Rotate(Vector3.right, -rotationSpeed * Time.deltaTime, Space.Self);
        }

        if (Input.GetKey(KeyCode.K))
        {
            transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}
