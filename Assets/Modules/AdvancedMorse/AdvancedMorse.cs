/*

-- On the Subject of Morsematics --
- Get it? Because it uses morse and maths! I'll see myself out... -

Press "Play" to receive a question.
Interpret the signal from the flashing light using the Morse Code chart.
The signal will usually be a maths question, but could also be a statement.
Maths questions will have a whole number response.
Statements will have either a "YES" or "NO" response.
Note: Do not agitate the bomb, it will beat you in a fight.

The question is changed every time "Play" is pressed, and your answer is reset.

Warning: The signal will only play once, and will contain spaces.

*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AdvancedMorse : MonoBehaviour
{
    public KMSelectable ButtonPlay, ButtonDot, ButtonDash, ButtonSpace, ButtonDone;
    public KMAudio Sound;

    protected int[] DisplaySequence;
    protected int DisplayProgress;
    protected bool Generated = false;

    protected int[] ReplySequence;
    protected int ReplyProgress;
    protected bool ReplyCorrect;

    private static Color BLACK = new Color(0, 0, 0), GREEN = new Color(0, 1, 0);
    private MeshRenderer LED;

    private static List<KeyValuePair<int[], bool>> QuestionAnswerList = new List<KeyValuePair<int[], bool>>()
    {
        {new KeyValuePair<int[], bool>(Morsify("ON BOMB"), true)},
        {new KeyValuePair<int[], bool>(Morsify("MODULE GREEN"), false)},
        {new KeyValuePair<int[], bool>(Morsify("FIGHT ME"), false)},
        {new KeyValuePair<int[], bool>(Morsify("2 IS TWO"), true)}
    };
    private static int[] YesResponse = Morsify("YES");
    private static int[] NoResponse = Morsify("NO");

    void Awake()
    {
        LED = gameObject.transform.Find("LED").GetComponent<MeshRenderer>();
        LED.material.color = BLACK;

        ButtonPlay.OnInteract += HandlePlay;
        ButtonDot.OnInteract += HandleDot;
        ButtonDash.OnInteract += HandleDash;
        ButtonSpace.OnInteract += HandleSpace;
        ButtonDone.OnInteract += HandleDone;

        DisplaySequence = new int[0];
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
                    int target = 15;
                    if (type == 1) target = 45;
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

        if (ReplySequence == null)
        {
            if (Random.Range(0, 100) > 0) GenerateMath();
            else
            {
                KeyValuePair<int[], bool> q = QuestionAnswerList[Random.Range(0, QuestionAnswerList.Count)];
                DisplaySequence = q.Key;
                if (q.Value) ReplySequence = YesResponse;
                else ReplySequence = NoResponse;
            }
        }
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
        }
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
            else GetComponent<KMBombModule>().HandleStrike();
        }
        return false;
    }

    private void GenerateMath()
    {
        int a, b, answer;
        int type = Random.Range(0, 4);
        if (type == 0)
        {
            //Multiplication
            a = Random.Range(3, 8);
            b = Random.Range(7, 14);
            answer = a * b;
        }
        else if (type == 1)
        {
            //Division
            b = Random.Range(3, 8);
            answer = Random.Range(13, 18);
            a = b * answer;
        }
        else if (type == 2)
        {
            //Modulo
            a = Random.Range(100, 201);
            b = Random.Range(7, 12);
            answer = a % b;
        }
        else
        {
            //Power
            a = Random.Range(3, 8);
            b = Random.Range(3, 6);
            answer = a;
            for (int i = 1; i < b; i++) answer *= a;
        }
        string display = "" + a;
        if (type == 0) display += " TIMES ";
        else if (type == 1) display += " OVER ";
        else if (type == 2) display += " MOD ";
        else display += " POWER ";
        display += b;
        DisplaySequence = Morsify(display);
        ReplySequence = Morsify("" + answer);
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
}