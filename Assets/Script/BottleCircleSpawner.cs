using UnityEngine;

public class BottleCircleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bottlePrefab; // Prefab của chai
    [SerializeField] private int numberOfBottles = 6; // Số lượng chai
    [SerializeField] private float radius = 5f; // Bán kính hình tròn
    [SerializeField] private BottleCircleRotator rotator; // Tham chiếu đến BottleCircleRotator
    [SerializeField] private float initialRotationSpeed = 30f; // Tốc độ xoay ban đầu

    private GameObject[] bottles; // Mảng lưu các chai
    private float currentRotationSpeed; // Tốc độ xoay hiện tại

    private void Start()
    {
        // Đảm bảo pivot của GameObject cha ở (0, 0, 0)
        transform.localPosition = new Vector3(0f, 350f, 0f);
        // transform.localRotation = Quaternion.identity; // Đặt rotation của cha về 0

        currentRotationSpeed = initialRotationSpeed; // Khởi tạo tốc độ xoay
        if (rotator != null)
        {
            rotator.SetRotationSpeed(currentRotationSpeed); // Đặt tốc độ xoay ban đầu
        }

        SpawnBottles();
    }

    private void SpawnBottles()
    {
        // Xóa các chai cũ nếu có
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Tăng tốc độ xoay lên 1.5 lần
        currentRotationSpeed *= 1.5f;
        if (rotator != null)
        {
            rotator.SetRotationSpeed(currentRotationSpeed); // Cập nhật tốc độ xoay
        }

        // Khởi tạo mảng lưu chai
        bottles = new GameObject[numberOfBottles];
        float angleStep = 360f / numberOfBottles;

        // Tạo và đặt vị trí các chai
        for (int i = 0; i < numberOfBottles; i++)
        {
            // Tính góc mỗi chai theo hình tròn
            float angle = i * Mathf.PI * 2f / numberOfBottles;

            // Tính vị trí trên vòng tròn
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            Vector3 spawnPos = new Vector3(x, y, 0f) + transform.position;
            GameObject bottle = Instantiate(bottlePrefab, spawnPos, Quaternion.identity, transform);
            bottles[i] = bottle; // Lưu chai vào mảng
        }

        Debug.Log($"Tạo lượt chai mới với tốc độ xoay: {currentRotationSpeed}");
    }

    void Update()
    {
        // Kiểm tra số lượng chai (các con của transform)
        if (transform.childCount == 0)
        {
            Debug.Log("Tất cả chai đã bị hủy, tạo lượt chai mới!");
            SpawnBottles(); // Tạo lại lượt chai mới
        }
    }
}