using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class GameManager : MonoBehaviour
{
    public Transform crosshair;
    public Camera mainCamera;
    public LayerMask bottleLayer;
    private GameObject[] bottles;
    private Vector2[] lastPositions;
    public float collisionThreshold = 0.1f;
    private bool isPause = false;
    public Image pause;
    public Image continueButton;
    public Image store;
    public GameObject menu;
    public GameObject panelStore;

    void Start()
    {
        bottles = GameObject.FindGameObjectsWithTag("Bottle");
        Debug.Log($"bottles:{bottles.Length}");
      
        lastPositions =new Vector2[bottles.Length];
        for (int i = 0; i < bottles.Length; i++)
        {
            if (bottles[i] != null)
            {
                lastPositions[i] = bottles[i].transform.position;
            }
        }
        if (bottleLayer.value == 0)
        {
            Debug.Log("bottleLayer is not set!");
        }
        OpenMenu();
       HideMenu();
        OpenStore();
    }
    private void Update()
    {
        if (crosshair == null) return;
        Vector2 crosshairPos = crosshair.transform.position;
        Debug.Log($"Crosshair:{crosshairPos}");
        Debug.Log($"bottles:{bottles.Length}");
        for (int i = 0; i < bottles.Length; i++)
        {
            GameObject bottle = bottles[i];
            if (bottle != null)
            {
                Vector2 currentPosition = bottle.transform.position;
                Vector2 lastPosition = lastPositions[i];
                Vector2 direction = currentPosition - lastPosition;
                float distance = direction.magnitude;
                RaycastHit2D hit = Physics2D.Raycast(lastPosition, direction.normalized, distance, bottleLayer);
                if(hit.collider == null)
                {
                    Debug.Log("No hit");
                }
              //  Debug.Log($"collider:{hit.collider.transform}");
                // Kiểm tra xem tia có chạm vào Collider của crosshair không
                if (hit.collider != null && hit.collider.transform == crosshair)
                {
                    Debug.Log($"Chai {bottle.name} đi qua tâm ngắm tại điểm: {hit.point}");
                    // Xử lý logic, ví dụ: gây sát thương, hủy chai
                    // Destroy(bottle);
                }
                else
                {
                    Debug.Log("1");
                    // Kiểm tra khoảng cách tĩnh (như mã gốc của bạn) để dự phòng
                    float staticDistance = Vector2.Distance(crosshairPos, currentPosition);
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

    public void OpenMenu()
    {
        Button openMenuButton = pause.GetComponent<Button>();
        openMenuButton.onClick.RemoveAllListeners();
        openMenuButton.onClick.AddListener(ShowMenu);
    }    
      
    void ShowMenu()
    {
        menu.gameObject.SetActive( true );
        TogglePause();
    }  
    void TogglePause()
    {
        isPause = !isPause;
        if (isPause)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }



    void HideMenu()
    {
        Button continuebtn = continueButton.GetComponent<Button>();
        continuebtn.onClick.RemoveAllListeners();
        continuebtn.onClick.AddListener(Hide);
    }
    void Hide()
    {
        menu.gameObject.SetActive(false);
        TogglePause();
    }


    void OpenStore()
    {
        Button storeButton = store.GetComponent<Button>();
        storeButton.onClick.RemoveAllListeners();   
        storeButton.onClick.AddListener(ShowStore);
    }    
    void ShowStore()
    {

        panelStore.gameObject.SetActive(true);
        menu.gameObject.SetActive(false);
    }    
}



