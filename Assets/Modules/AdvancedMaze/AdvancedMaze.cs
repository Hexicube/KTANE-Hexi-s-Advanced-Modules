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
    public static int loggingID = 1;
    public int thisLoggingID;

    private static char[][] PIPE_SEGMENTS = new char[][]
    {
        new char[]{'║','═','║','═'},
        new char[]{'╗','╝','╚','╔'},
        new char[]{'╠','╦','╣','╩'},
        new char[]{'╬','╬','╬','╬'},
        new char[]{'╥','╡','╨','╞'},
        
    };

    private char GetPipeAscii(int type, int rot)
    {
        return PIPE_SEGMENTS[type][rot];
    }

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
    private GameObject[] EntryLeftList, EntryRightList;

    private int[][] PlayFieldType, PlayFieldState;
    private int[] EntryLocations, ExitLocations;

    public GameObject PipeStraight, PipeCorner, PipeT, PipeCross, PipeEnd;

    private bool[] ActiveIn, ActiveOut;

    private bool Solved;
    private float fadeState;
    private bool[][] fadeList;

    public Material BaseMaterial;

    public GameObject PlayField;
    private Mesh MergedMesh;
    private bool PipesAreMerged = false;

    private static Material whiteMat, lightGreyMat, redMat, yellowMat, greenMat, blueMat, lightBlueMat, darkBlueMat;

    public void SetMergedMode(bool on) {
        if(on && !PipesAreMerged) {
            if(MergedMesh == null) {
                MergedMesh = new Mesh();

                CombineInstance[] ciList = new CombineInstance[36];
                for(int x = 0; x < 6; x++) {
                    for(int y = 0; y < 6; y++) {
                        CombineInstance ci = new CombineInstance();
                        Transform pipe = Buttons[x][y].transform.Find("Pipe");
                        ci.mesh = pipe.gameObject.GetComponent<MeshFilter>().sharedMesh;

                        ci.transform = pipe.transform.localToWorldMatrix;

                        if((x+y) % 2 == 1) ci.subMeshIndex = 0;
                        else ci.subMeshIndex = 1;
                        if(Solved && fadeList[x][y]) ci.subMeshIndex += 2;

                        ciList[x*6+y] = ci;
                    }
                }
                Mesh[] subMesh = new Mesh[4];
                for(int a = 0; a < 4; a++) {
                    List<CombineInstance> list = new List<CombineInstance>();
                    subMesh[a] = new Mesh();
                    for(int b = 0; b < 36; b++) {
                        if(ciList[b].subMeshIndex == a) {
                            list.Add(ciList[b]);
                        }
                    }
                    CombineInstance[] list2 = list.ToArray();
                    for(int b = 0; b < list2.Length; b++) list2[b].subMeshIndex = 0;
                    subMesh[a].CombineMeshes(list2);
                }

                ciList = new CombineInstance[2];
                for(int a = 0; a < 2; a++) {
                    ciList[a].transform = PlayField.transform.worldToLocalMatrix;
                    ciList[a].mesh = subMesh[a + (Solved?2:0)];
                }
                MergedMesh.CombineMeshes(ciList, false);
                ;
            }
            for(int x = 0; x < 6; x++) {
                for(int y = 0; y < 6; y++) {
                    Buttons[x][y].transform.Find("Pipe").gameObject.SetActive(false);
                }
            }
            MeshFilter mf = PlayField.GetComponent<MeshFilter>();
            mf.mesh = MergedMesh;
            if(Solved) PlayField.GetComponent<MeshRenderer>().materials = new Material[]{lightBlueMat, darkBlueMat};
        }
        if(!on && PipesAreMerged) {
            for(int x = 0; x < 6; x++) {
                for(int y = 0; y < 6; y++) {
                    Buttons[x][y].transform.Find("Pipe").gameObject.SetActive(true);
                }
            }

            PlayField.GetComponent<MeshFilter>().mesh = null;
        }
        PlayField.GetComponent<MeshRenderer>().enabled = on;
        PipesAreMerged = on;
    }

    private void ApplyModel(GameObject button, int type, int rot, bool light) {
        ApplyModel(button, type, rot, light ? whiteMat : lightGreyMat);
    }

    private void ApplyModel(GameObject button, int type, int rot, Material pipeMat) {
        if (type != -1) {
            GameObject g = null;
            if (type == 0) g = Instantiate(PipeStraight);
            else if (type == 1) g = Instantiate(PipeCorner);
            else if (type == 2) g = Instantiate(PipeT);
            else if (type == 3) g = Instantiate(PipeCross);
            else g = Instantiate(PipeEnd);
            g.SetActive(false);

            g.transform.name = "Pipe";
            g.transform.parent = button.transform;
            g.transform.localPosition = new Vector3(0, 0.5f, 0);
            g.transform.localScale = new Vector3(1, 4f, 1);
            g.transform.localEulerAngles = new Vector3(0, 90f * rot, 0);
            g.GetComponent<MeshRenderer>().material = pipeMat;
            g.SetActive(true);
        }
    }

    void Update()
    {
        if(fadeState > 0 && fadeState < 3)
        {
            float start = fadeState - 2;
            float end = start + Time.deltaTime;
            if(start < 0) start = 0;
            if(start > 1) start = 1;
            if(end < 0) end = 0;
            if(end > 1) end = 1;
            if(end > start)
            {
                Vector3 move = new Vector3(0, (end-start)*1.5f, 0);
                for(int x = 0; x < 6; x++)
                {
                    for(int y = 0; y < 6; y++)
                    {
                        if(!fadeList[x][y])
                        {
                            Buttons[x][y].transform.Find("Pipe").localPosition -= move;
                        }
                    }
                }
                for(int a = 0; a < 4; a++)
                {
                    if(!ActiveIn[a]) EntryLeftList[EntryLocations[a]-1].transform.Find("Pipe").localPosition -= move;
                    if(!ActiveOut[a]) EntryRightList[ExitLocations[a]-1].transform.Find("Pipe").localPosition -= move;
                }
                if(end == 1) SetMergedMode(true);
            }
            fadeState += Time.deltaTime;
        }
    }

    void Awake()
    {

        if(whiteMat == null) {
            whiteMat     = new Material(BaseMaterial); whiteMat.color     = new Color(1, 1, 1);
            lightGreyMat = new Material(BaseMaterial); lightGreyMat.color = new Color(0.6f, 0.6f, 0.6f);
            redMat       = new Material(BaseMaterial); redMat.color       = new Color(1, 0.1f, 0.1f);
            yellowMat    = new Material(BaseMaterial); yellowMat.color    = new Color(1, 1, 0.1f);
            greenMat     = new Material(BaseMaterial); greenMat.color     = new Color(0.1f, 0.8f, 0.1f);
            blueMat      = new Material(BaseMaterial); blueMat.color      = new Color(0.1f, 0.4f, 1);
            lightBlueMat = new Material(BaseMaterial); lightBlueMat.color = new Color(0.1f, 0.6f, 1);
            darkBlueMat  = new Material(BaseMaterial); darkBlueMat.color  = new Color(0.1f, 0.3f, 1);
        }

        thisLoggingID = loggingID++;

        transform.Find("Background").GetComponent<MeshRenderer>().material.color = new Color(1, 0.1f, 0.1f);

        Buttons = new KMSelectable[][]{
            new KMSelectable[]{ButtonA1, ButtonB1, ButtonC1, ButtonD1, ButtonE1, ButtonF1},
            new KMSelectable[]{ButtonA2, ButtonB2, ButtonC2, ButtonD2, ButtonE2, ButtonF2},
            new KMSelectable[]{ButtonA3, ButtonB3, ButtonC3, ButtonD3, ButtonE3, ButtonF3},
            new KMSelectable[]{ButtonA4, ButtonB4, ButtonC4, ButtonD4, ButtonE4, ButtonF4},
            new KMSelectable[]{ButtonA5, ButtonB5, ButtonC5, ButtonD5, ButtonE5, ButtonF5},
            new KMSelectable[]{ButtonA6, ButtonB6, ButtonC6, ButtonD6, ButtonE6, ButtonF6}
        };
        
        EntryLeftList  = new GameObject[]{EntryLeft1,  EntryLeft2,  EntryLeft3,  EntryLeft4,  EntryLeft5,  EntryLeft6};
        EntryRightList = new GameObject[]{EntryRight1, EntryRight2, EntryRight3, EntryRight4, EntryRight5, EntryRight6};

        ButtonCheck.GetComponent<MeshRenderer>().material.color = new Color(0.91f, 0.88f, 0.86f);

        Invoke("Init", 0.25f + 0.01f * (thisLoggingID % 100));
    }

    void HandleInteract(int x, int y)
    {
        if (Solved) return;
        Buttons[x][y].AddInteractionPunch(0.1f);

        int rot = (PlayFieldState[x][y] + 1) % 4;
        PlayFieldState[x][y] = rot;

        if (PlayFieldType[x][y] != -1)
        {
            Transform t = Buttons[x][y].gameObject.transform.Find("Pipe");
            t.localEulerAngles = new Vector3(0, rot * 90f, 0);
        }
        
        Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);

        if(MergedMesh != null) DestroyImmediate(MergedMesh);
        MergedMesh = null;
    }

    void Init()
    {
        PlayField.transform.Find("Backing").GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);
        PlayField.transform.Find("Backing").Find("Left").GetComponent<MeshRenderer>().material.color = new Color(0.3f, 0.3f, 0.3f);
        PlayField.transform.Find("Backing").Find("Right").GetComponent<MeshRenderer>().material.color = new Color(0.3f, 0.3f, 0.3f);

        ButtonCheck.OnInteract += HandleCheck;

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

        Material[] matList = new Material[]{
            redMat, yellowMat, greenMat, blueMat
        };

        for (int a = 0; a < 4; a++)
        {
            if (EntryLocations[a] == 1) ApplyModel(EntryLeft1, 4, 3, matList[a]);
            if (EntryLocations[a] == 2) ApplyModel(EntryLeft2, 4, 3, matList[a]);
            if (EntryLocations[a] == 3) ApplyModel(EntryLeft3, 4, 3, matList[a]);
            if (EntryLocations[a] == 4) ApplyModel(EntryLeft4, 4, 3, matList[a]);
            if (EntryLocations[a] == 5) ApplyModel(EntryLeft5, 4, 3, matList[a]);
            if (EntryLocations[a] == 6) ApplyModel(EntryLeft6, 4, 3, matList[a]);
        }

        for (int a = 0; a < 4; a++)
        {
            if (ExitLocations[a] == 1) ApplyModel(EntryRight1, 4, 1, matList[a]);
            if (ExitLocations[a] == 2) ApplyModel(EntryRight2, 4, 1, matList[a]);
            if (ExitLocations[a] == 3) ApplyModel(EntryRight3, 4, 1, matList[a]);
            if (ExitLocations[a] == 4) ApplyModel(EntryRight4, 4, 1, matList[a]);
            if (ExitLocations[a] == 5) ApplyModel(EntryRight5, 4, 1, matList[a]);
            if (ExitLocations[a] == 6) ApplyModel(EntryRight6, 4, 1, matList[a]);
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

        int batteries = 0;
        data = Info.QueryWidgets(KMBombInfo.QUERYKEY_GET_BATTERIES, null);
        foreach (string response in data)
        {
            Dictionary<string, int> responseDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(response);
            batteries += responseDict["numbatteries"];
        }

        string serial = null;
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

        ActiveIn = new bool[4];
        ActiveOut = new bool[4];

        int val = 0;
        if (serial.Contains("1"))
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR+] Serial contains a 1");
            val++;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR-] Serial doesn't contain a 1");
        if (portCount.ContainsKey("RJ45") && portCount["RJ45"] == 1)
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR+] Bomb has exactly 1 RJ-45 port");
            val++;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR-] Bomb does not have exactly 1 RJ-45 port");
        if (duplicatePort)
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA+] Bomb has duplicate ports");
            val--;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA-] Bomb doesn't have duplicate ports");
        if (serialDupe)
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA+] Serial contains a duplicate");
            val--;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA-] Serial doesn't contain a duplicate");
        if (val > 0)
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] RED IN is active");
            ActiveIn[0] = true;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] RED IN is inactive");

        val = 0;
        if (serial.Contains("2"))
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR+] Serial contains a 2");
            val++;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR-] Serial doesn't contain a 2");
        if (portCount.ContainsKey("StereoRCA"))
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR+] Bomb has a Stereo RCA port");
            val++;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR-] Bomb doesn't have a Stereo RCA port");
        if (!duplicatePort)
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA+] Bomb doesn't have duplicate ports");
            val--;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA-] Bomb has duplicate ports");
        if (serial.Contains("1") || serial.Contains("L"))
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA+] Serial contains a 1 or an L");
            val--;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA-] Serial doesn't contain a 1 or an L");
        if (val > 0)
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] YELLOW IN is active");
            ActiveIn[1] = true;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] YELLOW IN is inactive");

        val = 0;
        if (serialNumbers > 2)
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR+] Serial contains at least 3 numbers");
            val++;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR-] Serial doesn't contain at least 3 numbers");
        if (portCount.ContainsKey("DVI"))
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR+] Bomb has a DVI-D port");
            val++;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR-] Bomb doesn't have a DVI-D port");
        if (!ActiveIn[0])
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA+] RED IN is inactive");
            val--;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA-] RED IN is active");
        if (!ActiveIn[1])
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA+] YELLOW IN is inactive");
            val--;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA-] YELLOW IN is active");
        if (val > 0)
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] GREEN IN is active");
            ActiveIn[2] = true;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] GREEN IN is inactive");

        if (!ActiveIn[0] && !ActiveIn[1] && !ActiveIn[2])
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] BLUE IN is active by default");
            ActiveIn[3] = true;
        }
        else
        {
            val = 0;
            if (portCount.Count >= 4)
            {
                Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR+] Bomb has at least 4 port types");
                val++;
            }
            else Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR-] Bomb doesn't have at least 4 port types");
            if (batteries >= 4)
            {
                Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR+] Bomb has at least 4 batteries");
                val++;
            }
            else Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR-] Bomb doesn't have at least 4 batteries");
            if (portCount.Count == 0)
            {
                Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA+] Bomb has no ports");
                val--;
            }
            else Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA-] Bomb has ports");
            if (batteries == 0)
            {
                Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA+] Bomb has no batteries");
                val--;
            }
            else Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA-] Bomb has batteries");
            if (val > 0)
            {
                Debug.Log("[Plumbing #"+thisLoggingID+"] BLUE IN is active");
                ActiveIn[3] = true;
            }
            else Debug.Log("[Plumbing #"+thisLoggingID+"] BLUE IN is inactive");
        }

        int sum = 0;
        if (ActiveIn[0]) sum++;
        if (ActiveIn[1]) sum++;
        if (ActiveIn[2]) sum++;
        if (ActiveIn[3]) sum++;

        val = 0;
        if (portCount.ContainsKey("Serial"))
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR+] Bomb has a Serial port");
            val++;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR-] Bomb doesn't have a Serial port");
        if (batteries == 1)
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR+] Bomb has exactly 1 battery");
            val++;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR-] Bomb doesn't have exactly 1 battery");
        if (serialNumbers > 2)
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA+] Serial has more than 2 numbers");
            val--;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA-] Serial doesn't have more than 2 numbers");
        if (sum > 2)
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA+] More than 2 inputs are active");
            val--;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA-] No more than 2 inputs are active");
        if (val > 0)
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] RED OUT is active");
            ActiveOut[0] = true;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] RED OUT is inactive");

        val = 0;
        if (duplicatePort)
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR+] Bomb has duplicate ports");
            val++;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR-] Bomb doesn't have duplicate ports");
        if (serial.Contains("4") || serial.Contains("8"))
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR+] Serial contains a 4 or an 8");
            val++;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR-] Serial doesn't contain a 4 or an 8");
        if (!serial.Contains("2"))
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA+] Serial doesn't contain a 2");
            val--;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA-] Serial contains a 2");
        if (ActiveIn[2])
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA+] GREEN IN is active");
            val--;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA-] GREEN IN is inactive");
        if (val > 0)
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] YELLOW OUT is active");
            ActiveOut[1] = true;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] YELLOW OUT is inactive");

        val = 0;
        if (sum == 3)
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR+] Exactly 3 inputs are active");
            val++;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR-] Not exactly 3 inputs are active");
        if (portSum == 3)
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR+] Bomb has exactly 3 ports");
            val++;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR-] Bomb doesn't have exactly 3 ports");
        if (portSum < 3)
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA+] Bomb has less than 3 ports");
            val--;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA-] Bomb has at least 3 ports");
        if (serialNumbers > 3)
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA+] Serial has more than 3 numbers");
            val--;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA-] Serial doesn't have more than 3 numbers");
        if (val > 0)
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] GREEN OUT is active");
            ActiveOut[2] = true;
        }
        else Debug.Log("[Plumbing #"+thisLoggingID+"] GREEN OUT is inactive");

        if (!ActiveOut[0] && !ActiveOut[1] && !ActiveOut[2])
        {
            Debug.Log("[Plumbing #"+thisLoggingID+"] BLUE OUT is active by default");
            ActiveOut[3] = true;
        }
        else
        {
            val = 0;
            if (sum == 4)
            {
                Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR+] All inputs are active");
                val++;
            }
            else Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR-] Not all inputs are active");
            if (!ActiveOut[0] || !ActiveOut[1] || !ActiveOut[2])
            {
                Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR+] At least one other output is inactive");
                val++;
            }
            else Debug.Log("[Plumbing #"+thisLoggingID+"] [FOR-] All other outputs are active");
            if (batteries < 2)
            {
                Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA+] Bomb has less than 2 batteries");
                val--;
            }
            else Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA-] Bomb has at least 2 batteries");
            if (!portCount.ContainsKey("Parallel"))
            {
                Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA+] Bomb has no Parallel port");
                val--;
            }
            else Debug.Log("[Plumbing #"+thisLoggingID+"] [AGA-] Bomb has a Parallel port");
            if (val > 0)
            {
                Debug.Log("[Plumbing #"+thisLoggingID+"] BLUE OUT is active");
                ActiveOut[3] = true;
            }
            else Debug.Log("[Plumbing #"+thisLoggingID+"] BLUE OUT is inactive");
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
                if (grid[x * 2 + 1][y * 2 + 1])
                {
                    int num = 0;
                    if (grid[x * 2][y * 2 + 1]) num++;
                    if (grid[x * 2 + 2][y * 2 + 1]) num++;
                    if (grid[x * 2 + 1][y * 2]) num++;
                    if (grid[x * 2 + 1][y * 2 + 2]) num++;

                    if (num == 1)
                    {
                        if (Random.Range(0, 4) > 0)
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
                            if (!added && Random.Range(0, 3) > 0)
                            {
                                grid[x * 2][y * 2 + 1] = false;
                                grid[x * 2 + 2][y * 2 + 1] = false;
                                grid[x * 2 + 1][y * 2] = false;
                                grid[x * 2 + 1][y * 2 + 2] = false;
                                grid[x * 2 + 1][y * 2 + 1] = false;
                            }
                        }
                        else
                        {
                            grid[x * 2][y * 2 + 1] = false;
                            grid[x * 2 + 2][y * 2 + 1] = false;
                            grid[x * 2 + 1][y * 2] = false;
                            grid[x * 2 + 1][y * 2 + 2] = false;
                            grid[x * 2 + 1][y * 2 + 1] = false;
                        }
                    }
                }
            }
        }

        char[][] debugSolved = new char[6][];
        char[][] debugShown = new char[6][];

        PlayFieldType = new int[6][];
        PlayFieldState = new int[6][];
        for (int x = 0; x < 6; x++)
        {
            debugSolved[x] = new char[6];
            debugShown[x] = new char[6];

            PlayFieldType[x] = new int[6];
            PlayFieldState[x] = new int[6];
            for (int y = 0; y < 6; y++)
            {
                bool addedSolved = false;
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
                                debugSolved[x][y] += '┼';
                                addedSolved = true;
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
                            }
                        }
                    }
                }
                if (!addedSolved)
                {
                    debugSolved[x][y] = GetPipeAscii(PlayFieldType[x][y], PlayFieldState[x][y]);
                }
                PlayFieldState[x][y] = Random.Range(0, 4);
                debugShown[x][y] = GetPipeAscii(PlayFieldType[x][y], PlayFieldState[x][y]);
                ApplyModel(Buttons[x][y].gameObject, PlayFieldType[x][y], PlayFieldState[x][y], (x + y) % 2 == 1);
                x2 = x;
                y2 = y;
                Buttons[x][y].OnInteract += delegate() { HandleInteract(x2, y2); return false; };
            }
        }

        string debugShownText = "";
        string debugSolvedText = "";
        for (int y = 0; y < 6; y++)
        {
            for(int x = 0; x < 6; x++)
            {
                if(x == 0) {
                    debugShownText += "[Plumbing #"+thisLoggingID+"] ";
                    debugSolvedText += "[Plumbing #"+thisLoggingID+"] ";
                }
                debugShownText += debugShown[x][y];
                debugSolvedText += debugSolved[x][y];
                if (x == 5)
                {
                    debugShownText += "\n";
                    debugSolvedText += "\n";
                }
            }
        }

        Debug.Log("[Plumbing #"+thisLoggingID+"] Shown pipes:\n" + debugShownText);
        Debug.Log("[Plumbing #"+thisLoggingID+"] Intended solution:\n" + debugSolvedText);

        GetComponent<KMSelectable>().OnInteract += delegate(){if(!Solved) SetMergedMode(false); return true;};
        GetComponent<KMSelectable>().OnCancel += delegate(){if(!Solved) SetMergedMode(true); return true;};
        MeshRenderer mr = PlayField.GetComponent<MeshRenderer>();
        mr.sharedMaterials = new Material[]{whiteMat, lightGreyMat};
        SetMergedMode(true);
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
        
        string curState = "";
        bool[][] scanState = new bool[6][];
        for (int x = 0; x < 6; x++)
        {
            scanState[x] = new bool[6];
        }

        ButtonCheck.AddInteractionPunch();

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
            scanState[pos[0]][pos[1]] = true;

            allPipes.Add(pos);
            int[] conn = GetConnections(pos[0], pos[1]);
            foreach (int dir in conn)
            {
                if (dir == 0)
                {
                    if(pos[1] > 0) scanState[pos[0]][pos[1] - 1] = true;

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
                        Debug.Log("[Plumbing #"+thisLoggingID+"] Incorrect pipe at: " + (pos[0]+1) + ":" + (pos[1]+1));

                        curState = "";
                        for (int y = 0; y < 6; y++)
                        {
                            for(int x = 0; x < 6; x++)
                            {
                                if(x == 0) curState += "[Plumbing #"+thisLoggingID+"] ";
                                if(scanState[x][y]) curState += GetPipeAscii(PlayFieldType[x][y], PlayFieldState[x][y]);
                                else curState += "┼";
                                if (x == 5) curState += "\n";
                            }
                        }

                        Debug.Log("[Plumbing #"+thisLoggingID+"] Scanned state:\n" + curState);

                        GetComponent<KMBombModule>().HandleStrike();
                        return false;
                    }
                    if (pos[1] > 0) positions.Add(new int[] { pos[0], pos[1] - 1 });
                    done[pos[0]][pos[1]] = true;
                }
                if (dir == 1)
                {
                    if(pos[0] < 5) scanState[pos[0] + 1][pos[1]] = true;

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
                        Debug.Log("[Plumbing #"+thisLoggingID+"] Incorrect pipe at: " + (pos[0]+1) + ":" + (pos[1]+1));

                        curState = "";
                        for (int y = 0; y < 6; y++)
                        {
                            for(int x = 0; x < 6; x++)
                            {
                                if(x == 0) curState += "[Plumbing #"+thisLoggingID+"] ";
                                if(scanState[x][y]) curState += GetPipeAscii(PlayFieldType[x][y], PlayFieldState[x][y]);
                                else curState += "┼";
                                if (x == 5) curState += "\n";
                            }
                        }

                        Debug.Log("[Plumbing #"+thisLoggingID+"] Scanned state:\n" + curState);

                        GetComponent<KMBombModule>().HandleStrike();
                        return false;
                    }
                    if (pos[0] < 5) positions.Add(new int[] { pos[0] + 1, pos[1] });
                    done[pos[0]][pos[1]] = true;
                }
                if (dir == 2)
                {
                    if(pos[1] < 5) scanState[pos[0]][pos[1] + 1] = true;

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
                        Debug.Log("[Plumbing #"+thisLoggingID+"] Incorrect pipe at: " + (pos[0]+1) + ":" + (pos[1]+1));

                        curState = "";
                        for (int y = 0; y < 6; y++)
                        {
                            for(int x = 0; x < 6; x++)
                            {
                                if(x == 0) curState += "[Plumbing #"+thisLoggingID+"] ";
                                if(scanState[x][y]) curState += GetPipeAscii(PlayFieldType[x][y], PlayFieldState[x][y]);
                                else curState += "┼";
                                if (x == 5) curState += "\n";
                            }
                        }

                        Debug.Log("[Plumbing #"+thisLoggingID+"] Scanned state:\n" + curState);

                        GetComponent<KMBombModule>().HandleStrike();
                        return false;
                    }
                    if (pos[1] < 5) positions.Add(new int[] { pos[0], pos[1] + 1 });
                    done[pos[0]][pos[1]] = true;
                }
                if (dir == 3)
                {
                    if(pos[0] > 0) scanState[pos[0] - 1][pos[1]] = true;

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
                        Debug.Log("[Plumbing #"+thisLoggingID+"] Incorrect pipe at: " + (pos[0]+1) + ":" + (pos[1]+1));

                        curState = "";
                        for (int y = 0; y < 6; y++)
                        {
                            for(int x = 0; x < 6; x++)
                            {
                                if(x == 0) curState += "[Plumbing #"+thisLoggingID+"] ";
                                if(scanState[x][y]) curState += GetPipeAscii(PlayFieldType[x][y], PlayFieldState[x][y]);
                                else curState += "┼";
                                if (x == 5) curState += "\n";
                            }
                        }

                        Debug.Log("[Plumbing #"+thisLoggingID+"] Scanned state:\n" + curState);

                        GetComponent<KMBombModule>().HandleStrike();
                        return false;
                    }
                    if (pos[0] > 0) positions.Add(new int[] { pos[0] - 1, pos[1] });
                    done[pos[0]][pos[1]] = true;
                }
            }
        }

        //Precaution: Prevent a solution that doesn't physically connect in/out pipes but has valid piping next to them.
        for (int a = 0; a < 4; a++)
        {
            if (ActiveIn[a])
            {
                int pos = EntryLocations[a] - 1;
                int[] conn = GetConnections(0, pos);
                bool found = false;
                foreach (int val in conn)
                {
                    if (val == 3)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    Debug.Log("[Plumbing #"+thisLoggingID+"] Incorrect pipe at: 0:" + (pos+1) + " (input disconnected)");

                    curState = "";
                    for (int y = 0; y < 6; y++)
                    {
                        for(int x = 0; x < 6; x++)
                        {
                            if(x == 0) curState += "[Plumbing #"+thisLoggingID+"] ";
                            if(scanState[x][y]) curState += GetPipeAscii(PlayFieldType[x][y], PlayFieldState[x][y]);
                            else curState += "┼";
                            if (x == 5) curState += "\n";
                        }
                    }

                    Debug.Log("[Plumbing #"+thisLoggingID+"] Scanned state:\n" + curState);

                    GetComponent<KMBombModule>().HandleStrike();
                    return false;
                }
            }
            if (ActiveOut[a])
            {
                int pos = ExitLocations[a] - 1;
                int[] conn = GetConnections(5, pos);
                bool found = false;
                foreach (int val in conn)
                {
                    if (val == 1)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    Debug.Log("[Plumbing #"+thisLoggingID+"] Incorrect pipe at: 7:" + (pos+1) + " (output disconnected)");

                    curState = "";
                    for (int y = 0; y < 6; y++)
                    {
                        for(int x = 0; x < 6; x++)
                        {
                            if(x == 0) curState += "[Plumbing #"+thisLoggingID+"] ";
                            if(scanState[x][y]) curState += GetPipeAscii(PlayFieldType[x][y], PlayFieldState[x][y]);
                            else curState += "┼";
                            if (x == 5) curState += "\n";
                        }
                    }

                    Debug.Log("[Plumbing #"+thisLoggingID+"] Scanned state:\n" + curState);

                    GetComponent<KMBombModule>().HandleStrike();
                    return false;
                }
            }
        }

        //Note: Solutions where the inputs and outputs are not actually linked are possible, provided the pipes are both ends are connected properly. However, this is incredibly rare and not worth preventing.

        Debug.Log("[Plumbing #"+thisLoggingID+"] Module solved.");

        for (int y = 0; y < 6; y++)
        {
            for(int x = 0; x < 6; x++)
            {
                if(x == 0) curState += "[Plumbing #"+thisLoggingID+"] ";
                if(scanState[x][y]) curState += GetPipeAscii(PlayFieldType[x][y], PlayFieldState[x][y]);
                else curState += "┼";
                if (x == 5) curState += "\n";
            }
        }

        Debug.Log("[Plumbing #"+thisLoggingID+"] Given solution:\n" + curState);

        Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
        GetComponent<KMBombModule>().HandlePass();
        Solved = true;

        SetMergedMode(false);
        MergedMesh = null;
        foreach (int[] pos in allPipes)
        {
            if (pos[0] >= 0 && pos[0] <= 5 && pos[1] >= 0 && pos[1] <= 5)
                Buttons[pos[0]][pos[1]].transform.Find("Pipe").GetComponent<MeshRenderer>().material = ((pos[0] + pos[1]) % 2 == 1) ? lightBlueMat : darkBlueMat;
        }

        fadeState = .001f;
        fadeList = scanState;

        return false;
    }

    //Twitch Plays support

    string TwitchHelpMessage = "Rotate pipes using 'rotate A3 B4 B2 ...'. Submit answer using 'submit'. Pipe positions use battleship notation, letters are A-F left to right and numbers are 1-6 top to bottom.";
    
    public void TwitchHandleForcedSolve() {
        Debug.Log("[Plumbing #"+thisLoggingID+"] Module forcibly solved.");

        Solved = true;

        SetMergedMode(false);
        MergedMesh = null;

        fadeState = .001f;
        fadeList = new bool[6][];
        for(int a = 0; a < 6; a++) fadeList[a] = new bool[6];

        GetComponent<KMNeedyModule>().HandlePass();
    }

    public IEnumerator ProcessTwitchCommand(string cmd) {
        if(cmd.Equals("submit")) {
            yield return "Plumbing";
            ButtonCheck.OnInteract();
            yield break;
        }
        else if(cmd.StartsWith("rotate ")) {
            string[] list = cmd.Substring(7).Split(' ');
            KMSelectable[] blist = new KMSelectable[list.Length];
            for(int a = 0; a < list.Length; a++) {
                string s = list[a];
                if(s.Length != 2) {
                    yield return "sendtochaterror Bad pipe position: '" + s + "'";
                    yield break;
                }

                int horz = s[0] - 'A';
                if(horz < 0 || horz > 5) {
                    yield return "sendtochaterror Bad pipe position: '" + s + "'";
                    yield break;
                }
                int vert = s[1] - '1';
                if(horz < 0 || horz > 5) {
                    yield return "sendtochaterror Bad pipe position: '" + s + "'";
                    yield break;
                }

                blist[a] = Buttons[vert][horz];
            }
            
            yield return "Plumbing";
            foreach(KMSelectable btn in blist) {
                btn.OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            yield break;
        }
        else if(cmd.Equals("spinme")) {
            List<KMSelectable> allbtn = new List<KMSelectable>();
            for(int a = 0; a < 4; a++) {
                allbtn.AddRange(Buttons[0]);
                allbtn.AddRange(Buttons[1]);
                allbtn.AddRange(Buttons[2]);
                allbtn.AddRange(Buttons[3]);
                allbtn.AddRange(Buttons[4]);
                allbtn.AddRange(Buttons[5]);
            }

            yield return "Plumbing";
            while(allbtn.Count > 0) {
                int p = Random.Range(0, allbtn.Count);
                allbtn[p].OnInteract();
                allbtn.RemoveAt(p);
                yield return new WaitForSeconds(0.05f);
            }
            yield break;
        }
        else {
            yield return "sendtochaterror Valid commands are 'rotate' and 'submit'.";
        }
    }
}