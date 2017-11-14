using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class RangeLimiterForInpFields : MonoBehaviour
{
    private InputField inpField;

    [SerializeField] private int cantBeLessThan;

    // Use this for initialization
    private void Start()
    {
        inpField = GetComponent<InputField>();
        inpField.onEndEdit.AddListener(Task);
    }

    private void Task(string txt)
    {
        int value;
        bool succ = int.TryParse(txt, out value);
        if (succ)
        {
            if (value >= cantBeLessThan)
            {
                inpField.text = value.ToString();
            }
            else
            {
                inpField.text = cantBeLessThan.ToString();
            }
        }
        else
        {
            inpField.text = cantBeLessThan.ToString();
        }
    }
}
