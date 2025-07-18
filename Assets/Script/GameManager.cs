using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Transform crosshair; // Tâm ngắm
    public float collisionThreshold = 0.5f; // Ngưỡng khoảng cách (bán kính vùng va chạm)
    public LayerMask bottleLayer; // Layer của các chai
    private GameObject[] bottles; // Danh sách chai
    private Vector2[] lastPositions; // Lưu vị trí trước đó của từng chai

    void Start()
    {
        // Tìm tất cả các chai khi bắt đầu
        bottles = GameObject.FindGameObjectsWithTag("Bottle");
        lastPositions = new Vector2[bottles.Length];

        // Lưu vị trí ban đầu của từng chai
        for (int i = 0; i < bottles.Length; i++)
        {
            if (bottles[i] != null)
            {
                lastPositions[i] = bottles[i].transform.position;
            }
        }
    }

    void Update()
    {
        if (crosshair == null) return;

        // Lấy vị trí tâm ngắm
        Vector2 crosshairWorldPos = crosshair.position;
        Debug.Log($"Tâm: {crosshairWorldPos}");
        Debug.Log($"Số chai: {bottles.Length}");
        bottles = GameObject.FindGameObjectsWithTag("Bottle");
        // Duyệt qua từng chai
        for (int i = 0; i < bottles.Length; i++)
        {
            GameObject bottle = bottles[i];
            if (bottle != null)
            {
                Vector2 currentPosition = bottle.transform.position;
                Vector2 lastPosition = lastPositions[i];

                // Tính hướng và khoảng cách di chuyển của chai trong khung hình này
                Vector2 direction = currentPosition - lastPosition;
                float distance = direction.magnitude;

                // Bắn tia từ vị trí trước đến vị trí hiện tại của chai
                RaycastHit2D hit = Physics2D.Raycast(lastPosition, direction.normalized, distance, bottleLayer);

                // Kiểm tra xem tia có chạm vào Collider của crosshair không
                if (hit.collider != null && hit.collider.transform == crosshair)
                {
                    Debug.Log($"Chai {bottle.name} đi qua tâm ngắm tại điểm: {hit.point}");
                    // Xử lý logic, ví dụ: gây sát thương, hủy chai
                    // Destroy(bottle);
                }
                else
                {
                    // Kiểm tra khoảng cách tĩnh (như mã gốc của bạn) để dự phòng
                    float staticDistance = Vector2.Distance(crosshairWorldPos, currentPosition);
                    if (staticDistance <= collisionThreshold)
                    {
                        Debug.Log($"Chai {bottle.name} chạm tâm ngắm (theo khoảng cách tĩnh)");
                    }
                }

                // Vẽ tia để debug
                Debug.DrawRay(lastPosition, direction, Color.red);

                // Cập nhật vị trí trước đó
                lastPositions[i] = currentPosition;
            }
            else
            {
                Debug.Log($"Chai {i} là null");
            }
        }
    }
}
