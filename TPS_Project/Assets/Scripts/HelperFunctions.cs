using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS
{
    public static class HelperFunctions
    {
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
}

