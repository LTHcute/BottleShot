using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;

using TMPro;
using UniPay;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public GameObject crosshair;
    public Camera mainCamera;
    public LayerMask crosshairLayer;
    private GameObject[] bottles;
    private Vector2[] lastPositions;
    private int currentBulletCount;
    private int myBulletCount;
    // public float collisionThreshold = 0.1f;


    private int isPause ;
    public Image pause;
    public Image continueButton;
    public Image store;
    public GameObject menu;
    public GameObject panelStore;
    public Image notications;
    public Image reset;


 
    void Start()
    {
        PlayerPrefs.SetInt("isPause", 0);
        
       
        bottles = GameObject.FindGameObjectsWithTag("Bottle");

        Debug.Log(bottles.Length);
        lastPositions = new Vector2[bottles.Length];
        for (int i = 0; i < bottles.Length; i++)
        {
            if (bottles[i] != null)
                lastPositions[i] = bottles[i].transform.position;
        }
        


        OpenMenu();
        HideMenu();
        OpenStore();
    }
    void Update()
    {
        TogglePause();
        currentBulletCount = PlayerPrefs.GetInt("currentBulletCount", 1);
        myBulletCount = DBManager.GetCurrency("myBulletCount");
        Debug.Log($"myBulletCount:{myBulletCount}");
        if (currentBulletCount == 0 && myBulletCount ==0)
        {
            ShowNoti();
            return;
        }
        else if(currentBulletCount != 0)
        {
            // Kiểm tra input: Mouse cho Editor, Touch cho di động
            if (Input.GetMouseButtonDown(0) && !Application.isMobilePlatform)
            {


                PlayerPrefs.SetInt("currentBulletCount", currentBulletCount - 1);

            }
            else if (Input.touchCount > 0 && Application.isMobilePlatform)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {

                    PlayerPrefs.SetInt("currentBulletCount", currentBulletCount - 1);
                }
            }
        }
        else if (currentBulletCount == 0 && myBulletCount != 0)
        {
            if (Input.GetMouseButtonDown(0) && !Application.isMobilePlatform)
            {


                myBulletCount = myBulletCount - 1;
                PlayerPrefs.SetInt("myBulletCount", myBulletCount);

            }
            else if (Input.touchCount > 0 && Application.isMobilePlatform)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    myBulletCount = myBulletCount - 1;
                    PlayerPrefs.SetInt("myBulletCount", myBulletCount);
                }
            }
        }
        DBManager.SetCurrency("myBulletCount", myBulletCount);


    }

    void ShowNoti()
    {
        if (notications == null)
        {
            Debug.LogError("Notications is null!");
            return;
        }
    
        StopAllCoroutines(); // Dừng mọi coroutine hiển thị trước đó
        StartCoroutine(ShowNotiCoroutine());
       
    }

    private IEnumerator ShowNotiCoroutine()
    {
        notications.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1f); // Sử dụng Realtime để tránh ảnh hưởng của timeScale
        notications.gameObject.SetActive(false);
        HideNoti();
    }

    void HideNoti()
    {
        if (notications != null)
        {
            notications.gameObject.SetActive(false);
          
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
       
        PlayerPrefs.SetInt("isPause", 1);
        TogglePause();
    }  
    void TogglePause()
    {
        isPause = PlayerPrefs.GetInt("isPause",1);
        if (isPause==1)
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
        PlayerPrefs.SetInt("isPause", 0);
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
      
        PlayerPrefs.SetInt("isPause", 1);
    }


    private void Reset()
    {
        Button resetButton = reset.GetComponent<Button>();
        resetButton.onClick.RemoveAllListeners();
        resetButton.onClick.AddListener(PlayAgain);
    }
    void PlayAgain()
    {

    }    
}



