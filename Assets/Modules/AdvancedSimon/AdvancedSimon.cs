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

public class AdvancedSimon : MonoBehaviour
{
    public KMSelectable ButtonTL, ButtonTR, ButtonBL, ButtonBR;
    public KMAudio Sound;
    public KMBombInfo Info;

    private static Color     RED = new Color(1, 0, 0),         YELLOW = new Color(0.9f, 0.9f, 0),         GREEN = new Color(0, 0.9f, 0),       BLUE = new Color(0.25f, 0.25f, 1.5f),
                         DARKRED = new Color(0.25f, 0, 0), DARKYELLOW = new Color(0.225f, 0.225f, 0), DARKGREEN = new Color(0, 0.225f, 0), DARKBLUE = new Color(0.0625f, 0.0625f, 0.325f);
    private KMSelectable ButtonRed, ButtonYellow, ButtonGreen, ButtonBlue;

    private static int[][] PRIORITY = new int[][]{
        new int[]{ 0, 3, 2, 1 }, //R-RBGY
        new int[]{ 3, 1, 0, 2 }, //Y-BYRG
        new int[]{ 2, 0, 1, 3 }, //G-GRYB
        new int[]{ 1, 2, 3, 0 }  //B-YGBR
    };

    private int PuzzleType;

    private bool[][] PuzzleDisplay;
    private int DisplayPos;

    private int[] Answer;
    private int Progress, SubProgress;

    private bool soundActive = false;

    void Awake()
    {
        transform.Find("Background").GetComponent<MeshRenderer>().material.color = new Color(1, 0.1f, 0.1f);
        ButtonTL.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);
        ButtonTR.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);
        ButtonBL.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);
        ButtonBR.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);

        GetComponent<KMBombModule>().OnActivate += Init;
    }

    void Init()
    {
        List<KMSelectable> buttons = new List<KMSelectable>() { ButtonTL, ButtonTR, ButtonBL, ButtonBR };
        int i = 0;
        int TLtype = -1;
        while (buttons.Count > 0)
        {
            int pos = Random.Range(0, buttons.Count);
            if (pos == 0 && TLtype == -1) TLtype = i;
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
                    Debug.Log("Stage 1: One flashed, press it (" + Answer[0] + ")");
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
                        Debug.Log("Stage 1: Two flashed with blue, press highest (" + Answer[0] + ")");
                    }
                    else
                    {
                        Answer[0] = 3;
                        Debug.Log("Stage 1: Two flashed without blue, press blue (3)");
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
                        Debug.Log("Stage 1: Three flashed including red, press lowest (" + Answer[0] + ")");
                    }
                    else
                    {
                        Answer[0] = 0;
                        Debug.Log("Stage 1: Three flashed excluding red, press red (0)");
                    }
                }
                else
                {
                    Answer[0] = PRIORITY[TLtype][1];
                    Debug.Log("Stage 1: Four flashed, press second highest (" + Answer[0] + ")");
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
                        Debug.Log("Stage 2: Red and blue flashed, press highest out of yellow and green (" + Answer[1] + ")");
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
                        Debug.Log("Stage 2: Two flashed including yellow or green, press lowest that didn't flash (" + Answer[1] + ")");
                    }
                }
                else if (numFlashed == 1)
                {
                    if (!PuzzleDisplay[1][3])
                    {
                        Answer[1] = 3;
                        Debug.Log("Stage 2: One flashed but not blue, press blue (3)");
                    }
                    else
                    {
                        Answer[1] = 1;
                        Debug.Log("Stage 2: Blue flashed, press yellow (1)");
                    }
                }
                else if (numFlashed == 4)
                {
                    Answer[1] = Answer[0];
                    Debug.Log("Stage 2: Four flashed, press stage 1 (" + Answer[1] + ")");
                }
                else
                {
                    if (!PuzzleDisplay[1][0]) Answer[1] = 0;
                    else if (!PuzzleDisplay[1][1]) Answer[1] = 1;
                    else if (!PuzzleDisplay[1][2]) Answer[1] = 2;
                    else Answer[1] = 3;
                    Debug.Log("Stage 2: Three flashed, press whatever didn't flash (" + Answer[1] + ")");
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
                        Debug.Log("Stage 3: Three flashed and one was pressed, press highest unpressed that flashed (" + Answer[2] + ")");
                    }
                    else
                    {
                        for (int z = 0; z < 4; z++)
                        {
                            if (!PuzzleDisplay[2][PRIORITY[TLtype][z]])
                            {
                                Answer[2] = PRIORITY[TLtype][z];
                                break;
                            }
                        }
                        Debug.Log("Stage 3: Three flashed and weren't pressed, press other (" + Answer[2] + ")");
                    }
                }
                else if (numFlashed == 2)
                {
                    if ((PuzzleDisplay[2][0] && !R) || (PuzzleDisplay[2][1] && !Y) ||
                        (PuzzleDisplay[2][2] && !G) || (PuzzleDisplay[2][3] && !B))
                    {
                        Answer[2] = Answer[0];
                        Debug.Log("Stage 3: Two flashed and at least one unpressed, press stage 1 (" + Answer[2] + ")");
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
                        Debug.Log("Stage 3: Two flashed and both pressed, press lowest no-flash (" + Answer[2] + ")");
                    }
                }
                else if (numFlashed == 1)
                {
                    if (PuzzleDisplay[2][0]) Answer[2] = 0;
                    else if (PuzzleDisplay[2][1]) Answer[2] = 1;
                    else if (PuzzleDisplay[2][2]) Answer[2] = 2;
                    else Answer[2] = 3;
                    Debug.Log("Stage 3: One flashed, press it (" + Answer[2] + ")");
                }
                else
                {
                    Answer[2] = PRIORITY[TLtype][2];
                    Debug.Log("Stage 3: Four flashed, press second lowest (" + Answer[2] + ")");
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
                    Debug.Log("Stage 4: Three unique pressed, press other (" + Answer[3] + ")");
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
                        Debug.Log("Stage 4: Three flashed and exactly one unpressed, press it (" + Answer[3] + ")");
                    }
                    else
                    {
                        Answer[3] = PRIORITY[TLtype][3];
                        Debug.Log("Stage 4: Three flashed and not exactly one unpressed, press lowest (" + Answer[3] + ")");
                    }
                }
                else if (numFlashed == 4)
                {
                    Answer[3] = PRIORITY[TLtype][3];
                    Debug.Log("Stage 4: Four flashed, press lowest (" + Answer[3] + ")");
                }
                else if (numFlashed == 1)
                {
                    if (PuzzleDisplay[3][0]) Answer[3] = 0;
                    else if (PuzzleDisplay[3][1]) Answer[3] = 1;
                    else if (PuzzleDisplay[3][2]) Answer[3] = 2;
                    else Answer[3] = 3;
                    Debug.Log("Stage 4: One flashed, press it (" + Answer[3] + ")");
                }
                else
                {
                    Answer[3] = 2;
                    Debug.Log("Stage 4: Two flashed, press green (2)");
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
    void FixedUpdate()
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
            if (DisplayPos >= 0)
            {
                string tone = "";
                if (PuzzleDisplay[DisplayPos][0])
                {
                    ButtonRed.GetComponent<MeshRenderer>().material.color = RED;
                    tone += "R";
                }
                if (PuzzleDisplay[DisplayPos][1])
                {
                    ButtonYellow.GetComponent<MeshRenderer>().material.color = YELLOW;
                    tone += "Y";
                }
                if (PuzzleDisplay[DisplayPos][2])
                {
                    ButtonGreen.GetComponent<MeshRenderer>().material.color = GREEN;
                    tone += "G";
                }
                if (PuzzleDisplay[DisplayPos][3])
                {
                    ButtonBlue.GetComponent<MeshRenderer>().material.color = BLUE;
                    tone += "B";
                }
                if (soundActive) Sound.PlaySoundAtTransform(tone, transform);
            }
        }
        else if (ticker == 15)
        {
            if (DisplayPos >= 0)
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
                    GetComponent<KMBombModule>().HandlePass();
                    PuzzleDisplay = null;
                }
                /*else
                {
                    ButtonRed.OnInteract = null;
                    ButtonYellow.OnInteract = null;
                    ButtonGreen.OnInteract = null;
                    ButtonBlue.OnInteract = null;
                    List<KMSelectable> buttons = new List<KMSelectable>() { ButtonTL, ButtonTR, ButtonBL, ButtonBR };
                    int i = 0;
                    while (buttons.Count > 0)
                    {
                        int pos = Random.Range(0, buttons.Count);
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
                }*/
            }
            else SubProgress++;
        }
        else
        {
            GetComponent<KMBombModule>().HandleStrike();
            SubProgress = 0;
        }

        pressTicker = 15;
        //TODO: play tone
        if (val == 0)
        {
            ButtonRed.GetComponent<MeshRenderer>().material.color = RED;
            Sound.PlaySoundAtTransform("R", transform);
        }
        else if (val == 1)
        {
            ButtonYellow.GetComponent<MeshRenderer>().material.color = YELLOW;
            Sound.PlaySoundAtTransform("Y", transform);
        }
        else if (val == 2)
        {
            ButtonGreen.GetComponent<MeshRenderer>().material.color = GREEN;
            Sound.PlaySoundAtTransform("G", transform);
        }
        else
        {
            ButtonBlue.GetComponent<MeshRenderer>().material.color = BLUE;
            Sound.PlaySoundAtTransform("B", transform);
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
}