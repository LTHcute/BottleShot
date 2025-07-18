using System;
using UnityEngine;
using UnityEngine.LightTransport;
using UnityEngine.UIElements;
using static Unity.Burst.Intrinsics.X86.Avx;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Gun : MonoBehaviour
{

    [SerializeField] private float moveDistance = 2.0f; // Khoảng cách di chuyển lên
    [SerializeField] private float moveSpeed = 10f; // Tốc độ di chuyển lên
    [SerializeField] private float returnSpeed = 10f; // Tốc độ trở về
    [SerializeField] private float moveDuration = 0.3f; // Thời gian di chuyển lên
    [SerializeField] private bool useWorldPosition = false; // Tùy chọn dùng world position

    private Vector3 originalPosition; // Vị trí ban đầu
    private bool isMoving = false; // Trạng thái di chuyển
    private float moveTimer = 0f; // Bộ đếm thời gian
    private Vector3 targetPosition; // Vị trí đích khi di chuyển lên

    private void Start()
    {
        // Lưu vị trí ban đầu
        originalPosition = useWorldPosition ? transform.position : transform.localPosition;
      //  Debug.Log("GunMoveUpDown initialized at " + (useWorldPosition ? "world" : "local") + " position: " + originalPosition);
    }

    private void Update()
    {
        // Kiểm tra input: Mouse cho Editor, Touch cho di động
        if (Input.GetMouseButtonDown(0) && !Application.isMobilePlatform)
        {
          //  Debug.Log("Mouse click detected at: " + Input.mousePosition);
            if (!isMoving)
            {
                isMoving = true;
                moveTimer = 0f;
                targetPosition = originalPosition + new Vector3(0, moveDistance, 0); // Di chuyển lên theo trục Y
            }
        }
        else if (Input.touchCount > 0 && Application.isMobilePlatform)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
            //    Debug.Log("Touch detected at: " + touch.position);
                if (!isMoving)
                {
                    isMoving = true;
                    moveTimer = 0f;
                    targetPosition = originalPosition + new Vector3(0, moveDistance, 0); // Di chuyển lên theo trục Y
                }
            }
        }

        // Xử lý di chuyển
        if (isMoving)
        {
            moveTimer += Time.deltaTime;

            // Giai đoạn di chuyển lên
            if (moveTimer < moveDuration)
            {
                Vector3 currentPosition = useWorldPosition ? transform.position : transform.localPosition;
                Vector3 newPosition = Vector3.MoveTowards(currentPosition, targetPosition, moveSpeed * Time.deltaTime);
                if (useWorldPosition)
                    transform.position = newPosition;
                else
                    transform.localPosition = newPosition;
               // Debug.Log("Moving up to " + (useWorldPosition ? "world" : "local") + ": " + newPosition);
            }
            // Giai đoạn trở về
            else
            {
                Vector3 currentPosition = useWorldPosition ? transform.position : transform.localPosition;
                Vector3 newPosition = Vector3.MoveTowards(currentPosition, originalPosition, returnSpeed * Time.deltaTime);
                if (useWorldPosition)
                    transform.position = newPosition;
                else
                    transform.localPosition = newPosition;
               // Debug.Log("Returning to " + (useWorldPosition ? "world" : "local") + ": " + newPosition);
                if (Vector3.Distance(currentPosition, originalPosition) < 0.01f)
                {
                    if (useWorldPosition)
                        transform.position = originalPosition;
                    else
                        transform.localPosition = originalPosition;
                    isMoving = false;
                  //  Debug.Log("Move completed, returned to " + (useWorldPosition ? "world" : "local") + ": " + originalPosition);
                }
            }
        }
    }
}
