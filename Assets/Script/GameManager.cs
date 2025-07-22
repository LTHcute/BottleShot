using JetBrains.Annotations;
using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public GameObject crosshair;
    public Camera mainCamera;
    public LayerMask crosshairLayer;
    private GameObject[] bottles;
    private Vector2[] lastPositions;
   // public float collisionThreshold = 0.1f;


    private bool isPause = false;
    public Image pause;
    public Image continueButton;
    public Image store;
    public GameObject menu;
    public GameObject panelStore;

    void Start()
    {
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
      
        // Kiểm tra input: Mouse cho Editor, Touch cho di động
        if (Input.GetMouseButtonDown(0) && !Application.isMobilePlatform)
        {
            Debug.Log("CHạm");
            
         
        }
        else if (Input.touchCount > 0 && Application.isMobilePlatform)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Debug.Log("Chạm");
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



