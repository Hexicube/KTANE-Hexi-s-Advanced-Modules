using UnityEngine;
using System.Collections;

public class FixedTickFixer : MonoBehaviour
{
    public const int TARGET_RATE = 50;
    public static int CURRENT_RATE = -1;

    public FixedTicker script;

	void Start ()
    {
        if (CURRENT_RATE == -1)
        {
            CURRENT_RATE = Mathf.RoundToInt(1 / Time.fixedDeltaTime);
            Debug.Log("Tick delay: " + Time.fixedDeltaTime);
            Debug.Log("Calculated FPS: " + CURRENT_RATE);
        }
	}

    private int counter = 0;
	void FixedUpdate ()
    {
        counter += TARGET_RATE;
        while (counter >= CURRENT_RATE)
        {
            counter -= CURRENT_RATE;
            if (script != null) script.RealFixedTick();
        }
	}
}