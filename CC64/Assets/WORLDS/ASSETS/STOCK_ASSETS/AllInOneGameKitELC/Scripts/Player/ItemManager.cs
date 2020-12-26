/// All in One Game Kit - Easy Ledge Climb Character System
/// ItemManager.cs
///
/// This script allows the player to:
/// 1. Have an inventory of different items.
/// 2. Count and display the number of items the player currently holds (either by simply counting the number, showing the number relative to the item's limit (with zeros before it),
///	or by adding by a prefix or suffix to the number).
/// 3. Set limits for the number of items the player can have.
/// 4. Set UI images and text for different items.
/// 5. Position the UI images and text anywhere on the screen.
///
/// (C) 2015-2018 Grant Marrs

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour {
	
	public Transform playerCamera; //the camera set to follow the player (automatically assigns to Camera.main if not set)
	[System.Serializable]
	public class ItemList {
		
		//item data
		public string itemTag = "Coin"; //the tag of the item
		public Sprite itemUI; //the UI of the item
		public Vector3 uIPosition = new Vector3(7, 163, 1); //the position of the item UI
		public Vector2 uIScale = new Vector2(60, 60); //the scale of the item UI
		public Font font; //the font of the text
		public int fontSize = 72; //the size of the text font
		public int outlineSize = 19; //the size of the text outline
		public Color fontColor = Color.white; //the color of the text
		public Color outlineColor = Color.black; //the color of the text outline
		public string itemCountPrefix; //prefix that comes before the item count
		public string itemCountSuffix; //suffix that comes after the item count
		public bool useItemLimit = true; //determines whether or not limit the amount of items you can have
		public int maximumItemLimit = 99; //the maximum amount of items you can have
		public bool addZerosBeforeItemCount = true; //adds zeros before the item count (the number of zeros added is relative to the maximum item limit)
		public Vector2 uITextPosition = new Vector3(82, -3); //the position of the UI item count text
		
		//item count variables
		[HideInInspector]
		public int currentValue;
		[HideInInspector]
		public string itemCount;
		[HideInInspector]
		public GameObject item;
		[HideInInspector]
		public GameObject text;
		
		//UI text position values
		[HideInInspector]
		public float textValueX;
		[HideInInspector]
		public float textValueY;
		[HideInInspector]
		public float textValueX2;
		[HideInInspector]
		public float textValueY2;
		[HideInInspector]
		public Vector3 textPos;
		
		[HideInInspector]
		public GameObject itemHolder; //the parent of the item UI
		[HideInInspector]
		public Vector2 anchorMin;
		
	}
	public ItemList[] itemList = new ItemList[1]; //the items that the player can carry
	
	private GameObject canvas; //the parent of the UI
	
	// Use this for initialization
	void Start () {
		
		//setting the camera if it has not been assigned
		if (playerCamera == null && Camera.main.transform != null){
			playerCamera = Camera.main.transform;
		}
		
	}
	
	void FixedUpdate () {
		
		//getting the canvas to hold the UI
		if (canvas == null){
			canvas = GameObject.Find("Canvas");
		}
		
		for (int i = 0; i < itemList.Length; i++){
			
			if (itemList[i].useItemLimit && itemList[i].currentValue > itemList[i].maximumItemLimit){
				itemList[i].currentValue = itemList[i].maximumItemLimit;
			}
			
			string stringScoreLength = itemList[i].maximumItemLimit.ToString();
			
			// Change to how many digits you want
			int scoreLength = stringScoreLength.Length;
			
			// score as a string
			string scoreString = itemList[i].currentValue.ToString();
			
			// get number of 0s needed
			int numZeros = scoreLength - scoreString.Length;
			
			string newScore = "";
			for(int z = 0; z < numZeros; z++){
				newScore += "0";
			}
			
			if (itemList[i].useItemLimit && itemList[i].addZerosBeforeItemCount){
				itemList[i].itemCount = newScore + scoreString;
			}
			else {
				itemList[i].itemCount = scoreString;
			}
			
			//creating parent for item UI
			if (itemList[i].itemHolder == null){
				itemList[i].itemHolder = new GameObject();
				itemList[i].itemHolder.AddComponent<RectTransform>();
				itemList[i].itemHolder.gameObject.name = itemList[i].itemTag + "Holder";
				itemList[i].itemHolder.gameObject.layer = 5;
			}
			itemList[i].itemHolder.transform.SetParent(canvas.transform);
			itemList[i].itemHolder.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
			
			//creating the item UI
			if (itemList[i].itemHolder != null){
				
				//item image
				if (itemList[i].item == null){
					if (itemList[i].itemUI != null && itemList[i].itemHolder.transform.childCount < itemList.Length){
						//creating item UI Texture so that we can create the item's UI image
						GameObject itemImage = new GameObject();
						itemImage.transform.localScale = new Vector3(0, 0, 0);
						itemImage.AddComponent<Image>();
						itemImage.GetComponent<Image>().sprite = itemList[i].itemUI;
						//creating the UI image of the item
						itemList[i].item = (GameObject)Instantiate(itemImage.gameObject, transform.position, Quaternion.identity);
						itemList[i].item.gameObject.name = itemList[i].itemTag + "Image";
						itemList[i].item.gameObject.layer = 5;
						itemList[i].item.transform.SetParent(itemList[i].itemHolder.transform);
						itemList[i].item.GetComponent<RectTransform>().sizeDelta = new Vector2(itemList[i].uIScale.x, itemList[i].uIScale.y);
						itemList[i].item.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
						itemList[i].item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
						//destroying image UI texture
						Destroy(itemImage.gameObject);
					}
				}
				else {
					itemList[i].item.GetComponent<RectTransform>().sizeDelta = new Vector2(itemList[i].uIScale.x, itemList[i].uIScale.y);
					itemList[i].item.GetComponent<Image>().sprite = itemList[i].itemUI;
					itemList[i].itemHolder.transform.SetSiblingIndex((int)itemList[i].uIPosition.z);
					AutoAnchor(i);
				}
				
				//item text
				if (itemList[i].text == null){
					if (itemList[i].itemHolder.transform.childCount < itemList.Length + 1){
						//creating item UI Texture so that we can create the item's UI text
						GameObject itemText = new GameObject();
						itemText.transform.localScale = new Vector3(0, 0, 0);
						itemText.AddComponent<Text>();
						itemText.AddComponent<Outline>();
						//creating the UI text of the item
						itemList[i].text = (GameObject)Instantiate(itemText.gameObject, transform.position, Quaternion.identity);
						itemList[i].text.gameObject.name = itemList[i].itemTag + "Text";
						itemList[i].text.gameObject.layer = 5;
						itemList[i].text.transform.SetParent(itemList[i].itemHolder.transform);
						itemList[i].text.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
						itemList[i].text.GetComponent<RectTransform>().anchoredPosition = itemList[i].uITextPosition;
						//destroying image UI texture
						Destroy(itemText.gameObject);
					}
				}
				else {
					itemList[i].text.GetComponent<Text>().font = itemList[i].font;
					itemList[i].text.GetComponent<Text>().fontSize = itemList[i].fontSize;
					itemList[i].text.GetComponent<Text>().color = itemList[i].fontColor;
					itemList[i].text.GetComponent<Text>().text = itemList[i].itemCountPrefix + itemList[i].itemCount + itemList[i].itemCountSuffix;
					itemList[i].text.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
					itemList[i].text.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Overflow;
					itemList[i].text.GetComponent<Text>().verticalOverflow = VerticalWrapMode.Overflow;
					itemList[i].text.GetComponent<Outline>().effectColor = itemList[i].outlineColor;
					itemList[i].text.GetComponent<Outline>().effectDistance = new Vector2(itemList[i].outlineSize/10f, itemList[i].outlineSize/10f);
					itemList[i].text.GetComponent<RectTransform>().anchoredPosition = itemList[i].uITextPosition;
				}
				
			}
			
			
		}
		
	}
	
	void LateUpdate () {
		
		//getting the canvas to hold the UI
		if (canvas == null){
			canvas = GameObject.Find("Canvas");
		}
		
		for (int i = 0; i < itemList.Length; i++){
			
			//creating parent for item UI
			if (itemList[i].itemHolder == null){
				itemList[i].itemHolder = new GameObject();
				itemList[i].itemHolder.AddComponent<RectTransform>();
				itemList[i].itemHolder.gameObject.name = itemList[i].itemTag + "Holder";
				itemList[i].itemHolder.gameObject.layer = 5;
			}
			itemList[i].itemHolder.transform.SetParent(canvas.transform);
			itemList[i].itemHolder.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
			
			//creating the item UI
			if (itemList[i].itemHolder != null){
				
				//item image
				if (itemList[i].item == null){
					if (itemList[i].itemUI != null && itemList[i].itemHolder.transform.childCount < itemList.Length){
						//creating item UI Texture so that we can create the item's UI image
						GameObject itemImage = new GameObject();
						itemImage.transform.localScale = new Vector3(0, 0, 0);
						itemImage.AddComponent<Image>();
						itemImage.GetComponent<Image>().sprite = itemList[i].itemUI;
						//creating the UI image of the item
						itemList[i].item = (GameObject)Instantiate(itemImage.gameObject, transform.position, Quaternion.identity);
						itemList[i].item.gameObject.name = itemList[i].itemTag + "Image";
						itemList[i].item.gameObject.layer = 5;
						itemList[i].item.transform.SetParent(itemList[i].itemHolder.transform);
						itemList[i].item.GetComponent<RectTransform>().sizeDelta = new Vector2(itemList[i].uIScale.x, itemList[i].uIScale.y);
						itemList[i].item.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
						itemList[i].item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
						//destroying image UI texture
						Destroy(itemImage.gameObject);
					}
				}
				else {
					itemList[i].item.GetComponent<RectTransform>().sizeDelta = new Vector2(itemList[i].uIScale.x, itemList[i].uIScale.y);
					itemList[i].item.GetComponent<Image>().sprite = itemList[i].itemUI;
					itemList[i].itemHolder.transform.SetSiblingIndex((int)itemList[i].uIPosition.z);
					AutoAnchor(i);
				}
				
				//item text
				if (itemList[i].text == null){
					if (itemList[i].itemHolder.transform.childCount < itemList.Length + 1){
						//creating item UI Texture so that we can create the item's UI text
						GameObject itemText = new GameObject();
						itemText.transform.localScale = new Vector3(0, 0, 0);
						itemText.AddComponent<Text>();
						itemText.AddComponent<Outline>();
						//creating the UI text of the item
						itemList[i].text = (GameObject)Instantiate(itemText.gameObject, transform.position, Quaternion.identity);
						itemList[i].text.gameObject.name = itemList[i].itemTag + "Text";
						itemList[i].text.gameObject.layer = 5;
						itemList[i].text.transform.SetParent(itemList[i].itemHolder.transform);
						itemList[i].text.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
						itemList[i].text.GetComponent<RectTransform>().anchoredPosition = itemList[i].uITextPosition;
						//destroying image UI texture
						Destroy(itemText.gameObject);
					}
				}
				else {
					itemList[i].text.GetComponent<Text>().font = itemList[i].font;
					itemList[i].text.GetComponent<Text>().fontSize = itemList[i].fontSize;
					itemList[i].text.GetComponent<Text>().color = itemList[i].fontColor;
					itemList[i].text.GetComponent<Text>().text = itemList[i].itemCountPrefix + itemList[i].itemCount + itemList[i].itemCountSuffix;
					itemList[i].text.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
					itemList[i].text.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Overflow;
					itemList[i].text.GetComponent<Text>().verticalOverflow = VerticalWrapMode.Overflow;
					itemList[i].text.GetComponent<Outline>().effectColor = itemList[i].outlineColor;
					itemList[i].text.GetComponent<Outline>().effectDistance = new Vector2(itemList[i].outlineSize/10f, itemList[i].outlineSize/10f);
					itemList[i].text.GetComponent<RectTransform>().anchoredPosition = itemList[i].uITextPosition;
				}
				
			}
			
			
		}
		
		
	}
	
	public void AutoAnchor (int i) {
		
		RectTransform r = itemList[i].itemHolder.GetComponent<RectTransform>();
		RectTransform p = canvas.GetComponent<RectTransform>();
		
		Vector2 offsetMin = r.offsetMin;
		Vector2 offsetMax = r.offsetMax;
		
		if (itemList[i].anchorMin == Vector2.zero){
			itemList[i].anchorMin = r.anchorMin;
		}
		Vector2 _anchorMin = itemList[i].anchorMin;
		Vector2 _anchorMax = r.anchorMax;
		
		float parent_width = p.rect.width;      
		float parent_height = p.rect.height;  
		
		itemList[i].anchorMin = new Vector2(_anchorMin.x + (offsetMin.x / parent_width), _anchorMin.y + (offsetMin.y / parent_height));
		Vector2 anchorMax = new Vector2(_anchorMax.x + (offsetMax.x / parent_width), _anchorMax.y + (offsetMax.y / parent_height));
		
		r.anchorMin = itemList[i].anchorMin + new Vector2(itemList[i].uIPosition.x/100f, itemList[i].uIPosition.y/100f);
		r.anchorMax = anchorMax;
		
		r.offsetMin = new Vector2(0, 0);
		r.offsetMax = new Vector2(0, 0);
		r.pivot = new Vector2(0.5f, 0.5f);
		
	}
	
	
	void OnControllerColliderHit(ControllerColliderHit hit) {
		if (!enabled) return;
		
		//items
		for (int i = 0; i < itemList.Length; i++){
			
			if (hit.gameObject.tag == itemList[i].itemTag){
				itemList[i].currentValue ++;
				Destroy(hit.gameObject);
			}
			
		}
		
	}
	
	void OnCollisionStay (Collision hit) {
		if (!enabled) return;
		
		//items
		for (int i = 0; i < itemList.Length; i++){
			
			if (hit.gameObject.tag == itemList[i].itemTag){
				itemList[i].currentValue ++;
				Destroy(hit.gameObject);
			}
			
		}
		
	}
	
	void OnTriggerStay (Collider hit) {
		if (!enabled) return;
		
		//items
		for (int i = 0; i < itemList.Length; i++){
			
			if (hit.gameObject.tag == itemList[i].itemTag){
				itemList[i].currentValue ++;
				Destroy(hit.gameObject);
			}
			
		}
		
	}
	
}