/*

-- On the Subject of Forget Me Not --
- This one likes attention, but not *too* much attention. -

Complete a different module to unlock each stage.
Each stage will provide a different letter.
Using the rules below, change each letter to the appropriate number.
Once all letters have been displayed, the display will go blank. Enter the correct numbers.

*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class AdvancedMemory : MonoBehaviour
{
    public KMBombInfo BombInfo;
    public KMAudio Sound;

    public KMSelectable Button0, Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9;
    public TextMesh DisplayMesh;

    private int[] Display;
    private int[] Solution;
    private int Position;

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

        GetComponent<KMBombModule>().OnActivate += ActivateModule;

        gameObject.transform.Find("Plane").GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);
    }

    private void ActivateModule()
    {
        List<string> modules = BombInfo.GetSolvableModuleNames();
        int count = 0;
        foreach (string s in modules)
        {
            if (!s.Equals("Forget Me Not")) count++;
        }
        Display = new int[count];
        Solution = new int[count];

        if (count == 0) GetComponent<KMBombModule>().HandlePass(); //Prevent deadlock
        else
        {
            char[] serial = null;
            int largestSerial = -1, smallestOddSerial = -1;
            int prev1 = 0, prev2 = 0;
            for (int a = 0; a < count; a++)
            {
                Display[a] = Random.Range(0, 10);
                if (a == 0)
                {
                    int unlit = 0, lit = 0;
                    bool unlitCAR = false;
                    List<string> data = BombInfo.QueryWidgets(KMBombInfo.QUERYKEY_GET_INDICATOR, null);
                    foreach (string response in data)
                    {
                        Dictionary<string, string> responseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                        string label = responseDict["label"];
                        bool active = responseDict["on"].Equals("True");
                        Debug.Log(response);
                        Debug.Log(label + ":" + active);
                        if (label.Equals("CAR") && !active) unlitCAR = true;
                        if (active) lit++;
                        else unlit++;
                    }

                    if (unlitCAR) Solution[a] = 2;
                    else if (unlit > lit) Solution[a] = 7;
                    else if (unlit == 0) Solution[a] = lit;
                    else
                    {
                        data = BombInfo.QueryWidgets(KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER, null);
                        foreach (string response in data)
                        {
                            Dictionary<string, string> responseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                            serial = responseDict["serial"].ToCharArray();
                            break;
                        }
                        Solution[a] = GetDigit(serial[serial.Length - 1]);
                        if (Solution[a] == -1) Solution[a] = 0;
                    }
                }
                else if (a == 1)
                {
                    if (serial == null)
                    {
                        List<string> data = BombInfo.QueryWidgets(KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER, null);
                        foreach (string response in data)
                        {
                            Dictionary<string, string> responseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                            serial = responseDict["serial"].ToCharArray();
                            break;
                        }
                    }
                    int numSerialDigits = 0;
                    foreach (char c in serial)
                    {
                        if (GetDigit(c) >= 0) numSerialDigits++;
                    }
                    bool serialPort = false;
                    if (numSerialDigits >= 3)
                    {
                        List<string> data = BombInfo.QueryWidgets(KMBombInfo.QUERYKEY_GET_PORTS, null);
                        foreach (string response in data)
                        {
                            Debug.Log(response);
                            Dictionary<string, string[]> responseDict = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(response);
                            foreach (string s in responseDict["presentPorts"])
                            {
                                if (s.Equals("Serial")) serialPort = true;
                                break;
                            }
                            if (serialPort) break;
                        }
                    }
                    if (serialPort) Solution[a] = 3;
                    else if (prev1 % 2 == 0) Solution[a] = prev1 + 1;
                    else Solution[a] = prev1 - 1;
                }
                else
                {
                    if (prev1 == 0 || prev2 == 0)
                    {
                        if (largestSerial == -1)
                        {
                            largestSerial = 0;
                            foreach (char c in serial)
                            {
                                int val = GetDigit(c);
                                if (val > largestSerial) largestSerial = val;
                            }
                        }
                        Solution[a] = largestSerial;
                    }
                    else if (prev1 % 2 == 0 && prev2 % 2 == 0)
                    {
                        if (smallestOddSerial == -1)
                        {
                            smallestOddSerial = 9;
                            foreach (char c in serial)
                            {
                                int val = GetDigit(c);
                                if (val % 2 == 1 && val < smallestOddSerial) smallestOddSerial = val;
                            }
                        }
                        Solution[a] = smallestOddSerial;
                    }
                    else
                    {
                        Solution[a] = prev1 + prev2;
                        while (Solution[a] >= 10) Solution[a] /= 10;
                    }
                }
                Solution[a] = (Solution[a] + Display[a]) % 10;
                prev2 = prev1;
                prev1 = Solution[a];
            }
        }
    }

    int ticker = 0;
    bool done = false;
    void FixedUpdate()
    {
        ticker++;
        if (ticker == 5)
        {
            ticker = 0;
            if (Display == null) DisplayMesh.text = "";
            else
            {
                int progress = BombInfo.GetSolvedModuleNames().Count;
                if (progress >= Display.Length)
                {
                    if (!done)
                    {
                        DisplayMesh.text = "-";
                        done = true;
                    }
                }
                else DisplayMesh.text = "" + Display[progress];
            }
        }
    }

    private void Handle(int val)
    {
        if (Solution == null) return;
        if (Position < Solution.Length)
        {
            int progress = BombInfo.GetSolvedModuleNames().Count;
            Debug.Log(progress);
            if (progress < Solution.Length) GetComponent<KMBombModule>().HandleStrike();
            else if (val == Solution[Position])
            {
                if (DisplayMesh.text.Equals("-")) DisplayMesh.text = "";
                if (Position == 10) DisplayMesh.text += "\n"; //Support for double-decker bomb, will not happen normally.
                DisplayMesh.text += val;
                Position++;
                if (Position == Solution.Length) GetComponent<KMBombModule>().HandlePass();
                else Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
            }
            else GetComponent<KMBombModule>().HandleStrike();
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

    private int GetDigit(char c)
    {
        switch(c)
        {
            case '0': return 0;
            case '1': return 1;
            case '2': return 2;
            case '3': return 3;
            case '4': return 4;
            case '5': return 5;
            case '6': return 6;
            case '7': return 7;
            case '8': return 8;
            case '9': return 9;
            default: return -1;
        }
    }
}