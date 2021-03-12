/*

-- On the Subject of Simon Stated --
- I'm not sure this even qualifies as Simon Says... -

One or more colours will flash.
Using the tables below, press the correct response.
The sequence will extend by 1 for each correct answer.
Keep answering until the module is disarmed.

Stage 1:
- If one colour flashed, press that colour.
- Otherwise, if two colours flashed and one was blue, press the highest priority colour that flashed.
- Otherwise, if two colours flashed, press blue.
- Otherwise, if three colours flashed including red, press the lowest priority colour that flashed.
- Otherwise, if three colours flashed, press red.
- Otherwise, press the second highest priority colour.

Stage 2:
- If red and blue flashed, press the highest priority colour that didn't flash.
- Otherwise, if two colours flashed, press the lowest priority colour that flashed.
- Otherwise, if one colour flashed and it was not blue, press blue.
- Otherwise, if one colour flashed, press yellow.
- Otherwise, if all colours flashed, press the same colour as stage 1.
- Otherwise, press the colour that didn't flash.

Stage 3:
- If three colours flashed and at least one was pressed in a previous stage, press the highest priority colour that flashed that hasn't been pressed.
- Otherwise, if three colours flashed, press the highest priority colour that flashed.
- Otherwise, if two colours flashed and both have been pressed, press the lowest priority colour that didn't flash.
- Otherwise, if two colours flashed, press the same colour as stage 1.
- Otherwise, if one colour flashed, press that colour.
- Otherwise, press the second lowest priority colour.

Stage 4:
- If three unique colours have been pressed, press the fourth colour.
- Otherwise, if three colours flashed and exactly one hasn't been pressed, press that colour.
- Otherwise, if at least three colours flashed, press the lowest priority colour.
- Otherwise, if one colour flashed, press that colour.
- Otherwise, press green.

Stage 5:
- If three unique colours have been pressed, press the fourth colour.
- Otherwise, if two colours flashed and one was blue, press the other colour.
- Otherwise, if at two or three colours flashed and one was green, press yellow.
- Otherwise, if all colours that flashed have been pressed, press red.
- Otherwise, if four colours flashed and at least one colour hasn't been pressed, press the lowest priority colour that hasn't been pressed.
- Otherwise, press blue.

Stage 6:
- If three unique colours have been pressed, press the fourth colour.
- Otherwise, if two colours flashed and both have been pressed, press green.
- Otherwise, if three colours flashed and at least one hasn't been pressed, press the lowest priority colour.
- Otherwise, if one colour has been pressed more than all other colours, press that colour.
- Otherwise, press the highest priority colour.

*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AdvancedSimon : FixedTicker
{
    public static int loggingID = 1;
    public int thisLoggingID;

    public KMSelectable ButtonTL, ButtonTR, ButtonBL, ButtonBR;
    public KMAudio Sound;
    public KMBombInfo Info;

    private static Color     RED = new Color(1f,    0, 0, 0.4f),  YELLOW = new Color(0.8f,  0.8f,  0, 0.4f),  GREEN = new Color(0, 0.7f,  0, 0.4f),  BLUE = new Color(0, 0.3f, 1f,    0.4f),
                         DARKRED = new Color(0.25f, 0, 0, 0), DARKYELLOW = new Color(0.27f, 0.27f, 0, 0), DARKGREEN = new Color(0, 0.23f, 0, 0), DARKBLUE = new Color(0, 0.1f, 0.33f, 0);
    private KMSelectable ButtonRed, ButtonYellow, ButtonGreen, ButtonBlue;

    private static int[][] PRIORITY = new int[][]{
        new int[]{ 0, 3, 2, 1 }, //R-RBGY
        new int[]{ 3, 1, 0, 2 }, //Y-BYRG
        new int[]{ 2, 0, 1, 3 }, //G-GRYB
        new int[]{ 1, 2, 3, 0 }  //B-YGBR
    };

    private static string[] COL_LIST = new string[]{"R", "Y", "G", "B"};

    private int PuzzleType;

    private bool[][] PuzzleDisplay;
    private int DisplayPos;

    private int[] Answer;
    private int Progress, SubProgress;

    private bool soundActive = false;

    void Awake()
    {
        thisLoggingID = loggingID++;

        //transform.Find("Background").GetComponent<MeshRenderer>().material.color = new Color(1, 0.1f, 0.1f);

        ButtonTL.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);
        ButtonTR.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);
        ButtonBL.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);
        ButtonBR.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);

        GetComponent<KMBombModule>().OnActivate += Init;
    }

    void Init()
    {
        ButtonTL.OnInteract += delegate() { ButtonTL.AddInteractionPunch(); return false; };
        ButtonTR.OnInteract += delegate() { ButtonTR.AddInteractionPunch(); return false; };
        ButtonBL.OnInteract += delegate() { ButtonBL.AddInteractionPunch(); return false; };
        ButtonBR.OnInteract += delegate() { ButtonBR.AddInteractionPunch(); return false; };

        List<KMSelectable> buttons = new List<KMSelectable>() { ButtonTL, ButtonTR, ButtonBL, ButtonBR };
        int i = 0;
        int TLtype = -1;
        while (buttons.Count > 0)
        {
            int pos = Random.Range(0, buttons.Count);
            if (pos == 0 && TLtype == -1)
            {
                Debug.Log("[Simon States #"+thisLoggingID+"] Dominant: "+COL_LIST[i]);
                TLtype = i;
            }
            KMSelectable b = buttons[pos];
            if (i == 0)
            {
                ButtonRed = b;
                b.GetComponent<MeshRenderer>().material.color = DARKRED;
                b.OnInteract += HandleRed;
            }
            else if (i == 1)
            {
                ButtonYellow = b;
                b.GetComponent<MeshRenderer>().material.color = DARKYELLOW;
                b.OnInteract += HandleYellow;
            }
            else if (i == 2)
            {
                ButtonGreen = b;
                b.GetComponent<MeshRenderer>().material.color = DARKGREEN;
                b.OnInteract += HandleGreen;
            }
            else
            {
                ButtonBlue = b;
                b.GetComponent<MeshRenderer>().material.color = DARKBLUE;
                b.OnInteract += HandleBlue;
            }
            i++;
            buttons.RemoveAt(pos);
        }

        int len = 4;
        PuzzleDisplay = new bool[len][];
        for (int a = 0; a < len; a++)
        {
            PuzzleDisplay[a] = new bool[4];
            int num = Random.Range(1, 5);
            if (num > 2) num = Random.Range(2, 5);
            List<int> posList = new List<int>() { 0, 1, 2, 3 };
            while (num > 0)
            {
                num--;
                int pos = Random.Range(0, posList.Count);
                PuzzleDisplay[a][posList[pos]] = true;
                posList.RemoveAt(pos);
            }
            string col = "";
            if(PuzzleDisplay[a][0]) col += COL_LIST[0]; else col += "-";
            if(PuzzleDisplay[a][1]) col += COL_LIST[1]; else col += "-";
            if(PuzzleDisplay[a][2]) col += COL_LIST[2]; else col += "-";
            if(PuzzleDisplay[a][3]) col += COL_LIST[3]; else col += "-";
            Debug.Log("[Simon States #"+thisLoggingID+"] Stage "+(a+1)+" colours: "+col);
        }

        Answer = new int[len];

        bool R = false, Y = false, B = false, G = false;
        for (int a = 0; a < len; a++)
        {
            int numFlashed = 0;
            for (int z = 0; z < 4; z++)
            {
                if (PuzzleDisplay[a][z]) numFlashed++;
            }
            int numUniquePressed = 0;
            if (R) numUniquePressed++;
            if (Y) numUniquePressed++;
            if (G) numUniquePressed++;
            if (B) numUniquePressed++;
            if (a == 0)
            {
                if (numFlashed == 1)
                {
                    if (PuzzleDisplay[0][0]) Answer[0] = 0;
                    else if (PuzzleDisplay[0][1]) Answer[0] = 1;
                    else if (PuzzleDisplay[0][2]) Answer[0] = 2;
                    else Answer[0] = 3;
                    Debug.Log("[Simon States #"+thisLoggingID+"] Stage 1: One flashed, press it (" + Answer[0] + ":" + COL_LIST[Answer[0]] + ")");
                }
                else if (numFlashed == 2)
                {
                    if (PuzzleDisplay[0][3])
                    {
                        for (int z = 0; z < 4; z++)
                        {
                            if (PuzzleDisplay[0][PRIORITY[TLtype][z]])
                            {
                                Answer[0] = PRIORITY[TLtype][z];
                                break;
                            }
                        }
                        Debug.Log("[Simon States #"+thisLoggingID+"] Stage 1: Two flashed with blue, press highest (" + Answer[0] + ":" + COL_LIST[Answer[0]] + ")");
                    }
                    else
                    {
                        Answer[0] = 3;
                        Debug.Log("[Simon States #"+thisLoggingID+"] Stage 1: Two flashed without blue, press blue (3:B)");
                    }
                }
                else if (numFlashed == 3)
                {
                    if (PuzzleDisplay[0][0])
                    {
                        for (int z = 3; z >= 0; z--)
                        {
                            if (PuzzleDisplay[0][PRIORITY[TLtype][z]])
                            {
                                Answer[0] = PRIORITY[TLtype][z];
                                break;
                            }
                        }
                        Debug.Log("[Simon States #"+thisLoggingID+"] Stage 1: Three flashed including red, press lowest (" + Answer[0] + ":" + COL_LIST[Answer[0]] + ")");
                    }
                    else
                    {
                        Answer[0] = 0;
                        Debug.Log("[Simon States #"+thisLoggingID+"] Stage 1: Three flashed excluding red, press red (0:R)");
                    }
                }
                else
                {
                    Answer[0] = PRIORITY[TLtype][1];
                    Debug.Log("[Simon States #"+thisLoggingID+"] Stage 1: Four flashed, press second highest (" + Answer[0] + ":" + COL_LIST[Answer[0]] + ")");
                }
            }
            else if (a == 1)
            {
                if (numFlashed == 2)
                {
                    if (PuzzleDisplay[1][0] && PuzzleDisplay[1][3])
                    {
                        for (int z = 0; z < 4; z++)
                        {
                            if (!PuzzleDisplay[1][PRIORITY[TLtype][z]])
                            {
                                Answer[1] = PRIORITY[TLtype][z];
                                break;
                            }
                        }
                        Debug.Log("[Simon States #"+thisLoggingID+"] Stage 2: Red and blue flashed, press highest out of yellow and green (" + Answer[1] + ":" + COL_LIST[Answer[1]] + ")");
                    }
                    else
                    {
                        for (int z = 3; z >= 0; z--)
                        {
                            if (!PuzzleDisplay[1][PRIORITY[TLtype][z]])
                            {
                                Answer[1] = PRIORITY[TLtype][z];
                                break;
                            }
                        }
                        Debug.Log("[Simon States #"+thisLoggingID+"] Stage 2: Two flashed including yellow or green, press lowest that didn't flash (" + Answer[1] + ":" + COL_LIST[Answer[1]] + ")");
                    }
                }
                else if (numFlashed == 1)
                {
                    if (!PuzzleDisplay[1][3])
                    {
                        Answer[1] = 3;
                        Debug.Log("[Simon States #"+thisLoggingID+"] Stage 2: One flashed but not blue, press blue (3:B)");
                    }
                    else
                    {
                        Answer[1] = 1;
                        Debug.Log("[Simon States #"+thisLoggingID+"] Stage 2: Blue flashed, press yellow (1:Y)");
                    }
                }
                else if (numFlashed == 4)
                {
                    Answer[1] = Answer[0];
                    Debug.Log("[Simon States #"+thisLoggingID+"] Stage 2: Four flashed, press stage 1 (" + Answer[1] + ":" + COL_LIST[Answer[1]] + ")");
                }
                else
                {
                    if (!PuzzleDisplay[1][0]) Answer[1] = 0;
                    else if (!PuzzleDisplay[1][1]) Answer[1] = 1;
                    else if (!PuzzleDisplay[1][2]) Answer[1] = 2;
                    else Answer[1] = 3;
                    Debug.Log("[Simon States #"+thisLoggingID+"] Stage 2: Three flashed, press whatever didn't flash (" + Answer[1] + ":" + COL_LIST[Answer[1]] + ")");
                }
            }
            else if (a == 2)
            {
                if (numFlashed == 3)
                {
                    if ((PuzzleDisplay[2][0] && R) || (PuzzleDisplay[2][1] && Y) ||
                        (PuzzleDisplay[2][2] && G) || (PuzzleDisplay[2][3] && B))
                    {
                        for (int z = 0; z < 4; z++)
                        {
                            int trueVal = PRIORITY[TLtype][z];
                            if (PuzzleDisplay[2][trueVal])
                            {
                                if (trueVal == 0 && !R)
                                {
                                    Answer[2] = 0;
                                    break;
                                }
                                else if (trueVal == 1 && !Y)
                                {
                                    Answer[2] = 1;
                                    break;
                                }
                                else if (trueVal == 2 && !G)
                                {
                                    Answer[2] = 2;
                                    break;
                                }
                                else if (trueVal == 3 && !B)
                                {
                                    Answer[2] = 3;
                                    break;
                                }
                            }
                        }
                        Debug.Log("[Simon States #"+thisLoggingID+"] Stage 3: Three flashed and one was pressed, press highest unpressed that flashed (" + Answer[2] + ":" + COL_LIST[Answer[2]] + ")");
                    }
                    else
                    {
                        for (int z = 0; z < 4; z++)
                        {
                            if (PuzzleDisplay[2][PRIORITY[TLtype][z]])
                            {
                                Answer[2] = PRIORITY[TLtype][z];
                                break;
                            }
                        }
                        Debug.Log("[Simon States #"+thisLoggingID+"] Stage 3: Three flashed and weren't pressed, press highest that flashes (" + Answer[2] + ":" + COL_LIST[Answer[2]] + ")");
                    }
                }
                else if (numFlashed == 2)
                {
                    if ((PuzzleDisplay[2][0] && !R) || (PuzzleDisplay[2][1] && !Y) ||
                        (PuzzleDisplay[2][2] && !G) || (PuzzleDisplay[2][3] && !B))
                    {
                        Answer[2] = Answer[0];
                        Debug.Log("[Simon States #"+thisLoggingID+"] Stage 3: Two flashed and at least one unpressed, press stage 1 (" + Answer[2] + ":" + COL_LIST[Answer[2]] + ")");
                    }
                    else
                    {
                        for (int z = 3; z >= 0; z--)
                        {
                            if (!PuzzleDisplay[2][PRIORITY[TLtype][z]])
                            {
                                Answer[2] = PRIORITY[TLtype][z];
                                break;
                            }
                        }
                        Debug.Log("[Simon States #"+thisLoggingID+"] Stage 3: Two flashed and both pressed, press lowest no-flash (" + Answer[2] + ":" + COL_LIST[Answer[2]] + ")");
                    }
                }
                else if (numFlashed == 1)
                {
                    if (PuzzleDisplay[2][0]) Answer[2] = 0;
                    else if (PuzzleDisplay[2][1]) Answer[2] = 1;
                    else if (PuzzleDisplay[2][2]) Answer[2] = 2;
                    else Answer[2] = 3;
                    Debug.Log("[Simon States #"+thisLoggingID+"] Stage 3: One flashed, press it (" + Answer[2] + ":" + COL_LIST[Answer[2]] + ")");
                }
                else
                {
                    Answer[2] = PRIORITY[TLtype][2];
                    Debug.Log("[Simon States #"+thisLoggingID+"] Stage 3: Four flashed, press second lowest (" + Answer[2] + ":" + COL_LIST[Answer[2]] + ")");
                }
            }
            else if (a == 3)
            {
                if (numUniquePressed == 3)
                {
                    if (!R) Answer[3] = 0;
                    else if (!Y) Answer[3] = 1;
                    else if (!G) Answer[3] = 2;
                    else Answer[3] = 3;
                    Debug.Log("[Simon States #"+thisLoggingID+"] Stage 4: Three unique pressed, press other (" + Answer[3] + ":" + COL_LIST[Answer[3]] + ")");
                }
                else if (numFlashed == 3)
                {
                    int unpressed = 4;
                    if (!R && PuzzleDisplay[3][0]) unpressed = 0;
                    if (!Y && PuzzleDisplay[3][1])
                    {
                        if (unpressed == 4) unpressed = 1;
                        else unpressed = -1;
                    }
                    if (unpressed != -1 && !G && PuzzleDisplay[3][2])
                    {
                        if (unpressed == 4) unpressed = 2;
                        else unpressed = -1;
                    }
                    if (unpressed != -1 && !B && PuzzleDisplay[3][3])
                    {
                        if (unpressed == 4) unpressed = 3;
                        else unpressed = -1;
                    }
                    if (unpressed >= 0 && unpressed < 4)
                    {
                        Answer[3] = unpressed;
                        Debug.Log("[Simon States #"+thisLoggingID+"] Stage 4: Three flashed and exactly one unpressed, press it (" + Answer[3] + ":" + COL_LIST[Answer[3]] + ")");
                    }
                    else
                    {
                        Answer[3] = PRIORITY[TLtype][3];
                        Debug.Log("[Simon States #"+thisLoggingID+"] Stage 4: Three flashed and not exactly one unpressed, press lowest (" + Answer[3] + ":" + COL_LIST[Answer[3]] + ")");
                    }
                }
                else if (numFlashed == 4)
                {
                    Answer[3] = PRIORITY[TLtype][3];
                    Debug.Log("[Simon States #"+thisLoggingID+"] Stage 4: Four flashed, press lowest (" + Answer[3] + ":" + COL_LIST[Answer[3]] + ")");
                }
                else if (numFlashed == 1)
                {
                    if (PuzzleDisplay[3][0]) Answer[3] = 0;
                    else if (PuzzleDisplay[3][1]) Answer[3] = 1;
                    else if (PuzzleDisplay[3][2]) Answer[3] = 2;
                    else Answer[3] = 3;
                    Debug.Log("[Simon States #"+thisLoggingID+"] Stage 4: One flashed, press it (" + Answer[3] + ":" + COL_LIST[Answer[3]] + ")");
                }
                else
                {
                    Answer[3] = 2;
                    Debug.Log("[Simon States #"+thisLoggingID+"] Stage 4: Two flashed, press green (2:G)");
                }
            }
            if (Answer[a] == 0) R = true;
            if (Answer[a] == 1) Y = true;
            if (Answer[a] == 2) G = true;
            if (Answer[a] == 3) B = true;
        }
    }

    private int ticker = 0;
    private int pressTicker = 0;
    public override void RealFixedTick()
    {
        if (pressTicker > 0)
        {
            pressTicker--;
            if (pressTicker == 0)
            {
                ButtonRed.GetComponent<MeshRenderer>().material.color = DARKRED;
                ButtonYellow.GetComponent<MeshRenderer>().material.color = DARKYELLOW;
                ButtonGreen.GetComponent<MeshRenderer>().material.color = DARKGREEN;
                ButtonBlue.GetComponent<MeshRenderer>().material.color = DARKBLUE;
            }
        }

        if (PuzzleDisplay == null) return;
        if (ticker == 0)
        {
            if(DisplayPos >= 0)
            {
                string tone = "";
                if(PuzzleDisplay[DisplayPos][0]) {
                    ButtonRed.GetComponent<MeshRenderer>().material.color = RED;
                    tone += "R";
                }
                if(PuzzleDisplay[DisplayPos][1]) {
                    ButtonYellow.GetComponent<MeshRenderer>().material.color = YELLOW;
                    tone += "Y";
                }
                if(PuzzleDisplay[DisplayPos][2]) {
                    ButtonGreen.GetComponent<MeshRenderer>().material.color = GREEN;
                    tone += "G";
                }
                if(PuzzleDisplay[DisplayPos][3]) {
                    ButtonBlue.GetComponent<MeshRenderer>().material.color = BLUE;
                    tone += "B";
                }
                if(soundActive) PlaySound(tone, false);
            }
        }
        else if(ticker == 15)
        {
            if(DisplayPos >= 0)
            {
                if (PuzzleDisplay[DisplayPos][0]) ButtonRed.GetComponent<MeshRenderer>().material.color = DARKRED;
                if (PuzzleDisplay[DisplayPos][1]) ButtonYellow.GetComponent<MeshRenderer>().material.color = DARKYELLOW;
                if (PuzzleDisplay[DisplayPos][2]) ButtonGreen.GetComponent<MeshRenderer>().material.color = DARKGREEN;
                if (PuzzleDisplay[DisplayPos][3]) ButtonBlue.GetComponent<MeshRenderer>().material.color = DARKBLUE;
            }
            ticker = -20;
            if (DisplayPos == Progress)
            {
                DisplayPos = -1;
                ticker = -75;
            }
            else DisplayPos++;
        }
        ticker++;
    }

    private void PlaySound(string name, bool wasAns) {
        if(bop) {
            if(name.Equals("R") || name.Equals("RY") || name.Equals("RGB")) {
                if(wasAns) Sound.PlaySoundAtTransform("PULL2", transform);
                else       Sound.PlaySoundAtTransform("PULL",  transform);
            }
            else if(name.Equals("Y") || name.Equals("YG") || name.Equals("RYB")) {
                if(wasAns) Sound.PlaySoundAtTransform("TWIST2", transform);
                else       Sound.PlaySoundAtTransform("TWIST",  transform);
            }
            else if(name.Equals("G") || name.Equals("GB") || name.Equals("RYG")) {
                if(wasAns) Sound.PlaySoundAtTransform("FLICK2", transform);
                else       Sound.PlaySoundAtTransform("FLICK",  transform);
            }
            else if(name.Equals("B") || name.Equals("RB") || name.Equals("YGB")) {
                if(wasAns) Sound.PlaySoundAtTransform("SPIN2", transform);
                else       Sound.PlaySoundAtTransform("SPIN",  transform);
            }
            else if(name.Equals("RYGB") || name.Equals("RG") || name.Equals("YB")) {
                Sound.PlaySoundAtTransform("BOP", transform);
            }
            else  {
                Debug.Log("Bop-it error with tone: " + name);
            }
        }
        else Sound.PlaySoundAtTransform(name, transform);
    }

    private void bopsolve() { Sound.PlaySoundAtTransform("PASS", transform); }

    private void Handle(int val)
    {
        if (PuzzleDisplay == null) return;

        soundActive = true;

        if (ticker >= 0 || pressTicker > 0)
        {
            ButtonRed.GetComponent<MeshRenderer>().material.color = DARKRED;
            ButtonYellow.GetComponent<MeshRenderer>().material.color = DARKYELLOW;
            ButtonGreen.GetComponent<MeshRenderer>().material.color = DARKGREEN;
            ButtonBlue.GetComponent<MeshRenderer>().material.color = DARKBLUE;
        }
        ticker = -100;
        DisplayPos = 0;

        if (val == Answer[SubProgress])
        {
            if (SubProgress == Progress)
            {
                SubProgress = 0;
                Progress++;
                ticker = -50;
                if (Progress == PuzzleDisplay.Length)
                {
                    Debug.Log("[Simon States #"+thisLoggingID+"] Module solved.");
                    GetComponent<KMBombModule>().HandlePass();
                    if(bop) Invoke("bopsolve", 0.6f);
                    PuzzleDisplay = null;
                }
                else Debug.Log("[Simon States #"+thisLoggingID+"] Stage " + Progress + " complete.");
            }
            else SubProgress++;
        }
        else
        {
            string ans = Answer[0] + "";
            string ans2 = COL_LIST[Answer[0]];
            if(Progress >= 1) {
                ans += Answer[1];
                ans2 += COL_LIST[Answer[1]];
            }
            if(Progress >= 2) {
                ans += Answer[2];
                ans2 += COL_LIST[Answer[2]];
            }
            if(Progress >= 3) {
                ans += Answer[3];
                ans2 += COL_LIST[Answer[3]];
            }
            Debug.Log("[Simon States #"+thisLoggingID+"] Expected answer: " + ans + ":" + ans2);

            ans = "";
            ans2 = "";
            for(int a = 0; a < SubProgress; a++)
            {
                ans += Answer[a];
                ans2 += COL_LIST[Answer[a]];
            }
            ans += val;
            ans2 += COL_LIST[val];
            Debug.Log("[Simon States #"+thisLoggingID+"] Given answer: " + ans + ":" + ans2);

            GetComponent<KMBombModule>().HandleStrike();
            SubProgress = 0;
        }

        pressTicker = 15;
        if (val == 0) {
            ButtonRed.GetComponent<MeshRenderer>().material.color = RED;
            PlaySound("R", true);
        }
        else if(val == 1) {
            ButtonYellow.GetComponent<MeshRenderer>().material.color = YELLOW;
            PlaySound("Y", true);
        }
        else if(val == 2) {
            ButtonGreen.GetComponent<MeshRenderer>().material.color = GREEN;
            PlaySound("G", true);
        }
        else {
            ButtonBlue.GetComponent<MeshRenderer>().material.color = BLUE;
            PlaySound("B", true);
        }
    }

    private bool HandleRed()
    {
        Handle(0);
        return false;
    }

    private bool HandleYellow()
    {
        Handle(1);
        return false;
    }

    private bool HandleGreen()
    {
        Handle(2);
        return false;
    }

    private bool HandleBlue()
    {
        Handle(3);
        return false;
    }

    //Twitch Plays support

    #pragma warning disable 0414
    string TwitchHelpMessage = "Press buttons with 'press RYB'.";
    #pragma warning restore 0414

    public IEnumerator TwitchHandleForcedSolve() {
        Debug.Log("[Simon States #"+thisLoggingID+"] Module forcibly solved.");
        List<KMSelectable> buttons = new List<KMSelectable>() { ButtonRed, ButtonYellow, ButtonGreen, ButtonBlue };
        while (PuzzleDisplay != null)
        {
            buttons[Answer[SubProgress]].OnInteract();
            yield return new WaitForSeconds(bop ? 0.4f : 0.29f);
        }
    }
    
    private bool bop = false;
    public IEnumerator ProcessTwitchCommand(string cmd) {
        cmd = cmd.ToLowerInvariant();
        if(cmd.StartsWith("press ")) cmd = cmd.Substring(6);
        else if(cmd.StartsWith("submit ")) cmd = cmd.Substring(7);
        else if(cmd.Equals("soundpack")) {
            if(bop) {
                yield return "sendtochaterror That is already on.";
                yield break;
            }
            yield return "Simon States";
            Sound.PlaySoundAtTransform("BOP", transform);
            yield return "sendtochat Pull-it! Twist-it! Bop-it!";
            bop = true;
            yield break;
        }
        else {
            yield return "sendtochaterror Commands must start with 'press'.";
            yield break;
        }

        char[] buttons = cmd.ToCharArray();
        List<KMSelectable> seq = new List<KMSelectable>();
        foreach(char c in buttons) {
            if(c == ' ' || c == ',') continue;
                 if(c == 'r') seq.Add(ButtonRed);
            else if(c == 'y') seq.Add(ButtonYellow);
            else if(c == 'g') seq.Add(ButtonGreen);
            else if(c == 'b') seq.Add(ButtonBlue);
            else {
                yield return "sendtochaterror Bad character: " + c;
                yield break;
            }
        }

        yield return "Simon States";
        foreach(KMSelectable s in seq) {
            yield return s;
            yield return new WaitForSeconds(bop?0.4f:0.29f);
            yield return s;
        }
        yield break;
    }
}