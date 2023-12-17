using UnityEngine;

public class Rotater : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private Transform objTransform;

    private void Update() 
    {
        objTransform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}