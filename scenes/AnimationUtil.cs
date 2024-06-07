using Godot;
using System;

public partial class AnimationUtil : Node
{
    public enum Direction {
        AWAY,
        RIGHT,
        LEFT,
        TOWARDS
    }

    public static Direction GetDirection(float angleFromPlayer, float facingAngle)
    {
        float relativeAngle = angleFromPlayer - facingAngle;
        if (relativeAngle > -Math.PI / 4 && relativeAngle < Math.PI / 4)
        {
            // facing player
            return Direction.TOWARDS;
        }
        if (relativeAngle < -3 * Math.PI / 4 || relativeAngle > 3 * Math.PI / 4)
        {
            // facing away from player
            return Direction.AWAY;
        }
        if (relativeAngle > Math.PI / 4 && relativeAngle < 3 * Math.PI / 4)
        {
            // facing left of player
            return Direction.LEFT;
        }
        if (relativeAngle < -Math.PI / 4 && relativeAngle > -3 * Math.PI / 4)
        {
            // facing right of player
            return Direction.RIGHT;
        }
        return Direction.TOWARDS;
    }
}
