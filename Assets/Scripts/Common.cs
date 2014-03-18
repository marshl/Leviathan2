using UnityEngine;
using System.Collections;

public enum PLAYER_TYPE : int
{
	COMMANDER1,
	COMMANDER2,
	FIGHTER1,
	FIGHTER2,
};

public abstract class Common
{
	/// <summary>
	/// Returns a the value of a hermite curve at point t
	/// </summary>
	/// <param name="start">The start position of the curve</param>
	/// <param name="end">The end position of the curve</param>
	/// <param name="startTangent">The tangent that pulls the start of the curve out in this directon</param>
	/// <param name="endTangent">The tangent that pulls the end of the curve out in this directon</param>
	/// <param name="t">The point on the curve (0=start, 1=end)</param>
	public static Vector3 Hermite( Vector3 start, Vector3 end, Vector3 startTangent, Vector3 endTangent, float t )
	{
		float t2 = t*t;
		float t3 = t*t*t;
		return (1 - 3*t2 + 2*t3)*start + t2*(3 - 2*t)*end + t*(t-1)*(t-1)*startTangent + t2*(t-1)*endTangent;
	}

	/// <summary>
	/// Returns the position of the mouse on the specified plane
	/// </summary>
	/// <returns><c>true</c> if the position could be found <c>false</c> otherwise.</returns>
	/// <param name="_outPos">The position of the mouse on the plane</param>
	/// <param name="_planePos">The position of the plane</param>
	/// <param name="_planeNormal">The normal of the plane</param>
	public static bool MousePositionToPlanePoint( out Vector3 _outPos, Vector3 _planePos, Vector3 _planeNormal )
	{
		return MousePositionToPlanePoint( out _outPos, new Plane( _planeNormal, _planePos ) );	
	}

	/// <summary>
	/// Returns the position of the mouse on the specified plane
	/// </summary>
	/// <returns><c>true</c>if the position could be found, <c>false</c> otherwise.</returns>
	/// <param name="_outPos">The position of the mouse on the plane</param>
	/// <param name="_plane">The plane to compare against</param>
	public static bool MousePositionToPlanePoint( out Vector3 _outPos, Plane _plane )
	{
		Ray mouseRay = Camera.main.ScreenPointToRay( Input.mousePosition );
		float dist;
		
		bool hit = _plane.Raycast( mouseRay, out dist );
		_outPos = mouseRay.GetPoint( dist );
		return hit;
	}

	/// <summary>
	/// Returns a randomised vector with each x/y/z value between the corresponding values in the min/max vectors
	/// </summary>
	/// <returns>The randomised vector3.</returns>
	/// <param name="_min">The minimum random value</param>
	/// <param name="_max">The maximum random value</param>
	public static Vector3 RandomVector3( Vector3 _min, Vector3 _max )
	{
		return new Vector3( Random.Range( _min.x, _max.x ), Random.Range( _min.y, _max.y ), Random.Range( _min.z, _max.z ) );
	}

	/// <summary>
	/// Returns a random vector3 with x/y/z values between the min and max
	/// </summary>
	/// <returns>The vector3.</returns>
	/// <param name="_min">_min.</param>
	/// <param name="_max">_max.</param>
	public static Vector3 RandomVector3( float _min, float _max )
	{
		return Common.RandomVector3( new Vector3( _min, _min, _min), new Vector3( _max, _max, _max ) );
	}

	/// <summary>
	/// Returns the point x on a gaussian bell curve (as copied from UnityGenetics)
	/// </summary>
	/// <returns>The value of the point on the curve</returns>
	/// <param name="_x">The point on the curve</param>
	/// <param name="_height">The height of the bell of the curve</param>
	/// <param name="_centre">The centre point of the bell</param>
	/// <param name="_std">The standard deviations of the curve (99% of all values fall within 3 standard deviations of the centre)</param>
	/// <param name="_base">The minimum value of any point on the curve</param>
	public static float GaussianCurve( float _x, float _height, float _centre, float _std, float _base = 0.0f )
	{
		if ( _std == 0.0f )
		{
			throw new System.DivideByZeroException();
		}

		return _height * Mathf.Exp( - Mathf.Pow( _x - _centre, 2.0f ) / ( 2.0f * _std * _std ) ) + _base;
		/*_           
		 *          ( (x - c)^2 )
		 * h . e ^ -( --------_ ) + b
		 *          (  2 . o^2  )
		 */
	}

	public static float GaussianCurveClamped( float _x, float _height, float _centre, float _extents, float _base = 0.0f )
	{ 
		if ( _x < _centre - _extents || _x > _centre + _extents )
		{
			return _base;
		}
		else
		{
			float val = GaussianCurve( _x, _height, _centre, _extents / 3.0f, 0.0f );
			return val + _base ;
		}
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
		//TODO: There has to be a better way to to this LM:18/03/14
		return int.Parse( _player.ToString() );
	}

	public static int NetworkID()
	{
		return NetworkID( Network.player );
	}
	
	public static float SmoothLerp( float _value, float _min, float _max )
	{
		return ( (Mathf.Sin((_value-0.5f)*Mathf.PI) * (_max-_min) ) + _max + _min) / 2.0f;
	}
}
