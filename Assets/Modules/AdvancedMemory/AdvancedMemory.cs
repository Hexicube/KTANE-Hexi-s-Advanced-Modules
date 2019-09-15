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
using System.Linq;

public class AdvancedMemory : MonoBehaviour
{
    private const int ADDED_STAGES = 0;
    private const bool PERFORM_AUTO_SOLVE = false;
    private const float STAGE_DELAY = 1.5f;

    public ToneGenerator Tone;
    public static string[] ignoredModules = null;

    public static int loggingID = 1;
    public int thisLoggingID;

    public KMBombInfo BombInfo;
    public KMAudio Sound;

    public KMSelectable Button0, Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9;
    private KMSelectable[] Buttons;
    public TextMesh DisplayMesh, DisplayMeshBig, StageMesh;

    private int[] Display;
    private int[] Solution;
    private int Position;

    private bool forcedSolve = false;

    void Awake()
    {
        if (ignoredModules == null)
            ignoredModules = GetComponent<KMBossModule>().GetIgnoredModules("Forget Me Not", new string[]{
                "Forget Me Not",     //Mandatory to prevent unsolvable bombs.
                "Forget Everything", //Cruel FMN.
                "Turn The Key",      //TTK is timer based, and stalls the bomb if only it and FMN are left.
                "Souvenir",          //Similar situation to TTK, stalls the bomb.
                "The Time Keeper",   //Again, timilar to TTK.
                "Simon's Stages",    //Not sure, told to add it.
                "Alchemy",
                "Forget This",
                "Simon's Stages",
                "Timing is Everything",
            });

        thisLoggingID = loggingID++;

        Buttons = new KMSelectable[]{Button0, Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9};
        
        transform.Find("Background").GetComponent<MeshRenderer>().material.color = new Color(1, 0.1f, 0.1f);

        MeshRenderer mr = transform.Find("Wiring").GetComponent<MeshRenderer>();
        mr.materials[0].color = new Color(0.1f, 0.1f, 0.1f);
        mr.materials[1].color = new Color(0.3f, 0.3f, 0.3f);
        mr.materials[2].color = new Color(0.1f, 0.4f, 0.8f);

        transform.Find("Main Display").Find("Edge").GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);
        transform.Find("Stage Display").Find("Edge").GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);

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

        Color c = new Color(.71f, .70f, .68f); //new Color(0.91f, 0.88f, 0.86f);
        Button0.GetComponent<MeshRenderer>().material.color = c;
        Button1.GetComponent<MeshRenderer>().material.color = c;
        Button2.GetComponent<MeshRenderer>().material.color = c;
        Button3.GetComponent<MeshRenderer>().material.color = c;
        Button4.GetComponent<MeshRenderer>().material.color = c;
        Button5.GetComponent<MeshRenderer>().material.color = c;
        Button6.GetComponent<MeshRenderer>().material.color = c;
        Button7.GetComponent<MeshRenderer>().material.color = c;
        Button8.GetComponent<MeshRenderer>().material.color = c;
        Button9.GetComponent<MeshRenderer>().material.color = c;

        GetComponent<KMBombModule>().OnActivate += ActivateModule;
    }

    private void ActivateModule()
    {
        int count = BombInfo.GetSolvableModuleNames().Where(x => !ignoredModules.Contains(x)).Count() + ADDED_STAGES;
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

        if(PERFORM_AUTO_SOLVE) {
            TwitchHandleForcedSolve();
        }
    }

    int ticker = 0;
    bool done = false;

    float displayTimer = 1;
    int displayCurStage = 0;
    void FixedUpdate()
    {
        if(forcedSolve) return;

        if(displayTimer > 0) displayTimer -= Time.fixedDeltaTime;

        ticker++;
        if (ticker == 5)
        {
            ticker = 0;
            if (Display == null)
            {
                DisplayMesh.text = "";
                DisplayMeshBig.text = "";
                StageMesh.text = "";
            }
            else
            {
                int progress = BombInfo.GetSolvedModuleNames().Where(x => !ignoredModules.Contains(x)).Count() + ADDED_STAGES;
                if(progress > displayCurStage) {
                    if(displayTimer <= 0) {
                        displayTimer = STAGE_DELAY;
                        displayCurStage++;
                    }
                    progress = displayCurStage;
                }
                if (progress >= Display.Length)
                {
                    StageMesh.text = "--";
                    if(!done) {
                        UpdateDisplayMesh(-1);
                        done = true;
                    }
                }
                else {
                    int stage = (progress + 1) % 100;
                    if(stage < 10) {
                        if (Display.Length < 10) StageMesh.text = "" + stage;
                        else StageMesh.text = "0" + stage;
                    }
                    else StageMesh.text = "" + stage;

                    UpdateDisplayMesh(progress);
                }
            }
        }
    }

    private int litButton = -1;
    private bool Handle(int val)
    {
        if (Solution == null || Position >= Solution.Length) return false;

        int progress = BombInfo.GetSolvedModuleNames().Where(x => !ignoredModules.Contains(x)).Count() + ADDED_STAGES;
        if (progress < Solution.Length && !forcedSolve) {
            Debug.Log("[Forget Me Not #"+thisLoggingID+"] Tried to enter a value before solving all other modules.");
            GetComponent<KMBombModule>().HandleStrike();
            return false;
        }
        else if (val == Solution[Position])
        {
            if (litButton != -1)
            {
                Buttons[litButton].transform.Find("LED").GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);
                litButton = -1;
            }
            Position++;
            UpdateDisplayMesh(-1);
            if (Position == Solution.Length) {
                Debug.Log("[Forget Me Not #"+thisLoggingID+"] Module solved.");
                GetComponent<KMBombModule>().HandlePass();
            }
            Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
            //Tone.SetTone(500 + Position * 1200 / Solution.Length);
            return true;
        }
        else
        {
            Debug.Log("[Forget Me Not #"+thisLoggingID+"] Stage " + (Position+1) + ": Pressed " + val + " instead of " + Solution[Position]);
            GetComponent<KMBombModule>().HandleStrike();
            if (litButton == -1)
            {
                litButton = Display[Position];
                Buttons[litButton].transform.Find("LED").GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0);
            }
            return false;
        }
    }

    private void UpdateDisplayMesh(int solved)
    {
        if(solved == -1) {
            //New method: Scroll small display as needed.
            DisplayMeshBig.text = "";

            string text = "";

            int PositionModified = Position;
            int Offset = 0;
            while(PositionModified > 24) {
                PositionModified -= 12;
                Offset += 12;
            }

            for(int a = Offset; a < Mathf.Min(Offset + 24, Solution.Length); a++) {
                string val = "-";
                if (a < Position) val = "" + Solution[a];

                if(a > Offset) {
                    if (a % 3 == 0) {
                        if (a % 12 == 0) text += "\n";
                        else text += " ";
                    }
                }
                text += val;
            }

            DisplayMesh.text = text;

            //Old method: Use small for first 24, switch to XXX:YYY after.
            /*if(Position > 24) {
                DisplayMesh.text = "";
                string sum = ""+Solution.Length;
                string pos = ""+Position;
                while(pos.Length < sum.Length) pos = "0"+pos;
                DisplayMeshBig.text = pos + "/" + sum;
            }
            else {
                DisplayMeshBig.text = "";

                string text = "";

                for(int a = 0; a < Solution.Length; a++) {
                    string val = "-";
                    if (a < Position) val = "" + Solution[a];

                    if(a > 0) {
                        if (a % 3 == 0) {
                            if (a % 12 == 0) text += "\n";
                            else text += " ";
                        }
                    }
                    text += val;

                    if(a == 23) break;
                }

                DisplayMesh.text = text;
            }*/
        }
        else {
            DisplayMesh.text = "";
            DisplayMeshBig.text = "" + Display[solved];
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

    //Twitch Plays support

    #pragma warning disable 0414
    string TwitchHelpMessage = "Enter the Forget Me Not sequence with \"!{0} press 531820...\". The sequence length depends on how many modules were on the bomb. You may use spaces and commas in the digit sequence.";
    #pragma warning restore 0414

    public void TwitchHandleForcedSolve() {
        Debug.Log("[Forget Me Not #"+thisLoggingID+"] Module forcibly solved.");
        forcedSolve = true;
        StartCoroutine(Solver());
    }

    private IEnumerator Solver() {
        while(Position < Solution.Length) {
            yield return new WaitForSeconds(0.05f);
            Handle(Solution[Position]);
        }
    }

    public IEnumerator ProcessTwitchCommand(string cmd) {
        if(Solution == null || Position >= Solution.Length) yield break;
        cmd = cmd.ToLowerInvariant();

        int cut;
        if(cmd.StartsWith("submit ")) cut = 7;
        else if (cmd.StartsWith("press ")) cut = 6;
        else {
            yield return "sendtochaterror Use either 'submit' or 'press' followed by a number sequence.";
            yield break;
        }

        List<int> digits = new List<int>();
        char[] strSplit = cmd.Substring(cut).ToCharArray();
        foreach(char c in strSplit) {
            if(!"0123456789 ,".Contains(c)) {
                yield return "sendtochaterror Invalid character in number sequence: '" + c + "'.\nValid characters are 0-9, space, and comma.";
                yield break;
            }

            int d = GetDigit(c);
            if(d != -1) digits.Add(d);
        }
        if(digits.Count == 0) yield break;
        if(digits.Count > (Solution.Length - Position)) {
            yield return "sendtochaterror Too many digits submitted.";
            yield break;
        }

        int progress = BombInfo.GetSolvedModuleNames().Where(x => !ignoredModules.Contains(x)).Count() + ADDED_STAGES;
        if(progress < Solution.Length) {
            yield return "Forget Me Not";
            yield return "sendtochat DansGame A little early, don't you think?";
            Handle(digits[0]);
            yield break;
        }
        yield return "Forget Me Not";
        yield return "sendtochat PogChamp Here we go!";
        yield return "multiple strikes"; //Needed for fake solve.

        SolveType solve = pickSolveType(digits.Count, Solution.Length - Position);
        if (BombInfo.GetTime() / (Solution.Length - Position) < 0.1f) solve = SolveType.REGULAR;

        foreach(int d in digits) {
            Buttons[d].OnInteract();
            if (litButton != -1) {
                if(solve == SolveType.REGULAR && BombInfo.GetTime() >= 45 && Random.value > 0.95) {
                    yield return new WaitForSeconds(2);
                    yield return "sendtochat Kreygasm We did it reddit!";
                    yield return new WaitForSeconds(1);
                    yield return "sendtochat Kappa Nope, just kidding.";
                }
                else yield return "sendtochat DansGame This isn't correct...";
                yield return "sendtochat Correct digits entered: " + Position;
                break;
            }
            if(Position >= Solution.Length) {
                yield return "sendtochat Kreygasm We did it reddit!";
                break;
            }

            if(getMusicToggle(solve, Position, digits.Count, Solution.Length - Position)) yield return "toggle waiting music";
            yield return new WaitForSeconds(getDelay(solve, Position, digits.Count, Solution.Length - Position, BombInfo.GetTime()));
        }
        yield return "end multiple strikes";
        yield break;
    }

    public enum SolveType {
        REGULAR, ACCELERATOR, SLOWSTART
    }

    public static SolveType pickSolveType(int dlen, int slen) {
        if(dlen > slen) dlen = slen;

        if(dlen > 12 && Random.value > 0.9) return SolveType.SLOWSTART;
        if(dlen > 4 && Random.value > 0.75) return SolveType.ACCELERATOR;
        return SolveType.REGULAR;
    }

    public static float getDelay(SolveType type, int curpos, int dlen, int slen, float time) {
        float allowance = (time - 0.05f) / slen;
        if (allowance < 0.05f) return allowance;

        switch(type) {
            case SolveType.SLOWSTART: {
                if(curpos < 8) return 0.5f + Random.value * 2.5f;
                return 0.05f;
            }
            case SolveType.ACCELERATOR: return Mathf.Max(3f / (float)(curpos+1), 0.05f);
            default: return 0.05f;
        }
    }

    public static bool getMusicToggle(SolveType type, int curpos, int dlen, int slen) {
        if(type == SolveType.SLOWSTART) return (curpos == 1) || (curpos == 8);
        return false;
    }
}