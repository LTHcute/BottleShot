using UnityEngine;
using UnityEngine.UI;

public class ShotGun : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Image crosshair;
    public Camera mainCamera;
    private void Start()
    {
      //Vector3 localPos = transform.localPosition;
      //  Debug.Log($"{}");
    }

    private void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Lấy vị trí tâm ngắm
            Vector2 crosshairPos = crosshair.transform.position;

            // Chuyển vị trí tâm ngắm sang tọa độ màn hình
            Vector2 crosshairScreenPos = mainCamera.WorldToScreenPoint(crosshairPos);

            // Kiểm tra xem tâm ngắm có đang giao với chai nào không
            Collider2D hitCollider = Physics2D.OverlapPoint(crosshairPos);

            if (hitCollider != null && hitCollider.CompareTag("Bottle"))
            {
                // Nếu tâm ngắm giao với chai, phá chai
                Destroy(hitCollider.gameObject);
                Debug.Log("Chai bị vỡ!");
            }
        }
    }


   



}
