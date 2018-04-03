/*

-- On the Subject of Answering Questions --
- I hope you studied, it's quiz night! -

Respond to the computer prompts by pressing "Y" for "Yes" or "N" for "No".

*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class AdvancedVentingGas : MonoBehaviour
{
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

    protected bool DidHakuna = false;
    protected bool Exploded = false;

    protected bool abortMode = false;
    protected bool forceSolve = false;

    private List<KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>> QuestionList = new List<KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>>(){
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("What was your\nprevious answer?", delegate(AdvancedVentingGas Module, bool Response){if (Module.HasReply) return Response == Module.LastReply;return true;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("What was\nnot your\nprevious answer?", delegate(AdvancedVentingGas Module, bool Response){if (Module.HasReply) return Response == !Module.LastReply;return true;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Does this\nquestion contain\n3 lines?", delegate(AdvancedVentingGas Module, bool Response){return Response;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Does this\nquestion contain\n6 lines?", delegate(AdvancedVentingGas Module, bool Response){return !Response;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Strikes?", delegate(AdvancedVentingGas Module, bool Response){return (Module.BombInfo.GetStrikes() > 0) == Response;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Many strikes?", delegate(AdvancedVentingGas Module, bool Response){return (Module.BombInfo.GetStrikes() > 1) == Response;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Abort?", delegate(AdvancedVentingGas Module, bool Response){return !Response;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Is \"Hakuna Matata\"\na wonderful phrase?", delegate(AdvancedVentingGas Module, bool Response){Module.DidHakuna = true;return Response;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Are you a\ndirty cheater?", delegate(AdvancedVentingGas Module, bool Response){return !Response;})},
        {new KeyValuePair<string, System.Func<AdvancedVentingGas, bool, bool>>("Does the\nserial contain\nduplicate\ncharacters?", delegate(AdvancedVentingGas Module, bool Response){return Response == Module.SerialDuplicate();})}
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

        YesButton.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
        NoButton.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
    }

    protected bool HandleYes()
    {
        if(abortMode) return false;
        if (DidHakuna || CurQ != null) YesButton.AddInteractionPunch();
        HandleResponse(true);
        return false;
    }

    protected bool HandleNo()
    {
        if(abortMode) return false;
        if (DidHakuna || CurQ != null) NoButton.AddInteractionPunch();
        HandleResponse(false);
        return false;
    }

    protected void OnNeedyActivation()
    {
        if(forceSolve) {
            GetComponent<KMNeedyModule>().HandlePass();
            return;
        }
        if (DidHakuna) Display.text = "Is \"Hakuna Matata\"\na passing craze?";
        else NewQuestion();
    }

    protected void OnNeedyDeactivation()
    {
        CurQ = null;
        Display.text = "";
    }

    protected void HandleResponse(bool R)
    {
        if (DidHakuna && !Display.text.Equals(""))
        {
            Debug.Log("[Answering Questions #"+thisLoggingID+"] Quiz: Is \"Hakuna Matata\" a passing craze?");
            Debug.Log("[Answering Questions #"+thisLoggingID+"] Given answer: " + (R ? "Y" : "N"));
            DidHakuna = false;
            if (R)
            {
                Debug.Log("[Answering Questions #"+thisLoggingID+"] Answer was incorrect");
                GetComponent<KMNeedyModule>().HandleStrike();
            }
            else
            {
                Debug.Log("[Answering Questions #"+thisLoggingID+"] Answer was correct");
                Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
            }
            GetComponent<KMNeedyModule>().HandlePass();
        }
        else
        {
            if (CurQ == null) return;
            GetComponent<KMNeedyModule>().HandlePass();
            Debug.Log("[Answering Questions #"+thisLoggingID+"] Quiz: " + Display.text.Replace("\n", ""));
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
                    abortMode = true;
                }
                else
                {
                    Debug.Log("[Answering Questions #"+thisLoggingID+"] Answer was incorrect");
                    GetComponent<KMNeedyModule>().HandleStrike();
                }
            }
        }
        HasReply = true;
        LastReply = R;
        CurQ = null;
        Display.text = "";
    }

    private float ticker = 0f;
    void FixedUpdate() {
        if(!forceSolve && abortMode && !Exploded) {
            bool state = ticker >= 0.25f;
            ticker += Time.fixedDeltaTime;
            if(state) {
                if(ticker >= 0.5f) {
                    ticker -= 0.5f;
                    Display.text = "";
                }
            }
            else {
                if(ticker >= 0.25f) {
                    Service.CauseStrike("ABORT!");
                    Display.text = "ABORT!";
                }
            }
        }
    }

    protected void OnTimerExpired()
    {
        if (forceSolve || (CurQ == null && !DidHakuna)) return;
        DidHakuna = false;
        GetComponent<KMNeedyModule>().HandleStrike();
    }

    private char[] Serial = null;

    private bool SerialDuplicate()
    {
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

        List<char> list = new List<char>();
        foreach (char c in Serial)
        {
            if (list.Contains(c)) return true;
            list.Add(c);
        }

        return false;
    }

    protected void NewQuestion()
    {
        int val;
        if(HasReply) val = Random.Range(0, 10);
        else val = Random.Range(2, 10);
        CurQ = QuestionList[val].Value;
        Display.text = QuestionList[val].Key;
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