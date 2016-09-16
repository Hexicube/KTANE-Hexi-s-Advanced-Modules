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
        HandleResponse(true);
        return false;
    }

    protected bool HandleNo()
    {
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
        if (DidHakuna)
        {
            DidHakuna = false;
            if (R) GetComponent<KMNeedyModule>().HandleStrike();
            else Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
            GetComponent<KMNeedyModule>().HandlePass();
        }
        else
        {
            if (CurQ == null) return;
            GetComponent<KMNeedyModule>().HandlePass();
            if (CurQ(this, R))
            {
                Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
            }
            else
            {
                if (Display.text.Equals("Abort?"))
                {
                    while (!Exploded) Service.CauseStrike("ABORT!");
                }
                else GetComponent<KMNeedyModule>().HandleStrike();
            }
        }
        HasReply = true;
        LastReply = R;
        CurQ = null;
        Display.text = "";
    }

    protected void OnTimerExpired()
    {
        if (CurQ == null) return;
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