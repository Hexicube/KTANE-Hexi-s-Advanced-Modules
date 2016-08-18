/*

-- On the Subject of Password Sequences --
- The latest and greatest in security from the old days; 11 buttons, and no limit on retries. -

The 11-button lock must be solved by trying random combinations.
If the button is correct, it will light up.
Pressing an incorrect button *will not* cause a strike.
Creating a combination below *will* cause a strike, *unless* it's part of the solution.

 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AdvancedPassword : MonoBehaviour
{
    public KMSelectable Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9, Button10, Button11;
    public KMAudio Sound;

    protected KMSelectable[] Buttons;
    protected int[] Sequence;

    protected bool[] ButtonStates;
    protected int Progress;

    private static int[] BadStates = new int[]{
        //One button
        1024, //100 0000 0000
        //Two buttons
        768,  //011 0000 0000
        80,   //000 0101 0000
        6,    //000 0000 0110
        //Three buttons
        1344, //101 0100 0000
        25,   //000 0001 1001
        112,  //000 0111 0000
        1058, //100 0010 0010
        //Four buttons
        526,  //010 0000 1110
        562,  //010 0011 0010
        139,  //000 1000 1011
        1297, //101 0001 0001
        402,  //001 1001 0010
        //Eight buttons
        1278, //100 1111 1110
        958,  //011 1011 1110
        479,  //001 1101 1111
        1907, //111 0111 0011
        1885, //111 0101 1101
        //Nine buttons
        1775, //110 1110 1111
        1975, //111 1011 0111
        1503, //101 1101 1111
        2041, //111 1111 1001
        895,  //011 0111 1111
        //Ten buttons
        2015, //111 1101 1111
    };

    void Awake()
    {
        Buttons = new KMSelectable[] { Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9, Button10, Button11 };
        Button1.OnInteract += Handle1;
        Button2.OnInteract += Handle2;
        Button3.OnInteract += Handle3;
        Button4.OnInteract += Handle4;
        Button5.OnInteract += Handle5;
        Button6.OnInteract += Handle6;
        Button7.OnInteract += Handle7;
        Button8.OnInteract += Handle8;
        Button9.OnInteract += Handle9;
        Button10.OnInteract += Handle10;
        Button11.OnInteract += Handle11;
        foreach(KMSelectable b in Buttons)
        {
            b.gameObject.transform.Find("LED").GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);
        }

        List<int> values = new List<int>();
        for (int a = 0; a < 11; a++) values.Add(a);
        Sequence = new int[11];
        int pos = 0;
        while(pos < 11)
        {
            int val = Random.Range(0, values.Count);
            Sequence[pos++] = values[val];
            values.RemoveAt(val);
        }

        ButtonStates = new bool[11];
    }

    void Reset()
    {
        for (int a = 0; a < 11; a++)
        {
            ButtonStates[a] = false;
            Buttons[a].transform.Find("LED").GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);
        }
        Progress = 0;
    }

    private int GetStateVal()
    {
        int val = 0;
        for (int a = 0; a < 11; a++)
        {
           if (ButtonStates[a]) val += 1 << (10 - a);
        }
        return val;
    }

    void Guess(int pos)
    {
        if (Progress == 11 || ButtonStates[pos]) return;
        int oldval = GetStateVal();
        ButtonStates[pos] = true;
        if(Sequence[Progress] == pos)
        {
            Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
            Progress++;
            Buttons[pos].transform.Find("LED").GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0);
            if (Progress == 11) GetComponent<KMBombModule>().HandlePass();
        }
        else
        {
            int val = GetStateVal();
            bool match = false;
            foreach(int target in BadStates)
            {
                //if (((target & val) == target) && ((target & oldval) < target)) match = true;
                if (target == val && target != oldval)
                {
                    match = true;
                    break;
                }
            }
            if (match)
            {
                GetComponent<KMBombModule>().HandleStrike();
                Reset();
            }
            else Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, gameObject.transform);
            //Reset();
            ButtonStates[pos] = false;
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
    protected bool Handle9() { Guess(8); return false; }
    protected bool Handle10() { Guess(9); return false; }
    protected bool Handle11() { Guess(10); return false; }
}