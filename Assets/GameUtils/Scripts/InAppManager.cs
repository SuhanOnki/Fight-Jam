using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

#if IN_APP_PURCHASING
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using UnityEngine.Purchasing.Extension;
#endif

namespace YsoCorp {
    namespace GameUtils {

        [DefaultExecutionOrder(-15)]
        public class InAppManager : BaseManager
#if IN_APP_PURCHASING
            , IDetailedStoreListener
#endif
            {

            private Dictionary<string, UnityEvent> _onPurchased = new Dictionary<string, UnityEvent>();
            private Dictionary<string, UnityEvent> _onPurchaseFailed = new Dictionary<string, UnityEvent>();
            [HideInInspector] public UnityEvent OnIAPInit = new UnityEvent();

#if IN_APP_PURCHASING
            private IStoreController _StoreController = null;
            private IExtensionProvider _StoreExtensionProvider = null;
#endif

            private void Awake() {
                this.ycManager.inAppManager = this;
            }

#if IN_APP_PURCHASING
            async void Start() {
                try {
                    var options = new InitializationOptions().SetEnvironmentName("production");

                    await UnityServices.InitializeAsync(options);
                    this.Init();
                } catch (Exception exception) {
                    // An error occurred during services initialization.
                }
            }

            public void Init() {
                if (this.IsInitialized()) {
                    return;
                }
                var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
                if (this.ycManager.ycConfig.InAppRemoveAds != "") {
                    builder.AddProduct(this.ycManager.ycConfig.InAppRemoveAds, ProductType.NonConsumable);
                }
                foreach (CustomInapp inapp in this.ycManager.ycConfig.CustomInapps) {
                    ProductType type = inapp.isConsumable ? ProductType.Consumable : ProductType.NonConsumable;
                    builder.AddProduct(inapp.inappKey, type);
                }
                UnityPurchasing.Initialize(this, builder);
            }

            private bool IsInitialized() {
                return this._StoreController != null && this._StoreExtensionProvider != null;
            }

            public Product GetProductById(string productId) {
                if (this._StoreController != null && this._StoreController.products != null) {
                    return this._StoreController.products.WithID(productId);
                }
                return null;
            }
#endif
            public string GetProductPrice(string productId) {
#if IN_APP_PURCHASING
                Product p = this.GetProductById(productId);
                if (p != null) {
                    return p.metadata.localizedPriceString;
                }
#endif
                return "";
            }

            public void AddListener(string productId, UnityAction onPurchase, bool purchaseSucceeded = true) {
                if (this.IsProductIdValid(productId)) {
                    if (purchaseSucceeded) {
                        if (this._onPurchased.ContainsKey(productId) == false) {
                            this._onPurchased[productId] = new UnityEvent();
                        }
                        this._onPurchased[productId].AddListener(onPurchase);
                    } else {
                        if (this._onPurchaseFailed.ContainsKey(productId) == false) {
                            this._onPurchaseFailed[productId] = new UnityEvent();
                        }
                        this._onPurchaseFailed[productId].AddListener(onPurchase);
                    }
                }
            }

            public void RemoveListener(string productId, UnityAction onPurchase, bool purchaseSucceeded = true) {
                UnityEvent eve = this.GetEvent(productId, purchaseSucceeded);
                eve?.RemoveListener(onPurchase);
            }

            public void RemoveAllListener(string productId, bool purchaseSucceeded = true) {
                UnityEvent eve = this.GetEvent(productId, purchaseSucceeded);
                eve?.RemoveAllListeners();
            }

            private UnityEvent GetEvent(string productId, bool purchaseSucceeded) {
                if (this.HasListener(productId, purchaseSucceeded) == false) return null; // ici

                if (purchaseSucceeded) {
                    return this._onPurchased[productId];
                } else {
                    return this._onPurchaseFailed[productId];
                }
            }

            public bool HasListener(string productId, bool purchaseSucceeded = true) {
                if (this.IsProductIdValid(productId)) {
                    if (purchaseSucceeded) {
                        return this._onPurchased.ContainsKey(productId);
                    } else {
                        return this._onPurchaseFailed.ContainsKey(productId);
                    }
                }
                return false;
            }

            public void BuyProductID(string productId) {
#if IN_APP_PURCHASING
                if (this.IsInitialized()) {
                    Product product = this.GetProductById(productId);
                    if (product != null && product.availableToPurchase) {
                        this.DebugLog(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                        this._StoreController.InitiatePurchase(product);
                    } else {
                        Debug.LogError("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                    }
                } else {
                    Debug.LogError("BuyProductID FAIL. Not initialized.");
                }
#endif
            }

            public void RestorePurchases() {
#if IN_APP_PURCHASING
                if (!this.IsInitialized()) {
                    Debug.LogError("RestorePurchases FAIL. Not initialized.");
                    return;
                }

                if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer) {
                    this.DebugLog("RestorePurchases started ...");
                    var apple = this._StoreExtensionProvider.GetExtension<IAppleExtensions>();
                    apple.RestoreTransactions((result, error) => {
                        if (result) {
                            this.DebugLog("RestorePurchases continuing. If no further messages, no purchases available to restore.");
                        } else {
                            this.DebugLog("RestorePurchases failed: " + error);
                        }
                    });
                } else {
                    Debug.LogError("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
                }
#endif
            }

#if IN_APP_PURCHASING
            public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
                this.DebugLog("OnInitialized: PASS");
                this._StoreController = controller;
                this._StoreExtensionProvider = extensions;
                this.OnIAPInit.Invoke();
            }

            public void OnInitializeFailed(InitializationFailureReason error) {
                Debug.LogError("OnInitializeFailed InitializationFailureReason:" + error);
            }

            public void OnInitializeFailed(InitializationFailureReason error, string? message) {
                Debug.LogError("OnInitializeFailed InitializationFailureReason:" + error);
            }

            public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) {
                bool validPurchase = true;

#if (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_EDITOR
                var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);

                try {
                    IPurchaseReceipt[] result = validator.Validate(args.purchasedProduct.receipt);
                    Debug.Log("Receipt is valid. Contents:");
                    foreach (IPurchaseReceipt productReceipt in result) {
                        Product p = this.GetProductById(productReceipt.productID);
                        if (p == null) {
                            this.DebugLog("Invalid receipt, not unlocking content");
                            validPurchase = false;
                        }

                        //GooglePlayReceipt google = productReceipt as GooglePlayReceipt;
                        //AppleInAppPurchaseReceipt apple = productReceipt as AppleInAppPurchaseReceipt;
                    }
                } catch (IAPSecurityException e) {
                    this.DebugLog("Invalid receipt, not unlocking content");
                    if (e.Message != "Sandbox receipt used in production") {
                        validPurchase = false;
                    }
                }
#endif

                if (validPurchase) {
                    this.ycManager.mmpManager.tenjinManager.SendTenjinPurchaseEvent(args);
                    string productId = args.purchasedProduct.definition.id;
                    Product p = this.GetProductById(productId);
                    if (p != null) {
                        float price = (float)p.metadata.localizedPrice;
                        string isoCurrencyCode = p.metadata.isoCurrencyCode;
                        this.ycManager.analyticsManager.InAppBought(productId, price, isoCurrencyCode);
                    }
                    this._onPurchased[productId]?.Invoke();
                } else {
                    
                }

                return PurchaseProcessingResult.Complete;
            }

            public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) {
                Debug.LogError(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
            }

            public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription) {
                Debug.LogError(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureDescription: {1}", product.definition.storeSpecificId, failureDescription.message));
            }
#endif

            public void BuyProductIDAdsRemove() {
                this.BuyProductID(this.ycManager.ycConfig.InAppRemoveAds);
            }

            public bool IsProductIdValid(string productId) {
#if IN_APP_PURCHASING
                if (productId.CompareTo(this.ycManager.ycConfig.InAppRemoveAds) == 0) {
                    return true;
                }
                foreach (CustomInapp inapp in this.ycManager.ycConfig.CustomInapps) {
                    if (inapp.inappKey.CompareTo(productId) == 0) {
                        return true;
                    }
                }
                Debug.LogError("[InApps] The InApp key: " + productId + " does not exist in the YCConfig list");
                return false;
#else
                return true;
#endif
            }

            private void DebugLog(object message) {
                if (this.ycManager.ycConfig.InappDebug) {
                    Debug.Log("[GameUtils - Inapps] " + message);
                }
            }

            [Serializable]
            public struct CustomInapp {
                public string inappKey;
                public bool isConsumable;

                public CustomInapp(string inappKey, bool isConsumable) {
                    this.inappKey = inappKey;
                    this.isConsumable = isConsumable;
                }
            }

        }
    }
}