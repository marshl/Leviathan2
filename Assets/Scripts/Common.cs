using UnityEngine;
using System.Collections;

public class Common : MonoBehaviour {

	public static Vector3 Hermite( Vector3 start, Vector3 end, Vector3 startTangent, Vector3 endTangent, float t )
	{
		float t2 = t*t;
		float t3 = t*t*t;
		//return ( 2 * t3 - 3 * t2 + 1) * start + (t3 - 2*t2 + t) * startTangent + ( -2 * t3 + 3 * t2 ) * end + ( t3 - t2) * endTangent;
		return (1 - 3*t2 + 2*t3)*start + t2*(3 - 2*t)*end + t*(t-1)*(t-1)*startTangent + t2*(t-1)*endTangent;
	}
	
	public static bool MousePositionToPlanePoint( out Vector3 _outPos, Vector3 _planePos, Vector3 _planeNormal )
	{
		return MousePositionToPlanePoint( out _outPos, new Plane( _planeNormal, _planePos ) );	
	}
	
	public static bool MousePositionToPlanePoint( out Vector3 _outPos, Plane _plane )
	{
		Ray mouseRay = Camera.main.ScreenPointToRay( Input.mousePosition );
		float dist;
		
		bool hit = _plane.Raycast( mouseRay, out dist );
		_outPos = mouseRay.GetPoint( dist );
		return hit;
	}

	public float GetObjectIntersectionTime( Transform _obj1, Transform _obj2, float _speed1, float _speed2 )
	{
		return 0.0f;
	}
}
