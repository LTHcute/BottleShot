using UnityEngine;
using UnityEngine.UI;

public class Store : MonoBehaviour
{
    public Image imageClose;
    public GameObject store;
    public GameObject productPrefab;
    public Transform contentProduct;
    public Transform scrollProduct;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CloseStore();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void CloseStore()
    {
      
        Button buttonClose = imageClose.GetComponent<Button>();
        buttonClose.onClick.RemoveAllListeners();
        buttonClose.onClick.AddListener(Hide);
    }
    void Hide()
    {
        store.SetActive(false);
        PlayerPrefs.SetInt("isPause", 0);

    }


}
