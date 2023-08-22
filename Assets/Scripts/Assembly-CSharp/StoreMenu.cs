using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

public class StoreMenu : MonoBehaviour, IStoreListener
{
	private static bool ConfirmedPurchases;

	public SpriteRenderer HatSlot;

	public TextRenderer ItemName;

	public SpriteRenderer PurchaseBackground;

	public TextRenderer PriceText;

	public PurchaseButton PurchasablePrefab;

	public AmongUsProduct[] Purchasables = new AmongUsProduct[1]
	{
		new AmongUsProduct
		{
			ProductId = "bought_ads"
		}
	};

	public TextRenderer LoadingText;

	public TextRenderer RestorePurchasesButton;

	public SpriteRenderer HorizontalLine;

	public SpriteRenderer Banner;

	public SpriteRenderer TopArrow;

	public SpriteRenderer BottomArrow;

	public const string BoughtAdsProductId = "bought_ads";

	private IStoreController controller;

	private IExtensionProvider extensions;

	public Scroller Scroller;

	public Vector2 StartPositionVertical;

	public FloatRange XRange = new FloatRange(-1f, 1f);

	public int NumPerRow = 4;

	private PurchaseButton CurrentButton;

	private List<PurchaseButton> AllButtons = new List<PurchaseButton>();

	public PurchaseStates PurchaseState { get; private set; }

	public void Start()
	{
		// kay ey why s
	}

	public void Update()
	{
		TopArrow.enabled = !Scroller.AtTop;
		BottomArrow.enabled = !Scroller.AtBottom;
	}

	public void RestorePurchases()
	{
		if (ConfirmedPurchases)
		{
			return;
		}
		if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
		{
			// E
		}
		else
		{
			FinishRestoring();
		}
		ConfirmedPurchases = true;
		RestorePurchasesButton.transform.parent.gameObject.SetActive(false);
	}

	private void DestroyPurchaseButtons()
	{
		for (int i = 0; i < AllButtons.Count; i++)
		{
			Object.Destroy(AllButtons[i].gameObject);
		}
		AllButtons.Clear();
	}

	private void FinishRestoring()
	{
		DestroyPurchaseButtons();
		ShowAllButtons();
		RestorePurchasesButton.Text = "Purchases Restored";
	}

	public void SetProduct(PurchaseButton button)
	{
		if (PurchaseState != PurchaseStates.Started)
		{
			CurrentButton = button;
			if ((bool)CurrentButton.Product.HatData)
			{
				HatSlot.gameObject.SetActive(true);
				PlayerControl.SetHatImage(CurrentButton.Product.HatData, HatSlot);
				ItemName.Text = CurrentButton.Product.HatData.name;
			}
			else
			{
				HatSlot.gameObject.SetActive(false);
				ItemName.Text = "Remove All Ads";
			}
			if (button.Purchased)
			{
				PurchaseBackground.color = new Color(0.5f, 0.5f, 0.5f, 1f);
				PriceText.Color = new Color(0.8f, 0.8f, 0.8f, 1f);
				PriceText.Text = "Owned";
			}
			else
			{
				PurchaseBackground.color = Color.white;
				PriceText.Color = Color.white;
				PriceText.Text = button.Price;
			}
		}
	}

	public void BuyProduct()
	{
		if ((bool)CurrentButton && !CurrentButton.Purchased && PurchaseState != PurchaseStates.Started)
		{
			StartCoroutine(WaitForPurchaseAds(CurrentButton));
		}
	}

	public IEnumerator WaitForPurchaseAds(PurchaseButton button)
	{
		PurchaseState = PurchaseStates.Started;
		controller.InitiatePurchase(button.Product.ProductId);
		while (PurchaseState == PurchaseStates.Started)
		{
			yield return null;
		}
		if (PurchaseState == PurchaseStates.Success)
		{
			button.SetPurchased();
		}
		SetProduct(button);
	}

	public void Close()
	{
		HatsTab hatsTab = Object.FindObjectOfType<HatsTab>();
		if ((bool)hatsTab)
		{
			hatsTab.OnDisable();
			hatsTab.OnEnable();
		}
		base.gameObject.SetActive(false);
	}

	private void ShowAllButtons()
	{
		LoadingText.gameObject.SetActive(false);
		Banner.gameObject.SetActive(true);
		Vector3 localPosition = StartPositionVertical;
		RestorePurchasesButton.transform.parent.gameObject.SetActive(false);
		HorizontalLine.gameObject.SetActive(false);
		localPosition.y -= 0.3f;
		Banner.transform.localPosition = localPosition;
		localPosition.y -= 0.8f;
		localPosition.y -= 0.1f;
		Product[] array = (from o in controller.products.all
			orderby (!(o.definition.id == "bought_ads")) ? 1 : 0, o.metadata.localizedTitle
			select o).ToArray();
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			Product product = array[i];
			if (!product.availableToPurchase)
			{
				continue;
			}
			int num2 = Purchasables.IndexOf((AmongUsProduct p) => p.ProductId == product.definition.id);
			if (num2 != -1)
			{
				AmongUsProduct product2 = Purchasables[num2];
				int num3 = num % NumPerRow;
				if (product2.ProductId != "bought_ads")
				{
					localPosition.x = StartPositionVertical.x + XRange.Lerp((float)num3 / ((float)NumPerRow - 1f));
				}
				if (num3 == 0 && num > 1)
				{
					localPosition.y += -0.75f;
				}
				PurchaseButton purchaseButton = Object.Instantiate(PurchasablePrefab, Scroller.Inner);
				purchaseButton.transform.localPosition = localPosition;
				purchaseButton.Parent = this;
				purchaseButton.SetItem(product2, product.metadata.localizedTitle.Replace("(Among Us)", string.Empty), product.metadata.localizedPriceString, product.hasReceipt || SaveManager.GetPurchase(product2.ProductId));
				if (product.hasReceipt)
				{
					SaveManager.SetPurchased(product.definition.id);
				}
				if (i == 0)
				{
					localPosition.y += -0.45f;
				}
				else
				{
					num++;
				}
			}
		}
		localPosition.y += -0.75f;
		Scroller.YBounds.max = Mathf.Max(0f, 0f - localPosition.y - 2.5f);
	}

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		this.controller = controller;
		this.extensions = extensions;
		ShowAllButtons();
		if (ConfirmedPurchases)
		{
			RestorePurchasesButton.Text = "Purchases Restored";
		}
	}

	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
	{
		Debug.Log("Purchased product:" + e.purchasedProduct.metadata.localizedTitle);
		SaveManager.SetPurchased(e.purchasedProduct.definition.id);
		PurchaseState = PurchaseStates.Success;
		return PurchaseProcessingResult.Complete;
	}

	public void OnInitializeFailed(InitializationFailureReason error)
	{
		if (error == InitializationFailureReason.NoProductsAvailable)
		{
			LoadingText.Text = "Coming Soon!";
		}
		else
		{
			LoadingText.Text = "Loading Failed";
		}
	}

	public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
	{
		Debug.LogError("Failed: " + p);
		PurchaseState = PurchaseStates.Fail;
	}
}
