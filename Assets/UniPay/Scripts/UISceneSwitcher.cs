/*  This file is part of the "UniPay" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store, Epic FAB).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniPay
{
    /// <summary>
    /// Simple script to cycle through the scenes added to the build settings.
    /// Allows opening all of the asset's demo scenes without a separate menu.
    /// </summary>
    public class UISceneSwitcher : MonoBehaviour
    {
        void Awake()
        {
            UISceneSwitcher[] refs = FindObjectsByType<UISceneSwitcher>(FindObjectsSortMode.None);
            if (refs.Length > 1) Destroy(gameObject);
        }

        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }


        void OnGUI()
        {
            if (GUI.Button(new Rect(0, Screen.height - 80, 80, 80), "<"))
            {
                SceneManager.LoadScene(Mathf.Clamp(SceneManager.GetActiveScene().buildIndex - 1, 0, SceneManager.sceneCountInBuildSettings - 1));
            }

            if (GUI.Button(new Rect(Screen.width - 80, Screen.height - 80, 80, 80), ">"))
            {
                SceneManager.LoadScene(Mathf.Clamp(SceneManager.GetActiveScene().buildIndex + 1, 0, SceneManager.sceneCountInBuildSettings - 1));
            }
        }
    }
}
