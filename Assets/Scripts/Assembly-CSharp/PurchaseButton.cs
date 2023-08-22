using UnityEngine;

public class PurchaseButton : MonoBehaviour
{
	public SpriteRenderer PurchasedIcon;

	public TextRenderer NameText;

	public SpriteRenderer HatImage;

	public SpriteRenderer Background;

	public AmongUsProduct Product;

	public bool Purchased;

	public string Name;

	public string Price;

	public StoreMenu Parent { get; set; }

	public void SetItem(AmongUsProduct product, string name, string price, bool purchased)
	{
		Product = product;
		Purchased = purchased;
		Name = name;
		Price = price;
		PurchasedIcon.enabled = Purchased;
		if ((bool)Product.HatData)
		{
			NameText.gameObject.SetActive(false);
			HatImage.transform.parent.gameObject.SetActive(true);
			PlayerControl.SetHatImage(Product.HatData, HatImage);
			Background.size = new Vector2(0.7f, 0.7f);
			Background.GetComponent<BoxCollider2D>().size = new Vector2(0.7f, 0.7f);
			PurchasedIcon.transform.localPosition = new Vector3(0f, 0f, -1f);
		}
		else
		{
			NameText.Text = Name;
		}
	}

	internal void SetPurchased()
	{
		Purchased = true;
		PurchasedIcon.enabled = true;
	}

	public void DoPurchase()
	{
		Parent.SetProduct(this);
	}
}
