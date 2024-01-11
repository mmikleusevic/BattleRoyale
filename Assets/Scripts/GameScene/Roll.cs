using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Roll : NetworkBehaviour
{
    private float interactDistance = 0.51f;

    protected float rotationSpeed = 150f;
    protected float rotationTime = 2f;
    protected float factor;

    protected int GetSideAndRotate(Vector3 direction, Vector3 cameraPosition, GameObject die)
    {
        Physics.Raycast(cameraPosition, direction, out RaycastHit raycastHit, interactDistance);

        int.TryParse(raycastHit.collider.name, out int result);

        Vector3 side = GetSide(result);

        StartCoroutine(RotateToward(side, die));

        return result;
    }

    protected Vector3 GetSide(int number)
    {
        Vector3 side = Vector3.zero;

        switch (number)
        {
            case 1:
                side = Vector3.right;
                break;
            case 2:
                side = Vector3.up;
                break;
            case 3:
                side = Vector3.back;
                break;
            case 4:
                side = Vector3.forward;
                break;
            case 5:
                side = Vector3.down;
                break;
            case 6:
                side = Vector3.left;
                break;
        }

        return side;
    }

    protected IEnumerator RotateToward(Vector3 side, GameObject die)
    {
        float timer = 1f;
        float speed = 10f * Time.deltaTime;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            Quaternion rotation = Quaternion.LookRotation(side);

            die.transform.rotation = Quaternion.Slerp(die.transform.rotation, rotation, speed);

            yield return null;
        }
    }
}
