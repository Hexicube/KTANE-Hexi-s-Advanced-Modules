using UnityEngine;
using System.Collections;

public class Nixie : MonoBehaviour
{
    private GameObject[] Bars, Glows;

    void Start()
    {
        Bars = new GameObject[10];
        Glows = new GameObject[10];
        for (int a = 0; a < 10; a++)
        {
            Transform t = transform.Find("" + a);

            Bars[a] = t.Find("Bar").gameObject;
            Glows[a] = t.Find("Glow").gameObject;
            Glows[a].SetActive(false);

            Bars[a].GetComponent<MeshRenderer>().material.color = new Color(0.2f, 0.2f, 0.2f);
            Glows[a].GetComponent<MeshRenderer>().material.color = new Color(1, 0.4f, 0, 1);
        }
	}

    private int val = -1;
    public void SetValue(int v)
    {
        if (v == val) return;

        if (val != -1)
        {
            Bars[val].SetActive(true);
            Glows[val].SetActive(false);
        }
        val = v;
        if (val != -1)
        {
            Bars[val].SetActive(false);
            Glows[val].SetActive(true);
        }
    }
}