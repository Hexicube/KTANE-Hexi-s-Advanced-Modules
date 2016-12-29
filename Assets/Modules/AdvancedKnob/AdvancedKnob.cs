/*

-- On the Subject of Rotary Phones --
- Hello, this is emergency services, please hold... -

Three numbers are displayed at all times. Whenever the numbers change, add them to your current number and dial it.

*/

using UnityEngine;
using System.Collections;

public class AdvancedKnob : FixedTicker
{
    public static bool HasFailed; //Prevent voice-line from playing the very first time.

    public KMAudio Sound;

    public KMSelectable Button0, Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9;
    public Nixie Num1, Num2, Num3;

    public Transform PhoneRing;

    protected int CurAnswer = 0, DisplayNumber = 0;
    protected bool Active;
    protected int Response, ResponsePos, PhoneDelay;

    private static string[] SoundNames = new string[]{
        "hummus1", "hummus2", "hummus3", "hummus4",
        "tunehello1", "tunehello2", "tunehello3", "tunepronto"
    };

    private void SetDisplay()
    {
        Num1.SetValue(DisplayNumber / 100);
        Num2.SetValue((DisplayNumber / 10) % 10);
        Num3.SetValue(DisplayNumber % 10);
    }

    void Awake()
    {
        Button0.OnInteract += Handle0;
        Button1.OnInteract += Handle1;
        Button2.OnInteract += Handle2;
        Button3.OnInteract += Handle3;
        Button4.OnInteract += Handle4;
        Button5.OnInteract += Handle5;
        Button6.OnInteract += Handle6;
        Button7.OnInteract += Handle7;
        Button8.OnInteract += Handle8;
        Button9.OnInteract += Handle9;

        PhoneRing.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);

        GetComponent<KMNeedyModule>().OnActivate += OnActivate;
        GetComponent<KMNeedyModule>().OnNeedyActivation += OnNeedyActivation;
        GetComponent<KMNeedyModule>().OnNeedyDeactivation += OnNeedyDeactivation;
        GetComponent<KMNeedyModule>().OnTimerExpired += OnTimerExpired;
        CurAnswer = Random.Range(0, 1000);
        DisplayNumber = CurAnswer;
        Debug.Log("Rotary Phone initial display: " + CurAnswer);
    }

    protected void OnActivate()
    {
        SetDisplay();
    }

    protected void OnNeedyActivation()
    {
        Debug.Log("Rotary Phone old value: " + CurAnswer);
        DisplayNumber = Random.Range(0, 1000);
        Debug.Log("New display: " + DisplayNumber);
        CurAnswer = (CurAnswer + DisplayNumber) % 1000;
        Debug.Log("New value: " + CurAnswer);
        SetDisplay();
        Active = true;
        Response = 0;
        ResponsePos = 0;
    }

    protected void OnNeedyDeactivation()
    {
        Num1.SetValue(-1);
        Num2.SetValue(-1);
        Num3.SetValue(-1);
    }

    protected void OnTimerExpired()
    {
        GetComponent<KMNeedyModule>().HandleStrike();
        CurAnswer = Random.Range(0, 1000);
        DisplayNumber = CurAnswer;
        SetDisplay();
        Active = false;
    }

    private bool InSpin = false;
    private int Target = -1;
    private int Progress = -1;

    public override void RealFixedTick()
    {
        if (InSpin)
        {
            Progress++;

            float angle;
            if (Progress >= GetSpinLimit())
            {
                InSpin = false;
                angle = 180f;

                Response = Response * 10 + Target;
                ResponsePos++;
                if (ResponsePos == 3)
                {
                    Active = false;
                    if (Response == CurAnswer)
                    {
                        GetComponent<KMNeedyModule>().HandlePass();
                        Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
                    }
                    else
                    {
                        GetComponent<KMNeedyModule>().HandleStrike();
                        GetComponent<KMNeedyModule>().HandlePass();
                        CurAnswer = DisplayNumber;
                        PhoneDelay = 75;
                    }
                }
            }
            else
            {
                angle = GetSpinPosition();
            }
            PhoneRing.localEulerAngles = new Vector3(0, angle, 0);
        }

        if (PhoneDelay > 0)
        {
            PhoneDelay--;
            if (PhoneDelay == 0)
            {
                bool voice = false;
                if (HasFailed)
                {
                    if (Random.Range(0, 7) == 0) voice = true;
                }
                HasFailed = true;
                if (voice)
                {
                    string name = SoundNames[Random.Range(0, SoundNames.Length)];
                    //Debug.Log(name);
                    Sound.PlaySoundAtTransform(name, gameObject.transform);
                }
                else Sound.PlaySoundAtTransform("NoNumber", gameObject.transform);
            }
        }
    }

    private static int[][] spindata = new int[][]{
        new int[]{40, 8, 60},  //0
        new int[]{10, 12, 15}, //1
        new int[]{12, 8, 20},  //2
        new int[]{14, 8, 25},  //3
        new int[]{17, 8, 30},  //4
        new int[]{22, 8, 35},  //5
        new int[]{24, 8, 40},  //6
        new int[]{24, 8, 45},  //7
        new int[]{30, 8, 50},  //8
        new int[]{36, 8, 55}   //9
    };

    private int GetSpinLimit()
    {
        return spindata[Target][0] + spindata[Target][1] + spindata[Target][2];
    }

    private float GetSpinPosition()
    {
        int forward = spindata[Target][0], pause = spindata[Target][1], back = spindata[Target][2];

        int pos = Progress;
        int lim = forward;
        if(lim < pos)
        {
            pos -= lim;
            lim = pause;
            if (lim < pos)
            {
                pos -= lim;
                lim = back;
                pos = lim - pos - 1;
            }
            else pos = lim;
        }

        float p = (float)pos / (float)lim;
        if (Target == 0) p *= 300f;
        else p *= Target * 30f;

        float angle = 180f + p;
        if (angle >= 360f) angle -= 360f;
        return angle;
    }

    protected void Handle(int val)
    {
        if (!InSpin && Active)
        {
            InSpin = true;
            Target = val;
            Progress = 0;
            Sound.PlaySoundAtTransform("rotary" + Target, gameObject.transform);
        }
    }

    private bool Handle0()
    {
        Handle(0);
        return false;
    }

    private bool Handle1()
    {
        Handle(1);
        return false;
    }

    private bool Handle2()
    {
        Handle(2);
        return false;
    }

    private bool Handle3()
    {
        Handle(3);
        return false;
    }

    private bool Handle4()
    {
        Handle(4);
        return false;
    }

    private bool Handle5()
    {
        Handle(5);
        return false;
    }

    private bool Handle6()
    {
        Handle(6);
        return false;
    }

    private bool Handle7()
    {
        Handle(7);
        return false;
    }

    private bool Handle8()
    {
        Handle(8);
        return false;
    }

    private bool Handle9()
    {
        Handle(9);
        return false;
    }
}