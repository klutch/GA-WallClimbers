using UnityEngine;
using System.Collections.Generic;

public class CreateMarkers : MonoBehaviour
{
    public GameObject MarkerPrefab;
    public int Length;

    void Start()
    {
        for (int i = 0, limit = Length + 1; i < limit; i++)
        {
            GameObject marker = Instantiate<GameObject>(MarkerPrefab);
            TextMesh textMesh = marker.GetComponentInChildren<TextMesh>();

            marker.transform.position = marker.transform.position + new Vector3(0f, i, 0f);
            textMesh.text = i + "m";
        }
    }
}
