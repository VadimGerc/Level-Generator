using UnityEngine;

public static class CalculationsHelper
{
    /// <summary>
    /// Calculates the angle between 2 directions
    /// </summary>
    public static float AngleBetween(Vector3 direction1, Vector3 direction2)
    {
        if (direction1 == Vector3.zero || direction2 == Vector3.zero) return 0;
            
        var dir1 = Quaternion.LookRotation(direction1);
        var dir1Angle = dir1.eulerAngles.y;
        if (dir1Angle > 180) 
            dir1Angle -= 360;

        var dir2 = Quaternion.LookRotation(direction2);
        var dir2Angle = dir2.eulerAngles.y;
        if (dir2Angle > 180) 
            dir2Angle -= 360;

        var middleAngle = Mathf.DeltaAngle(dir1Angle, dir2Angle);
            
        return middleAngle;
    }
}
