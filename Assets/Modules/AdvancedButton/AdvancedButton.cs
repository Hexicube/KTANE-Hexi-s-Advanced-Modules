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
        new Color(1, 0.5f, 0),      //Orange
        new Color(0, 1, 0)          //Green
    };

    private static Color[] buttonColours = new Color[]{
        new Color(1, 1, 0),             //Yellow
        new Color(0, 0.4f, 1),          //Blue
        new Color(0.91f, 0.88f, 0.86f), //White
        new Color(0.2f, 0.2f, 0.2f)     //Black
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

        ticker = -1;
        Light.material.color = BLACK;
        Button.OnInteract += Press;
        buttonCol = Random.Range(0, buttonColours.Length);
        Button.GetComponent<MeshRenderer>().material.color = buttonColours[buttonCol];
        buttonText = Random.Range(0, buttonLabels.Length);
        Button.gameObject.transform.Find("Label").GetComponent<TextMesh>().text = buttonLabels[buttonText];
        Button.OnInteractEnded += Release;
    }

    public override void RealFixedTick()
    {
        if (buttonDown)
        {
            ticker++;
            if (ticker == 50) ticker = 0;
            if (ticker == 0) Light.material.color = colours[holdColour];
            if (flashing && ticker == 15) Light.material.color = BLACK;
        }
    }

    protected bool Press()
    {
        if (buttonDown) return false;

        Button.AddInteractionPunch(0.2f);

        buttonDown = true;

        Button.transform.localPosition = new Vector3(-0.0125f, -0.01f, -0.0125f);

        Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
        flashing = Random.Range(0, 2) == 1;
        holdColour = Random.Range(0, colours.Length);
        ticker = -50;
        return false;
    }

    protected void Release()
    {
        if (!buttonDown) return;

        Button.AddInteractionPunch();

        buttonDown = false;

        Button.transform.localPosition = new Vector3(-0.0125f, 0.01f, -0.0125f);

        if (batAA == -1)
        {
            batAA = 0;

            List<string> data = Info.QueryWidgets(KMBombInfo.QUERYKEY_GET_BATTERIES, null);
            foreach (string response in data)
            {
                Dictionary<string, int> responseDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(response);
                if (responseDict["numbatteries"] == 2) batAA += 2;
                else batD++;
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

            Debug.Log("[Square Button #"+thisLoggingID+"] Batteries: " + batAA + "AA, " + batD + "D");
            Debug.Log("[Square Button #"+thisLoggingID+"] Highest serial number: " + highSerial);
            Debug.Log("[Square Button #"+thisLoggingID+"] Serial contains a vowel: " + serialVowel);
            Debug.Log("[Square Button #"+thisLoggingID+"] Button colour: " + buttonCol);
            Debug.Log("[Square Button #"+thisLoggingID+"] Button text: " + "\"" + buttonLabels[buttonText] + "\" " + buttonText);

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
                GetComponent<KMBombModule>().HandlePass();
            }
        }
        Light.material.color = BLACK;
        Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, gameObject.transform);
        return;
    }
}