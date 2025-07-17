using UnityEngine;

public class BottleCircleRotator : MonoBehaviour
{
   public float rotationSpeed = 30f;
    private bool isBroken = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
    public void Break()
    {
        if(!isBroken)
        {
            isBroken = true;
            Debug.Log("Vỡ chai");
            Destroy(gameObject);
        }    

    }    
}
