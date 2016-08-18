using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Logger : MonoBehaviour
{
    private static Logger logger;
    private Text textComponent;

    void Awake()
    {
        logger = this;
        textComponent = GetComponent<Text>();
    }

    public static void Add(string text)
    {
        logger.textComponent.text += text + "\n";
    }
}
