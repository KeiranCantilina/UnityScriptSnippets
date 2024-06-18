using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class AccumulateDistance : MonoBehaviour
{
    public TextMeshPro DataText;
    public TextMeshPro SumText;
    public string AddKey;
    public string SubtractKey;
    private List<string> ListOfDistancesText;
    private List<float> ListOfDistancesFloat;
    public MeasureDistance MeasureDistanceScript;
    private int elementNumber;

    // Start is called before the first frame update
    void Start()
    {
        elementNumber = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(AddKey))
        {
            // Add a number to vector/List
            float currentDistance = MeasureDistanceScript.FetchDistance();
            ListOfDistancesFloat.Add(currentDistance);
            ListOfDistancesText.Add(currentDistance.ToString()+'\n');
            elementNumber++;

        }

        // If erase point button is pressed
        if (Input.GetKeyDown(SubtractKey))
        {
            // Remove number from vector/List
            ListOfDistancesFloat.Remove(ListOfDistancesFloat[elementNumber]);
            ListOfDistancesText.Remove(ListOfDistancesText[elementNumber]);
            elementNumber--;
        }

        // Calculate sum
        float sum = Sum(ListOfDistancesFloat);
        string text = Glue(ListOfDistancesText);

        // Print by concatenating all strings in list with for loop and appending newline to each item
        DataText.text = text;
        SumText.text = sum.ToString();

    }

    private static float Sum(List<float> array)
    {
        float result = 0.0f;
        for (int i = 0; i < array.Count; i++)
            result += array[i];
        return result;
    }

    private string Glue(List<string> list)
    {
        string output = "";
        for (int i = 0; i < list.Count; i++)
            output += list[i];
        return output;
    }
}
