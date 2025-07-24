using UnityEngine;
using TMPro;
using UniPay;
public class BottleCircleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bottlePrefab; // Prefab của chai
    [SerializeField] private int numberOfBottles; // Số lượng chai
    [SerializeField] private float radius = 5f; // Bán kính hình tròn
    [SerializeField] private BottleCircleRotator rotator; // Tham chiếu đến BottleCircleRotator
    [SerializeField] private float initialRotationSpeed = 20f; // Tốc độ xoay ban đầu
    [SerializeField] private int bulletsPerRound = 5; // Số đạn mỗi lượt
    [SerializeField] private GameObject bulletUIPrefab; // Prefab của UI đạn
    [SerializeField] private Transform bulletPanel; // Panel chứa các hình ảnh đạn
    public TextMeshProUGUI spawnCountText;
    public TextMeshProUGUI myBulletCount;

    private GameObject[] bottles; // Mảng lưu các chai
    private float currentRotationSpeed; // Tốc độ xoay hiện tại
    private int currentBulletCount; // Số đạn hiện tại
    private GameObject[] bulletUIObjects; // Mảng lưu các đối tượng UI đạn
    private int spawnCount;

    void Awake()
    {
        numberOfBottles = 7;
        currentBulletCount = bulletsPerRound * 2; // Khởi tạo số đạn ban đầu
        spawnCount = 0; // Khởi tạo số lần sinh lượt
        Debug.Log("1");
     
    }
    private void Start()
    {
          PlayerPrefs.SetInt("spawnCount", 1);
        myBulletCount.text = DBManager.GetCurrency("bullet").ToString();
        PlayerPrefs.SetInt("currentBulletCount", currentBulletCount);
        Debug.Log($"currentBulletCount start:{currentBulletCount}");
        // Đảm bảo pivot của GameObject cha ở (0, 0, 0)
        transform.localPosition = new Vector3(0f, 350f, 0f);

        currentRotationSpeed = initialRotationSpeed; // Khởi tạo tốc độ xoay
        if (rotator != null)
        {
            rotator.SetRotationSpeed(currentRotationSpeed); // Đặt tốc độ xoay ban đầu
        }

        UpdateBulletUI(); // Cập nhật UI ban đầu
        SpawnBottles();
    }

    private void SpawnBottles()
    {
        // Xóa các chai cũ nếu có
        spawnCount++;
        PlayerPrefs.SetInt("spawnCount", spawnCount);
      
      
        UpdateSpawnCountUI();
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Tăng tốc độ xoay lên 1.5 lần
        if(spawnCount!=1)
        {
            currentRotationSpeed = currentRotationSpeed + (currentRotationSpeed / spawnCount);
            if (rotator != null)
            {
                rotator.SetRotationSpeed(currentRotationSpeed); // Cập nhật tốc độ xoay
            }
        }    
      
        if (rotator != null)
        {
            rotator.SetRotationSpeed(currentRotationSpeed); // Cập nhật tốc độ xoay
        }

        // Sinh đạn mới cho lượt mới
        currentBulletCount = bulletsPerRound*2;
        PlayerPrefs.SetInt("currentBulletCount", currentBulletCount);
        UpdateBulletUI();

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

        Debug.Log($"Tạo lượt chai mới với tốc độ xoay: {currentRotationSpeed}, Số đạn: {currentBulletCount}");
    }


    private void UpdateSpawnCountUI()
    {
        if (spawnCountText != null)
        {
            spawnCountText.text = spawnCount.ToString();
        }
    }
    // Hàm cập nhật UI hiển thị đạn
    public void UpdateBulletUI()
    {
       
     //   Debug.Log($"bulletUIObjects:{bulletUIObjects.Length}");
        // Xóa các hình ảnh đạn cũ
        if (bulletUIObjects != null)
        {
            foreach (GameObject bulletObj in bulletUIObjects)
            {
                if (bulletObj != null)
                {
                    Destroy(bulletObj);
                }
            }
        }

        // Khởi tạo mảng lưu các hình ảnh đạn
        bulletUIObjects = new GameObject[currentBulletCount];
        currentBulletCount = PlayerPrefs.GetInt("currentBulletCount", bulletUIObjects.Length);

        for (int i = 0; i < currentBulletCount; i++)
        {
            if (bulletUIPrefab != null && bulletPanel != null)
            {
                GameObject bulletUI = Instantiate(bulletUIPrefab, bulletPanel);
                bulletUIObjects[i] = bulletUI;
            }
        }
    }

    // Hàm gọi khi bắn đạn
    public void UseBullet()
    {
        if (currentBulletCount > 0)
        {
            currentBulletCount--;
            UpdateBulletUI();
            Debug.Log($"Đã bắn 1 viên đạn. Số đạn còn lại: {currentBulletCount}");
        }
    }

    void Update()
    {
        myBulletCount.text = DBManager.GetCurrency("bullet").ToString();
        UpdateBulletUI();
        // Kiểm tra số lượng chai (các con của transform)
        if (transform.childCount == 0)
        {
            Debug.Log("Tất cả chai đã bị hủy, tạo lượt chai mới!");
            SpawnBottles(); // Tạo lại lượt chai mới
        }
    }
    // Getter để lấy số lần sinh lượt
    public int GetSpawnCount()
    {
        return spawnCount;
    }
    // Getter để lấy số đạn hiện tại
    public int GetBulletCount()
    {
        return currentBulletCount;
    }
}