﻿using UnityEngine;
using System.Collections;

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
	
	public static int MyNetworkID()
	{
		return NetworkID( Network.player );
	}
	
	/// <summary>
	/// Gets the position where two objects will meet if the target stays on a constant heading and speed
	/// </summary>
	/// <returns>The intersection point</returns>
	/// <param name="_origin">The point where the target is </param>
	/// <param name="_target">_target.</param>
	/// <param name="_speed">_speed.</param>
	public static Vector3 GetTargetLeadPosition( Vector3 _origin, Transform _target, float _speed )
	{
		/*if ( _target.rigidbody == null )
		{
			return _target.position;
		}
		
		float flightDuration = ( _target.position - _origin ).magnitude
			/ ( _speed - _target.rigidbody.velocity.magnitude );
		
		if ( flightDuration <= 0.0f )
		{
			DebugConsole.Warning( "Projectile cannot catch up to target " + _target.gameObject.name, _target );
			return _target.position;
		}
		else
		{
			return _target.position + _target.rigidbody.velocity * flightDuration;
		}*/

		//return ((_target.position - _origin).magnitude / _speed ) * _target.rigidbody.velocity + _target.position;

		float s = _speed;
		float v = 0;
		if(_target.rigidbody != null)
		{
			v = _target.rigidbody.velocity.magnitude;
		}
		float d = ( _origin - _target.position ).magnitude;

		//float theta = Vector3.Angle( _target.forward.normalized, _origin-_target.position.normalized );
		float theta = Mathf.Acos( Vector3.Dot( _target.forward, (_origin - _target.position).normalized ) );
		//Debug.Log( theta + " : Cos " + Mathf.Cos( theta ) );
		float a = s*s-v*v;
		float b = 2*v*d*Mathf.Cos( theta );
		float c = -d*d;

		float x1 = (-b+Mathf.Sqrt( b*b - 4*a*c ))/(2*a);
		float x2 = (-b-Mathf.Sqrt( b*b - 4*a*c ))/(2*a);
		//Debug.Log( x1+" : "+x2);
		if ( x1 >= 0.0f )
		{
			return _target.position + _target.rigidbody.velocity * x1;
		}
		else
		{
			return _target.position + _target.rigidbody.velocity * x2;
		}

		/*Vector3 leadDirection = _target.position - _origin;
		leadDirection += leadDirection.magnitude
				* _target.rigidbody.velocity
				/ _speed;
		return leadDirection + _origin;*/
	}
	
	/// <summary>
	/// Returns a sine wave between the two values, starting at min at x = 0
	/// </summary>
	/// <returns>The value on the wave</returns>
	/// <param name="_x">The x value of the point on the wave</param>
	/// <param name="_min">The minimum extent of the wave</param>
	/// <param name="_max">The maximum extent of the wave</param>
	/// <param name="_interval">The distance between the min and max of the wave</param>
	public static float SmoothPingPong( float _x, float _min, float _max, float _interval )
	{
		return ( Mathf.Cos( _x * Mathf.PI / _interval) * (_min - _max) + _max + _min) / 2;
	}

	/// <summary>
	/// Helper function to return the opposite team while returning an error if neutral
	/// </summary>
	/// <returns>The opposing team of the team given</returns>
	/// <param name="_team">The team</param>
	public static TEAM OpposingTeam( TEAM _team )
	{
		if ( _team == TEAM.TEAM_1 ) 
		{
			return TEAM.TEAM_2;
		}
		else if ( _team == TEAM.TEAM_2 )
		{
			return TEAM.TEAM_1;
		}
		else
		{
			DebugConsole.Error( "Cannot find opposing team for netural" );
			return TEAM.NEUTRAL;
		} 
	}
}
