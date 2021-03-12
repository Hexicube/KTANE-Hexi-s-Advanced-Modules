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
    public static int loggingID = 1;
    public int thisLoggingID;

    public KMSelectable Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8;
    public KMAudio Sound;

    protected KMSelectable[] Buttons;

    protected bool[] ButtonStates;
    protected bool[] Solution;

    private static char[] CharList = new char[]{
        'Ѽ', 'æ', '©', 'Ӭ', 'Ҩ', 'Ҋ', 'ϗ', 'Ϟ',
        'Ԇ', 'Ϙ', 'Ѯ', 'ƛ', 'Ω', '¶', 'ψ', '¿',
        'Ϭ', 'Ͼ', 'Ͽ', '★', '☆', 'ټ', '҂', 'Ѣ',
        'Ѭ', 'Ѧ', 'Җ'
    };
    private static char[][] ResultLists = new char[][]{
        new char[]{'Ϙ', 'Ѧ', 'ƛ', 'Ϟ', 'Ѭ', 'ϗ', 'Ͽ'},
        new char[]{'Ӭ', 'Ϙ', 'Ͽ', 'Ҩ', '☆', 'ϗ', '¿'},
        new char[]{'©', 'Ѽ', 'Ҩ', 'Җ', 'Ԇ', 'ƛ', '☆'},
        new char[]{'Ϭ', '¶', 'Ѣ', 'Ѭ', 'Җ', '¿', 'ټ'},
        new char[]{'ψ', 'ټ', 'Ѣ', 'Ͼ', '¶', 'Ѯ', '★'},
        new char[]{'Ϭ', 'Ӭ', '҂', 'æ', 'ψ', 'Ҋ', 'Ω'}
    };

    void Awake()
    {
        thisLoggingID = loggingID++;

        Buttons = new KMSelectable[] { Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8 };
        Button1.OnInteract += Handle1;
        Button2.OnInteract += Handle2;
        Button3.OnInteract += Handle3;
        Button4.OnInteract += Handle4;
        Button5.OnInteract += Handle5;
        Button6.OnInteract += Handle6;
        Button7.OnInteract += Handle7;
        Button8.OnInteract += Handle8;

        foreach (KMSelectable b in Buttons)
        {
            b.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);
        }

        List<char> values = new List<char>();
        foreach (char a in CharList) values.Add(a);
        char[] labels = new char[8];
        int pos = 0;
        string s = "Round Keypad symbol list: ";
        while (pos < 8)
        {
            int val = Random.Range(0, values.Count);
            labels[pos++] = values[val];
            values.RemoveAt(val);
            if (pos > 1) s += ",";
            s += labels[pos - 1];
        }
        Debug.Log("[Round Keypad #"+thisLoggingID+"] "+s);
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
        Debug.Log("[Round Keypad #"+thisLoggingID+"] Correct column: " + (bestPos + 1));
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
        s = "Solution: ";
        bool first = true;
        for (pos = 0; pos < 8; pos++)
        {
            if (Solution[pos])
            {
                if (!first) s += ",";
                s += labels[pos];
            }
        }
        Debug.Log("[Round Keypad #"+thisLoggingID+"] "+s);
    }

    void Guess(int pos)
    {
        if (ButtonStates[pos]) return;
        Buttons[pos].AddInteractionPunch(0.5f);
        Buttons[pos].transform.localPosition += new Vector3(0, -0.001f, 0);
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
            if (done) {
                Debug.Log("[Round Keypad #"+thisLoggingID+"] Module solved.");
                GetComponent<KMBombModule>().HandlePass();
            }
        }
        else
        {
            Buttons[pos].transform.Find("LED").GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0);
            GetComponent<KMBombModule>().HandleStrike();
            Debug.Log("[Round Keypad #"+thisLoggingID+"] Incorrect symbol: "+Buttons[pos].transform.Find("Label").GetComponent<TextMesh>().text);
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

    //Twitch Plays support

    #pragma warning disable 0414
    string TwitchHelpMessage = "Submit a solution using 'press 1 4 2...'. You can use either numbers starting with 1 at the top going clockwise (1 through 8), or compass directions (N, NE, E, etc.).";
    #pragma warning restore 0414

    public IEnumerator TwitchHandleForcedSolve() {
        Debug.Log("[Round Keypad #"+thisLoggingID+"] Module forcibly solved.");
        for(int a = 0; a < ButtonStates.Length; a++) {
            if(ButtonStates[a]) continue;
            if(Solution[a]) { 
                Buttons[a].OnInteract();
                yield return new WaitForSeconds(0.25f);
            }
        }
    }

    public IEnumerator ProcessTwitchCommand(string cmd) {
        cmd = cmd.ToUpperInvariant();
        if(cmd.StartsWith("PRESS ")) cmd = cmd.Substring(6);
        else if(cmd.StartsWith("SUBMIT ")) cmd = cmd.Substring(7);
        else {
            yield return "sendtochaterror Solutions must start with press or submit.";
            yield break;
        }

        string[] list = cmd.Split(' ');
        int[] positions = new int[list.Length];
        for(int a = 0; a < list.Length; a++) {
            string s = list[a];
                 if(s.Equals("1") || s.Equals("N"))  positions[a] = 1;
            else if(s.Equals("2") || s.Equals("NE")) positions[a] = 2;
            else if(s.Equals("3") || s.Equals("E"))  positions[a] = 3;
            else if(s.Equals("4") || s.Equals("SE")) positions[a] = 4;
            else if(s.Equals("5") || s.Equals("S"))  positions[a] = 5;
            else if(s.Equals("6") || s.Equals("SW")) positions[a] = 6;
            else if(s.Equals("7") || s.Equals("W"))  positions[a] = 7;
            else if(s.Equals("8") || s.Equals("NW")) positions[a] = 8;
            else {
                yield return "sendtochaterror Unknown button: '" + s + "'";
                yield break;
            }
        }

        yield return "Advanced Keypad";
        foreach(int i in positions) {
            Buttons[i-1].OnInteract();
            yield return new WaitForSeconds(0.25f);
        }
        yield break;
    }
}