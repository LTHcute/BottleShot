/*  This file is part of the "UniPay" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store, Epic FAB).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using System.Collections.Generic;
using UnityEngine;

namespace UniPay
{
    /// <summary>
    /// Manages the display of various purchase windows.
    /// Presents UI feedback to the user for various purchase states, e.g. failed or successful purchases.
    /// Also handles showing a transaction confirmation popup where necessary, e.g. for PayPal transactions.
    /// </summary>
    public class UIShopFeedback : MonoBehaviour
    {
        //static reference to this script.
        private static UIShopFeedback instance;

        /// <summary>
        /// Reference to the UIWindowPurchase class to double-ask users for the purchase.
        /// </summary>
        public UIWindowPurchase purchaseWindow;

        /// <summary>
        /// Window showing that a native purchase dialog is currently processing.
        /// Recommended with dialogs that take long to load, e.g. Apple.
        /// Only possible for platforms sending success+fail events (Steam does not).
        /// </summary>
        public GameObject loadingWindow;

        /// <summary>
        /// Message window for showing feedback on purchase events to the user.
        /// </summary>
        public UIWindowMessage messageWindow;

        /// <summary>
        /// Confirmation window for refreshing external transactions. Only required when using third party services, e.g. PayPal.
        /// </summary>
        public GameObject confirmWindow;

        /// <summary>
        /// Reference to the UIWindowPreview class to set the products to display.
        /// </summary>
        public UIWindowPreview previewWindow;


        /// <summary>
        /// Returns a static reference to this script.
        /// </summary>
        public static UIShopFeedback GetInstance()
        {
            return instance;
        }


        void Awake()
        {
            instance = this;

            //check for IAPManager existence before other containers initialize
            if (!IAPManager.GetInstance())
            {
                //double check if there is an IAPManager in the scene that just isn't ready yet
                IAPManager manager = (IAPManager)FindAnyObjectByType(typeof(IAPManager));
                if (manager != null) return;

                //there really is no IAPManager in the scene, this should be avoided
                Debug.LogWarning("UIShopFeedback: Could not find IAPManager prefab. Have you placed it in the first scene of your app and started from there? Instantiating copy...");
                GameObject obj = Instantiate(Resources.Load("IAPManager", typeof(GameObject))) as GameObject;
                //remove clone tag from its name. Not necessary, but nice to have
                obj.name = obj.name.Replace("(Clone)", "");
            }
        }


        //initialize window components without data, where necessary
        void Start()
        {
            #if !STEAM_IAP
            IAPManager.purchaseStartedEvent += HandlePurchaseStarted;
            IAPManager.purchaseSucceededEvent += HandleSuccessfulPurchase;
            IAPManager.purchaseFailedEvent += HandleFailedPurchase;
            #endif
        }


        /// <summary>
        /// Show purchase window with the product for the user to accept or deny.
        /// Best practice to use for virtual products that do not have their own native popup.
        /// <summary>
        public static void ShowPurchase(string productID)
        {
            if (!instance.purchaseWindow) return;

            instance.purchaseWindow.Set(productID);
        }


        /// <summary>
        /// Show loading window that forces the user to wait for a purchase reponse.
        /// Should only be used on platforms sending a successful or fail event to disable it again.
        /// <summary>
        public static void ShowLoading(bool state)
        {
            if (!instance.loadingWindow) return;

            instance.loadingWindow.gameObject.SetActive(state);
        }


        /// <summary>
        /// Show feedback/error window with text received.
        /// This gets called in IAPListener's HandleSuccessfulPurchase method with some custom text,
        /// or from the IAPManager with the error message when a purchase failed at billing.
        /// <summary>
        public static void ShowMessage(string text)
        {
            if (!instance.messageWindow) return;

            instance.messageWindow.Set(text);
        }


        /// <summary>
        /// Shows window waiting for transaction confirmation. This gets called by the store
        /// when waiting for the user to confirm his purchase payment with the third party service.
        /// </summary>
        public static void ShowConfirmation()
        {
            if (!instance.confirmWindow) return;

            instance.confirmWindow.SetActive(true);
        }


        /// <summary>
        /// Shows a preview window containing the items passed in.
        /// Used for bundle products or when redeeming codes to see the reward prior to purchase.
        /// </summary>
        public static void ShowPreview(List<KeyValuePairStringInt> products)
        {
            if (!instance.previewWindow) return;

            instance.previewWindow.Set(products);
        }


        void HandlePurchaseStarted(string productId)
        {
            if (loadingWindow && !loadingWindow.activeInHierarchy)
            {
                IAPProduct product = IAPManager.GetIAPProduct(productId);
                if (product == null || product.IsVirtual()) return;

                loadingWindow.SetActive(true);
            }
        }


        void HandleSuccessfulPurchase(string productId)
        {
            if (loadingWindow && loadingWindow.activeInHierarchy)
            {
                loadingWindow.SetActive(false);
            }
        }


        void HandleFailedPurchase(string error)
        {
            if (loadingWindow && loadingWindow.activeInHierarchy)
            {
                loadingWindow.SetActive(false);
            }
        }


        void OnDestroy()
        {
            #if !STEAM_IAP
            IAPManager.purchaseStartedEvent -= HandlePurchaseStarted;
            IAPManager.purchaseSucceededEvent -= HandleSuccessfulPurchase;
            IAPManager.purchaseFailedEvent -= HandleFailedPurchase;
            #endif
        }
    }
}
