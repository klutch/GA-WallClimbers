using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Logger : MonoBehaviour
{
    private static Logger logger;
    private Text textComponent;
    private int lineCount = 0;

    void Awake()
    {
        logger = this;
        textComponent = GetComponent<Text>();
    }

    public static void Add(string text)
    {
        logger.textComponent.text += text + "\n";
        logger.lineCount++;

        if (logger.lineCount > 60)
        {
            int newLineIndex = logger.textComponent.text.IndexOf("\n");
            int startIndex = newLineIndex + 1;
            int length = logger.textComponent.text.Length - startIndex;
            logger.textComponent.text = logger.textComponent.text.Substring(startIndex, length);
            logger.lineCount--;
        }
    }
}
