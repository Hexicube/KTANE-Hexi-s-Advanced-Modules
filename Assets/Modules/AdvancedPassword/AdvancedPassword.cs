﻿/*

-- On the Subject of Answering Questions --
- I hope you studied, it's quiz night! -

Respond to the computer prompts by pressing "Y" for "Yes" or "N" for "No".

*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class AdvancedPassword : MonoBehaviour
{
    public static int loggingID = 1;
    public int thisLoggingID;

    public KMSelectable Dial1, Dial2, Dial3, Dial4, Dial5, Dial6, Lever;
    private KMSelectable[] Dials;

    private int[] DialPos = new int[6];

    public KMBombInfo Info;
    public KMAudio Sound;

    bool Pass = false;

    private int[] ClickPos = new int[6], AnswerPos = new int[6];

    private static int[][] DialChart = new int[][]{
        new int[]{8, 10, 2, 11, 0, 4, 7, 8, 0, 2, 5, 1, 1, 9, 5, 3, 4, 8, 9, 7, 11, 11, 6, 4, 10, 3, 7, 9, 2, 10, 6, 6, 1, 0, 5, 3},
        new int[]{3, 1, 1, 6, 5, 2, 4, 3, 11, 11, 2, 9, 7, 5, 9, 10, 10, 0, 4, 6, 9, 11, 0, 2, 7, 7, 0, 10, 5, 8, 8, 3, 1, 6, 4, 8},
        new int[]{4, 3, 1, 11, 5, 7, 4, 6, 0, 8, 5, 8, 9, 1, 8, 9, 6, 4, 0, 7, 6, 2, 11, 7, 10, 1, 3, 10, 11, 10, 0, 3, 5, 2, 9, 2},
        new int[]{8, 7, 5, 11, 8, 7, 2, 6, 0, 0, 1, 11, 5, 4, 10, 1, 1, 0, 6, 11, 3, 8, 6, 2, 10, 10, 5, 9, 7, 4, 3, 3, 2, 4, 9, 9},
        new int[]{9, 3, 3, 7, 2, 1, 10, 6, 9, 5, 0, 11, 6, 4, 2, 9, 4, 6, 3, 5, 11, 1, 11, 8, 8, 0, 8, 1, 7, 10, 5, 0, 7, 2, 10, 4},
        new int[]{0, 8, 6, 7, 1, 5, 5, 5, 10, 6, 4, 11, 2, 9, 8, 7, 8, 11, 10, 3, 1, 0, 2, 10, 9, 4, 6, 2, 3, 4, 0, 11, 3, 1, 7, 9}
    };

    private int GetDialChartVal(int dial, char c)
    {
        int charVal;
        if (c == 'A') charVal = 0;
        else if (c == 'B') charVal = 1;
        else if (c == 'C') charVal = 2;
        else if (c == 'D') charVal = 3;
        else if (c == 'E') charVal = 4;
        else if (c == 'F') charVal = 5;
        else if (c == 'G') charVal = 6;
        else if (c == 'H') charVal = 7;
        else if (c == 'I') charVal = 8;
        else if (c == 'J') charVal = 9;
        else if (c == 'K') charVal = 10;
        else if (c == 'L') charVal = 11;
        else if (c == 'M') charVal = 12;
        else if (c == 'N') charVal = 13;
        else if (c == 'O') charVal = 14;
        else if (c == 'P') charVal = 15;
        else if (c == 'Q') charVal = 16;
        else if (c == 'R') charVal = 17;
        else if (c == 'S') charVal = 18;
        else if (c == 'T') charVal = 19;
        else if (c == 'U') charVal = 20;
        else if (c == 'V') charVal = 21;
        else if (c == 'W') charVal = 22;
        else if (c == 'X') charVal = 23;
        else if (c == 'Y') charVal = 24;
        else if (c == 'Z') charVal = 25;
        else if (c == '0') charVal = 26;
        else if (c == '1') charVal = 27;
        else if (c == '2') charVal = 28;
        else if (c == '3') charVal = 29;
        else if (c == '4') charVal = 30;
        else if (c == '5') charVal = 31;
        else if (c == '6') charVal = 32;
        else if (c == '7') charVal = 33;
        else if (c == '8') charVal = 34;
        else if (c == '9') charVal = 35;
        else charVal = 0; //Shouldn't happen.

        return DialChart[dial][charVal];
    }

    void Awake()
    {
        thisLoggingID = loggingID++;

        GetComponent<KMBombModule>().OnActivate += Init;

        Dials = new KMSelectable[] { Dial1, Dial2, Dial3, Dial4, Dial5, Dial6 };

        for (int a = 0; a < 6; a++)
        {
            Dials[a].transform.Find("LED").GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);
            Dials[a].transform.Find("default").GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
        }

        for (int a = 0; a < 6; a++)
        {
            Dials[a].transform.Find("Bar").GetComponent<MeshRenderer>().material.color = new Color(0.4f, 0.4f, 0.4f);
            DialPos[a] = Random.Range(0, 12);
            Dials[a].transform.Find("Bar").transform.localEulerAngles = new Vector3(0, DialPos[a] * 30f, 0);
            ClickPos[a] = Random.Range(0, 12);
            int a2 = a;
            Dials[a].OnInteract += delegate() { HandleInteract(a2); return false; };
        }

        Debug.Log("[Safety Safe #"+thisLoggingID+"] Safety Safe dial click locations: " + ClickPos[0] + "," + ClickPos[1] + "," + ClickPos[2] + "," + ClickPos[3] + "," + ClickPos[4] + "," + ClickPos[5]);

        Lever.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
        Lever.transform.Find("default").GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
    }

    void Init()
    {
        //transform.Find("Background").GetComponent<MeshRenderer>().material.color = new Color(1, 0.1f, 0.1f);

        Lever.OnInteract += HandleLever;

        int dialOffset = 0;

        List<string> ports = new List<string>();
        List<string> data = Info.QueryWidgets(KMBombInfo.QUERYKEY_GET_PORTS, null);
        foreach (string response in data)
        {
            Dictionary<string, string[]> responseDict = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(response);
            foreach (string s in responseDict["presentPorts"])
            {
                if (!ports.Contains(s))
                {
                    ports.Add(s);
                    dialOffset += 7;
                }
            }
        }

        char[] serial = "AB12C3".ToCharArray();
        data = Info.QueryWidgets(KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER, null);
        foreach (string response in data)
        {
            Dictionary<string, string> responseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
            serial = responseDict["serial"].ToCharArray();
            break;
        }
        
        data = Info.QueryWidgets(KMBombInfo.QUERYKEY_GET_INDICATOR, null);
        foreach (string response in data)
        {
            Dictionary<string, string> responseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
            char[] label = responseDict["label"].ToCharArray();

            bool match = false;
            foreach (char c in label)
            {
                foreach (char c2 in serial)
                {
                    if (c == c2)
                    {
                        match = true;
                        break;
                    }
                }
                if (match) break;
            }

            if (match)
            {
                if (responseDict["on"].Equals("True")) dialOffset += 5;
                else dialOffset++;
            }
        }

        Debug.Log("[Safety Safe #"+thisLoggingID+"] Global offset: " + dialOffset % 12);

        string offsetList = "";

        int[] logAnswers = new int[6];
        for (int a = 0; a < 6; a++)
        {
            int serialVal = 0;
            serialVal = GetDialChartVal(a, serial[a]);
            if (a == 5)
            {
                serialVal += GetDialChartVal(5, serial[0]);
                serialVal += GetDialChartVal(5, serial[1]);
                serialVal += GetDialChartVal(5, serial[2]);
                serialVal += GetDialChartVal(5, serial[3]);
                serialVal += GetDialChartVal(5, serial[4]);
                offsetList += serialVal % 12;
            }
            else offsetList += serialVal % 12 + ",";
            AnswerPos[a] = (ClickPos[a] + serialVal + dialOffset) % 12;
            logAnswers[a] = (serialVal + dialOffset) % 12;
        }

        Debug.Log("[Safety Safe #"+thisLoggingID+"] Local offsets: " + offsetList);
        Debug.Log("[Safety Safe #"+thisLoggingID+"] Answer: " + logAnswers[0] + "," + logAnswers[1] + "," + logAnswers[2] + "," + logAnswers[3] + "," + logAnswers[4] + "," + logAnswers[5]);
        Debug.Log("[Safety Safe #"+thisLoggingID+"] Final positions: " + AnswerPos[0] + "," + AnswerPos[1] + "," + AnswerPos[2] + "," + AnswerPos[3] + "," + AnswerPos[4] + "," + AnswerPos[5]);

        foreach (GameObject o in TreasureChoices)
        {
            o.SetActive(false);
        }
        if (TREASURE_ENABLED || FORCE_TREASURE)
        {
            if (FORCE_TREASURE || Random.value >= 0.9975) //1 in 400
            {
                doesOpen = true;
                if (FORCE_TREASURE || Random.value >= 0.7) //30%
                {
                    int choice = Random.Range(0, TreasureChoices.Length);
                    for(int a = 0; a < TreasureChoices.Length; a++) {
                        if(a == choice) TreasureChoices[a].SetActive(true);
                        else TreasureChoices[a].SetActive(false);
                    }
                }
            }
        }
    }

    void HandleInteract(int dial)
    {
        if (Pass) return;
        Dials[dial].AddInteractionPunch(0.1f);

        DialPos[dial] = (DialPos[dial] + 1) % 12;
        Dials[dial].transform.Find("Bar").transform.localEulerAngles = new Vector3(0, DialPos[dial] * 30f, 0);

        if (DialPos[dial] == ClickPos[dial]) Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        else Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);

        Dials[dial].transform.Find("LED").GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);
    }

    bool HandleLever()
    {
        if (Pass) return false;
        Lever.AddInteractionPunch(2.5f);

        bool ans = true;
        for (int a = 0; a < 6; a++)
        {
            if (DialPos[a] != AnswerPos[a])
            {
                ans = false;
                break;
            }
        }
        
        int[] curPosList = new int[6];
        for(int a = 0; a < 6; a++) {
            curPosList[a] = DialPos[a] - ClickPos[a];
            if(curPosList[a] < 0) curPosList[a] += 12;
        }
        Debug.Log("[Safety Safe #"+thisLoggingID+"] Input: " + curPosList[0] + "," + curPosList[1] + "," + curPosList[2] + "," + curPosList[3] + "," + curPosList[4] + "," + curPosList[5]);

        if (ans)
        {
            Debug.Log("[Safety Safe #"+thisLoggingID+"] Module solved.");
            Lever.transform.localEulerAngles = new Vector3(0, 210, 0);
            Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            GetComponent<KMBombModule>().HandlePass();
            Pass = true;
        }
        else {
            Debug.Log("[Safety Safe #"+thisLoggingID+"] Answer incorrect.");
            GetComponent<KMBombModule>().HandleStrike();
        }
        for (int a = 0; a < 6; a++)
        {
            Dials[a].transform.Find("LED").GetComponent<MeshRenderer>().material.color = (DialPos[a] == AnswerPos[a]) ? new Color(0, 1, 0) : new Color(1, 0, 0);
        }

        return false;
    }

    private const bool TREASURE_ENABLED = false;
    private const bool FORCE_TREASURE = false;

    public GameObject Door;
    public GameObject[] TreasureChoices;

    private bool doesOpen, doneUnlock;
    private float counter = 0f;
    void Update()
    {
        if (TREASURE_ENABLED && doesOpen && Pass)
        {
            if (counter < 5f)
            {
                counter += Time.deltaTime;
                if (counter > 5f) counter = 5f;
                if (!doneUnlock && counter > 0.5f)
                {
                    doneUnlock = true;
                    Lever.AddInteractionPunch(0.1f);
                    Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);
                }
                if (counter > 1f)
                {
                    Door.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 5f * (counter - 1f)));
                }
            }
        }
    }

    //Twitch Plays support

    #pragma warning disable 0414
    string TwitchHelpMessage = "Cycle a dial with 'press TL 6'. Cycle all dials with 'press 1 8 2 12 0 5'. Listen to dials with 'listen' or 'listen BR'. Submit an answer with 'submit'.\nDial positions are specified with positions (TL, TM, TR, BL, BM, BR).";
    #pragma warning restore 0414

    public IEnumerator TwitchHandleForcedSolve() {
        Debug.Log("[Safety Safe #"+thisLoggingID+"] Module forcibly solved.");
        for (int a = 0; a < 6; a++)
        {
            while (DialPos[a] != AnswerPos[a])
            {
                Dials[a].OnInteract();
                yield return new WaitForSeconds(0.05f);
            }
        }
        Lever.OnInteract();
    }

    private int ParseDial(string dial) {
        if(dial.Equals("tl")) return 0;
        if(dial.Equals("tm") || dial.Equals("tc")) return 1;
        if(dial.Equals("tr")) return 2;
        if(dial.Equals("bl")) return 3;
        if(dial.Equals("bm") || dial.Equals("bc")) return 4;
        if(dial.Equals("br")) return 5;
        return -1;
    }

    public IEnumerator ProcessTwitchCommand(string cmd) {
        string[] command = cmd.ToLowerInvariant().Split(' ');
        int temp;
        if(!int.TryParse(command[0], out temp)) {
            if(command.Length == 1) {
                int dial = ParseDial(command[0]);
                if(dial == -1) {
                    if(command[0].Equals("listen") || command[0].Equals("cycle")) {
                        yield return "Safety Safe";
                        for(int a = 0; a < 6; a++) {
                            for(int b = 0; b < 12; b++) {
                                HandleInteract(a);
                                yield return new WaitForSeconds(0.4f);
                            }
                            yield return new WaitForSeconds(0.6f);
                        }
                        yield break;
                    }

                    if(command[0].Equals("submit") || command[0].Equals("guess") || command[0].Equals("lever")) {
                        yield return "Safety Safe";
                        HandleLever();
                        yield break;
                    }

                    yield return "sendtochaterror Unknown dial or command: " + command[0];
                    yield break;
                }
                
                yield return "Safety Safe";
                    for(int a = 0; a < 12; a++) {
                    HandleInteract(dial);
                    yield return new WaitForSeconds(0.25f);
                }
                yield break;
            }
            if(command.Length == 2) {
                int dial = ParseDial(command[0]);
                if(dial == -1) {
                    if(command[0].Equals("listen") || command[0].Equals("cycle")) {
                        dial = ParseDial(command[1]);
                        if(dial == -1) {
                            if(command[1].Equals("fast")) {
                                yield return "Safety Safe";
                                for(int a = 0; a < 6; a++) {
                                    for(int b = 0; b < 12; b++) {
                                        HandleInteract(a);
                                        yield return new WaitForSeconds(0.25f);
                                    }
                                    yield return new WaitForSeconds(0.75f);
                                }
                                yield break;
                            }
                            yield return "sendtochaterror Unknown dial: " + command[0];
                            yield break;
                        }

                        yield return "Safety Safe";
                            for(int a = 0; a < 12; a++) {
                            HandleInteract(dial);
                            yield return new WaitForSeconds(0.4f);
                        }
                        yield break;
                    }

                    yield return "sendtochaterror Unknown dial: " + command[0];
                    yield break;
                }
                int amt = int.Parse(command[1]) % 12;
                if(amt < 0) amt += 12;
                
                yield return "Safety Safe";
                for(int a = 0; a < amt; a++) {
                    HandleInteract(dial);
                    yield return new WaitForSeconds(0.1f);
                }
                yield break;
            }
            if(command.Length == 3) {
                if(command[0].Equals("press") || command[0].Equals("cycle")) {
                    int dial = ParseDial(command[1]);
                    if(dial == -1) {
                        yield return "sendtochaterror Unknown dial: " + command[0];
                        yield break;
                    }
                    int amt = int.Parse(command[2]) % 12;
                    if(amt < 0) amt += 12;
                
                    yield return "Safety Safe";
                    for(int a = 0; a < amt; a++) {
                        HandleInteract(dial);
                        yield return new WaitForSeconds(0.1f);
                    }
                    yield break;
                }
            }
            if(command.Length == 7) {
                if(command[0].Equals("press") || command[0].Equals("cycle") || command[0].Equals("submit") || command[0].Equals("guess")) {
                    int[] amt = new int[6];
                    for(int a = 0; a < 6; a++) {
                        amt[a] = int.Parse(command[a+1]) % 12;
                        if(amt[a] < 0) amt[a] += 12;
                    }

                    yield return "Safety Safe";
                    for(int a = 0; a < 6; a++) {
                        for(int b = 0; b < amt[a]; b++) {
                            HandleInteract(a);
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    if(command[0].Equals("submit") || command[0].Equals("guess")) HandleLever();

                    yield break;
                }
            }
            yield return "sendtochaterror Unknown command: " + command[0] + " (with " + (command.Length-1) + " arguments)";
            yield break;
        }
        if(command.Length == 6) {
            int[] amt = new int[6];
            amt[0] = temp % 12;
            if(amt[0] < 0) amt[0] += 12;
            for(int a = 0; a < 6; a++) {
                amt[a] = int.Parse(command[a]) % 12;
                if(amt[a] < 0) amt[a] += 12;
            }

            yield return "Safety Safe";
            for(int a = 0; a < 6; a++) {
                for(int b = 0; b < amt[a]; b++) {
                    HandleInteract(a);
                    yield return new WaitForSeconds(0.1f);
                }
            }
            yield break;
        }
        yield return "sendtochaterror Unknown command. Refer to help for a list of valid commands.";
        yield break;
    }
}