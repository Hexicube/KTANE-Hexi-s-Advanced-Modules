/*

-- On the Subject of Morsematics --
- Get it? Because it uses morse and maths! I'll see myself out... -

Press "Play" to receive a question.
Interpret the signal from the flashing light using the Morse Code chart.
The signal will usually be a maths question, but could also be a statement.
Maths questions will have a whole number response.
Statements will have either a "YES" or "NO" response.
Note: Do not agitate the bomb, it will beat you in a fight.

Warning: The signal will only play once, and will contain spaces.

*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AdvancedMorse : MonoBehaviour
{
    public KMSelectable ButtonPlay, ButtonDot, ButtonDash, ButtonSpace, ButtonClear, ButtonDone;
    public KMAudio Sound;
    public TextMesh DisplayArea;
    public KMBombInfo Info;

    protected int[] DisplaySequence;
    protected int DisplayProgress;
    protected bool Generated = false;

    protected int ReplyAnswer;

    protected int[] ReplySequence;
    protected int ReplyProgress;
    protected bool ReplyCorrect;

    private List<int> EnteredCharacters;

    private static Color BLACK = new Color(0, 0, 0), GREEN = new Color(0, 1, 0);
    private MeshRenderer LED;

    /*private static List<KeyValuePair<int[], bool>> QuestionAnswerList = new List<KeyValuePair<int[], bool>>()
    {
        {new KeyValuePair<int[], bool>(Morsify("ON BOMB"), true)},
        {new KeyValuePair<int[], bool>(Morsify("MODULE GREEN"), false)},
        {new KeyValuePair<int[], bool>(Morsify("FIGHT ME"), false)},
        {new KeyValuePair<int[], bool>(Morsify("2 IS TWO"), true)}
    };
    private static int[] YesResponse = Morsify("YES");
    private static int[] NoResponse = Morsify("NO");*/

    void Awake()
    {
        transform.Find("Background").GetComponent<MeshRenderer>().material.color = new Color(1, 0.1f, 0.1f);

        LED = gameObject.transform.Find("LED").GetComponent<MeshRenderer>();
        LED.material.color = BLACK;

        ButtonPlay.OnInteract += HandlePlay;
        ButtonDot.OnInteract += HandleDot;
        ButtonDash.OnInteract += HandleDash;
        ButtonSpace.OnInteract += HandleSpace;
        ButtonClear.OnInteract += HandleClear;
        ButtonDone.OnInteract += HandleDone;

        ButtonPlay.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
        ButtonDot.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
        ButtonDash.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
        ButtonSpace.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
        ButtonClear.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
        ButtonDone.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);

        DisplaySequence = new int[0];
        EnteredCharacters = new List<int>();
        DisplayArea.text = "";
    }

    private int ticker = 0;
    void FixedUpdate()
    {
        //Debug.Log(Time.deltaTime);
        if (DisplayProgress < DisplaySequence.Length)
        {
            ticker++;
            if (ticker >= 0)
            {
                int type = DisplaySequence[DisplayProgress];
                if (ticker == 0)
                {
                    if(type == -1)
                    {
                        DisplayProgress++;
                        ticker = -45;
                    }
                    else LED.material.color = GREEN;
                }
                else
                {
                    int target = 12;
                    if (type == 1) target = 30;
                    if (ticker >= target)
                    {
                        LED.material.color = BLACK;
                        ticker = -15;
                        DisplayProgress++;
                    }
                }
            }
        }
    }

    protected bool HandlePlay()
    {
        Generated = true;
        ReplyProgress = 0;
        ReplyCorrect = true;
        DisplayProgress = 0;
        ticker = -50;
        LED.material.color = BLACK;
        EnteredCharacters = new List<int>();
        DisplayArea.text = "";

        if (ReplySequence == null) GenerateMath();
            /*else
            {
                KeyValuePair<int[], bool> q = QuestionAnswerList[Random.Range(0, QuestionAnswerList.Count)];
                DisplaySequence = q.Key;
                if (q.Value) ReplySequence = YesResponse;
                else ReplySequence = NoResponse;
            }*/
        Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
        return false;
    }

    protected bool HandleDot()
    {
        AddSeq(0);
        return false;
    }

    protected bool HandleDash()
    {
        AddSeq(1);
        return false;
    }

    protected bool HandleSpace()
    {
        AddSeq(-1);
        return false;
    }

    private void AddSeq(int val)
    {
        if (Generated)
        {
            Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
            if (ReplyProgress >= ReplySequence.Length) ReplyCorrect = false;
            if (ReplyCorrect)
            {
                if (ReplySequence[ReplyProgress] == val) ReplyProgress++;
                else ReplyCorrect = false;
            }

            EnteredCharacters.Add(val);
            DisplayArea.text = DeMorsify();
        }
    }

    protected bool HandleClear()
    {
        if (!Generated) return false;

        if (Info.GetTime() >= 30f)
        {
            char[] ans = ("" + ReplyAnswer).ToCharArray();
            char[] time = Info.GetFormattedTime().ToCharArray();
            bool match = false;
            foreach (char c1 in ans)
            {
                foreach (char c2 in time)
                {
                    if (c1 == c2)
                    {
                        match = true;
                        break;
                    }
                }
                if (match) break;
            }

            if (!match) GetComponent<KMBombModule>().HandleStrike();
            else Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
        }
        else Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);

        ReplyProgress = 0;
        ReplyCorrect = true;
        EnteredCharacters = new List<int>();
        DisplayArea.text = "";
        return false;
    }

    protected bool HandleDone()
    {
        if (Generated)
        {
            if (ReplyCorrect && ReplyProgress == ReplySequence.Length)
            {
                Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
                GetComponent<KMBombModule>().HandlePass();
            }
            else
            {
                GetComponent<KMBombModule>().HandleStrike();
                ReplyProgress = 0;
                ReplyCorrect = true;
                EnteredCharacters = new List<int>();
                DisplayArea.text = "";
            }
        }
        return false;
    }

    private void GenerateMath()
    {
        int a, b, answer;
        int type = Random.Range(0, 5);
        if (type == 0)
        {
            //Multiplication
            a = Random.Range(3, 10);
            b = Random.Range(7, 14);
            answer = a * b;
        }
        else if (type == 1)
        {
            //Division
            b = Random.Range(3, 14);
            answer = Random.Range(13, 18);
            a = b * answer;
        }
        else if (type == 2)
        {
            //Modulo
            a = Random.Range(50, 151);
            b = Random.Range(7, 18);
            answer = a % b;
        }
        else if (type == 3)
        {
            //Power
            a = Random.Range(2, 6);
            b = Random.Range(2, 5);
            answer = a;
            for (int i = 1; i < b; i++) answer *= a;
        }
        else if (type == 4)
        {
            //XOR
            a = Random.Range(1, 16);
            b = Random.Range(1, 16);
            answer = a ^ b;
        }
        else
        {
            //Error
            a = 0;
            b = 0;
            answer = 0;
        }
        string display = "" + a;
        string[] words;
        if (type == 0) words = new string[] { " TIMES ", " MULT " };
        else if (type == 1) words = new string[] { " OVER ", " DIV " };
        else if (type == 2) words = new string[] { " MOD ", " REM " };
        else if (type == 3) words = new string[] { " POW ", " EXP " };
        else if (type == 4) words = new string[] { " XOR " };
        else words = new string[] { " ERROR " };
        display += words[Random.Range(0, words.Length)];
        display += b;
        DisplaySequence = Morsify(display);
        ReplySequence = Morsify("" + answer);
        ReplyAnswer = answer;
    }

    private static int[] Morsify(string text)
    {
        char[] values = text.ToCharArray();
        List<int> data = new List<int>();
        for(int a = 0; a < values.Length; a++)
        {
            if (a > 0) data.Add(-1);
            char c = values[a];
            switch(c)
            {
                /*case ' ':
                    data.Add(-1);
                    break;*/
                case 'A':
                    data.Add(0);
                    data.Add(1);
                    break;
                case 'B':
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    break;
                case 'C':
                    data.Add(1);
                    data.Add(0);
                    data.Add(1);
                    data.Add(0);
                    break;
                case 'D':
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    break;
                case 'E':
                    data.Add(0);
                    break;
                case 'F':
                    data.Add(0);
                    data.Add(0);
                    data.Add(1);
                    data.Add(0);
                    break;
                case 'G':
                    data.Add(1);
                    data.Add(1);
                    data.Add(0);
                    break;
                case 'H':
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    break;
                case 'I':
                    data.Add(0);
                    data.Add(0);
                    break;
                case 'J':
                    data.Add(0);
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    break;
                case 'K':
                    data.Add(1);
                    data.Add(0);
                    data.Add(1);
                    break;
                case 'L':
                    data.Add(0);
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    break;
                case 'M':
                    data.Add(1);
                    data.Add(1);
                    break;
                case 'N':
                    data.Add(1);
                    data.Add(0);
                    break;
                case 'O':
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    break;
                case 'P':
                    data.Add(0);
                    data.Add(1);
                    data.Add(1);
                    data.Add(0);
                    break;
                case 'Q':
                    data.Add(1);
                    data.Add(1);
                    data.Add(0);
                    data.Add(1);
                    break;
                case 'R':
                    data.Add(0);
                    data.Add(1);
                    data.Add(0);
                    break;
                case 'S':
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    break;
                case 'T':
                    data.Add(1);
                    break;
                case 'U':
                    data.Add(0);
                    data.Add(0);
                    data.Add(1);
                    break;
                case 'V':
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(1);
                    break;
                case 'W':
                    data.Add(0);
                    data.Add(1);
                    data.Add(1);
                    break;
                case 'X':
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    data.Add(1);
                    break;
                case 'Y':
                    data.Add(1);
                    data.Add(0);
                    data.Add(1);
                    data.Add(1);
                    break;
                case 'Z':
                    data.Add(1);
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    break;
                case '1':
                    data.Add(0);
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    break;
                case '2':
                    data.Add(0);
                    data.Add(0);
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    break;
                case '3':
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(1);
                    data.Add(1);
                    break;
                case '4':
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(1);
                    break;
                case '5':
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    break;
                case '6':
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    break;
                case '7':
                    data.Add(1);
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    break;
                case '8':
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    break;
                case '9':
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    data.Add(0);
                    break;
                case '0':
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    break;
            }
        }
        return data.ToArray();
    }

    private string DeMorsify()
    {
        int[] data = EnteredCharacters.ToArray();
        List<int> values = new List<int>();
        string result = "";
        foreach (int i in data)
        {
            if (i == -1)
            {
                result += GetLetter(values.ToArray());
                values.Clear();
            }
            else values.Add(i);
        }
        result += GetLetter(values.ToArray());
        return result;
    }

    private string GetLetter(int[] val)
    {
        if (val.Length == 0) return "?";
        else if (val.Length > 5) return "?";
        else if (val[0] == 0)
        {
            //.
            if (val.Length == 1) return "E";
            else if(val[1] == 0)
            {
                //..
                if (val.Length == 2) return "I";
                else if(val[2] == 0)
                {
                    //...
                    if (val.Length == 3) return "S";
                    else if(val[3] == 0)
                    {
                        //....
                        if (val.Length == 4) return "H";
                        else if(val[4] == 0)
                        {
                            //.....
                            if (val.Length == 5) return "5";
                            else return "?";
                        }
                        else
                        {
                            //....-
                            if (val.Length == 5) return "4";
                            else return "?";
                        }
                    }
                    else
                    {
                        //...-
                        if (val.Length == 4) return "V";
                        else if (val[4] == 0) return "?";
                        else
                        {
                            //...--
                            if (val.Length == 5) return "3";
                            else return "?";
                        }
                    }
                }
                else
                {
                    //..-
                    if (val.Length == 3) return "U";
                    else if(val[3] == 0)
                    {
                        //..-.
                        if (val.Length == 4) return "F";
                        else return "?";
                    }
                    else
                    {
                        //..--
                        if (val.Length == 4 || val[4] == 0) return "?";
                        else
                        {
                            //..---
                            if (val.Length == 5) return "2";
                            else return "?";
                        }
                    }
                }
            }
            else
            {
                //.-
                if (val.Length == 2) return "A";
                else if(val[2] == 0)
                {
                    //.-.
                    if (val.Length == 3) return "R";
                    else if (val[3] == 0)
                    {
                        //.-..
                        if (val.Length == 4) return "L";
                        else return "?";
                    }
                    else return "?";
                }
                else
                {
                    //.--
                    if (val.Length == 3) return "W";
                    else if(val[3] == 0)
                    {
                        //.--.
                        if (val.Length == 4) return "P";
                        else return "?";
                    }
                    else
                    {
                        //.---
                        if (val.Length == 4) return "J";
                        else if (val[4] == 0) return "?";
                        else
                        {
                            //.----
                            if (val.Length == 5) return "1";
                            else return "?";
                        }
                    }
                }
            }
        }
        else
        {
            //-
            if (val.Length == 1) return "T";
            else if(val[1] == 0)
            {
                //-.
                if (val.Length == 2) return "N";
                else if(val[2] == 0)
                {
                    //-..
                    if (val.Length == 3) return "D";
                    else if(val[3] == 0)
                    {
                        //-...
                        if (val.Length == 4) return "B";
                        else if (val[4] == 0)
                        {
                            if (val.Length == 5) return "6";
                            else return "?";
                        }
                        else return "?";
                    }
                    else
                    {
                        //-..-
                        if (val.Length == 4) return "X";
                        else return "?";
                    }
                }
                else
                {
                    //-.-
                    if (val.Length == 3) return "K";
                    else if(val[3] == 0)
                    {
                        //-.-.
                        if (val.Length == 4) return "C";
                        else return "?";
                    }
                    else
                    {
                        //-.--
                        if (val.Length == 4) return "Y";
                        else return "?";
                    }
                }
            }
            else
            {
                //--
                //O890
                if (val.Length == 2) return "M";
                else if(val[2] == 0)
                {
                    //--.
                    if (val.Length == 3) return "G";
                    else if(val[3] == 0)
                    {
                        //--..
                        if (val.Length == 4) return "Z";
                        else if (val[4] == 0)
                        {
                            if (val.Length == 5) return "7";
                            else return "?";
                        }
                        else return "?";
                    }
                    else
                    {
                        //--.-
                        if (val.Length == 4) return "Q";
                        else return "?";
                    }
                }
                else
                {
                    //---
                    if (val.Length == 3) return "O";
                    else if(val[3] == 0)
                    {
                        //---.
                        if (val.Length == 4 || val[4] == 1) return "?";
                        else
                        {
                            //---..
                            if (val.Length == 5) return "8";
                            else return "?";
                        }
                    }
                    else
                    {
                        //----
                        if (val.Length == 4) return "?";
                        else if(val[4] == 0)
                        {
                            //----.
                            if (val.Length == 5) return "9";
                            else return "?";
                        }
                        else
                        {
                            //-----
                            if (val.Length == 5) return "0";
                            else return "?";
                        }
                    }
                }
            }
        }
    }
}