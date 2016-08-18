using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Logger : MonoBehaviour
{
    private static Logger logger;
    private Text textComponent;
    private int lineCount;

    void Awake()
    {
        logger = this;
        textComponent = GetComponent<Text>();
    }

    public static void Add(string text)
    {
        if (logger.lineCount > 60)
        {
            logger.textComponent.text = "";
            logger.lineCount = 0;
        }

        logger.textComponent.text += text + "\n";
        logger.lineCount++;
    }
}
