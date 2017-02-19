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
    public static int loggingID = 1;
    public int thisLoggingID;

    public KMBombInfo BombInfo;
    public KMAudio Sound;

    public KMSelectable Button0, Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9;
    private KMSelectable[] Buttons;
    public TextMesh DisplayMesh, StageMesh;

    private int[] Display;
    private int[] Solution;
    private int Position;

    void Awake()
    {
        thisLoggingID = loggingID++;

        Buttons = new KMSelectable[]{Button0, Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9};

        transform.Find("Background").GetComponent<MeshRenderer>().material.color = new Color(1, 0.1f, 0.1f);

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

        Button0.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
        Button1.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
        Button2.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
        Button3.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
        Button4.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
        Button5.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
        Button6.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
        Button7.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
        Button8.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
        Button9.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);

        GetComponent<KMBombModule>().OnActivate += ActivateModule;
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
                        int val = GetDigit(c);
                        if (val >= 0) numSerialDigits++;
                    }

                    bool serialPort = false;
                    if (numSerialDigits >= 3)
                    {
                        List<string> data = BombInfo.QueryWidgets(KMBombInfo.QUERYKEY_GET_PORTS, null);
                        foreach (string response in data)
                        {
                            Dictionary<string, string[]> responseDict = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(response);
                            foreach (string s in responseDict["presentPorts"])
                            {
                                if (s.Equals("Serial"))
                                {
                                    serialPort = true;
                                    break;
                                }
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

        Debug.Log("[Forget Me Not #"+thisLoggingID+"] Non-FMN modules: " + count);
        string displayText = "";
        string solutionText = "";
        for (int a = 0; a < count; a++)
        {
            if (a > 0 && a % 3 == 0)
            {
                displayText += " ";
                solutionText += " ";
            }
            displayText += Display[a];
            solutionText += Solution[a];
        }
        Debug.Log("[Forget Me Not #"+thisLoggingID+"] Display: " + displayText);
        Debug.Log("[Forget Me Not #"+thisLoggingID+"] Solution: " + solutionText);
    }

    int ticker = 0;
    bool done = false;
    void FixedUpdate()
    {
        ticker++;
        if (ticker == 5)
        {
            ticker = 0;
            if (Display == null)
            {
                DisplayMesh.text = "";
                StageMesh.text = "";
            }
            else
            {
                int progress = BombInfo.GetSolvedModuleNames().Count;
                if (progress >= Display.Length)
                {
                    StageMesh.text = "-";
                    if (!done)
                    {
                        DisplayMesh.text = "-";
                        done = true;
                    }
                }
                else
                {
                    DisplayMesh.text = "" + Display[progress];
                    StageMesh.text = "" + (progress + 1);
                }
            }
        }
    }

    private int litButton = -1;
    private void Handle(int val)
    {
        if (Solution == null) return;
        if (Position < Solution.Length)
        {
            int progress = BombInfo.GetSolvedModuleNames().Count;
            if (progress < Solution.Length) {
                Debug.Log("[Forget Me Not #"+thisLoggingID+"] Tried to enter a value before solving all other modules.");
                GetComponent<KMBombModule>().HandleStrike();
            }
            else if (val == Solution[Position])
            {
                if (litButton != -1)
                {
                    Buttons[litButton].GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
                    litButton = -1;
                }
                if (DisplayMesh.text.Equals("-")) DisplayMesh.text = "";
                if (Solution.Length > 10) //Double-decker bomb, vanilla caps at 10 slots (one is timer, one is this module)
                {
                    if (Position == (Solution.Length + 1) / 2) DisplayMesh.text += "\n";
                }
                DisplayMesh.text += val;
                Position++;
                if (Position == Solution.Length) {
                    Debug.Log("[Forget Me Not #"+thisLoggingID+"] Module solved.");
                    GetComponent<KMBombModule>().HandlePass();
                }
                Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
            }
            else
            {
                Debug.Log("[Forget Me Not #"+thisLoggingID+"] Stage " + (Position+1) + ": Pressed " + val + " instead of " + Solution[Position]);
                GetComponent<KMBombModule>().HandleStrike();
                if (litButton == -1)
                {
                    litButton = Display[Position];
                    Buttons[litButton].GetComponent<MeshRenderer>().material.color = new Color(0.5f, 0.8f, 0.5f);
                }
            }
        }
    }

    private bool Handle0()
    {
        if (Solution == null || Position == Solution.Length) return false;
        Button0.AddInteractionPunch(0.2f);
        Handle(0);
        return false;
    }

    private bool Handle1()
    {
        if (Solution == null || Position == Solution.Length) return false;
        Button1.AddInteractionPunch(0.2f);
        Handle(1);
        return false;
    }

    private bool Handle2()
    {
        if (Solution == null || Position == Solution.Length) return false;
        Button2.AddInteractionPunch(0.2f);
        Handle(2);
        return false;
    }

    private bool Handle3()
    {
        if (Solution == null || Position == Solution.Length) return false;
        Button3.AddInteractionPunch(0.2f);
        Handle(3);
        return false;
    }

    private bool Handle4()
    {
        if (Solution == null || Position == Solution.Length) return false;
        Button4.AddInteractionPunch(0.2f);
        Handle(4);
        return false;
    }

    private bool Handle5()
    {
        if (Solution == null || Position == Solution.Length) return false;
        Button5.AddInteractionPunch(0.2f);
        Handle(5);
        return false;
    }

    private bool Handle6()
    {
        if (Solution == null || Position == Solution.Length) return false;
        Button6.AddInteractionPunch(0.2f);
        Handle(6);
        return false;
    }

    private bool Handle7()
    {
        if (Solution == null || Position == Solution.Length) return false;
        Button7.AddInteractionPunch(0.2f);
        Handle(7);
        return false;
    }

    private bool Handle8()
    {
        if (Solution == null || Position == Solution.Length) return false;
        Button8.AddInteractionPunch(0.2f);
        Handle(8);
        return false;
    }

    private bool Handle9()
    {
        if (Solution == null || Position == Solution.Length) return false;
        Button9.AddInteractionPunch(0.2f);
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