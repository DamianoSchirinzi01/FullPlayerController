using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS
{
    public static class HelperFunctions
    {
        #region clamping functions
        public static float ClampAngle(float angle, float min, float max)
        {   //Normalises angle value passed in between -180 to 180 to make the angle clampable
            angle = NormalizeAngle(angle);
            if (angle > 180)
            {
                angle -= 360;
            }
            else if (angle < -180)
            {
                angle += 360;
            }

            min = NormalizeAngle(min);
            if (min > 180)
            {
                min -= 360;
            }
            else if (min < -180)
            {
                min += 360;
            }

            max = NormalizeAngle(max);
            if (max > 180)
            {
                max -= 360;
            }
            else if (max < -180)
            {
                max += 360;
            }

            return Mathf.Clamp(angle, min, max);
        }

        public static float NormalizeAngle(float angle)
        { //If the angle is above or below 360 degrees, normalise it
            while (angle > 360)
                angle -= 360;
            while (angle < 0)
                angle += 360;
            return angle;
        }

        #endregion

        #region Easing Functions

        public static float smoothLerp(float value, float _start, float _end, float duration)
        {
            float t = 0;

            if (t < 1)
            {
                t += Time.deltaTime * duration;

                value = Mathf.Lerp(_start, _end, easeInQuint(t));
            }

            return value;
        }

        public static float easeInCubic(float value)
        {
            return value * value * value;
        }

        public static float easeInQuint(float value)
        {
            return value * value * value * value;
        }

        public static float easeOutBack(float value)
        {
            const float C1 = 1.70158f;
            const float C3 = C1 + 1;

            return 1 + C3 * Mathf.Pow(value - 1, 3) + C1 * Mathf.Pow(value - 1, 2);
        }
    }
    #endregion
}

