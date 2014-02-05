using UnityEngine;
using System.Collections;

public abstract class Common : MonoBehaviour
{
	public static Vector3 Hermite( Vector3 start, Vector3 end, Vector3 startTangent, Vector3 endTangent, float t )
	{
		float t2 = t*t;
		float t3 = t*t*t;
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

	public static Vector3 RandomVector3( Vector3 _min, Vector3 _max )
	{
		return new Vector3( Random.Range( _min.x, _max.x ), Random.Range( _min.y, _max.y ), Random.Range( _min.z, _max.z ) );
	}

	public static Vector3 RandomVector3( float _min, float _max )
	{
		return Common.RandomVector3( new Vector3( _min, _min, _min), new Vector3( _max, _max, _max ) );
	}

	public static float GaussianCurve( float _x, float _height, float _centre, float _std, float _base = 0.0f )
	{
		return _height * Mathf.Exp( - Mathf.Pow( _x - _centre, 2.0f ) / ( 2.0f * _std * _std ) ) + _base;
		/*_           
		 *          ( (x - c)^2 )
		 * h . e ^ -( --------_ ) + b
		 *          (  2 . o^2  )
		 */
	}

	/// Keeps the camera angle in the limits
	static public float ClampAngleDegrees( float _angle, float _min, float _max )
	{
		while ( _angle < -360.0f )
			_angle += 360.0f;
		while ( _angle > 360.0f )
			_angle -= 360.0f;
		
		return Mathf.Clamp( _angle, _min, _max );
	}

	public static int NetworkID( NetworkPlayer _player )
	{
		return int.Parse( _player.ToString() );
	}
}
