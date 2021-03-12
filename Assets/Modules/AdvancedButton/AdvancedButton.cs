/*

-- On the Subject of The Square Button --
- This may look like the button you know and love, but don't be fooled! It's a brilliantly disguised imposter foiled only by a single mistake: It's the wrong shape. -

Follow these rules in the order they are listed. Perform the first action that applies:
1. If the button is blue and the number of AA batteries is larger than the number of D batteries, hold the button and refer to "Releasing a Held Button".
2. If the button is yellow or blue and has as at least as many letters on the label as the highest number in the serial, press and immediately release.
3. If the button is yellow or blue and the label states a colour, hold the button and refer to "Releasing a Held Button".
4. If the button has no label, press and immediately release when the two seconds digits on the timer match.
5. If the button is not black and the number of letters on the label is larger than the number of lit indicators, press and immediately release.
6. If there are at least 2 unlit indicators and the serial contains a vowel, press and immediately release.
7. If no other rule applies, hold the button and refer to "Releasing a Held Button".

- Releasing a Held Button -

If you start holding the button down, a coloured strip will light up on the right side of the module. Based on its colour, follow the rules below:

Cyan: Release when the two seconds digits add up to 7.
Orange: Release when the two seconds digits add up to 3 or 13.
Other: Release when the two seconds digits add up to 5.

If the strip is flashing, follow these rules instead:

Cyan: Release when the number of seconds remaining is a multiple of 7.
Orange: Release when the number of seconds displayed is either prime or 0.
Other: Release one second after the two seconds digits add up to a multiple of 4.

*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class AdvancedButton : FixedTicker
{
    public static int loggingID = 1;
    public int thisLoggingID;

    public KMSelectable Button;
    public KMAudio Sound;
    public KMBombInfo Info;
    public MeshRenderer Light;
    public GameObject Guard;

    private int batAA = -1, batD = 0, highSerial = 0, litInd = 0, unlitInd = 0;
    private bool serialVowel = false, hold = false, catch22 = false;

    private int ticker;
    private int holdColour;
    private bool flashing;
    private bool buttonDown;
    private int buttonCol;
    private int buttonText;

    public void HandleError(object source, Newtonsoft.Json.Serialization.ErrorEventArgs args)
    {
        args.ErrorContext.Handled = true;
    }

    private static Color[] colours = new Color[]{
        new Color(0.25f, 0.75f, 1), //Cyan
        new Color(1, 0.25f, 0),      //Orange
        new Color(0, 1, 0)          //Green
    };

    private static Color[] buttonColours = new Color[]{
        new Color(1, 1, 0),             //Yellow
        new Color(0, 0.4f, 1),          //Blue
        new Color(0.91f, 0.88f, 0.86f), //White
        new Color(0.2f, 0.2f, 0.2f)     //Black
    };
    private static string[] colourNames = new string[]{
        "Yellow",
        "Blue",
        "White",
        "Black"
    };

    private static string[] buttonLabels = new string[]{
        "purple", "jade", "maroon", "indigo",
        "elevate", "run", "detonate", ""
    };

    private static Color BLACK = new Color(0, 0, 0);

    void Awake()
    {
        thisLoggingID = loggingID++;

        transform.Find("Background").GetComponent<MeshRenderer>().material.color = new Color(1, 0.1f, 0.1f);
        transform.Find("Casing")    .GetComponent<MeshRenderer>().material.color = new Color(0.1f, 0.1f, 0.1f);
        transform.Find("Jack")      .GetComponent<MeshRenderer>().materials[0].color = new Color(0.1f, 0.1f, 0.1f);
        transform.Find("Jack")      .GetComponent<MeshRenderer>().materials[1].color = new Color(0.8f, 0.8f, 0.8f);

        ticker = -1;
        Light.material.color = BLACK;
        Button.OnInteract += Press;
        buttonCol = Random.Range(0, buttonColours.Length);
        Button.GetComponent<MeshRenderer>().material.color = buttonColours[buttonCol];
        buttonText = Random.Range(0, buttonLabels.Length);
        Button.gameObject.transform.Find("Label").GetComponent<TextMesh>().text = buttonLabels[buttonText];
        Button.OnInteractEnded += Release;

        flashing = Random.Range(0, 2) == 1;

        if(!flashing) {
            System.DateTime curTime = System.DateTime.Now;
            if(curTime.Month == 4 && curTime.Day == 1) {
                //April Fools! Always flashing!
                flashing = true;
            }
        }
        if(flashing) transform.Find("Jack").transform.localPosition = new Vector3(0, 0, .005f);

        //Detecting module deselection sucks.
        GetComponent<KMSelectable>().OnInteract += delegate(){Invoke("Open", 0.01f); return true;};

        GetComponent<KMSelectable>().OnDeselect += delegate(){GuardState = false;};
        Button.OnDeselect += delegate(){GuardState = false;};
        Button.OnCancel += delegate(){GuardState = false; return true;};
    }

    private void Open() { GuardState = true; }

    private bool GuardState = false;
    private float GuardRot = 0;
    private const float GUARD_SPEED = 7.5f;

    private int flickerTime = 0;
    public override void RealFixedTick()
    {
        float targetRot = (GuardState || buttonDown) ? 90 : 0;
        float diff = targetRot - GuardRot;
        if(diff != 0) {
            if(Mathf.Abs(diff) < GUARD_SPEED) GuardRot = targetRot;
            else if(diff < 0)       GuardRot -= GUARD_SPEED;
            else                    GuardRot += GUARD_SPEED;
            Guard.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, GuardRot));
        }

        if (buttonDown)
        {
            if(ticker < 0) {
                ticker++;
                if(ticker == 0) {
                    if(flashing) flickerTime = 1;
                    else Light.material.color = colours[holdColour];
                }
            }
            else if(flashing) {
                flickerTime--;
                if(flickerTime <= 0) {
                    flickerTime = 5;
                    Light.material.color = PickRandom(colours[holdColour]);
                }
            }
        }
    }

    private Color PickRandom(Color c) {
        float scale = Random.value;
        return new Color(c.r * scale, c.g * scale, c.b * scale);
    }

    protected bool Press()
    {
        if (buttonDown) return false;

        Button.AddInteractionPunch(0.2f);

        buttonDown = true;

        Button.transform.localPosition = new Vector3(0, -0.002f, 0);

        Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);

        holdColour = Random.Range(0, colours.Length);
        ticker = -50;
        return false;
    }

    protected void Release()
    {
        if (!buttonDown) return;

        Button.AddInteractionPunch();

        buttonDown = false;

        Button.transform.localPosition = new Vector3(0, 0, 0);

        if (batAA == -1)
        {
            GetEdgeworkAndRule();

            Debug.Log("[Square Button #"+thisLoggingID+"] Batteries: " + batAA + "AA, " + batD + "D");
            Debug.Log("[Square Button #"+thisLoggingID+"] Highest serial number: " + highSerial);
            Debug.Log("[Square Button #"+thisLoggingID+"] Serial contains a vowel: " + serialVowel);
            Debug.Log("[Square Button #"+thisLoggingID+"] Button colour: " + colourNames[buttonCol]);
            Debug.Log("[Square Button #"+thisLoggingID+"] Button text: " + "\"" + buttonLabels[buttonText] + "\" " + buttonText);

            Debug.Log("[Square Button #"+thisLoggingID+"] Hold: " + hold);
            if (!hold) Debug.Log("[Square Button #"+thisLoggingID+"] Release and Match: " + catch22);
        }

        Debug.Log("[Square Button #"+thisLoggingID+"] Handling Square Button release...");

        if (hold)
        {
            if (ticker >= 0)
            {
                Debug.Log("[Square Button #"+thisLoggingID+"] Rule dictates holding, button was held.");
                int time = (int)Info.GetTime();
                Debug.Log("[Square Button #"+thisLoggingID+"] Current seconds remaining: " + time);
                if (flashing)
                {
                    if (holdColour == 0) //Cyan
                    {
                        Debug.Log("[Square Button #"+thisLoggingID+"] Flashing Cyan (total time multiple 7), time % 7 = " + (time % 7));
                        if (time % 7 == 0) GetComponent<KMBombModule>().HandlePass();
                        else GetComponent<KMBombModule>().HandleStrike();
                    }
                    else if (holdColour == 1) //Orange
                    {
                        time %= 60;
                        Debug.Log("[Square Button #"+thisLoggingID+"] Flashing Orange (prime or 0), seconds = " + time);
                        if (time == 0 || time == 2 || time == 3 || time == 5 ||
                            time == 7 || time == 11 || time == 13 || time == 17 ||
                            time == 19 || time == 23 || time == 29 || time == 31 ||
                            time == 37 || time == 41 || time == 43 || time == 47 ||
                            time == 53 || time == 59) GetComponent<KMBombModule>().HandlePass();
                        else GetComponent<KMBombModule>().HandleStrike();
                    }
                    else
                    {
                        time++;
                        time %= 60;
                        Debug.Log("[Square Button #"+thisLoggingID+"] Flashing Green (multiple of 4), one second earlier digits sum = " + (time / 10) + "+" + (time % 10) + "=" + ((time / 10) + (time % 10)));
                        time = (time / 10) + (time % 10);
                        if (time % 4 == 0) GetComponent<KMBombModule>().HandlePass();
                        else GetComponent<KMBombModule>().HandleStrike();
                    }
                }
                else
                {
                    time %= 60;
                    time = (time / 10) + (time % 10);
                    if (time > 10) time -= 10;
                    if (holdColour == 0) //Cyan
                    {
                        Debug.Log("[Square Button #"+thisLoggingID+"] Solid Cyan, seconds sum = " + time);
                        if (time == 7) GetComponent<KMBombModule>().HandlePass();
                        else GetComponent<KMBombModule>().HandleStrike();
                    }
                    else if (holdColour == 1) //Orange
                    {
                        Debug.Log("[Square Button #"+thisLoggingID+"] Solid Orange, seconds sum = " + time);
                        if (time == 3) GetComponent<KMBombModule>().HandlePass();
                        else GetComponent<KMBombModule>().HandleStrike();
                    }
                    else
                    {
                        Debug.Log("[Square Button #"+thisLoggingID+"] Solid Green, seconds sum = " + time);
                        if (time == 5) GetComponent<KMBombModule>().HandlePass();
                        else GetComponent<KMBombModule>().HandleStrike();
                    }
                }
            }
            else
            {
                Debug.Log("[Square Button #"+thisLoggingID+"] Rule dictates holding, button was not held.");
                GetComponent<KMBombModule>().HandleStrike();
            }
        }
        else if (catch22)
        {
            if (ticker >= 0)
            {
                Debug.Log("[Square Button #"+thisLoggingID+"] Rule dictates not holding, button was held.");
                GetComponent<KMBombModule>().HandleStrike();
            }
            else
            {
                Debug.Log("[Square Button #"+thisLoggingID+"] Rule dictates not holding, button was not held.");
                Debug.Log("[Square Button #"+thisLoggingID+"] Rule also dictates matching seconds.");
                int time = (int)Info.GetTime() % 60;
                Debug.Log("[Square Button #"+thisLoggingID+"] Displayed seconds: " + time);
                if (time % 11 == 0) GetComponent<KMBombModule>().HandlePass();
                else GetComponent<KMBombModule>().HandleStrike();
            }
        }
        else
        {
            if (ticker >= 0)
            {
                Debug.Log("[Square Button #"+thisLoggingID+"] Rule dictates not holding, button was held.");
                GetComponent<KMBombModule>().HandleStrike();
            }
            else
            {
                Debug.Log("[Square Button #"+thisLoggingID+"] Rule dictates not holding, button was not held.");
                Debug.Log("[Square Button #"+thisLoggingID+"] Rule does not dictate matching seconds.");
                GetComponent<KMBombModule>().HandlePass();
            }
        }
        Light.material.color = BLACK;
        Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, gameObject.transform);
        return;
    }

    protected void GetEdgeworkAndRule()
    {
        batAA = 0;

        List<string> data = Info.QueryWidgets(KMBombInfo.QUERYKEY_GET_BATTERIES, null);
        foreach (string response in data)
        {
            Dictionary<string, int> responseDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(response);
            if (responseDict["numbatteries"] == 1) batD++;
            else batAA += responseDict["numbatteries"];
        }
        data = Info.QueryWidgets(KMBombInfo.QUERYKEY_GET_INDICATOR, null);
        foreach (string response in data)
        {
            Dictionary<string, bool> responseDict = JsonConvert.DeserializeObject<Dictionary<string, bool>>(response, new JsonSerializerSettings
            {
                Error = HandleError
            });
            if (responseDict["on"]) litInd++;
            else unlitInd++;
        }
        data = Info.QueryWidgets(KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER, null);
        foreach (string response in data)
        {
            Dictionary<string, string> responseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
            string serial = responseDict["serial"];
            if (serial.Contains("A") || serial.Contains("E") || serial.Contains("I") || serial.Contains("O") || serial.Contains("U")) serialVowel = true;

            if (serial.Contains("9")) highSerial = 9;
            else if (serial.Contains("8")) highSerial = 8;
            else if (serial.Contains("7")) highSerial = 7;
            else if (serial.Contains("6")) highSerial = 6;
            else if (serial.Contains("5")) highSerial = 5;
            else if (serial.Contains("4")) highSerial = 4;
            else if (serial.Contains("3")) highSerial = 3;
            else if (serial.Contains("2")) highSerial = 2;
            else if (serial.Contains("1")) highSerial = 1;
        }

        if (buttonCol == 1 && batAA > batD) hold = true;
        else if ((buttonCol < 2) && buttonLabels[buttonText].Length >= highSerial) hold = false;
        else if (buttonCol < 2 && buttonText < 4) hold = true;
        else if (buttonLabels[buttonText].Length == 0)
        {
            hold = false;
            catch22 = true;
        }
        else if (buttonCol != 3 && buttonLabels[buttonText].Length > litInd) hold = false;
        else if (unlitInd >= 2 && serialVowel) hold = false;
        else hold = true;
    }

    //Twitch Plays support

    #pragma warning disable 0414
    bool TwitchZenMode = false;
    string TwitchHelpMessage = "Hold the button down with 'hold', or press and release with 'press' or 'tap'.\nIf you want to press and release at a specific time, use 'press <time>'.\nRelease the button with 'release <time> <time> ...'.\nTimes are specified as either '23, 35, 40' (seconds only), or '1:40 1:47 1:54' (full timer), but cannot be mixed.\nRaise the molly guard and examine the button with 'show'.";
    #pragma warning restore 0414

    public IEnumerator TwitchHandleForcedSolve() {
        Debug.Log("[Square Button #" + thisLoggingID + "] Module forcibly solved.");
        if (batAA == -1)
            GetEdgeworkAndRule();
        if ((!hold && buttonDown && ticker >= 0) || (buttonDown && ticker < 0 && catch22 && (int)(Info.GetTime() % 60 % 11) != 0))
        {
            //Force the solve since the module would strike otherwise
            buttonDown = false;
            Light.material.color = BLACK;
            Button.transform.localPosition = new Vector3(0, 0, 0);
            GetComponent<KMBombModule>().HandlePass();
            yield break;
        }
        else if (!hold)
        {
            if (catch22)
            {
                while ((int)(Info.GetTime() % 60 % 11) != 0) yield return true;
                if (!buttonDown)
                {
                    Button.OnInteract();
                    if ((!TwitchZenMode && (int)(Info.GetTime() - 0.05f) == (int)Info.GetTime()) || (TwitchZenMode && (int)(Info.GetTime() + 0.05f) == (int)Info.GetTime()))
                        yield return new WaitForSeconds(0.05f);
                }
                Button.OnInteractEnded();
            }
            else
            {
                if (!buttonDown)
                {
                    Button.OnInteract();
                    yield return new WaitForSeconds(0.05f);
                }
                Button.OnInteractEnded();
            }
        }
        else
        {
            if (!buttonDown)
                Button.OnInteract();
            while (ticker < 0) yield return true;
            if (!flashing)
            {
                while (true)
                {
                    int time = (int)Info.GetTime();
                    time %= 60;
                    time = (time / 10) + (time % 10);
                    if (time > 10) time -= 10;
                    if (holdColour == 0)
                    {
                        if (time == 7) break;
                    }
                    else if (holdColour == 1)
                    {
                        if (time == 3) break;
                    }
                    else
                    {
                        if (time == 5) break;
                    }
                    yield return true;
                }
            }
            else
            {
                while (true)
                {
                    int time = (int)Info.GetTime();
                    if (holdColour == 0)
                    {
                        if (time % 7 == 0) break;
                    }
                    else if (holdColour == 1)
                    {
                        time %= 60;
                        if (time == 0 || time == 2 || time == 3 || time == 5 ||
                            time == 7 || time == 11 || time == 13 || time == 17 ||
                            time == 19 || time == 23 || time == 29 || time == 31 ||
                            time == 37 || time == 41 || time == 43 || time == 47 ||
                            time == 53 || time == 59) break;
                    }
                    else
                    {
                        time++;
                        time %= 60;
                        time = (time / 10) + (time % 10);
                        if (time % 4 == 0) break;
                    }
                    yield return true;
                }
            }
            Button.OnInteractEnded();
        }
    }

    public IEnumerator ProcessTwitchCommand(string cmd) {
        cmd = cmd.ToLowerInvariant();
        if(cmd.StartsWith("hold")) {
            if(buttonDown) {
                yield return "sendtochaterror Button is already held.";
                yield break;
            }

            yield return "Advanced Button";
            yield return Button;
            yield break;
        }
        else if(cmd.StartsWith("press") || cmd.StartsWith("tap")) {
            if(cmd.StartsWith("press ")) cmd = cmd.Substring(6);
            else if(cmd.StartsWith("tap ")) cmd = cmd.Substring(4);
            else if(cmd.Equals("press") || cmd.Equals("tap")) {
                if(buttonDown) {
                    yield return "sendtochaterror Button is currently held.";
                    yield break;
                }

                yield return "Advanced Button";
                yield return Button;
                yield return new WaitForSeconds(0.05f);
                yield return Button;
                yield break;
            }
            else {
                yield return "sendtochaterror Commands must start with hold, press, tap, or release.";
                yield break;
            }
            if(buttonDown) {
                yield return "sendtochaterror Button is currently held.";
                yield break;
            }

            string[] clist = cmd.Split(' ');
            List<int> times = new List<int>();
            bool secondsMode = false;
            string ex = null;
            try {
                times.Add(TimeToSeconds(clist[0], out secondsMode));
                for(int a = 1; a < clist.Length; a++) {
                    bool mode = false;
                    times.Add(TimeToSeconds(clist[a], out mode));
                    if(mode != secondsMode) throw new System.Exception("Times can only be specified as seconds or full timer, not both.");
                }
            }
            catch(System.Exception e) {ex = e.Message;}
            if(ex != null) {
                yield return "sendtochaterror " + ex;
                yield break;
            }

            yield return "Advanced Button";
            IEnumerator releaseCoroutine = ScheduleAction(false, times, secondsMode);
            while(releaseCoroutine.MoveNext()) {
                yield return releaseCoroutine.Current;
            }
            if ((!TwitchZenMode && (int)(Info.GetTime() - 0.05f) == (int)Info.GetTime()) || (TwitchZenMode && (int)(Info.GetTime() + 0.05f) == (int)Info.GetTime()))
                yield return new WaitForSeconds(0.05f);
            yield return Button;
            yield break;
        }
        else if(cmd.StartsWith("release")) {
            if(!cmd.StartsWith("release ")) {
                yield return "sendtochaterror No release time(s) specified.";
                yield break;
            }
            if(!buttonDown) {
                yield return "sendtochaterror Button is not currently held.";
                yield break;
            }
            cmd = cmd.Substring(8);

            string[] clist = cmd.Split(' ');
            List<int> times = new List<int>();
            bool secondsMode = false;
            //yield return "sendtochat DEBUG1: " + clist[0];
            times.Add(TimeToSeconds(clist[0], out secondsMode));
            for(int a = 1; a < clist.Length; a++) {
                //yield return "sendtochat DEBUG1: " + clist[a];
                bool mode = false;
                times.Add(TimeToSeconds(clist[a], out mode));
                if(mode != secondsMode) {
                    yield return "sendtochaterror Times can only be specified as seconds or full timer, not both.";
                    yield break;
                }
            }

            yield return "Advanced Button";
            IEnumerator releaseCoroutine = ScheduleAction(true, times, secondsMode);
            while(releaseCoroutine.MoveNext()) {
                yield return releaseCoroutine.Current;
            }
        }
        else yield return "sendtochaterror Commands must start with hold, press, tap, or release.";
        yield break;
    }

    private IEnumerator ScheduleAction(bool buttonDown, List<int> times, bool secondsMode) {
        int curTime = (int)Info.GetTime();

        yield return null; //Start camera movement.

        int targetTime = -1;
        //yield return "sendtochat DEBUG | Seconds mode: " + secondsMode;
        //yield return "sendtochat DEBUG | Curtime: " + curTime;
        if(secondsMode) {
            if(TwitchZenMode) {
                foreach(int time in times) {
                    int t2 = time;
                    while(t2 < curTime) t2 += 60;
                    if(t2 < targetTime || targetTime == -1) targetTime = t2;
                    //yield return "sendtochat DEBUG2: " + t2;
                }
            }
            else {
                foreach(int time in times) {
                    int t2 = time;
                    while(t2 <= curTime) t2 += 60;
                    t2 -= 60;
                    if(t2 > targetTime) targetTime = t2;
                    //yield return "sendtochat DEBUG2: " + t2;
                }
            }
        }
        else {
            if(TwitchZenMode) {
                foreach(int time in times) {
                    if(time < curTime) continue;
                    if(time < targetTime || targetTime == -1) targetTime = time;
                    //yield return "sendtochat DEBUG2: " + time;
                }
            }
            else {
                foreach(int time in times) {
                    if(time > curTime) continue;
                    if(time > targetTime) targetTime = time;
                    //yield return "sendtochat DEBUG2: " + time;
                }
            }
        }

        if(targetTime == -1) {
            yield return "sendtochaterror No valid times.";
            yield break;
        }
        yield return "sendtochat Target time: " + (targetTime / 60).ToString("D2") + ":" + (targetTime % 60).ToString("D2");
        //if(TwitchZenMode) targetTime++; //Zen mode is weird
        if(Mathf.Abs(curTime-targetTime) > 15) yield return "waiting music";

        while(true) {
            curTime = (int)Info.GetTime();
            if(curTime != targetTime) yield return "trycancel";
            else {
                yield return Button;
                break;
            }
        }
        yield break;
    }

    private int TimeToSeconds(string time, out bool seconds) {
        if(time.Contains(":")) {
            string[] spl = time.Split(':');
            if(spl.Length < 2 || spl.Length > 3) throw new System.FormatException("Invalid time format: '"+time+"'");

            seconds = false;
            if(spl.Length == 2) {
                int m = int.Parse(spl[0]);
                if(m < 0 || m > 59) throw new System.FormatException("Numbers on full timer must be in the 0-59 range: '"+time+"'");
                int s = int.Parse(spl[1]);
                if(s < 0 || s > 59) throw new System.FormatException("Numbers on full timer must be in the 0-59 range: '"+time+"'");
                return m * 60 + s;
            }
            else {
                int h = int.Parse(spl[0]);
                if(h < 0 || h > 59) throw new System.FormatException("Numbers on full timer must be in the 0-59 range: '"+time+"'");
                int m = int.Parse(spl[1]);
                if(m < 0 || m > 59) throw new System.FormatException("Numbers on full timer must be in the 0-59 range: '"+time+"'");
                int s = int.Parse(spl[2]);
                if(s < 0 || s > 59) throw new System.FormatException("Numbers on full timer must be in the 0-59 range: '"+time+"'");
                return h * 3600 + m * 60 + s;
            }
        }
        else {
            seconds = true;
            int amt = int.Parse(time);
            if(amt < 0 || amt > 59) throw new System.FormatException("Seconds values must be in the 0-59 range: '"+time+"'");
            return amt;
        }
    }
}