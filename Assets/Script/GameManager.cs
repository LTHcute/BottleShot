using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject crosshair;
    public Camera mainCamera;
    public LayerMask bottleLayer;


    private void Update()
    {
      
        //if(Input.touchCount> 0 && Input.GetTouch(0).phase == TouchPhase.Began || Input.GetMouseButtonDown(0))
        //{
            Vector2 crosshairPos = crosshair.transform.position;
        Collider2D[] hits = Physics2D.OverlapPointAll(crosshairPos, bottleLayer);
        Debug.Log($"hits:{hits.Length}");
        Debug.Log($"crosshairPos:{crosshairPos}");
      //  Debug.DrawRay(crosshairPos, Vector2.up * 0.1f, Color.red, 1f);
        Collider2D hit = Physics2D.OverlapPoint(crosshairPos, bottleLayer);
        if (hit == null)
        {
            Debug.Log("Hit null");
        }    
        //if (hit.CompareTag("Bottle") == false)
        //{
        //    Debug.Log("false");
        //}    
        if (hit != null && hit.CompareTag("Bottle"))
        {
            Debug.Log("Vỡ");
        }
        else
        {
            Debug.Log("No hit");
        }
       // }    
    }


}
