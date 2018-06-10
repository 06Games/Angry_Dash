using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IconSetup : MonoBehaviour 
{
	public Image image;

	public void Setup(Sprite sprite)
	{
		this.image.sprite = sprite;
	}
}
