using UnityEngine;

public class BottleCircleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bottlePrefab; // Prefab của chai
    [SerializeField] private int numberOfBottles = 6; // Số lượng chai
    [SerializeField] private float radius = 5f; // Bán kính hình tròn

    private GameObject[] bottles; // Mảng lưu các chai

    private void Start()
    {
        // Kiểm tra prefab
        if (bottlePrefab == null)
        {
            Debug.LogError("Bottle Prefab is not assigned in the Inspector!");
            return;
        }

        if (bottlePrefab.GetComponent<SpriteRenderer>() == null)
        {
            Debug.LogError("Bottle Prefab has no SpriteRenderer component!");
            return;
        }

        // Đảm bảo pivot của GameObject cha ở (0, 0, 0)
        transform.localPosition =new Vector3(0f,300f,0f); ;
      //  transform.localRotation = Quaternion.identity; // Đặt rotation của cha về 0

        SpawnBottles();
    }

    private void SpawnBottles()
    {
        // Xóa các chai cũ nếu có
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
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
            Debug.Log($"spawnPos:{spawnPos}");
            Instantiate(bottlePrefab, spawnPos, Quaternion.identity, transform);
        }

      
    }

    // Cập nhật khi thay đổi thông số trong Inspector
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            SpawnBottles();
        }
    }
}
