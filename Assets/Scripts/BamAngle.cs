using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// This class represents a BAM angle.
/// </summary>
/// <remarks>
/// BAM is a 360 angle system that goes counter clockwise from 0-360 degrees. It is what the original DOOM uses.
/// </remarks>
public class BamAngle
{
    private float _Angle;



    public BamAngle(float angle = 0f)
    {
        _Angle = angle;
    }




    private void Normalize360()
    {
        _Angle = _Angle % 360;
        _Angle = _Angle >= 0 ? _Angle : _Angle + 360;
    }


    // OVERLOADED OPERATORS
    // ========================================================================================================================================================================================================

    public static BamAngle operator +(BamAngle a, BamAngle b)
    {
        return new BamAngle(a.Value + b.Value);
    }

    public static BamAngle operator -(BamAngle a, BamAngle b)
    {
        return new BamAngle(a.Value - b.Value);
    }

    /// <summary>
    /// The negation operator.
    /// </summary>
    /// <param name="a">The angle to negate.</param>
    /// <returns>Returns the passed in angle negated.</returns>
    public static BamAngle operator -(BamAngle a)
    {
        return new BamAngle(360 - a.Value);
    }

    public static bool operator <(BamAngle a, BamAngle b)
    {
        return a.Value < b.Value;
    }

    public static bool operator <(BamAngle a, float b)
    {
        return a.Value < b;
    }

    public static bool operator >(BamAngle a, BamAngle b)
    {
        return a.Value > b.Value;
    }

    public static bool operator >(BamAngle a, float b)
    {
        return a.Value > b;
    }

    public static bool operator <=(BamAngle a, BamAngle b)
    {
        return a.Value <= b.Value;
    }

    public static bool operator <=(BamAngle a, float b)
    {
        return a.Value <= b;
    }

    public static bool operator >=(BamAngle a, BamAngle b)
    {
        return a.Value >= b.Value;
    }

    public static bool operator >=(BamAngle a, float b)
    {
        return a.Value >= b;
    }

    public static bool operator ==(BamAngle a, BamAngle b)
    {
        return a.Value == b.Value;
    }

    public static bool operator ==(BamAngle a, float b)
    {
        return a.Value == b;
    }

    public static bool operator !=(BamAngle a, BamAngle b)
    {
        return a.Value != b.Value;
    }

    public static bool operator !=(BamAngle a, float b)
    {
        return a.Value != b;
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        BamAngle second = obj as BamAngle;

        return second != null && this == second;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_Angle);
    }   



    // PROPERTIES
    // ========================================================================================================================================================================================================

    public static BamAngle GetAngleFromAtoB(Vector2 positionA, Vector2 positionB)
    {
        Vector3 delta = positionB - positionA;

        return new BamAngle(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
    }



    // PROPERTIES
    // ========================================================================================================================================================================================================

    /// <summary>
    /// This property takes the place of the overload of the = operator in the original code in the repo linked in the Readme file in the root folder of this project.
    /// That operator cannot be overriden in C#. So I made it a property instead.
    /// </summary>
    public float Value 
    { 
        get { return _Angle; } 
        set
        {
            _Angle = value;
            Normalize360();
        }
    }
  
}
