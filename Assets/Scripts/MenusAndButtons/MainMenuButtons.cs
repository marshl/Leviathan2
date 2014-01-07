using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenuButtons : MonoBehaviour
{
	public GameObject mainPanelObj;
	public GameObject hostPanelObj;

	public GUITextField hostportField;
	public GUITextField hostNameField;
	public GUITextField hostDescriptionField;

	private void Awake()
	{
		this.mainPanelObj.SetActive( true );
	}

	private void OnHostButtonDown()
	{
		this.mainPanelObj.SetActive( false );
		this.hostPanelObj.SetActive( true );
	}

	private void OnReturnToMainDown()
	{
		this.mainPanelObj.SetActive( true );
		this.hostPanelObj.SetActive( false );
	}
}
