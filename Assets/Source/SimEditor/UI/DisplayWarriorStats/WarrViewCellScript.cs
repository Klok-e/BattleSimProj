using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimEditor
{
    [RequireComponent(typeof(Image))]
    public class WarrViewCellScript : MonoBehaviour
    {
        private Image img;

        private float minInpVal;
        private float maxInpVal;

        public void Initialize(float min, float max)
        {
            img = GetComponent<Image>();
            minInpVal = min;
            maxInpVal = max;
        }

        public void SetColour(float col)
        {
            Debug.Assert(minInpVal <= col && col <= maxInpVal, $"{col.ToString()} not in range {minInpVal}..{maxInpVal}");

            var normalized = Helpers.NormalizeNumber(col, minInpVal, maxInpVal, true);//normalize to range 0..1

            var newCol = new Color(normalized, normalized, normalized);
            img.color = newCol;
        }
    }
}
