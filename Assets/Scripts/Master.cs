using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Master : MonoBehaviour
{
    [Header("Emeralds Counter")]
    public int emeralds;
    public TextMeshProUGUI coinCounter;

    public float animationTime = 1.5f;
    

    //Fix with emeralds
    private float desiredNumber;
    private float initialNumber;
    private float currentNumber;

    public void SetNumber(float value)
    {
        initialNumber = currentNumber;
        desiredNumber = value;
    }

    public void AddToNumber(float value)
    {
        initialNumber = currentNumber;
        desiredNumber += value;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
            if (Input.GetKey("a"))
                AddToNumber(-100);
            if (Input.GetKey("d"))
                AddToNumber(100);

        if(currentNumber != desiredNumber)
        {
            if(initialNumber < desiredNumber)
            {
                currentNumber += (animationTime * Time.deltaTime) * (desiredNumber - initialNumber);
                if (currentNumber >= desiredNumber)
                    currentNumber = desiredNumber;
            }
            else
            {
                currentNumber -= (animationTime * Time.deltaTime) * (initialNumber - desiredNumber);
                if (currentNumber <= desiredNumber)
                    currentNumber = desiredNumber;
            }

            coinCounter.text = currentNumber.ToString("#,##" + "0");
        }

        emeralds = Mathf.Max(0,(int)desiredNumber);
        Debug.Log(emeralds);
    }
}
