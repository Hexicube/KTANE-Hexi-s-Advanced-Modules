/*

-- On the Subject of Answering Questions --
- I hope you studied, it's quiz night! -

Respond to the computer prompts by pressing "Y" for "Yes" or "N" for "No".

*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

public class AdvancedVentingGas : MonoBehaviour
{
    private char[] Serial = null;
    private int Batteries = -1;

    private static int TEST_MODE_IDX = -1; // -1 to disable

    public static int loggingID = 1;
    public int thisLoggingID;

    public KMBombInfo BombInfo;
    public KMAudio Sound;

    public KMGameCommands Service;

    public KMSelectable YesButton;
    public KMSelectable NoButton;
    public TextMesh Display;

    protected bool LastReply;
    protected bool HasReply;
    protected System.Func<AdvancedVentingGas, bool, bool> CurQ;

    protected bool LastWasSelfReference = false;
    protected bool EverMadeMistake = false;
    protected bool EverSaidYes = false;
    protected bool EverSaidNo = false;
    protected bool Exploded = false;

    protected string abortMode = null;
    protected bool forceSolve = false;

    private int NumHasReply = 3;
    private int NumTriesForAbort = 2;
    private List<KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>> QuestionList = new List<KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>>(){
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("What was your\nprevious answer?", delegate(AdvancedVentingGas Module, bool Response){Module.LastWasSelfReference = true;if (Module.HasReply) return Response == Module.LastReply;return true;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("What wasn't your\nprevious answer?", delegate(AdvancedVentingGas Module, bool Response){Module.LastWasSelfReference = true;if (Module.HasReply) return Response == !Module.LastReply;return true;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Was the last\nanswered question\nabout a previous\nquestion or answer?", delegate(AdvancedVentingGas Module, bool Response){bool Correct = Response == Module.LastWasSelfReference;Module.LastWasSelfReference = true;if (Module.HasReply) return Correct;return true;})},
        
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Have you previously\nanswered Yes\nto a question?", delegate(AdvancedVentingGas Module, bool Response){Module.LastWasSelfReference = true;return Response == Module.EverSaidYes;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Have you previously\nanswered No\nto a question?", delegate(AdvancedVentingGas Module, bool Response){Module.LastWasSelfReference = true;return Response == Module.EverSaidNo;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Have you previously\nanswered not Yes\nto a question?", delegate(AdvancedVentingGas Module, bool Response){Module.LastWasSelfReference = true;return Response == Module.EverSaidNo;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Have you previously\nanswered not No\nto a question?", delegate(AdvancedVentingGas Module, bool Response){Module.LastWasSelfReference = true;return Response == Module.EverSaidYes;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Have you previously\nanswered a question\nincorrectly?", delegate(AdvancedVentingGas Module, bool Response){Module.LastWasSelfReference = true;return Response == Module.EverMadeMistake;})},
        
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Does this\nquestion contain\nthree lines?", delegate(AdvancedVentingGas Module, bool Response){Module.LastWasSelfReference = false;return Response;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Does this\nquestion contain\nsix lines?", delegate(AdvancedVentingGas Module, bool Response){Module.LastWasSelfReference = false;return !Response;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Does this\nquestion contain\nthree words?", delegate(AdvancedVentingGas Module, bool Response){Module.LastWasSelfReference = false;return !Response;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Does this\nquestion contain\nsix words?", delegate(AdvancedVentingGas Module, bool Response){Module.LastWasSelfReference = false;return Response;})},

        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Do you have\nat least\n1 strike?", delegate(AdvancedVentingGas Module, bool Response){Module.LastWasSelfReference = false;return (Module.BombInfo.GetStrikes() > 0) == Response;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Do you have\nmore than\n1 strike?", delegate(AdvancedVentingGas Module, bool Response){Module.LastWasSelfReference = false;return (Module.BombInfo.GetStrikes() > 1) == Response;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Do you have\nup to\n1 strike?", delegate(AdvancedVentingGas Module, bool Response){Module.LastWasSelfReference = false;return (Module.BombInfo.GetStrikes() < 2) == Response;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Do you have\nless than\n1 strike?", delegate(AdvancedVentingGas Module, bool Response){Module.LastWasSelfReference = false;return (Module.BombInfo.GetStrikes() < 1) == Response;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Do you have\nmore strikes\nthan batteries?", delegate(AdvancedVentingGas Module, bool Response){Module.LastWasSelfReference = false;return (Module.BombInfo.GetStrikes() > Module.GetBatteries()) == Response;})},
        
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Abort?", delegate(AdvancedVentingGas Module, bool Response){Module.LastWasSelfReference = false;return !Response;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("SEGFAULT", delegate(AdvancedVentingGas Module, bool Response){Module.LastWasSelfReference = false;return Response;})},
        
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Are you a\ndirty cheater?", delegate(AdvancedVentingGas Module, bool Response){Module.LastWasSelfReference = false;return !Response;})},
        
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Does the\nserial contain\nduplicate\ncharacters?", delegate(AdvancedVentingGas Module, bool Response){Module.LastWasSelfReference = false;return Response == Module.SerialDuplicate();})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Does the number of\nbatteries and the\nsum of serial number\ndigits parities match?", delegate(AdvancedVentingGas Module, bool Response){Module.LastWasSelfReference = false;return Response == Module.CheckParity();})},
    };

    void Awake()
    {
        thisLoggingID = loggingID++;

        transform.Find("Plane").GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);

        Display.text = "";

        GetComponent<KMNeedyModule>().OnNeedyActivation += OnNeedyActivation;
        GetComponent<KMNeedyModule>().OnNeedyDeactivation += OnNeedyDeactivation;
        YesButton.OnInteract += HandleYes;
        NoButton.OnInteract += HandleNo;
        GetComponent<KMNeedyModule>().OnTimerExpired += OnTimerExpired;
        BombInfo.OnBombExploded += delegate() { Exploded = true; };

        MeshRenderer mr = transform.Find("Wiring").GetComponent<MeshRenderer>();
        mr.materials[2].color = new Color(0.1f, 0.1f, 0.1f);
        mr.materials[1].color = new Color(0.3f, 0.3f, 0.3f);
        mr.materials[0].color = new Color(0.1f, 0.4f, 0.8f);

        YesButton.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
        NoButton.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);

        if (TEST_MODE_IDX != -1) NewQuestion();
    }

    protected bool HandleYes()
    {
        if (TEST_MODE_IDX != -1) {
            TEST_MODE_IDX = (TEST_MODE_IDX + 1) % QuestionList.Count;
            NewQuestion();
            return false;
        }

        if (abortMode != null) return false;
        if (CurQ != null) YesButton.AddInteractionPunch();
        HandleResponse(true);
        EverSaidYes = true;
        return false;
    }

    protected bool HandleNo()
    {
        if (TEST_MODE_IDX != -1) {
            TEST_MODE_IDX = (TEST_MODE_IDX + QuestionList.Count - 1) % QuestionList.Count;
            NewQuestion();
            return false;
        }

        if (abortMode != null) return false;
        if (CurQ != null) NoButton.AddInteractionPunch();
        HandleResponse(false);
        EverSaidNo = true;
        return false;
    }

    protected void OnNeedyActivation()
    {
        if(forceSolve) {
            GetComponent<KMNeedyModule>().HandlePass();
            return;
        }
        if (abortMode != null) return;
        else NewQuestion();
    }

    protected void OnNeedyDeactivation()
    {
        if (TEST_MODE_IDX != -1) return;

        if (abortMode != null) return;
        CurQ = null;
        Display.text = "";
    }

    protected void HandleResponse(bool R)
    {
        if (CurQ == null) return;
        Debug.Log("[Answering Questions #"+thisLoggingID+"] Quiz: " + Display.text.Replace("\n", " "));
        Debug.Log("[Answering Questions #"+thisLoggingID+"] Given answer: " + (R ? "Y" : "N"));
        if (CurQ(this, R))
        {
            Debug.Log("[Answering Questions #"+thisLoggingID+"] Answer was correct");
            Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
        }
        else
        {
            if (Display.text.Equals("Abort?"))
            {
                Debug.Log("[Answering Questions #"+thisLoggingID+"] ABORT! ABORT!!! ABOOOOOOORT!!!!!");
                abortMode = "ABORT!";
            }
            else if (Display.text.StartsWith("SEGFAULT")) {
                Debug.Log("[Answering Questions #"+thisLoggingID+"] SEGFAULT (ABORT!)");
                abortMode = "SEGFAULT!";
            }
            else
            {
                Debug.Log("[Answering Questions #"+thisLoggingID+"] Answer was incorrect");
                GetComponent<KMNeedyModule>().HandleStrike();
            }
            EverMadeMistake = true;
        }
        GetComponent<KMNeedyModule>().HandlePass();

        HasReply = true;
        LastReply = R;
        CurQ = null;
        Display.text = "";
    }

    private float ticker = 0f;
    private float segTicker = 0f;
    void FixedUpdate() {
        segTicker += Time.fixedDeltaTime;
        if (segTicker > 0.5f) {
            segTicker -= 0.5f;
            if (Display.text.StartsWith("SEGFAULT")) {
                StringBuilder builder = new StringBuilder();
                builder.Append("SEGFAULT\n");
                for (int a = 0; a < 10; a++) builder.Append((char)Random.Range((int)'A', (int)'Z'+1));
                builder.Append("\n");
                for (int a = 0; a < 10; a++) builder.Append((char)Random.Range((int)'A', (int)'Z'+1));
                builder.Append("\nContinue?");
                Display.text = builder.ToString();
            }
            else if (abortMode == "SEGFAULT!") {
                StringBuilder builder = new StringBuilder();
                for (int a = 0; a < 10; a++) builder.Append((char)Random.Range((int)'A', (int)'Z'+1));
                builder.Append("\n");
                for (int a = 0; a < 10; a++) builder.Append((char)Random.Range((int)'A', (int)'Z'+1));
                builder.Append("\n");
                for (int a = 0; a < 10; a++) builder.Append((char)Random.Range((int)'A', (int)'Z'+1));
                builder.Append("\n");
                for (int a = 0; a < 10; a++) builder.Append((char)Random.Range((int)'A', (int)'Z'+1));
                Display.text = builder.ToString();
            }
        }
        if(!forceSolve && abortMode != null && !Exploded) {
            bool state = ticker >= 1f;
            ticker += Time.fixedDeltaTime;
            if (state) {
                if(ticker >= 2f) {
                    ticker -= 2f;
                    if (abortMode != "SEGFAULT!") Display.text = "";
                }
            }
            else {
                if(ticker >= 1f) {
                    Service.CauseStrike(abortMode);
                    if (abortMode != "SEGFAULT!") {
                        Display.text = abortMode;
                        Display.fontSize = 380;
                    }
                }
            }
        }
    }

    protected void OnTimerExpired()
    {
        if (TEST_MODE_IDX != -1) return;
        if (forceSolve || CurQ == null) return;
        GetComponent<KMNeedyModule>().HandleStrike();
    }

    private void SetSerialVariable() {
        if (Serial == null)
        {
            List<string> data = BombInfo.QueryWidgets(KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER, null);
            foreach (string response in data)
            {
                Dictionary<string, string> responseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                Serial = responseDict["serial"].ToCharArray();
                break;
            }
        }
    }

    private int GetBatteries() {
        if (Batteries == -1) {
            List<string> data = BombInfo.QueryWidgets(KMBombInfo.QUERYKEY_GET_BATTERIES, null);
            foreach (string response in data)
            {
                Dictionary<string, int> responseDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(response);
                Batteries += responseDict["numbatteries"];
            }
        }
        return Batteries;
    }

    private bool SerialDuplicate()
    {
        SetSerialVariable();
        List<char> list = new List<char>();
        foreach (char c in Serial)
        {
            if (list.Contains(c)) return true;
            list.Add(c);
        }

        return false;
    }

    private bool CheckParity() {
        SetSerialVariable();
        bool answer = true;
        foreach (char c in Serial) {
            if (c == '1' || c == '3' || c == '5' || c == '7' || c == '9') answer = !answer;
        }
        if (GetBatteries() % 2 == 1) answer = !answer;
        return answer;
    }

    protected void NewQuestion()
    {
        if (TEST_MODE_IDX != -1) {
            CurQ = QuestionList[TEST_MODE_IDX].Value;
            Display.text = QuestionList[TEST_MODE_IDX].Key;
            if (Display.text == "SEGFAULT") segTicker = 0.11f;
            return;
        }

        int val = 0;
        for (int i = 0; i < NumTriesForAbort; i++) {
            if(HasReply) val = Random.Range(0, QuestionList.Count);
            else val = Random.Range(NumHasReply, QuestionList.Count);
            if (QuestionList[val].Key == "Abort?") break;
        }
        CurQ = QuestionList[val].Value;
        Display.text = QuestionList[val].Key;
        if (Display.text == "SEGFAULT") segTicker = 0.51f;
    }

    //Twitch Plays support

    #pragma warning disable 0414
    string TwitchHelpMessage = "Submit answers using 'submit N' or 'submit Y'.";
    #pragma warning restore 0414

    public void TwitchHandleForcedSolve() {
        forceSolve = true;
        Display.text = "";
        Debug.Log("[Answering Questions #"+thisLoggingID+"] Module forcibly solved.");
        GetComponent<KMNeedyModule>().HandlePass();
    }

    public IEnumerator ProcessTwitchCommand(string cmd) {
        cmd = cmd.ToLowerInvariant();
        if(cmd.StartsWith("submit ")) cmd = cmd.Substring(7);
        else if(cmd.StartsWith("press ")) cmd = cmd.Substring(6);
        else {
            yield return "sendtochaterror Commands must start with press or submit.";
            yield break;
        }

        KMSelectable btn;
             if(cmd.Equals("y") || cmd.Equals("yes") || cmd.Equals("t") || cmd.Equals("true")) btn = YesButton;
        else if(cmd.Equals("n") || cmd.Equals("no")  || cmd.Equals("f") || cmd.Equals("false")) btn = NoButton;
        else {
            yield return "sendtochaterror Valid answers are 'Y', 'Yes', 'N', or 'No'.";
            yield break;
        }

        if(btn == YesButton && Display.text.Equals("Abort?")) {
            //Twitch Plays sometimes has extreme strike amounts, so we avoid the strike repeater and directly detonate the bomb.

            yield return "Answering Questions";
            yield return "detonate ABORT! ABORT!!! ABOOOOOOORT!!!!!";
            yield break;
        }

        yield return "Answering Questions";
        btn.OnInteract();
        yield break;
    }
}