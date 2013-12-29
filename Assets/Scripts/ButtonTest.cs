using UnityEngine;
using System.Collections;

public class ButtonTest : MonoBehaviour {

	private void OnMouseEnter()
	{
		this.GetComponent<TextMesh>().color = Color.red;
	}
}
