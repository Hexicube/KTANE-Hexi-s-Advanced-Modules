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
        if (DidHakuna || CurQ != null) YesButton.AddInteractionPunch();
        HandleResponse(true);
        return false;
    }

    protected bool HandleNo()
    {
        if (DidHakuna || CurQ != null) NoButton.AddInteractionPunch();
        HandleResponse(false);
        return false;
    }

    protected void OnNeedyActivation()
    {
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
                    while (!Exploded) Service.CauseStrike("ABORT!");
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

    protected void OnTimerExpired()
    {
        if (CurQ == null && !DidHakuna) return;
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
}