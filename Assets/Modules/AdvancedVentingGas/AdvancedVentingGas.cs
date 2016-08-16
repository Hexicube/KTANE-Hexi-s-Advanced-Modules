/*

-- On the Subject of Answering Questions --
- I hope you studied, it's quiz night! -

Respond to the computer prompts by pressing "Y" for "Yes" or "N" for "No".

What was your previous answer? (previous)
What was not your previous answer? (not previous)
Are you experiencing increased levels of soduim chloride? (yes)
If a tree falls in the forest with nobody around, does it matter? (no)
Does is time fast? (yes is strikes, no otherwise)
Does is time super fast? (yes if 2 strikes, no otherwise)
Does it take two to tango? (yes)
Abort? (no)
Is that the way the news goes? (yes)
Is "sweet freedom" a wonderful phrase? (no)

*/

using UnityEngine;
using System.Collections;

public class AdvancedVentingGas : MonoBehaviour
{
    public KMBombInfo BombInfo;
    public KMAudio Sound;

    public KMSelectable YesButton;
    public KMSelectable NoButton;
    public TextMesh Display;

    protected bool LastReply;
    protected bool HasReply;
    protected int CurQ = -1;

    void Awake()
    {
        GetComponent<KMNeedyModule>().OnNeedyActivation += OnNeedyActivation;
        GetComponent<KMNeedyModule>().OnNeedyDeactivation += OnNeedyDeactivation;
        YesButton.OnInteract += HandleYes;
        NoButton.OnInteract += HandleNo;
        GetComponent<KMNeedyModule>().OnTimerExpired += OnTimerExpired;
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
        NewQuestion();
        Display.text = GetText();
    }

    protected void OnNeedyDeactivation()
    {
        CurQ = -1;
        Display.text = "";
    }

    protected void HandleResponse(bool R)
    {
        if (CurQ == -1) return;
        if (GetCorrect(R))
        {
            GetComponent<KMNeedyModule>().HandlePass();
            Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
        }
        else
        {
            GetComponent<KMNeedyModule>().HandleStrike();
        }
        HasReply = true;
        LastReply = R;
        CurQ = -1;
        Display.text = "";
    }

    protected void OnTimerExpired()
    {
        if (CurQ == -1) return;
        GetComponent<KMNeedyModule>().HandleStrike();
    }

    protected bool GetCorrect(bool Response)
    {
        switch(CurQ)
        {
            case(0):
                if (HasReply) return Response == LastReply;
                return true;
            case(1):
                if (HasReply) return Response != LastReply;
                return true;
            case(2):
                return Response;
            case(3):
                return !Response;
            case(4):
                return (BombInfo.GetStrikes() > 0) == Response;
            case (5):
                return (BombInfo.GetStrikes() > 1) == Response;
            case(6):
                return !Response;
            case(7):
                return Response;
            case(8):
                return !Response;
            default:
                return true;
        }
    }

    protected string GetText()
    {
        switch(CurQ)
        {
            case(0):
                return "What was\nyour previous answer?";
            case(1):
                return "What was not\nyour previous answer?";
            case(2):
                return "Does this\nquestion\ncontain\n4 lines?";
            case(3):
                return "Does this\nquestion contain\n6 lines?";
            case(4):
                return "Strikes?";
            case(5):
                return "Many strikes?";
            case(6):
                return "Abort?";
            case(7):
                return "Is \"Hakuna Matata\"\na wonderful phrase?";
            case(8):
                return "Are you a\ndirty cheater?";
            default:
                return "";
        }
    }

    protected void NewQuestion()
    {
        if(HasReply) CurQ = Random.Range(0, 9);
        else CurQ = Random.Range(2, 9);
    }
}