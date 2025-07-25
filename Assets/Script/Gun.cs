using System;
using UnityEngine;

using UnityEngine.UIElements;


public class Gun : MonoBehaviour
{

    [SerializeField] private float moveDistance = 25f;
    [SerializeField] private float moveSpeed = 200f;
    [SerializeField] private float returnSpeed = 100f;
    [SerializeField] private float moveDuration = 0.1f;
    [SerializeField] private bool useWorldPosition = false;

    private Vector3 originalPosition;
    private bool isMoving = false;
    private float moveTimer = 0f;
    private Vector3 targetPosition;

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
                targetPosition = originalPosition + new Vector3(0, moveDistance, 0);
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