using UnityEngine;
using System.Collections;

public class ComponentTower : MonoBehaviour
{
	public CapitalShipMaster capitalShip;
	public Renderer baseRenderer;

	public TurretBehavior turret;

	public Color highlightedColour;

	public bool isActive = true;
	public bool isHighlighted = false;
	
	public void Deactivate()
	{
		this.isActive = false;
		this.baseRenderer.enabled = false;
		this.turret = null;
	}

	public void Activate()
	{
		this.isActive = true;
		this.baseRenderer.enabled = true;
		this.turret = null;
	}

	public void Occupy( TurretBehavior _turret )
	{
		this.turret = _turret;
		this.turret.componentTower = this;
	}

	public void Unoccupy()
	{
		this.turret = null;
	}

	public void Highlight()
	{
		this.isHighlighted = true;
		this.baseRenderer.material.color = this.highlightedColour;
	}

	public void RemoveHighlight()
	{
		this.isHighlighted = false;
		this.baseRenderer.material.color = Color.white;
	}
}
