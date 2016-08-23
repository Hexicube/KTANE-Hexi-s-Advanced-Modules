/*

-- On the Subject of Plumbing --
- Is this bomb water-cooled or something? -

This module has 4 input pipes (left) and 4 output pipes (right), and a 6x6 grid of 36 pipe pieces.
The defuser must connect all active inputs to all active outputs, by rotating individual pieces.
All pipe connections on any pipe connected to an active input must connect to another pipe, another active input, or an active output.
Connecting an active input or output to an inactive input or output makes a solution invalid.
Once the inputs and outputs are properly connected, press "CHECK" to verify it.

Active inputs/outputs are determined by the chart below:


- Determining inputs and outputs -
Each input and output has rules for and against it. If more for rules than against rules are satisfied, that connection is active.

- Input 1 (Red) -
For: Serial contains a 1
For: Exactly one RJ45 port
Against: Any duplicate ports
Against: Any duplicate serial characters

- Input 2 (Yellow) -
For: Serial contains a 2
For: One or more Stereo RCA ports
Against: No duplicate ports
Against: Serial contains a 1 or an L

- Input 3 (Green) -
For: Serial contains 3 or more numbers
For: One or more DVI-D ports
Against: Input 1 is inactive
Against: Input 2 is inactive

- Input 4 (Blue) -
Note: If no other input is active, this one is.
For: At least 4 unique ports
For: At least 4 batteries
Against: No ports
Against: No batteries

- Output 1 (Red) -
For: One or more Serial ports
For: Exactly one battery
Against: Serial contains more than 2 numbers
Against: More than 2 inputs are active

- Output 2 (Yellow) -
For: Any duplicate ports
For: Serial contains a 4 or an 8
Against: Serial doesn't contain a 2
Against: Input 3 is active

- Output 3 (Green) -
For: Exactly 3 inputs are active
For: Exactly 3 ports are present
Against: Less than 3 ports are present
Against: Serial contains more than 3 numbers

- Output 4 (Blue) -
Note: If no other output is active, this one is.
For: All inputs are active
For: Any other output is inactive
Against: Less than 2 batteries
Against: No Parallel port

 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class AdvancedMaze : MonoBehaviour
{
    public KMAudio Sound;
    public KMBombInfo Info;
    public KMSelectable ButtonCheck,
                        ButtonA1, ButtonA2, ButtonA3, ButtonA4, ButtonA5, ButtonA6,
                        ButtonB1, ButtonB2, ButtonB3, ButtonB4, ButtonB5, ButtonB6,
                        ButtonC1, ButtonC2, ButtonC3, ButtonC4, ButtonC5, ButtonC6,
                        ButtonD1, ButtonD2, ButtonD3, ButtonD4, ButtonD5, ButtonD6,
                        ButtonE1, ButtonE2, ButtonE3, ButtonE4, ButtonE5, ButtonE6,
                        ButtonF1, ButtonF2, ButtonF3, ButtonF4, ButtonF5, ButtonF6;
    private KMSelectable[][] Buttons;
    public GameObject EntryLeft1,  EntryLeft2,  EntryLeft3,  EntryLeft4,  EntryLeft5,  EntryLeft6,
                      EntryRight1, EntryRight2, EntryRight3, EntryRight4, EntryRight5, EntryRight6;

    private int[][] PlayFieldType, PlayFieldState;
    private int[] EntryLocations, ExitLocations;

    public GameObject PipeStraight, PipeCorner, PipeT, PipeCross, PipeEnd;

    private bool[] ActiveIn, ActiveOut;

    private bool Solved;

    private void ApplyModel(GameObject button, int type, int rot, bool light)
    {
        ApplyModel(button, type, rot, light ? new Color(1, 1, 1) : new Color(0.6f, 0.6f, 0.6f), new Color(0, 0, 0));
    }

    private void ApplyModel(GameObject button, int type, int rot, Color pipeCol, Color bgCol)
    {
        if (type != -1)
        {
            GameObject g = null;
            if (type == 0) g = Instantiate(PipeStraight);
            else if (type == 1) g = Instantiate(PipeCorner);
            else if (type == 2) g = Instantiate(PipeT);
            else if (type == 3) g = Instantiate(PipeCross);
            else g = Instantiate(PipeEnd);
            g.transform.name = "Pipe";

            g.transform.parent = button.transform;
            g.transform.localPosition = new Vector3(0, 0.5f, 0);
            g.transform.localScale = new Vector3(1, 4f, 1);
            g.transform.localEulerAngles = new Vector3(0, 90f * rot, 0);
            g.GetComponent<MeshRenderer>().material.color = pipeCol;
        }

        button.GetComponent<MeshRenderer>().material.color = bgCol;
    }

    void Awake()
    {
        transform.Find("Background").GetComponent<MeshRenderer>().material.color = new Color(1, 0.1f, 0.1f);

        Buttons = new KMSelectable[][]{
            new KMSelectable[]{ButtonA1, ButtonB1, ButtonC1, ButtonD1, ButtonE1, ButtonF1},
            new KMSelectable[]{ButtonA2, ButtonB2, ButtonC2, ButtonD2, ButtonE2, ButtonF2},
            new KMSelectable[]{ButtonA3, ButtonB3, ButtonC3, ButtonD3, ButtonE3, ButtonF3},
            new KMSelectable[]{ButtonA4, ButtonB4, ButtonC4, ButtonD4, ButtonE4, ButtonF4},
            new KMSelectable[]{ButtonA5, ButtonB5, ButtonC5, ButtonD5, ButtonE5, ButtonF5},
            new KMSelectable[]{ButtonA6, ButtonB6, ButtonC6, ButtonD6, ButtonE6, ButtonF6}
        };

        ButtonCheck.OnInteract += HandleCheck;

        GetComponent<KMBombModule>().OnActivate += Init;
    }

    void HandleInteract(int x, int y)
    {
        if (Solved) return;

        int rot = (PlayFieldState[x][y] + 1) % 4;
        PlayFieldState[x][y] = rot;

        if (PlayFieldType[x][y] != -1)
        {
            Transform t = Buttons[x][y].gameObject.transform.Find("Pipe");
            t.localEulerAngles = new Vector3(0, rot * 90f, 0);
        }
        
        Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
    }

    void Init()
    {
        EntryLocations = new int[4];
        List<int> posList = new List<int>() { 1, 2, 3, 4, 5, 6 };
        for (int a = 0; a < 4; a++)
        {
            int pos = Random.Range(0, posList.Count);
            EntryLocations[a] = posList[pos];
            posList.RemoveAt(pos);
        }
        ExitLocations = new int[4];
        posList = new List<int>() { 1, 2, 3, 4, 5, 6 };
        for (int a = 0; a < 4; a++)
        {
            int pos = Random.Range(0, posList.Count);
            ExitLocations[a] = posList[pos];
            posList.RemoveAt(pos);
        }

        Color[] colList = new Color[]{
            new Color(1, 0.1f, 0.1f),
            new Color(1, 1, 0.1f),
            new Color(0.1f, 0.8f, 0.1f),
            new Color(0.1f, 0.4f, 1)
        };

        ApplyModel(EntryLeft1, -1, 0, new Color(1, 1, 1), new Color(0.3f, 0.3f, 0.3f));
        ApplyModel(EntryLeft2, -1, 0, new Color(1, 1, 1), new Color(0.3f, 0.3f, 0.3f));
        ApplyModel(EntryLeft3, -1, 0, new Color(1, 1, 1), new Color(0.3f, 0.3f, 0.3f));
        ApplyModel(EntryLeft4, -1, 0, new Color(1, 1, 1), new Color(0.3f, 0.3f, 0.3f));
        ApplyModel(EntryLeft5, -1, 0, new Color(1, 1, 1), new Color(0.3f, 0.3f, 0.3f));
        ApplyModel(EntryLeft6, -1, 0, new Color(1, 1, 1), new Color(0.3f, 0.3f, 0.3f));

        for (int a = 0; a < 4; a++)
        {
            if (EntryLocations[a] == 1) ApplyModel(EntryLeft1, 4, 3, colList[a], new Color(0.3f, 0.3f, 0.3f));
            if (EntryLocations[a] == 2) ApplyModel(EntryLeft2, 4, 3, colList[a], new Color(0.3f, 0.3f, 0.3f));
            if (EntryLocations[a] == 3) ApplyModel(EntryLeft3, 4, 3, colList[a], new Color(0.3f, 0.3f, 0.3f));
            if (EntryLocations[a] == 4) ApplyModel(EntryLeft4, 4, 3, colList[a], new Color(0.3f, 0.3f, 0.3f));
            if (EntryLocations[a] == 5) ApplyModel(EntryLeft5, 4, 3, colList[a], new Color(0.3f, 0.3f, 0.3f));
            if (EntryLocations[a] == 6) ApplyModel(EntryLeft6, 4, 3, colList[a], new Color(0.3f, 0.3f, 0.3f));
        }

        ApplyModel(EntryRight1, -1, 0, new Color(1, 1, 1), new Color(0.3f, 0.3f, 0.3f));
        ApplyModel(EntryRight2, -1, 0, new Color(1, 1, 1), new Color(0.3f, 0.3f, 0.3f));
        ApplyModel(EntryRight3, -1, 0, new Color(1, 1, 1), new Color(0.3f, 0.3f, 0.3f));
        ApplyModel(EntryRight4, -1, 0, new Color(1, 1, 1), new Color(0.3f, 0.3f, 0.3f));
        ApplyModel(EntryRight5, -1, 0, new Color(1, 1, 1), new Color(0.3f, 0.3f, 0.3f));
        ApplyModel(EntryRight6, -1, 0, new Color(1, 1, 1), new Color(0.3f, 0.3f, 0.3f));

        for (int a = 0; a < 4; a++)
        {
            if (ExitLocations[a] == 1) ApplyModel(EntryRight1, 4, 1, colList[a], new Color(0.3f, 0.3f, 0.3f));
            if (ExitLocations[a] == 2) ApplyModel(EntryRight2, 4, 1, colList[a], new Color(0.3f, 0.3f, 0.3f));
            if (ExitLocations[a] == 3) ApplyModel(EntryRight3, 4, 1, colList[a], new Color(0.3f, 0.3f, 0.3f));
            if (ExitLocations[a] == 4) ApplyModel(EntryRight4, 4, 1, colList[a], new Color(0.3f, 0.3f, 0.3f));
            if (ExitLocations[a] == 5) ApplyModel(EntryRight5, 4, 1, colList[a], new Color(0.3f, 0.3f, 0.3f));
            if (ExitLocations[a] == 6) ApplyModel(EntryRight6, 4, 1, colList[a], new Color(0.3f, 0.3f, 0.3f));
        }

        Dictionary<string, int> portCount = new Dictionary<string, int>();
        List<string> data = Info.QueryWidgets(KMBombInfo.QUERYKEY_GET_PORTS, null);
        int portSum = 0;
        bool duplicatePort = false;
        foreach (string response in data)
        {
            Dictionary<string, string[]> responseDict = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(response);
            foreach (string s in responseDict["presentPorts"])
            {
                if (portCount.ContainsKey(s))
                {
                    portCount[s]++;
                    duplicatePort = true;
                }
                else portCount[s] = 1;
                portSum++;
            }
        }
        string serial = "AB12C3";
        data = Info.QueryWidgets(KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER, null);
        foreach (string response in data)
        {
            Dictionary<string, string> responseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
            serial = responseDict["serial"];
            break;
        }
        char[] serialChar = serial.ToCharArray();
        int serialNumbers = 0;
        bool serialDupe = false;
        List<char> serialChars = new List<char>();
        foreach (char c in serialChar)
        {
            if (c == '0' || c == '1' || c == '2' || c == '3' || c == '4' || c == '5' || c == '6' || c == '7' || c == '8' || c == '9') serialNumbers++;
            if (serialChars.Contains(c)) serialDupe = true;
            else serialChars.Add(c);
        }
        int batteries = 0;
        data = Info.QueryWidgets(KMBombInfo.QUERYKEY_GET_BATTERIES, null);
        foreach (string response in data)
        {
            Dictionary<string, int> responseDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(response);
            batteries += responseDict["numbatteries"];
        }

        ActiveIn = new bool[4];
        ActiveOut = new bool[4];

        int val = 0;
        if (serial.Contains("1")) val++;
        if (portCount.ContainsKey("RJ45") && portCount["RJ45"] == 1) val++;
        if (duplicatePort) val--;
        if (serialDupe) val--;
        if (val > 0) ActiveIn[0] = true;

        val = 0;
        if (serial.Contains("2")) val++;
        if (portCount.ContainsKey("StereoRCA")) val++;
        if (!duplicatePort) val--;
        if (serial.Contains("1") || serial.Contains("L")) val--;
        if (val > 0) ActiveIn[1] = true;

        val = 0;
        if (serialNumbers > 2) val++;
        if (portCount.ContainsKey("DVI")) val++;
        if (!ActiveIn[0]) val--;
        if (!ActiveIn[1]) val--;
        if (val > 0) ActiveIn[2] = true;

        if (!ActiveIn[0] && !ActiveIn[1] && !ActiveIn[2]) ActiveIn[3] = true;
        else
        {
            val = 0;
            if (portCount.Count >= 4) val++;
            if (batteries >= 4) val++;
            if (portCount.Count == 0) val--;
            if (batteries == 0) val--;
            if (val > 0) ActiveIn[3] = true;
        }

        int sum = 0;
        if (ActiveIn[0]) sum++;
        if (ActiveIn[1]) sum++;
        if (ActiveIn[2]) sum++;
        if (ActiveIn[3]) sum++;

        val = 0;
        if (portCount.ContainsKey("Serial")) val++;
        if (batteries == 1) val++;
        if (serialNumbers > 2) val--;
        if (sum > 2) val--;
        if (val > 0) ActiveOut[0] = true;

        val = 0;
        if (duplicatePort) val++;
        if (serial.Contains("4") || serial.Contains("8")) val++;
        if (!serial.Contains("2")) val--;
        if (ActiveIn[2]) val--;
        if (val > 0) ActiveOut[1] = true;

        val = 0;
        if (sum == 3) val++;
        if (portSum == 3) val++;
        if (portSum < 3) val--;
        if (serialNumbers > 3) val--;
        if (val > 0) ActiveOut[2] = true;

        if (!ActiveOut[0] && !ActiveOut[1] && !ActiveOut[2]) ActiveOut[3] = true;
        else
        {
            val = 0;
            if (sum == 4) val++;
            if (!ActiveOut[0] || !ActiveOut[1] || !ActiveOut[2]) val++;
            if (batteries < 2) val--;
            if (!portCount.ContainsKey("Parallel")) val--;
            if (val > 0) ActiveOut[3] = true;
        }

        bool[][] grid = new bool[13][];
        for (int a = 0; a < 13; a++) grid[a] = new bool[13];

        int[][] ownership = new int[6][];
        for (int a = 0; a < 6; a++) ownership[a] = new int[6];

        List<int[]> positions = new List<int[]>();
        int numUnique = 0;

        for (int a = 0; a < 4; a++)
        {
            if (ActiveIn[a])
            {
                ownership[0][EntryLocations[a] - 1] = a + 1;
                grid[1][EntryLocations[a] * 2 - 1] = true;
                grid[0][EntryLocations[a] * 2 - 1] = true;
                positions.Add(new int[] { 0, EntryLocations[a] - 1 });
                numUnique++;
            }
            if (ActiveOut[a])
            {
                ownership[5][ExitLocations[a] - 1] = a + 5;
                grid[11][ExitLocations[a] * 2 - 1] = true;
                grid[12][ExitLocations[a] * 2 - 1] = true;
                positions.Add(new int[] { 5, ExitLocations[a] - 1 });
                numUnique++;
            }
        }

        while(numUnique > 1)
        {
            int pos = Random.Range(0, positions.Count);
            int[] location = positions[pos];
            positions.RemoveAt(pos);

            List<int> order = new List<int>() { 0, 1, 2, 3 };
            while (order.Count > 0)
            {
                pos = Random.Range(0, order.Count);
                int dir = order[pos];
                order.RemoveAt(pos);

                if (dir == 0 && location[0] > 0)
                {
                    if (ownership[location[0] - 1][location[1]] == 0)
                    {
                        ownership[location[0] - 1][location[1]] = ownership[location[0]][location[1]];
                        grid[location[0] * 2][location[1] * 2 + 1] = true;
                        grid[location[0] * 2 - 1][location[1] * 2 + 1] = true;
                        positions.Add(new int[] { location[0] - 1, location[1] });
                    }
                    else if (ownership[location[0] - 1][location[1]] == ownership[location[0]][location[1]])
                    {
                        
                    }
                    else
                    {
                        grid[location[0] * 2][location[1] * 2 + 1] = true;

                        int oldOwner = ownership[location[0] - 1][location[1]];
                        for (int x = 0; x < 6; x++)
                        {
                            for (int y = 0; y < 6; y++)
                            {
                                if (ownership[x][y] == oldOwner) ownership[x][y] = ownership[location[0]][location[1]];
                            }
                        }

                        numUnique--;
                    }
                }
                if (dir == 1 && location[0] < 5)
                {
                    if (ownership[location[0] + 1][location[1]] == 0)
                    {
                        ownership[location[0] + 1][location[1]] = ownership[location[0]][location[1]];
                        grid[location[0] * 2 + 2][location[1] * 2 + 1] = true;
                        grid[location[0] * 2 + 3][location[1] * 2 + 1] = true;
                        positions.Add(new int[] { location[0] + 1, location[1] });
                    }
                    else if (ownership[location[0] + 1][location[1]] == ownership[location[0]][location[1]])
                    {
                        
                    }
                    else
                    {
                        grid[location[0] * 2 + 2][location[1] * 2 + 1] = true;

                        int oldOwner = ownership[location[0] + 1][location[1]];
                        for (int x = 0; x < 6; x++)
                        {
                            for (int y = 0; y < 6; y++)
                            {
                                if (ownership[x][y] == oldOwner) ownership[x][y] = ownership[location[0]][location[1]];
                            }
                        }

                        numUnique--;
                    }
                }
                if (dir == 2 && location[1] > 0)
                {
                    if (ownership[location[0]][location[1] - 1] == 0)
                    {
                        ownership[location[0]][location[1] - 1] = ownership[location[0]][location[1]];
                        grid[location[0] * 2 + 1][location[1] * 2] = true;
                        grid[location[0] * 2 + 1][location[1] * 2 - 1] = true;
                        positions.Add(new int[] { location[0], location[1] - 1 });
                    }
                    else if (ownership[location[0]][location[1] - 1] == ownership[location[0]][location[1]])
                    {
                        
                    }
                    else
                    {
                        grid[location[0] * 2 + 1][location[1] * 2] = true;

                        int oldOwner = ownership[location[0]][location[1] - 1];
                        for (int x = 0; x < 6; x++)
                        {
                            for (int y = 0; y < 6; y++)
                            {
                                if (ownership[x][y] == oldOwner) ownership[x][y] = ownership[location[0]][location[1]];
                            }
                        }

                        numUnique--;
                    }
                }
                if (dir == 3 && location[1] < 5)
                {
                    if (ownership[location[0]][location[1] + 1] == 0)
                    {
                        ownership[location[0]][location[1] + 1] = ownership[location[0]][location[1]];
                        grid[location[0] * 2 + 1][location[1] * 2 + 2] = true;
                        grid[location[0] * 2 + 1][location[1] * 2 + 3] = true;
                        positions.Add(new int[] { location[0], location[1] + 1 });
                    }
                    else if (ownership[location[0]][location[1] + 1] == ownership[location[0]][location[1]])
                    {
                        
                    }
                    else
                    {
                        grid[location[0] * 2 + 1][location[1] * 2 + 2] = true;

                        int oldOwner = ownership[location[0]][location[1] + 1];
                        for (int x = 0; x < 6; x++)
                        {
                            for (int y = 0; y < 6; y++)
                            {
                                if (ownership[x][y] == oldOwner) ownership[x][y] = ownership[location[0]][location[1]];
                            }
                        }

                        numUnique--;
                    }
                }
            }
        }

        for (int x = 0; x < 6; x++)
        {
            for (int y = 0; y < 6; y++)
            {
                if (grid[x*2+1][y*2+1])
                {
                    int num = 0;
                    if (x > 0 && grid[x * 2][y * 2 + 1]) num++;
                    if (x < 5 && grid[x * 2 + 2][y * 2 + 1]) num++;
                    if (y > 0 && grid[x * 2 + 1][y * 2]) num++;
                    if (y < 5 && grid[x * 2 + 1][y * 2 + 2]) num++;

                    if (num == 1 && Random.Range(0, 3) > 0)
                    {
                        bool added = false;
                        List<int> order = new List<int>() { 0, 1, 2, 3 };
                        while (order.Count > 0)
                        {
                            int pos = Random.Range(0, order.Count);
                            int dir = order[pos];
                            order.RemoveAt(pos);

                            if (dir == 0 && x > 0)
                            {
                                if (grid[x * 2 - 1][y * 2 + 1] && !grid[x * 2][y * 2 + 1])
                                {
                                    if (!added || Random.Range(0, 4) == 0)
                                    {
                                        added = true;
                                        grid[x * 2 - 1][y * 2 + 1] = true;
                                        grid[x * 2][y * 2 + 1] = true;
                                    }
                                }
                            }
                            if (dir == 1 && x < 5)
                            {
                                if (grid[x * 2 + 3][y * 2 + 1] && !grid[x * 2 + 2][y * 2 + 1])
                                {
                                    if (!added || Random.Range(0, 4) == 0)
                                    {
                                        added = true;
                                        grid[x * 2 + 2][y * 2 + 1] = true;
                                        grid[x * 2 + 3][y * 2 + 1] = true;
                                    }
                                }
                            }
                            if (dir == 2 && y > 0)
                            {
                                if (grid[x * 2 + 1][y * 2 - 1] && !grid[x * 2 + 1][y * 2])
                                {
                                    if (!added || Random.Range(0, 4) == 0)
                                    {
                                        added = true;
                                        grid[x * 2 + 1][y * 2 - 1] = true;
                                        grid[x * 2 + 1][y * 2] = true;
                                    }
                                }
                            }
                            if (dir == 3 && y < 5)
                            {
                                if (grid[x * 2 + 1][y * 2 + 3] && !grid[x * 2 + 1][y * 2 + 2])
                                {
                                    if (!added || Random.Range(0, 4) == 0)
                                    {
                                        added = true;
                                        grid[x * 2 + 1][y * 2 + 2] = true;
                                        grid[x * 2 + 1][y * 2 + 3] = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        PlayFieldType = new int[6][];
        PlayFieldState = new int[6][];
        for (int x = 0; x < 6; x++)
        {
            PlayFieldType[x] = new int[6];
            PlayFieldState[x] = new int[6];
            for (int y = 0; y < 6; y++)
            {
                int x2 = x * 2 + 1;
                int y2 = y * 2 + 1;
                if (grid[x2][y2-1])
                {
                    if (grid[x2][y2+1])
                    {
                        if (grid[x2-1][y2])
                        {
                            if (grid[x2+1][y2])
                            {
                                //UP DOWN LEFT RIGHT
                                PlayFieldType[x][y] = 3;
                                PlayFieldState[x][y] = 0;
                            }
                            else
                            {
                                //UP DOWN LEFT -RIGHT
                                PlayFieldType[x][y] = 2;
                                PlayFieldState[x][y] = 2;
                            }
                        }
                        else
                        {
                            if (grid[x2+1][y2])
                            {
                                //UP DOWN -LEFT RIGHT
                                PlayFieldType[x][y] = 2;
                                PlayFieldState[x][y] = 0;
                            }
                            else
                            {
                                //UP DOWN -LEFT -RIGHT
                                PlayFieldType[x][y] = 0;
                                PlayFieldState[x][y] = 0;
                            }
                        }
                    }
                    else
                    {
                        if (grid[x2-1][y2])
                        {
                            if (grid[x2+1][y2])
                            {
                                //UP -DOWN LEFT RIGHT
                                PlayFieldType[x][y] = 2;
                                PlayFieldState[x][y] = 3;
                            }
                            else
                            {
                                //UP -DOWN LEFT -RIGHT
                                PlayFieldType[x][y] = 1;
                                PlayFieldState[x][y] = 1;
                            }
                        }
                        else
                        {
                            if (grid[x2+1][y2])
                            {
                                //UP -DOWN -LEFT RIGHT
                                PlayFieldType[x][y] = 1;
                                PlayFieldState[x][y] = 2;
                            }
                            else
                            {
                                //UP -DOWN -LEFT -RIGHT
                                PlayFieldType[x][y] = 4;
                                PlayFieldState[x][y] = 2;
                            }
                        }
                    }
                }
                else
                {
                    if (grid[x2][y2+1])
                    {
                        if (grid[x2-1][y2])
                        {
                            if (grid[x2+1][y2])
                            {
                                //-UP DOWN LEFT RIGHT
                                PlayFieldType[x][y] = 2;
                                PlayFieldState[x][y] = 1;
                            }
                            else
                            {
                                //-UP DOWN LEFT -RIGHT
                                PlayFieldType[x][y] = 1;
                                PlayFieldState[x][y] = 0;
                            }
                        }
                        else
                        {
                            if (grid[x2+1][y2])
                            {
                                //-UP DOWN -LEFT RIGHT
                                PlayFieldType[x][y] = 1;
                                PlayFieldState[x][y] = 3;
                            }
                            else
                            {
                                //-UP DOWN -LEFT -RIGHT
                                PlayFieldType[x][y] = 4;
                                PlayFieldState[x][y] = 0;
                            }
                        }
                    }
                    else
                    {
                        if (grid[x2-1][y2])
                        {
                            if (grid[x2+1][y2])
                            {
                                //-UP -DOWN LEFT RIGHT
                                PlayFieldType[x][y] = 0;
                                PlayFieldState[x][y] = 1;
                            }
                            else
                            {
                                //-UP -DOWN LEFT -RIGHT
                                PlayFieldType[x][y] = 4;
                                PlayFieldState[x][y] = 1;
                            }
                        }
                        else
                        {
                            if (grid[x2+1][y2])
                            {
                                //-UP -DOWN -LEFT RIGHT
                                PlayFieldType[x][y] = 4;
                                PlayFieldState[x][y] = 3;
                            }
                            else
                            {
                                //-UP -DOWN -LEFT -RIGHT
                                PlayFieldType[x][y] = -1;
                                if (x == 0 || x == 5 || y == 0 || y == 5)
                                {
                                    bool nearEndPoint = false;
                                    if (x == 0)
                                    {
                                        if (EntryLocations[0] - 1 == y || EntryLocations[1] - 1 == y || EntryLocations[2] - 1 == y || EntryLocations[3] - 1 == y) nearEndPoint = true;
                                    }
                                    if (x == 5)
                                    {
                                        if (ExitLocations[0] - 1 == y || ExitLocations[1] - 1 == y || ExitLocations[2] - 1 == y || ExitLocations[3] - 1 == y) nearEndPoint = true;
                                    }
                                    if ((x == 0 || x == 5) && (y == 0 || y == 5))
                                    {
                                        if (nearEndPoint)
                                        {
                                            if (Random.Range(0, 3) == 0) PlayFieldType[x][y] = 2;
                                            else PlayFieldType[x][y] = 1;
                                        }
                                        else
                                        {
                                            if (Random.Range(0, 3) == 0) PlayFieldType[x][y] = 1;
                                            else PlayFieldType[x][y] = 4;
                                        }
                                    }
                                    else
                                    {
                                        if (nearEndPoint)
                                        {
                                            if (Random.Range(0, 3) == 0) PlayFieldType[x][y] = 3;
                                            else if (Random.Range(0, 3) == 0) PlayFieldType[x][y] = 1;
                                            else PlayFieldType[x][y] = 2;
                                        }
                                        else
                                        {
                                            if (Random.Range(0, 3) == 0) PlayFieldType[x][y] = 2;
                                            else if (Random.Range(0, 3) == 0) PlayFieldType[x][y] = 4;
                                            else PlayFieldType[x][y] = 1;
                                        }
                                    }
                                }
                                else
                                {
                                    if (Random.Range(0, 3) == 0) PlayFieldType[x][y] = 0;
                                    else
                                    {
                                        PlayFieldType[x][y] = Random.Range(-1, 3);
                                        if (PlayFieldType[x][y] == -1) PlayFieldType[x][y] = Random.Range(0, 5);
                                    }
                                }
                                PlayFieldState[x][y] = Random.Range(0, 4);
                            }
                        }
                    }
                }
                PlayFieldState[x][y] = Random.Range(0, 4);
                ApplyModel(Buttons[x][y].gameObject, PlayFieldType[x][y], PlayFieldState[x][y], (x + y) % 2 == 1);
                x2 = x;
                y2 = y;
                Buttons[x][y].OnInteract += delegate() { HandleInteract(x2, y2); return false; };
            }
        }
    }

    private int[] GetConnections(int x, int y)
    {
        if (y < 0 || y > 5) return new int[0];
        if (x < -1 || x > 6) return new int[0];
        if (x == -1)
        {
            for (int a = 0; a < 4; a++)
            {
                if (EntryLocations[a] - 1 == y)
                {
                    if (ActiveIn[a]) return new int[] { 1 };
                    return new int[0];
                }
            }
            return new int[0];
        }
        if (x == 6)
        {
            for (int a = 0; a < 4; a++)
            {
                if (ExitLocations[a] - 1 == y)
                {
                    if (ActiveOut[a]) return new int[] { 3 };
                    return new int[0];
                }
            }
            return new int[0];
        }

        int type = PlayFieldType[x][y];
        int rot = PlayFieldState[x][y] % 4;
        if (type == -1) return new int[0];
        else if (type == 0) return new int[] { rot, (rot + 2) % 4 };
        else if (type == 1) return new int[] { (rot + 2) % 4, (rot + 3) % 4 };
        else if (type == 2) return new int[] { rot, (rot + 1) % 4, (rot + 2) % 4 };
        else if (type == 3) return new int[] { 0, 1, 2, 3 };
        else return new int[] { (rot + 2) % 4 };
    }

    public bool HandleCheck()
    {
        if (Solved) return false;

        List<int[]> positions = new List<int[]>();
        for (int a = 0; a < 4; a++)
        {
            if (ActiveIn[a])
            {
                positions.Add(new int[] { 0, EntryLocations[a] - 1 });
            }
            if (ActiveOut[a])
            {
                positions.Add(new int[] { 5, ExitLocations[a] - 1 });
            }
        }

        bool[][] done = new bool[6][];
        for (int a = 0; a < 6; a++) done[a] = new bool[6];

        List<int[]> allPipes = new List<int[]>();

        while(positions.Count > 0)
        {
            int[] pos = positions[0];
            positions.RemoveAt(0);
            if (done[pos[0]][pos[1]]) continue;

            allPipes.Add(pos);
            int[] conn = GetConnections(pos[0], pos[1]);
            foreach (int dir in conn)
            {
                if (dir == 0)
                {
                    int[] otherConn = GetConnections(pos[0], pos[1] - 1);
                    bool found = false;
                    foreach (int otherDir in otherConn)
                    {
                        if (otherDir == 2)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        GetComponent<KMBombModule>().HandleStrike();
                        return false;
                    }
                    if (pos[1] > 0) positions.Add(new int[] { pos[0], pos[1] - 1 });
                    done[pos[0]][pos[1]] = true;
                }
                if (dir == 1)
                {
                    int[] otherConn = GetConnections(pos[0] + 1, pos[1]);
                    bool found = false;
                    foreach (int otherDir in otherConn)
                    {
                        if (otherDir == 3)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        GetComponent<KMBombModule>().HandleStrike();
                        return false;
                    }
                    if (pos[0] < 5) positions.Add(new int[] { pos[0] + 1, pos[1] });
                    done[pos[0]][pos[1]] = true;
                }
                if (dir == 2)
                {
                    int[] otherConn = GetConnections(pos[0], pos[1] + 1);
                    bool found = false;
                    foreach (int otherDir in otherConn)
                    {
                        if (otherDir == 0)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        GetComponent<KMBombModule>().HandleStrike();
                        return false;
                    }
                    if (pos[1] < 5) positions.Add(new int[] { pos[0], pos[1] + 1 });
                    done[pos[0]][pos[1]] = true;
                }
                if (dir == 3)
                {
                    int[] otherConn = GetConnections(pos[0] - 1, pos[1]);
                    bool found = false;
                    foreach (int otherDir in otherConn)
                    {
                        if (otherDir == 1)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        GetComponent<KMBombModule>().HandleStrike();
                        return false;
                    }
                    if (pos[0] > 0) positions.Add(new int[] { pos[0] - 1, pos[1] });
                    done[pos[0]][pos[1]] = true;
                }
            }
        }

        Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
        GetComponent<KMBombModule>().HandlePass();
        Solved = true;
        foreach (int[] pos in allPipes)
        {
            if (pos[0] >= 0 && pos[0] <= 5 && pos[1] >= 0 && pos[1] <= 5)
                Buttons[pos[0]][pos[1]].transform.Find("Pipe").GetComponent<MeshRenderer>().material.color = ((pos[0] + pos[1]) % 2 == 1) ? new Color(0.1f, 0.6f, 1) : new Color(0.1f, 0.3f, 1);
        }
        return false;
    }
}