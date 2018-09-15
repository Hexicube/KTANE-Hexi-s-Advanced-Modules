/*

-- On the Subject of Rotary Phones --
- Hello, this is emergency services, please hold... -

Three numbers are displayed at all times. Whenever the numbers change, add them to your current number and dial it.

*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AdvancedKnob : FixedTicker
{
    private const int NUM_DIGITS = 3;
    private static int MODULO = -1;

    public static int loggingID = 1;
    public int thisLoggingID;

    public static bool HasFailed; //Prevent voice-line from playing the very first time.

    public KMAudio Sound;

    public KMSelectable Button0, Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9;
    private KMSelectable[] Buttons;
    public Nixie[] NumList;
    public TimerDial Dial1, Dial2;

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
        int counter = 0;
        int val = DisplayNumber;
        while(counter < NUM_DIGITS) {
            NumList[NumList.Length - 1 - counter].SetValue(val % 10);
            counter++;
            val /= 10;
        }
    }

    void Awake()
    {
        if(MODULO == -1) {
            MODULO = 1;
            int counter = 0;
            while(counter < NUM_DIGITS) {
                counter++;
                MODULO *= 10;
            }
            Debug.Log("[Rotary Phone] Digits: " + NUM_DIGITS + ", Modulo: " + MODULO);
        }

        thisLoggingID = loggingID++;

        Buttons = new KMSelectable[] {Button0, Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9};

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
        CurAnswer = Random.Range(0, MODULO);
        DisplayNumber = CurAnswer;
        Debug.Log("[Rotary Phone #"+thisLoggingID+"] Rotary Phone initial display: " + CurAnswer);
    }

    protected void OnActivate()
    {
        if(forceSolve) {
            GetComponent<KMNeedyModule>().HandlePass();
            return;
        }

        SetDisplay();
    }

    protected void OnNeedyActivation()
    {
        Debug.Log("[Rotary Phone #"+thisLoggingID+"] Rotary Phone old value: " + CurAnswer);
        DisplayNumber = Random.Range(0, MODULO);
        Debug.Log("[Rotary Phone #"+thisLoggingID+"] New display: " + DisplayNumber);
        CurAnswer = (CurAnswer + DisplayNumber) % MODULO;
        Debug.Log("[Rotary Phone #"+thisLoggingID+"] New value: " + CurAnswer);
        SetDisplay();
        Active = true;
        Response = 0;
        ResponsePos = 0;
    }

    protected void OnNeedyDeactivation()
    {
        Dial1.Move(0);
        Dial2.Move(0);

        foreach(Nixie n in NumList) n.SetValue(-1);
    }

    protected void OnTimerExpired()
    {
        if(forceSolve) return;

        GetComponent<KMNeedyModule>().HandleStrike();
        CurAnswer = Random.Range(0, MODULO);
        DisplayNumber = CurAnswer;
        SetDisplay();
        Active = false;
    }

    private bool InSpin = false;
    private int Target = -1;
    private int Progress = -1;

    public override void RealFixedTick()
    {
        if (Active)
        {
            int t = (int)(GetComponent<KMNeedyModule>().GetNeedyTimeRemaining() + 0.8f);
            Dial1.Move(t / 10);
            Dial2.Move(t % 10);
        }

        if (InSpin)
        {
            Progress++;

            float angle;
            if (Progress >= GetSpinLimit())
            {
                InSpin = false;
                angle = 180f;
                if (Active)
                {
                    Response = Response * 10 + Target;
                    ResponsePos++;
                    if (ResponsePos == NUM_DIGITS)
                    {
                        Debug.Log("[Rotary Phone #"+thisLoggingID+"] Provided value: " + Response);
                        Debug.Log("[Rotary Phone #"+thisLoggingID+"] Expected value: " + CurAnswer);
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
                            PhoneDelay = 50;
                        }
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
                    if (Random.Range(0, 3) == 0) voice = true;
                }
                HasFailed = true;
                if (voice)
                {
                    string name = SoundNames[Random.Range(0, SoundNames.Length)];
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

        if (Progress == forward || Progress == (forward + pause + back - 1)) GetComponent<KMSelectable>().AddInteractionPunch(0.1f);

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

    //Twitch Plays support

    #pragma warning disable 0414
    string TwitchHelpMessage = "Submit answers with 'dial 217'.";
    #pragma warning restore 0414

    private bool forceSolve = false;
    public void TwitchHandleForcedSolve() {
        Debug.Log("[Rotary Phone #"+thisLoggingID+"] Module forcibly solved.");
        forceSolve = true;
        GetComponent<KMNeedyModule>().HandlePass();
    }

    public IEnumerator ProcessTwitchCommand(string cmd) {
        cmd = cmd.ToLowerInvariant();
        if(cmd.StartsWith("press ")) cmd = cmd.Substring(6);
        else if(cmd.StartsWith("submit ")) cmd = cmd.Substring(7);
        else if(cmd.StartsWith("dial ")) cmd = cmd.Substring(5);
        else {
            yield return "sendtochaterror Commands must start with 'dial'.";
            yield break;
        }

        char[] vals = cmd.ToCharArray();
        List<KMSelectable> seq = new List<KMSelectable>();
        foreach(char c in vals) {
            if(c == ' ' || c == ',') continue;
            int val = c - '0';
            if(val < 0 || val > 9) {
                yield return "sendtochaterror Bad character: " + c;
                yield break;
            }
            seq.Add(Buttons[val]);
        }

        yield return "Rotary Phone";
        foreach(KMSelectable s in seq) {
            yield return s;
            yield return s;
            while(InSpin) yield return null;
            yield return new WaitForSeconds(0.1f);
        }
        yield break;
    }
}