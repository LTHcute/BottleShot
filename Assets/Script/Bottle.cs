using UnityEngine;

public class Bottle : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Debug thông tin khởi tạo
        Collider2D collider = GetComponent<Collider2D>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        GameObject crosshair = GameObject.FindWithTag("Crosshair");

  

       
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.identity;
        // Debug vị trí mỗi frame
        GameObject crosshair = GameObject.FindWithTag("Crosshair");
      
    }


    public void OnTriggerStay2D(Collider2D collision)
    {
        Debug.Log("VA");
        if (Input.GetMouseButtonDown(0) && !Application.isMobilePlatform)
        {
            Debug.Log("CHạm");
            if (collision.gameObject.CompareTag("Crosshair"))
            {
                Debug.Log($"Bottle {gameObject.name} collided with Crosshair at position: {transform.position}");
                // Thêm logic, ví dụ: hủy bottle
                Destroy(gameObject);
            }
        }
        else if (Input.touchCount > 0 && Application.isMobilePlatform)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Debug.Log("Chạm");
                if (collision.gameObject.CompareTag("Crosshair"))
                {
                    Debug.Log($"Bottle {gameObject.name} collided with Crosshair at position: {transform.position}");
                    // Thêm logic, ví dụ: hủy bottle
                    Destroy(gameObject);
                }
            }
        }
    }
}
