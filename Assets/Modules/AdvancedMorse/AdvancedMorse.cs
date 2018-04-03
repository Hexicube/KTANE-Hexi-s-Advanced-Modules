/*

-- On the subject of Morse Messages --
- This was the cutting-edge of communication at one point... -

For this module, every letter of the alphabet is considered to have numeric value equal to its position (A=1, B=2 ... Z=26)
For this module, numeric values outside the 1-26 range wrap around (Z+1=A, A-1=Z)
Three unique letters are being received on a loop, shown by the flashing LED in the top-left
To solve the module, a correct response letter must be sent in morse using the transmit button in the bottom-right
To assist with timings, a scrolling display with marks shows how long to hold the button for one unit of time

Perform each step below in sequence:
- Take the 4th and 5th character of the serial number, this is your character pair
- For each indicator that has a matching letter in the received letters, add one to the first character of your pair if the indicator is on or the second character if it is off
- If the sum of your character pair is a square number, add 4 to the first character; otherwise, subtract 4 from the second
- If either character from your character pair matches a letter in the name of any port on the bomb, swap the characters
- Add the largest received character to the first character in your pair
- Add the two smaller received characters to the second character in your pair
- If any received characters are in the fibbonachi sequence, add them to both characters in your pair
- If any received characters are prime, subtract them from the first character in your pair
- If any received characters are square, subtract them from the second character in your pair
- If batteries are present and any received characters are divisible by the number of batteries present, subtract them from both characters in your pair

Once complete, transmit the sum of your character pair.

*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class AdvancedMorse : FixedTicker
{
    public static int loggingID = 1;
    public int thisLoggingID;

    private static char[] PRIME = new char[]{
        (char)2,
        (char)3,
        (char)5,
        (char)7,
        (char)11,
        (char)13,
        (char)17,
        (char)19,
        (char)23
    };

    private static char[] SQUARE = new char[]{
        (char)1,
        (char)4,
        (char)9,
        (char)16,
        (char)25
    };

    public KMSelectable ButtonTransmit, ButtonSwitch;
    public KMAudio Sound;
    public KMBombInfo Info;

    public PolyDraw Draw;
    public TextMesh Wrong;

    private static Color LED_OFF = new Color(0, 0, 0, 0), LED_ON = new Color(0.7f, 0.6f, 0.2f, 0.4f);
    private MeshRenderer LED;

    private string[] DisplayCharsRaw = new string[3];
    private int[][] DisplayChars;

    private string Answer;

    void Awake()
    {
        thisLoggingID = loggingID++;

        transform.Find("Background").GetComponent<MeshRenderer>().material.color = new Color(1, 0.1f, 0.1f);

        ButtonTransmit.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);

        ButtonTransmit.OnInteract += HandleTransDown;
        ButtonTransmit.OnInteractEnded += HandleTransUp;
        ButtonSwitch.OnInteract += HandleSwitch;

        LED = transform.Find("Lights").GetComponent<MeshRenderer>();
        LED.materials[0].color = new Color(0.3f, 0.3f, 0.3f);
        LED.materials[1].color = LED_OFF;
        LED.materials[2].color = LED_OFF;
        LED.materials[3].color = LED_OFF;

        transform.Find("ReceiveBox").GetComponent<MeshRenderer>().material.color = new Color(0.3f, 0.3f, 0.3f);
        transform.Find("ReceiveSwitch").GetComponent<MeshRenderer>().material.color = new Color(0.6f, 0.6f, 0.6f);

        ButtonTransmit.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);

        List<string> charList = new List<string>(){
            "A", "B", "C", "D", "E", "F",
            "G", "H", "I", "J", "K", "L",
            "M", "N", "O", "P", "Q", "R",
            "S", "T", "U", "V", "W", "X",
            "Y", "Z"
        };

        DisplayChars = new int[3][];
        for (int a = 0; a < 3; a++)
        {
            int pos = Random.Range(0, charList.Count);
            DisplayCharsRaw[a] = charList[pos];
            DisplayChars[a] = Morsify(charList[pos]);
            charList.RemoveAt(pos);
        }

        GetComponent<KMBombModule>().OnActivate += GenSolution;

        HandleSwitch();
    }

    public void GenSolution()
    {
        Debug.Log("[Morsematics #"+thisLoggingID+"] Morsematics display characters: " + DisplayCharsRaw[0] + DisplayCharsRaw[1] + DisplayCharsRaw[2]);

        int disp1base = DisplayCharsRaw[0][0] - 'A' + 1;
        int disp2base = DisplayCharsRaw[1][0] - 'A' + 1;
        int disp3base = DisplayCharsRaw[2][0] - 'A' + 1;

        string serial = "AB1CD2";
        List<string> data = Info.QueryWidgets(KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER, null);
        foreach (string response in data)
        {
            Dictionary<string, string> responseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
            serial = responseDict["serial"];
            break;
        }

        List<string> indOn = new List<string>();
        List<string> indOff = new List<string>();

        data = Info.QueryWidgets(KMBombInfo.QUERYKEY_GET_INDICATOR, null);
        foreach (string response in data)
        {
            Dictionary<string, string> responseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);

            if (responseDict["on"].Equals("True")) indOn.Add(responseDict["label"]);
            else indOff.Add(responseDict["label"]);
        }

        int batteries = 0;

        data = Info.QueryWidgets(KMBombInfo.QUERYKEY_GET_BATTERIES, null);
        foreach (string response in data)
        {
            Dictionary<string, int> responseDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(response);
            batteries += responseDict["numbatteries"];
        }

        char firstChar = serial[3];
        char secondChar = serial[4];

        Debug.Log("[Morsematics #"+thisLoggingID+"] Initial character pair: " + firstChar + secondChar + "(" + (int)(firstChar - 'A' + 1) + "," + (int)(secondChar - 'A' + 1) + ")");

        foreach (string ind in indOn)
        {
            if (ind.Contains(DisplayCharsRaw[0]) ||
                ind.Contains(DisplayCharsRaw[1]) ||
                ind.Contains(DisplayCharsRaw[2]))
            {
                Debug.Log("[Morsematics #"+thisLoggingID+"] Matching indicator: " + ind + " (ON)");
                firstChar++;
            }
        }
        foreach (string ind in indOff)
        {
            if (ind.Contains(DisplayCharsRaw[0]) ||
                ind.Contains(DisplayCharsRaw[1]) ||
                ind.Contains(DisplayCharsRaw[2]))
            {
                Debug.Log("[Morsematics #"+thisLoggingID+"] Matching indicator: " + ind + " (OFF)");
                secondChar++;
            }
        }
        if (firstChar > 'Z') firstChar -= (char)26;
        if (secondChar > 'Z') secondChar -= (char)26;

        Debug.Log("[Morsematics #"+thisLoggingID+"] After indicators: " + firstChar + secondChar + "(" + (int)(firstChar - 'A' + 1) + "," + (int)(secondChar - 'A' + 1) + ")");

        int sum = ((firstChar - 'A') + (secondChar - 'A') + 1) % 26 + 1;
        int root = (int)Mathf.Sqrt(sum);
        if (root * root == sum)
        {
            Debug.Log("[Morsematics #"+thisLoggingID+"] Character sum is square");
            firstChar += (char)4;
            if (firstChar > 'Z') firstChar -= (char)26;
        }
        else
        {
            Debug.Log("[Morsematics #"+thisLoggingID+"] Character sum is not square");
            secondChar -= (char)4;
            if (secondChar < 'A') secondChar += (char)26;
        }

        Debug.Log("[Morsematics #"+thisLoggingID+"] After square check: " + firstChar + secondChar + "(" + (int)(firstChar - 'A' + 1) + "," + (int)(secondChar - 'A' + 1) + ")");

        int largest;
        if (disp1base > disp2base && disp1base > disp3base)
        {
            Debug.Log("[Morsematics #"+thisLoggingID+"] " + DisplayCharsRaw[0] + " is largest");
            largest = disp1base;
        }
        else if (disp2base > disp3base)
        {
            Debug.Log("[Morsematics #"+thisLoggingID+"] " + DisplayCharsRaw[1] + " is largest");
            largest = disp2base;
        }
        else
        {
            Debug.Log("[Morsematics #"+thisLoggingID+"] " + DisplayCharsRaw[2] + " is largest");
            largest = disp3base;
        }
        firstChar += (char)largest;
        if (firstChar > 'Z') firstChar -= (char)26;

        Debug.Log("[Morsematics #"+thisLoggingID+"] After big add: " + firstChar + secondChar + "(" + (int)(firstChar - 'A' + 1) + "," + (int)(secondChar - 'A' + 1) + ")");

        foreach (char p in PRIME)
        {
            if (disp1base == p)
            {
                Debug.Log("[Morsematics #"+thisLoggingID+"] " + DisplayCharsRaw[0] + " is prime");
                firstChar -= p;
            }
            if (firstChar < 'A') firstChar += (char)26;
            if (disp2base == p)
            {
                Debug.Log("[Morsematics #"+thisLoggingID+"] " + DisplayCharsRaw[1] + " is prime");
                firstChar -= p;
            }
            if (firstChar < 'A') firstChar += (char)26;
            if (disp3base == p)
            {
                Debug.Log("[Morsematics #"+thisLoggingID+"] " + DisplayCharsRaw[2] + " is prime");
                firstChar -= p;
            }
            if (firstChar < 'A') firstChar += (char)26;
        }

        Debug.Log("[Morsematics #"+thisLoggingID+"] After prime: " + firstChar + secondChar + "(" + (int)(firstChar - 'A' + 1) + "," + (int)(secondChar - 'A' + 1) + ")");

        foreach (char s in SQUARE)
        {
            if (disp1base == s)
            {
                Debug.Log("[Morsematics #"+thisLoggingID+"] " + DisplayCharsRaw[0] + " is square");
                secondChar -= s;
            }
            if (secondChar < 'A') secondChar += (char)26;
            if (disp2base == s)
            {
                Debug.Log("[Morsematics #"+thisLoggingID+"] " + DisplayCharsRaw[1] + " is square");
                secondChar -= s;
            }
            if (secondChar < 'A') secondChar += (char)26;
            if (disp3base == s)
            {
                Debug.Log("[Morsematics #"+thisLoggingID+"] " + DisplayCharsRaw[2] + " is square");
                secondChar -= s;
            }
            if (secondChar < 'A') secondChar += (char)26;
        }

        Debug.Log("[Morsematics #"+thisLoggingID+"] After square: " + firstChar + secondChar + "(" + (int)(firstChar - 'A' + 1) + "," + (int)(secondChar - 'A' + 1) + ")");

        if (batteries > 0)
        {
            if (disp1base % batteries == 0)
            {
                Debug.Log("[Morsematics #"+thisLoggingID+"] " + DisplayCharsRaw[0] + " is divisible");
                firstChar -= (char)disp1base;
                secondChar -= (char)disp1base;
            }
            if (firstChar < 'A') firstChar += (char)26;
            if (secondChar < 'A') secondChar += (char)26;
            if (disp2base % batteries == 0)
            {
                Debug.Log("[Morsematics #"+thisLoggingID+"] " + DisplayCharsRaw[1] + " is divisible");
                firstChar -= (char)disp2base;
                secondChar -= (char)disp2base;
            }
            if (firstChar < 'A') firstChar += (char)26;
            if (secondChar < 'A') secondChar += (char)26;
            if (disp3base % batteries == 0)
            {
                Debug.Log("[Morsematics #"+thisLoggingID+"] " + DisplayCharsRaw[2] + " is divisible");
                firstChar -= (char)disp3base;
                secondChar -= (char)disp3base;
            }
            if (firstChar < 'A') firstChar += (char)26;
            if (secondChar < 'A') secondChar += (char)26;
        }

        Debug.Log("[Morsematics #"+thisLoggingID+"] After batteries: " + firstChar + secondChar + "(" + (int)(firstChar - 'A' + 1) + "," + (int)(secondChar - 'A' + 1) + ")");

        if (firstChar == secondChar)
        {
            Answer = "" + firstChar;
            Debug.Log("[Morsematics #"+thisLoggingID+"] Characters match, answer: " + Answer);
        }
        else if (firstChar > secondChar)
        {
            char finalVal = (char)(firstChar - secondChar + 'A' - 1);
            Answer = "" + finalVal;
            Debug.Log("[Morsematics #"+thisLoggingID+"] First character is larger (diff), answer: " + Answer);
        }
        else
        {
            char finalVal = (char)(firstChar + secondChar - 'A' + 1);
            if (finalVal > 'Z') finalVal -= (char)26;
            Answer = "" + finalVal;
            Debug.Log("[Morsematics #"+thisLoggingID+"] Second character is larger (sum), answer: " + Answer);
        }
    }

    private bool transDown;

    public bool HandleTransDown()
    {
        if (Answer == null || transDown) return false;

        Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, ButtonTransmit.transform);
        transDown = true;
        if (transmitTicker >= 0) transmitTimings.Add(transmitTicker);
        transmitTicker = 0;
        return false;
    }

    public void HandleTransUp()
    {
        if (Answer == null || !transDown) return;

        transDown = false;
        transmitTimings.Add(transmitTicker);
        transmitTicker = 0;
    }

    private bool switchState;
    public bool HandleSwitch()
    {
        Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, ButtonTransmit.transform);
        
        switchState = !switchState;
        if (switchState) transform.Find("ReceiveSwitch").localPosition = new Vector3(0.0075f, 0.01925f, 0.086f);
        else
        {
            LED.materials[1].color = LED_OFF;
            LED.materials[2].color = LED_OFF;
            LED.materials[3].color = LED_OFF;
            transform.Find("ReceiveSwitch").localPosition = new Vector3(-0.0075f, 0.01925f, 0.086f);
        }
        return false;
    }

    private bool solved = false;

    private List<int> transmitTimings = new List<int>();
    private int transmitTicker = -1;
    private int wrongAnswerTimer = 1;

    private int[] displayPosition = new int[3];
    private int[] displayTicker = new int[]{-1, -1, -1};

    private const int TIME_UNIT = 12;
    override public void RealFixedTick()
    {
        if (wrongAnswerTimer > 0)
        {
            wrongAnswerTimer--;
            if (wrongAnswerTimer == 0) Wrong.text = "";
        }

        if (solved || Answer == null) return;

        if (transmitTicker >= 0) transmitTicker++;
        Draw.AddState(transDown);
        if (transmitTicker >= 100 && !transDown)
        {
            transmitTicker = -1;
            int[] responseData = DeMorsify(transmitTimings);
            string response = GetLetter(responseData);

            Debug.Log("[Morsematics #"+thisLoggingID+"] Provided response: " + response);
            Debug.Log("[Morsematics #"+thisLoggingID+"] Expected response: " + Answer);

            if (response.Equals("E") && Answer.Equals("T"))
            {
                Debug.Log("[Morsematics #"+thisLoggingID+"] Interpreting E as T as they are indistinguishable");
                response = "T";
            }

            if (response.Equals(Answer))
            {
                solved = true;
                Draw.Clear();
                LED.materials[1].color = LED_OFF;
                LED.materials[2].color = LED_OFF;
                LED.materials[3].color = LED_OFF;
                GetComponent<KMBombModule>().HandlePass();
            }
            else
            {
                Wrong.transform.localPosition = new Vector3(-0.015f, 0.016f, -0.04f);
                wrongAnswerTimer = 50;
                if (response == "?")
                {
                    string data = "";
                    foreach (int i in responseData)
                    {
                        if (i == 0) data += ".";
                        else data += "_";
                    }
                    Wrong.transform.localPosition = new Vector3(-0.015f, 0.016f, -0.035f);
                    wrongAnswerTimer = 100;
                    Wrong.text = data;
                }
                else if (response == "E") Wrong.text = "E/T";
                else Wrong.text = response;
                GetComponent<KMBombModule>().HandleStrike();
            }
            transmitTimings.Clear();
        }
        Draw.ApplyState();

        for (int a = 0; a < 3; a++)
        {
            //Note: 3-a is used instead of a+1 because the lights are up-side-down. This ensures the letters displayed are in the same order on the bomb as they are internally.

            int curDisplay = DisplayChars[a][displayPosition[a]];
            if (displayTicker[a] == 0 && switchState) LED.materials[3 - a].color = LED_ON;

            displayTicker[a]++;
            if ((curDisplay == 0 && displayTicker[a] == TIME_UNIT) || (curDisplay == 1 && displayTicker[a] == (TIME_UNIT * 3)))
            {
                LED.materials[3 - a].color = LED_OFF;
                displayTicker[a] = -TIME_UNIT;
                displayPosition[a]++;
                if (displayPosition[a] == DisplayChars[a].Length)
                {
                    displayTicker[a] = -(TIME_UNIT * 7);
                    displayPosition[a] = 0;
                }
            }
        }
    }

    private static int[] Morsify(string text)
    {
        char[] values = text.ToCharArray();
        List<int> data = new List<int>();
        for (int a = 0; a < values.Length; a++)
        {
            if (a > 0) data.Add(-1);
            char c = values[a];
            switch (c)
            {
                /*case ' ':
                    data.Add(-1);
                    break;*/
                case 'A':
                    data.Add(0);
                    data.Add(1);
                    break;
                case 'B':
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    break;
                case 'C':
                    data.Add(1);
                    data.Add(0);
                    data.Add(1);
                    data.Add(0);
                    break;
                case 'D':
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    break;
                case 'E':
                    data.Add(0);
                    break;
                case 'F':
                    data.Add(0);
                    data.Add(0);
                    data.Add(1);
                    data.Add(0);
                    break;
                case 'G':
                    data.Add(1);
                    data.Add(1);
                    data.Add(0);
                    break;
                case 'H':
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    break;
                case 'I':
                    data.Add(0);
                    data.Add(0);
                    break;
                case 'J':
                    data.Add(0);
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    break;
                case 'K':
                    data.Add(1);
                    data.Add(0);
                    data.Add(1);
                    break;
                case 'L':
                    data.Add(0);
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    break;
                case 'M':
                    data.Add(1);
                    data.Add(1);
                    break;
                case 'N':
                    data.Add(1);
                    data.Add(0);
                    break;
                case 'O':
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    break;
                case 'P':
                    data.Add(0);
                    data.Add(1);
                    data.Add(1);
                    data.Add(0);
                    break;
                case 'Q':
                    data.Add(1);
                    data.Add(1);
                    data.Add(0);
                    data.Add(1);
                    break;
                case 'R':
                    data.Add(0);
                    data.Add(1);
                    data.Add(0);
                    break;
                case 'S':
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    break;
                case 'T':
                    data.Add(1);
                    break;
                case 'U':
                    data.Add(0);
                    data.Add(0);
                    data.Add(1);
                    break;
                case 'V':
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(1);
                    break;
                case 'W':
                    data.Add(0);
                    data.Add(1);
                    data.Add(1);
                    break;
                case 'X':
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    data.Add(1);
                    break;
                case 'Y':
                    data.Add(1);
                    data.Add(0);
                    data.Add(1);
                    data.Add(1);
                    break;
                case 'Z':
                    data.Add(1);
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    break;
                case '1':
                    data.Add(0);
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    break;
                case '2':
                    data.Add(0);
                    data.Add(0);
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    break;
                case '3':
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(1);
                    data.Add(1);
                    break;
                case '4':
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(1);
                    break;
                case '5':
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    break;
                case '6':
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    break;
                case '7':
                    data.Add(1);
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    break;
                case '8':
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    break;
                case '9':
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    data.Add(0);
                    break;
                case '0':
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    break;
            }
        }
        return data.ToArray();
    }

    private int[] DeMorsify(List<int> timings)
    {
        if (timings.Count == 1) return new int[]{0};
        Debug.Log(timings.Count);

        int[] gapTimes = new int[timings.Count / 2];
        int[] holdTimes = new int[gapTimes.Length + 1];

        for (int a = 0; a < gapTimes.Length; a++)
        {
            gapTimes[a] = timings[a * 2 + 1];
        }

        for (int a = 0; a < holdTimes.Length; a++)
        {
            holdTimes[a] = timings[a * 2];
        }

        int averageGap = 0;
        for (int a = 0; a < gapTimes.Length; a++)
        {
            averageGap += gapTimes[a];
        }
        averageGap /= gapTimes.Length;

        for (int a = 0; a < holdTimes.Length; a++)
        {
            if (holdTimes[a] < averageGap * 2) holdTimes[a] = 0;
            else holdTimes[a] = 1;
        }

        return holdTimes;
    }

    private string GetLetter(int[] val)
    {
        if (val.Length == 0) return "?";
        else if (val.Length > 5) return "?";
        else if (val[0] == 0)
        {
            //.
            if (val.Length == 1) return "E";
            else if (val[1] == 0)
            {
                //..
                if (val.Length == 2) return "I";
                else if (val[2] == 0)
                {
                    //...
                    if (val.Length == 3) return "S";
                    else if (val[3] == 0)
                    {
                        //....
                        if (val.Length == 4) return "H";
                        else if (val[4] == 0)
                        {
                            //.....
                            if (val.Length == 5) return "5";
                            else return "?";
                        }
                        else
                        {
                            //....-
                            if (val.Length == 5) return "4";
                            else return "?";
                        }
                    }
                    else
                    {
                        //...-
                        if (val.Length == 4) return "V";
                        else if (val[4] == 0) return "?";
                        else
                        {
                            //...--
                            if (val.Length == 5) return "3";
                            else return "?";
                        }
                    }
                }
                else
                {
                    //..-
                    if (val.Length == 3) return "U";
                    else if (val[3] == 0)
                    {
                        //..-.
                        if (val.Length == 4) return "F";
                        else return "?";
                    }
                    else
                    {
                        //..--
                        if (val.Length == 4 || val[4] == 0) return "?";
                        else
                        {
                            //..---
                            if (val.Length == 5) return "2";
                            else return "?";
                        }
                    }
                }
            }
            else
            {
                //.-
                if (val.Length == 2) return "A";
                else if (val[2] == 0)
                {
                    //.-.
                    if (val.Length == 3) return "R";
                    else if (val[3] == 0)
                    {
                        //.-..
                        if (val.Length == 4) return "L";
                        else return "?";
                    }
                    else return "?";
                }
                else
                {
                    //.--
                    if (val.Length == 3) return "W";
                    else if (val[3] == 0)
                    {
                        //.--.
                        if (val.Length == 4) return "P";
                        else return "?";
                    }
                    else
                    {
                        //.---
                        if (val.Length == 4) return "J";
                        else if (val[4] == 0) return "?";
                        else
                        {
                            //.----
                            if (val.Length == 5) return "1";
                            else return "?";
                        }
                    }
                }
            }
        }
        else
        {
            //-
            if (val.Length == 1) return "T";
            else if (val[1] == 0)
            {
                //-.
                if (val.Length == 2) return "N";
                else if (val[2] == 0)
                {
                    //-..
                    if (val.Length == 3) return "D";
                    else if (val[3] == 0)
                    {
                        //-...
                        if (val.Length == 4) return "B";
                        else if (val[4] == 0)
                        {
                            if (val.Length == 5) return "6";
                            else return "?";
                        }
                        else return "?";
                    }
                    else
                    {
                        //-..-
                        if (val.Length == 4) return "X";
                        else return "?";
                    }
                }
                else
                {
                    //-.-
                    if (val.Length == 3) return "K";
                    else if (val[3] == 0)
                    {
                        //-.-.
                        if (val.Length == 4) return "C";
                        else return "?";
                    }
                    else
                    {
                        //-.--
                        if (val.Length == 4) return "Y";
                        else return "?";
                    }
                }
            }
            else
            {
                //--
                //O890
                if (val.Length == 2) return "M";
                else if (val[2] == 0)
                {
                    //--.
                    if (val.Length == 3) return "G";
                    else if (val[3] == 0)
                    {
                        //--..
                        if (val.Length == 4) return "Z";
                        else if (val[4] == 0)
                        {
                            if (val.Length == 5) return "7";
                            else return "?";
                        }
                        else return "?";
                    }
                    else
                    {
                        //--.-
                        if (val.Length == 4) return "Q";
                        else return "?";
                    }
                }
                else
                {
                    //---
                    if (val.Length == 3) return "O";
                    else if (val[3] == 0)
                    {
                        //---.
                        if (val.Length == 4 || val[4] == 1) return "?";
                        else
                        {
                            //---..
                            if (val.Length == 5) return "8";
                            else return "?";
                        }
                    }
                    else
                    {
                        //----
                        if (val.Length == 4) return "?";
                        else if (val[4] == 0)
                        {
                            //----.
                            if (val.Length == 5) return "9";
                            else return "?";
                        }
                        else
                        {
                            //-----
                            if (val.Length == 5) return "0";
                            else return "?";
                        }
                    }
                }
            }
        }
    }

    //Twitch Plays support

    #pragma warning disable 0414
    string TwitchHelpMessage = "Submit a solution using 'submit ..-.'. Toggle the lights using 'toggle'.";
    #pragma warning restore 0414

    public void TwitchHandleForcedSolve() {
        Debug.Log("[Morsematics #"+thisLoggingID+"] Module forcibly solved.");
        StartCoroutine(Solver());
    }

    private IEnumerator Solver() {
        foreach(int i in Morsify(Answer)) {
            ButtonTransmit.OnInteract();
            if(i == 1) yield return new WaitForSeconds(0.6f);
            else yield return new WaitForSeconds(0.2f);
            ButtonTransmit.OnInteractEnded();
            yield return new WaitForSeconds(0.2f);
        }
    }

    public IEnumerator ProcessTwitchCommand(string cmd) {
        cmd = cmd.ToLowerInvariant();
        if(cmd.Equals("toggle")) {
            yield return "Morsematics";
            ButtonSwitch.OnInteract();
            yield break;
        }
        if(cmd.Equals("lights on")) {
            if(!switchState) {
                yield return "Morsematics";
                ButtonSwitch.OnInteract();
            }
            yield break;
        }
        if(cmd.Equals("lights off")) {
            if(switchState) {
                yield return "Morsematics";
                ButtonSwitch.OnInteract();
            }
            yield break;
        }
        if(cmd.StartsWith("submit ")) cmd = cmd.Substring(7);
        else if(cmd.StartsWith("transmit ")) cmd = cmd.Substring(9);
        else if(cmd.StartsWith("tx ")) cmd = cmd.Substring(3);
        else {
            yield return "sendtochaterror Valid commands are submit and toggle.";
            yield break;
        }

        int[] input = new int[cmd.Length];
        for(int a = 0; a < cmd.Length; a++) {
            if(cmd[a] == '.') input[a] = 0;
            else if(cmd[a] == '-') input[a] = 1;
            else {
                yield return "sendtochaterror Invalid morse character: '" + cmd[a] + "'";
                yield break;
            }
        }

        /*if(val.Length != 1) {
            yield return "sendtochaterror Solutions must be a single letter.";
            yield break;
        }
        int c = val[0] - 'a';
        if(c < 0 || c > 25) {
            yield return "sendtochaterror Solutions must be a single letter.";
            yield break;
        }

        int[] input = Morsify(val.ToUpperInvariant());*/
        yield return "Morsematics";
        foreach(int i in input) {
            yield return ButtonTransmit;
            if(i == 1) yield return new WaitForSeconds(0.6f);
            else yield return new WaitForSeconds(0.2f);
            yield return ButtonTransmit;
            yield return new WaitForSeconds(0.2f);
        }
        yield return new WaitForSeconds(2f); //Allow answer to pass through before returning so that it's credited properly
        yield break;
    }
}