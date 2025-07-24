using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;

using TMPro;
using UniPay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public GameObject crosshair;
    public Camera mainCamera;
    public LayerMask crosshairLayer;
    private GameObject[] bottles;
    private Vector2[] lastPositions;
    public AudioSource audioGunShot;
    public AudioSource audioSource;
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
    public Image gun;

 
    void Start()
    {
        audioGunShot.gameObject.SetActive(false);
        audioSource.Play();

        PlayerPrefs.SetInt("isPause", 0);
        Debug.Log(DBManager.GetCurrency("bullet").ToString());
       
        bottles = GameObject.FindGameObjectsWithTag("Bottle");

        Debug.Log(bottles.Length);
        lastPositions = new Vector2[bottles.Length];
        for (int i = 0; i < bottles.Length; i++)
        {
            if (bottles[i] != null)
                lastPositions[i] = bottles[i].transform.position;
        }


        PlayAgain();
        OpenMenu();
        HideMenu();
        OpenStore();
        Shot();
    }
    void Update()
    {
        TogglePause();
       
    }


    void Shot()
    {
        Button gunButton = gun.GetComponent<Button>();
        gunButton.onClick.RemoveAllListeners();
        gunButton.onClick.AddListener(CheckShot);
    }    

    void CheckShot()
    {
        audioGunShot.gameObject.SetActive(true);
        if (audioGunShot != null)
        {
            audioGunShot.Play();
        }
        else
        {
            Debug.LogWarning("AudioSource or GunshotSound not assigned!");
        }
        currentBulletCount = PlayerPrefs.GetInt("currentBulletCount", 1);
        myBulletCount = DBManager.GetCurrency("bullet");

        // Nếu không còn đạn, hiển thị thông báo và thoát
        if (currentBulletCount == 0 && myBulletCount == 0)
        {
            ShowNoti();
            return;
        }
        if (currentBulletCount != 0)
        {
            PlayerPrefs.SetInt("currentBulletCount", currentBulletCount - 1);
            Debug.Log($"currentBulletCount{currentBulletCount}");
        }
        else
        {
            myBulletCount--;
            DBManager.SetCurrency("bullet", myBulletCount);
        }

            Collider2D crosshairCollider = crosshair.GetComponent<Collider2D>();

        if (crosshairCollider != null)
        {
            // Kiểm tra xem collider của crosshair có va chạm với đối tượng có tag "bottle" không
            Collider2D[] hits = Physics2D.OverlapPointAll(crosshairCollider.transform.position);

            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Bottle"))
                {
                    Debug.Log("Crosshair va chạm với bottle!");
                    Destroy(hit.gameObject);

                    // Thực hiện hành động khi phát hiện va chạm
                }
            }
        }
        else
        {
            Debug.LogWarning("Crosshair không có Collider2D!");
        }


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


    void PlayAgain()
    {
        Button resetButton = reset.GetComponent<Button>();
        resetButton.onClick.RemoveAllListeners();
        resetButton.onClick.AddListener(Play);
    }
    void Play()
    {
        SceneManager.LoadScene("Play");
    }

}



