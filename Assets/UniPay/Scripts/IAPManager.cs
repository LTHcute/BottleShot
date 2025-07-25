/*  This file is part of the "UniPay" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store, Epic FAB).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniPay
{
    using Unity.Services.Core;
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Extension;

    /// <summary>
    /// Unity IAP cross-platform wrapper for real money purchases, as well as for virtual ingame purchases (for virtual currency).
    /// Initializes the Unity IAP billing system, handles different store interfaces and integrates their callbacks respectively.
    /// </summary>
    public class IAPManager : MonoBehaviour, IDetailedStoreListener
    {
        /// <summary>
        /// Reference to the IAP configuration asset made in the Project Settings window.
        /// </summary>
        public IAPScriptableObject asset;

        /// <summary>
        /// Toggle that defines whether the IAPManager should initialize itself on first load.
        /// </summary> 
        public bool autoInitialize = true;

        /// <summary>
        /// Debug messages are enabled in Development build automatically.
        /// </summary>
        public static bool isDebug = false;

        /// <summary>
        /// The current Store define, determined at runtime.
        /// </summary>
        public static string appStore = "NotSpecified";

        /// <summary>
        /// Instantiated shop items in the current scene, accessible via their product ID.
        /// </summary>
        public static Dictionary<string, ShopItem2D> shopItems = new Dictionary<string, ShopItem2D>();

        /// <summary>
        /// fired when Unity IAP initialization completes
        /// </summary>
        public static event Action initializeSucceededEvent;

        /// <summary>
        /// fired when Unity IAP initialization fails, providing error text
        /// </summary>
        public static event Action<string> initializeFailedEvent;

        /// <summary>
        /// fired when a purchase is initiated, delivering its product id
        /// </summary>
        public static event Action<string> purchaseStartedEvent;

        /// <summary>
        /// fired when a purchase succeeds, delivering its product id
        /// </summary>
        public static event Action<string> purchaseSucceededEvent;

        /// <summary>
        /// fired when a purchase fails, providing error text
        /// </summary>
        public static event Action<string> purchaseFailedEvent;

        /// <summary>
        /// fired when a consume is initiated, delivering its product id
        /// </summary>
        public static event Action<string> consumeStartedEvent;

        /// <summary>
        /// fired when a consume succeeds, delivering its product id
        /// </summary>
        public static event Action<string> consumeSucceededEvent;

        /// <summary>
        /// fired when a consume fails, providing error text
        /// </summary>
        public static event Action<string> consumeFailedEvent;

        /// <summary>
        /// fired when a restore transactions workflow is initiated
        /// </summary>
        public static event Action restoreTransactionsStartedEvent;

        /// <summary>
        /// fired when a restore transactions workflow completes, delivering its state
        /// </summary>
        public static event Action<bool> restoreTransactionsFinishedEvent;

        /// <summary>
        /// fired when billing initialized and would be ready for validating current products
        /// </summary>
        public static event Action receiptValidationInitializeEvent;

        /// <summary>
        /// fired when a purchase transaction completes locally, delivering the product bought
        /// </summary>
        public static event Action<Product> receiptValidationPurchaseEvent;

        #pragma warning disable 0649
        //fired when trying to purchase virtual product from remote source
        internal static event Action<IAPProduct> remotePurchaseVirtualEvent;
        //fired when trying to consume a product purchase from remote source
        internal static event Action<IAPProduct, int> remoteConsumePurchaseEvent;
        #pragma warning restore 0649

        public static IStoreController controller;
		public static IExtensionProvider extensions;
        public static ConfigurationBuilder builder;

        //static reference to this script
        private static IAPManager instance;


        /// <summary>
        /// Returns a static reference to this script.
        /// </summary>
        public static IAPManager GetInstance()
        {
            return instance;
        }


        //initialize IAPs, billing systems and database,
        //as well as shop components in this order
        void Awake()
        {
            //make sure we keep one instance of this script
            if (instance)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            isDebug = Debug.isDebugBuild;

            //set static reference
            instance = this;
            asset = Instantiate(asset);

            //set up components
            GetComponent<DBManager>().Init();
            //let the ShopManager instantiate items with local data
            SceneManager.sceneLoaded += OnSceneWasLoaded;
            DBManager.dataUpdateEvent += RefreshShopItem;

            //disable auto initialization to e.g. to wait for an external login callback
            //and for querying store later after the external service has been initialized first
            if (autoInitialize) Initialize();
        }


        /// <summary>
        /// Initializes the IAP service with a platform-dependant billing connection.
        /// </summary>
        public async void Initialize()
        {
            //initialized already
            if(controller != null)
                return;

            try
            {
                //Unity Gaming Services are required first
                await UnityServices.InitializeAsync();

                //then Unity IAP
                CustomPurchasingModule customModule = new CustomPurchasingModule();
                StandardPurchasingModule standardModule = StandardPurchasingModule.Instance();
                #if UNITY_EDITOR
                    standardModule.useFakeStoreAlways = true;
                #endif

                //create Unity IAP builder
                builder = ConfigurationBuilder.Instance(customModule, standardModule);
                builder.Configure<IGooglePlayConfiguration>().SetServiceDisconnectAtInitializeListener(() => {OnInitializeFailed("NoLinkedGoogleAccount");});
                builder.Configure<IGooglePlayConfiguration>().SetDeferredPurchaseListener((Product p) => {OnPurchaseFailed("Purchase of product " + p.metadata.localizedTitle + " is pending.");});

                if (isDebug)
                {
                    builder.Configure<IMicrosoftConfiguration>().useMockBillingSystem = true;
                    builder.Configure<IAmazonConfiguration>().WriteSandboxJSON(builder.products);
                }
            
                appStore = !string.IsNullOrEmpty(customModule.appStore) ? customModule.appStore : standardModule.appStore.ToString();
                RequestProductData(builder, GetStoreIDs());
                //now we're ready to initialize Unity IAP
                UnityPurchasing.Initialize(this, builder);
            }
            catch (Exception ex)
            {
                OnInitializeFailed(ex.Message);
            }
        }


        /// <summary>
        /// Reload ShopItem state visualizations on scene change.
        /// </summary>
        public void OnSceneWasLoaded(Scene scene, LoadSceneMode m)
        {
            shopItems.Clear();
        }


        //initialize product IDs for the current store
        private string[] GetStoreIDs()
        {
            List<string> IDs = new List<string>();

            if (asset.productList.Count == 0)
            {
                Debug.LogWarning("No products found. Are you sure you've added them to the Project Settings?");
                return null;
            }

            //loop over all products
            for (int i = 0; i < asset.productList.Count; i++)
            {
                IAPProduct product = asset.productList[i];

                if (string.IsNullOrEmpty(product.ID))
                {
                    Debug.LogError("Found IAP Object in IAP Settings without an identifier. Skipping product.");
                    continue;
                }

                if (product.IsVirtual())
                    continue;

                //check overrides
                IAPCategory category = asset.categoryList.Find(x => x.referenceID == product.category.referenceID);
                if (category.storeIDs.Find(x => x.store == appStore && !x.active) != null) continue;
                else if (product.storeIDs.Find(x => x.store == appStore && !x.active) != null) continue;

                IDs.Add(product.ID);
            }

            return IDs.ToArray();
        }


        //construct IAP product data with their App Store identifiers
        private void RequestProductData(ConfigurationBuilder builder, string[] products)
        {
            if (products == null) return;
            for (int i = 0; i < products.Length; i++)
            {
                IAPProduct product = GetIAPProduct(products[i]);
                builder.AddProduct(product.ID, product.type, product.GetIDs());
            }
        }


        /// <summary>
        ///  Initialization callback of Unity IAP. Optionally: verify old purchases online.
        ///  Once we've received the product list, we overwrite the existing shop item values with this online data.
        /// </summary>
        public void OnInitialized(IStoreController ctrl, IExtensionProvider ext)
        {
            controller = ctrl;
            extensions = ext;
            OverwriteWithFetch();

            #if UNITY_IOS
                extensions.GetExtension<IAppleExtensions>().RegisterPurchaseDeferredListener((Product p) => {OnPurchaseFailed("Purchase of product " + p.metadata.localizedTitle + " is pending.");});
            #endif

            initializeSucceededEvent?.Invoke();
            receiptValidationInitializeEvent?.Invoke();
        }


        private void OverwriteWithFetch()
        {
            foreach(Product p in controller.products.all)
            {
                IAPProduct product = GetIAPProduct(p.definition.id);
                if (product == null || !product.fetch) continue;

                //cache
                string title = p.metadata.localizedTitle;
                string descr = p.metadata.localizedDescription;
                string price = p.metadata.localizedPriceString;

                //always check for empty strings (product missing on store)
                if (!string.IsNullOrEmpty(title))
                {
                    //do not populate item with fake data from Unity IAP in test mode
                    if (title.StartsWith("FAKE", StringComparison.OrdinalIgnoreCase))
                        return;

                    //normally, the online item name received from the App Store
                    //has the application name attached, so we remove that here
                    int cap = title.IndexOf("(");
                    if (cap > 0)
                        title = title.Substring(0, cap - 1);

                    product.title = title;
                }

                if (!string.IsNullOrEmpty(descr))
                {
                    //replace line breaks with proper formatting
                    product.description = descr.Replace("\\n", "\n");
                }

                if (!string.IsNullOrEmpty(price))
                {
                    IAPExchangeObject exchange = product.priceList.Find(x => x.type == IAPExchangeObject.ExchangeType.RealMoney);
                    if (exchange != null) exchange.realPrice = price;
                }
            }
        }

			
        /// <summary>
        /// Purchase product based on its product identifier.
        /// Our delegates then fire the appropriate started/succeeded/fail event.
        /// </summary>
        public static void Purchase(string productID)
        {
            IAPProduct product = GetIAPProduct(productID);
            Debug.Log("PurchaseIap");
            Debug.Log($"PurchaseIap{productID}");
            if (product == null)
			{
			    if(isDebug) Debug.LogError("Product " + productID + " not found in IAP Settings.");
				return;
            }

            //product is set to already owned, this should not happen
            if (product.type == ProductType.NonConsumable && DBManager.GetPurchase(productID) > 0)
            {
                OnPurchaseFailed("Product already owned.");
                return;
            }

            purchaseStartedEvent?.Invoke(productID);
            //distinguish between virtual and real products
            switch (product.IsVirtual())
            {
                case false:
                    if (controller == null)
                    {
                        Debug.Log($"1{productID}");
                        OnPurchaseFailed("Billing is not available.");
                        if (isDebug) Debug.LogError("Unity IAP is not initialized correctly! Please check your billing settings.");
                        return;
                    }
                    Debug.Log($"2{productID}");
                    Product p = controller.products.WithID(productID);
                    IAPCategory category = GetInstance().asset.categoryList.Find(x => x.referenceID == product.category.referenceID);
                    controller.InitiatePurchase(p);
                    break;


                case true:
                    //product is set to already owned, this should not happen
                    //double check here again in case this method gets called from somewhere directly
                    if (product.type == ProductType.NonConsumable && DBManager.GetPurchase(productID) > 0)
                    {
                        Debug.Log($"3{productID}");
                        OnPurchaseFailed("Product already owned.");
                        return;
                    }
                    Debug.Log($"4{productID}");
                    //check whether the player has enough funds locally
                    bool canPurchase = DBManager.CanPurchaseVirtual(product);
                    if (isDebug) Debug.Log("Purchasing virtual product " + productID + ", canPurchase: " + canPurchase);

                    if (!canPurchase)
                    {
                        Debug.Log($"PurchaseIap{productID}");
                        OnPurchaseFailed("Not enough currency.");
                        return;
                    }

                    if (remotePurchaseVirtualEvent != null)
                    {
                        remotePurchaseVirtualEvent(product);
                        return;
                    }

                    //on success, substract the purchase funds locally
                    //non-consumables are saved to the database. After that fire the succeeded event
                    DBManager.PurchaseVirtual(product);
                    GetInstance().CompletePurchase(product.ID);
                    break;
            }
        }


        /// <summary>
        /// Tries to consume a product with default or amount specified.
        /// Fully consuming a product will ultimately remove it from the user's inventory. 
        /// </summary>
        public static void Consume(string productID, int amount = 1)
        {
            IAPProduct product = GetIAPProduct(productID);
            if (product == null)
            {
                if (isDebug) Debug.LogError("Product " + productID + " not found in IAP Settings.");
                return;
            }

            consumeStartedEvent?.Invoke(productID);
            if (amount > DBManager.GetPurchase(productID))
            {
                OnConsumeFailed("Not enough inventory to consume.");
                return;
            }

            if (remoteConsumePurchaseEvent != null)
            {
                remoteConsumePurchaseEvent(product, amount);
                return;
            }

            GetInstance().CompleteConsume(productID, amount);
        }

		
		/// <summary>
		/// This will be called when a purchase completes.
        /// Pending purchases are not passed to this method anymore since Unity IAP 4.6.0.
		/// Optional: verify new product receipt.
		/// </summary>
		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
		{
            if (GetIAPProduct(e.purchasedProduct.definition.id) == null)
            {
                return PurchaseProcessingResult.Complete;
            }

            //also done when auto-restoring transactions on first app launch
            if(receiptValidationPurchaseEvent != null)
            {
                receiptValidationPurchaseEvent(e.purchasedProduct);
                return PurchaseProcessingResult.Pending;
            }
            else
            {
                CompletePurchase(e.purchasedProduct.definition.id);
            }
                
            // Indicate we have handled this purchase, we will not be informed of it again
            return PurchaseProcessingResult.Complete;
		}


        /// <summary>
        /// Sets a product to purchased after successful validation (or without).
        /// This alters the database entry for non-consumable or products with usage as well.
        /// </summary>
        public void CompletePurchase(string productID, bool withEvent = true)
        {
            IAPProduct product = GetIAPProduct(productID);
            if (product == null) return;

            foreach (IAPExchangeObject exchange in product.rewardList)
            {
                switch(exchange.type)
                {
                    case IAPExchangeObject.ExchangeType.VirtualCurrency:
                        DBManager.AddCurrency(exchange.currency.ID, exchange.amount);
                        break;

                    case IAPExchangeObject.ExchangeType.VirtualProduct:

                        switch(exchange.product.type)
                        {
                            case ProductType.Consumable:
                                DBManager.AddPurchase(exchange.product.ID, exchange.amount);
                                break;

                            //a non-consumable product should definitely be granted to the player
                            case ProductType.NonConsumable:
                            case ProductType.Subscription:
                                DBManager.AddPurchase(exchange.product.ID, 1);
                                break;
                        }
                        break;
                }               
            }

            if (withEvent)
            {
                purchaseSucceededEvent?.Invoke(product.ID);
            }
        }


        /// <summary>
        /// Consumes a purchase after successful validation (or without).
        /// This alters the database entry for this specific product.
        /// </summary>
        public void CompleteConsume(string productID, int amount)
        {
            DBManager.ConsumePurchase(productID, amount);
            consumeSucceededEvent?.Invoke(productID);
        }


        /// <summary>
        /// Restore already purchased user's transactions for non consumable IAPs.
        /// If receipt validation is used, the restored receipts are also getting validated again.
        /// </summary>
        public static void RestoreTransactions()
        {
            restoreTransactionsStartedEvent?.Invoke();

            #if UNITY_ANDROID
                foreach (Product product in controller.products.all)
                {
                    //skip products without receipt but also consumables since they retain their receipt
                    //in the current session when they have been bought previously
                    if (!product.hasReceipt || product.definition.type == ProductType.Consumable)
                        continue;

                    if (receiptValidationPurchaseEvent != null) receiptValidationPurchaseEvent(product);
                    else GetInstance().CompletePurchase(product.definition.id, false);
                }

                OnTransactionsRestored(true, string.Empty);
            #elif UNITY_IOS
			    extensions.GetExtension<IAppleExtensions>().RestoreTransactions(OnTransactionsRestored);
            #endif
        }


        /// <summary>
        /// Callback invoked after initiating a restore attempt.
		/// </summary>
        public static void OnTransactionsRestored(bool success, string error)
        {
			if(isDebug && !success)
                Debug.LogWarning("IAPManager reports: Restore failed. " + error);

            //the IAPListener will try to present a transaction restore message
            //if you are using server validation, it is likely that the restore requests take longer
            //than the transactions loop so instead a random product restored message will be shown
            restoreTransactionsFinishedEvent?.Invoke(success);
        }


        /// <summary>
        /// Callback of Unity IAP returning why the store could not be initialized to begin with.
        /// You might want to subscribe to the initializeFailedEvent in your UI for displaying the reason to the user. 
        /// </summary>
        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            switch (error)
            {
                case InitializationFailureReason.AppNotKnown:
                    Debug.LogError("Is your App correctly uploaded on the relevant publisher console?");
                    break;
                case InitializationFailureReason.PurchasingUnavailable:
                    // Ask the user if billing is disabled in device settings.
                    Debug.LogWarning("Billing disabled!");
                    break;
                case InitializationFailureReason.NoProductsAvailable:
                    // Developer configuration error; check product metadata.
                    Debug.LogWarning("No products available for purchase!");
                    break;
            }

            if (isDebug) Debug.LogError("IAPManager reports: InitializeFailed. Error: " + message);
            initializeFailedEvent?.Invoke(message);
        }


        /// <summary>
        /// Overload for failed billing initialization with failure reason enum.
        /// </summary>
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            if (isDebug) Debug.LogError("IAPManager reports: InitializeFailed. Error: " + error);
            initializeFailedEvent?.Invoke(error.ToString());
        }


        /// <summary>
		/// Overload for failed billing initialization with string error message.
        /// </summary>
        public void OnInitializeFailed(string error)
        {
            switch (error)
            {
                case "NoLinkedGoogleAccount":
                    //this could be fired multiple times, after pausing/resuming the app!
                    //make sure to not annoy users with the error, e.g. by showing it only once per app launch
                    Debug.LogWarning("Unable to connect to Google Play. User may not have a Google account on their device.");
                    break;
            }

            if (isDebug) Debug.LogError("IAPManager reports: InitializeFailed. Error: " + error);
            initializeFailedEvent?.Invoke(error);
        }


        /// <summary>
        /// This will be called when an attempted purchase fails. PurchaseFailureDescription
        /// </summary>
        public void OnPurchaseFailed(Product item, PurchaseFailureDescription description)
        {
            if (isDebug) Debug.Log("IAPManager reports: PurchaseFailed. Error: " + description.reason);
            purchaseFailedEvent?.Invoke(description.reason + "\n" + description.message);
        }


        /// <summary>
        /// This will be called when an attempted purchase fails. PurchaseFailureReason
        /// </summary>
        public void OnPurchaseFailed(Product item, PurchaseFailureReason reason)
		{
            if (isDebug) Debug.Log("IAPManager reports: PurchaseFailed. Error: " + reason);
            purchaseFailedEvent?.Invoke(reason.ToString());
        }

		
		/// <summary>
        /// This will be called when an attempted purchase fails. string
		/// </summary>
        public static void OnPurchaseFailed(string error)
        {
            if (isDebug) Debug.Log("IAPManager reports: PurchaseFailed. Error: " + error);
            purchaseFailedEvent?.Invoke(error);
        }


        /// <summary>
        /// This will be called when an attempted consume fails.
        /// </summary>
        public static void OnConsumeFailed(string error)
        {
            if (isDebug) Debug.Log("IAPManager reports: ConsumeFailed. Error: " + error);
            consumeFailedEvent?.Invoke(error);
        }


        /// <summary>
        /// Refreshes the visual representation of all shop items based on previous actions
        /// or user interaction, meaning we set them to 'purchased' or 'selected' in the GUI.
        /// You can call this manually in case PlayerData (unlock requirements) have changed.
        /// Can only refresh shop items which were active and added to the shopItems dic before.
        /// </summary>
        public void RefreshShopItemAll()
        {
            foreach (string key in shopItems.Keys)
                RefreshShopItem(key);
        }


        /// <summary>
        /// Refreshes the visual representation of a specific group from the Project Settings.
        /// Same as RefreshShopItemAll(), but only for items within this specific group name.
        /// You can call this manually i.e. when manually changing product selections in a group.
        /// Can only refresh shop items which were active and added to the shopItems dic before.
        /// </summary>
        public void RefreshShopItemGroup(string groupName)
        {
            IAPCategory category = asset.categoryList.SingleOrDefault(x => x.ID == groupName);

            if (category == null)
            {
                if (isDebug) Debug.LogWarning("IAPManager RefreshGroup: groupName not found.");
                return;
            }

            List<IAPProduct> products = asset.productList.FindAll(x => x.category == category);

            for (int i = 0; i < products.Count; i++)
                RefreshShopItem(products[i].ID);
        }


        /// <summary>
        /// Refreshes the visual representation of a specific shop item. This is called automatically
        /// because of subscribing to the DBManager update event. It also means saving performance due
        /// to not refreshing all items on each database change every time.
        /// Can only refresh shop items which were active and added to the shopItems dic before.
        /// </summary>
        public void RefreshShopItem(string productID)
        {
            if (shopItems.ContainsKey(productID))
                shopItems[productID].Refresh();
        }


        /// <summary>
        /// Returns a list of all upgrade IDs associated to a product.
        /// </summary>
        public static List<string> GetAllUpgrades(string productId)
        {
            List<string> list = new List<string>();
            IAPProduct product = GetIAPProduct(productId);

            if (product == null)
            {
                if (isDebug)
                    Debug.LogError("Product " + productId + " not found in IAP Settings. Make sure "
                                   + "to remove your app from the device before deploying it again!");
            }
            else
            {
                while (product != null && product.nextUpgrade != null)
                {
                    list.Add(product.nextUpgrade.ID);
                    product = product.nextUpgrade;
                }
            }
           
            return list;
        }


        /// <summary>
        /// Returns the last purchased upgrade ID of a product,
        /// or the main product itself if it hasn't been purchased yet.
        /// </summary>
        public static string GetCurrentUpgrade(string productId)
        {
            if (DBManager.GetPurchase(productId) == 0)
                return productId;

            string id = productId;
            List<string> upgrades = GetAllUpgrades(productId);

            for (int i = upgrades.Count - 1; i >= 0; i--)
            {
                if (DBManager.GetPurchase(upgrades[i]) > 0)
                {
                    id = upgrades[i];
                    break;
                }
            }

            return id;
        }


        /// <summary>
        /// Returns the next unpurchased upgrade ID of a product.
        /// </summary>
        public static string GetNextUpgrade(string productId)
        {
            string currentID = GetCurrentUpgrade(productId);
            IAPProduct product = GetIAPProduct(currentID);

            if (DBManager.GetPurchase(currentID) == 0 || product == null || product.nextUpgrade == null) return currentID;
            else return product.nextUpgrade.ID;
        }


        /// <summary>
        /// Returns the global identifier of an in-app product, specified in the IAP Project Settings.
        /// </summary>
        public static string GetProductGlobalIdentifier(string storeId)
        {
            if(controller != null && controller.products != null)
            {
                Product p = controller.products.WithStoreSpecificID(storeId);
                if (p != null)
                    return p.definition.id;
            }

            //fallback in case Unity IAP has not been initialized yet
            foreach (IAPProduct product in instance.asset.productList)
            {
                if (product.storeIDs.Exists(x => x.active && x.ID == storeId))
                    return product.ID;
            }

            return storeId;
        }
      
        
        /// <summary>
        /// Returns the list of products used when initializing Unity IAP.
        /// </summary>
        public static ProductDefinition[] GetProductDefinitions()
        {
            if (builder == null || builder.products == null)
                return new ProductDefinition[0];
            else
                return builder.products.ToArray();
        }


        /// <summary>
        /// Returns whether there are any deferred/pending purchases, supported on Google Play.
        /// </summary>
        public static bool HasPendingPurchases()
        {
            //Unity IAP is not initialized yet
            if (controller == null) return false;

            #if UNITY_ANDROID
            if(appStore == AppStore.GooglePlay.ToString())
            {
                IGooglePlayStoreExtensions ext = extensions.GetExtension<IGooglePlayStoreExtensions>();
                foreach(Product p in controller.products.all)
                {
                    if (p.hasReceipt && ext.IsPurchasedProductDeferred(p))
                        return true;
                }
            }
            #endif

            return false;
        }


        /// <summary>
        /// Returns whether the product either has currency or product rewards except from itself.
        /// </summary>
        public static bool HasProductRewards(string productId)
        {
            IAPProduct product = GetIAPProduct(productId);
            if (product == null) return false;

            if (product.rewardList.Exists(x => x.currency != null) || product.rewardList.Exists(x => x.product != null && x.product.ID != productId))
                return true;
            else
                return false;
        }


        /// <summary>
        /// Returns the product's reward list which consists of currency/product ID and amount.
        /// </summary>
        public static List<KeyValuePairStringInt> GetProductRewards(string productId)
        {
            List<KeyValuePairStringInt> items = new List<KeyValuePairStringInt>();
            IAPProduct product = GetIAPProduct(productId);
            if (product == null) return items;

            foreach (IAPExchangeObject obj in product.rewardList)
            {
                switch (obj.type)
                {
                    case IAPExchangeObject.ExchangeType.VirtualCurrency:
                        items.Add(new KeyValuePairStringInt() { Key = obj.currency.ID, Value = obj.amount });
                        break;
                    case IAPExchangeObject.ExchangeType.VirtualProduct:
                        //ignore self-references on non-consumables
                        if (obj.product.ID == product.ID) continue;
                        items.Add(new KeyValuePairStringInt() { Key = obj.product.ID, Value = obj.amount });
                        break;
                }
            }

            return items;
        }

        /// <summary>
        /// Returns a string array of all IAP IDs.
        /// </summary>
        public static string[] GetAllIDs()
        {
            return instance.asset.productList.Select(product => product.ID).ToArray();
        }
		
		
		/// <summary>
        /// Returns a string array of all real money IAP IDs only.
        /// </summary>
        public static string[] GetRealMoneyIDs()
        {
            return instance.asset.productList.Where(x => x.priceList.Exists(z => z.type == IAPExchangeObject.ExchangeType.RealMoney)).Select(x => x.ID).ToArray();
        }


        /// <summary>
        /// Returns the IAPProduct with a specific global ID.
        /// </summary>
        public static IAPProduct GetIAPProduct(string productID)
        {
            if (!instance || string.IsNullOrEmpty(productID)) return null;

            IAPProduct product = instance.asset.productList.SingleOrDefault(x => x.ID == productID);

            if (product == null)
            {
                //fallback in case we have passed in a storeID
                string globalID = GetProductGlobalIdentifier(productID);
                if(productID != globalID)
                    product = instance.asset.productList.SingleOrDefault(x => x.ID == globalID);
            }

            return product;
        }


        /// <summary>
        /// Returns instantiated IAPItem shop item reference.
        /// Null if there is none in the current scene.
        /// </summary>
        public static ShopItem2D GetShopItem(string productID)
        {
            if (shopItems.ContainsKey(productID))
                return shopItems[productID];
            else
                return null;
        }


        /// <summary>
        /// Returns the group name of a specific product ID.
        /// <summary>
        public static string GetProductCategoryName(string productID)
        {
            IAPProduct product = GetIAPProduct(productID);
            return (product != null && product.category != null) ? product.category.ID : null;
        }
    }
}