/*

-- On the Subject of Round Keypads --
- I think someone tried to make this module look *really* cool, but failed. -

The circular keypad contains 8 symbols from the columns for regular keypads. Press all symbols that do not appear on the column that has the most matching symbols.
If multiple columns have the same number of matching symbols, use the right-most column.

 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AdvancedKeypad : MonoBehaviour
{
    public KMSelectable Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8;
    public KMAudio Sound;

    protected KMSelectable[] Buttons;

    protected bool[] ButtonStates;
    protected bool[] Solution;

    private static char[] CharList = new char[]{
        'Ѽ', 'æ', '©', 'Ӭ', 'Ҩ', 'Ҋ', 'ϗ', 'ϰ',
        'Ԇ', 'Ϙ', 'Ѯ', 'ƛ', 'Ω', '¶', 'ψ', '¿',
        'Ϭ', 'Ͼ', 'Ͽ', '★', '☆', 'ټ', '҂', 'Ѣ',
        'Ѭ', 'Ѧ', 'Җ'
    };
    private static char[][] ResultLists = new char[][]{
        new char[]{'Ϙ', 'Ѧ', 'ƛ', 'ϰ', 'Ѭ', 'ϗ', 'Ͽ'},
        new char[]{'Ӭ', 'Ϙ', 'Ͽ', 'Ҩ', '☆', 'ϗ', '¿'},
        new char[]{'©', 'Ѽ', 'Ҩ', 'Җ', 'Ԇ', 'ƛ', '☆'},
        new char[]{'Ϭ', '¶', 'Ѣ', 'Ѭ', 'Җ', '¿', 'ټ'},
        new char[]{'ψ', 'ټ', 'Ѣ', 'Ͼ', '¶', 'Ѯ', '★'},
        new char[]{'Ϭ', 'Ӭ', '҂', 'æ', 'ψ', 'Ҋ', 'Ω'}
    };

    void Awake()
    {
        Buttons = new KMSelectable[] { Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8 };
        Button1.OnInteract += Handle1;
        Button2.OnInteract += Handle2;
        Button3.OnInteract += Handle3;
        Button4.OnInteract += Handle4;
        Button5.OnInteract += Handle5;
        Button6.OnInteract += Handle6;
        Button7.OnInteract += Handle7;
        Button8.OnInteract += Handle8;

        List<char> values = new List<char>();
        foreach (char a in CharList) values.Add(a);
        char[] labels = new char[8];
        string chars = "";
        int pos = 0;
        while (pos < 8)
        {
            int val = Random.Range(0, values.Count);
            labels[pos++] = values[val];
            values.RemoveAt(val);
            chars += labels[pos - 1];
        }
        Debug.Log(chars);
        pos = 0;
        while (pos < 8)
        {
            Buttons[pos].gameObject.transform.Find("LED").GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);
            Buttons[pos].gameObject.transform.Find("Label").GetComponent<TextMesh>().text = "" + labels[pos++];
        }

        ButtonStates = new bool[8];
        Solution = new bool[8];
        int bestCount = 0;
        int bestPos = 0;
        pos = 0;
        while(pos < 6)
        {
            char[] result = ResultLists[pos];
            int thisCount = 0;
            foreach(char c in result)
            {
                bool match = false;
                foreach(char a in labels)
                {
                    if(a == c)
                    {
                        match = true;
                        break;
                    }
                }
                if(match) thisCount++;
            }
            if (thisCount >= bestCount)
            {
                bestPos = pos;
                bestCount = thisCount;
            }
            pos++;
        }
        pos = 0;
        while(pos < 8)
        {
            bool match = false;
            foreach(char c in ResultLists[bestPos])
            {
                if (labels[pos] == c)
                {
                    match = true;
                    break;
                }
            }
            Solution[pos++] = !match;
        }
    }

    void Guess(int pos)
    {
        if (ButtonStates[pos]) return;
        ButtonStates[pos] = true;
        if(Solution[pos])
        {
            Buttons[pos].transform.Find("LED").GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0);
            Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
            bool done = true;
            for (int a = 0; a < 8; a++)
            {
                if (Solution[a] && !ButtonStates[a])
                {
                    done = false;
                    break;
                }
            }
            if (done) GetComponent<KMBombModule>().HandlePass();
        }
        else
        {
            Buttons[pos].transform.Find("LED").GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0);
            GetComponent<KMBombModule>().HandleStrike();
        }
    }

    protected bool Handle1() { Guess(0); return false; }
    protected bool Handle2() { Guess(1); return false; }
    protected bool Handle3() { Guess(2); return false; }
    protected bool Handle4() { Guess(3); return false; }
    protected bool Handle5() { Guess(4); return false; }
    protected bool Handle6() { Guess(5); return false; }
    protected bool Handle7() { Guess(6); return false; }
    protected bool Handle8() { Guess(7); return false; }
}