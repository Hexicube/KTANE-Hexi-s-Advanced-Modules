using UnityEngine;
using System.Collections.Generic;

public class PolyDraw : MonoBehaviour
{
    public float Width, Height;
    public float BeatWidth, BeatHeight;
    public int Length;

    private bool[] stateList;
    private bool[] beatList;

    private float Left, Unit;

    public void Awake()
    {
        gameObject.transform.Find("Cube").GetComponent<MeshRenderer>().material.color = new Color(0.5f, 0.5f, 0.5f);

        Unit = Width / (float)Length;
        Left = -Unit * (float)Length / 2;
        BeatWidth = Unit * BeatWidth;

        stateList = new bool[Length];
        beatList = new bool[Length + 1];

        ApplyState();
    }

    public void AddState(bool state)
    {
        for(int a = 0; a < Length - 1; a++)
        {
            stateList[a] = stateList[a+1];
        }
        stateList[Length-1] = state;

        for (int a = 0; a < Length; a++)
        {
            beatList[a] = beatList[a + 1];
        }
        beatList[Length] = false;

        ApplyState();
    }

    public void AddBeat()
    {
        beatList[Length] = true;
    }

    public void Clear()
    {
        for (int a = 0; a < Length; a++)
        {
            stateList[a] = false;
            beatList[a] = false;
        }
        beatList[Length] = false;

        ApplyState();
    }

    List<GameObject> currentQuads = new List<GameObject>();

    private void GenQuad(int start, int end)
    {
        GameObject o = new GameObject();
        o.transform.parent = gameObject.transform;
        o.transform.localPosition = new Vector3();
        o.transform.localRotation = new Quaternion();
        o.transform.localScale = new Vector3(1, 1, 1);

        Mesh mesh = new Mesh();
        MeshRenderer r = o.AddComponent<MeshRenderer>();
        MeshFilter f = o.AddComponent<MeshFilter>();
        f.mesh = mesh;

        mesh.vertices = new Vector3[] {
            new Vector3(Left + Unit * start, 0.01f, -Height),
            new Vector3(Left + Unit * end, 0.01f, -Height),
            new Vector3(Left + Unit * end, 0.01f, Height),
            new Vector3(Left + Unit * start, 0.01f, Height)
        };
        mesh.uv = new Vector2[] {
            new Vector2 (0, 0),
            new Vector2 (0, 1),
            new Vector2(1, 1),
            new Vector2 (1, 0)
        };
        mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
        mesh.RecalculateNormals();

        r.material.shader = Shader.Find("KT/Mobile/DiffuseTint");
        r.material.color = new Color(0, 0, 0);

        currentQuads.Add(o);
    }

    private void GenBeat(int pos)
    {
        GameObject o = new GameObject();
        o.transform.parent = gameObject.transform;
        o.transform.localPosition = new Vector3();
        o.transform.localRotation = new Quaternion();
        o.transform.localScale = new Vector3(1, 1, 1);

        Mesh mesh = new Mesh();
        MeshRenderer r = o.AddComponent<MeshRenderer>();
        MeshFilter f = o.AddComponent<MeshFilter>();
        f.mesh = mesh;

        mesh.vertices = new Vector3[] {
            new Vector3(Left + Unit * pos - BeatWidth / 2, 0.01f, -BeatHeight / 2),
            new Vector3(Left + Unit * pos + BeatWidth / 2, 0.01f, -BeatHeight / 2),
            new Vector3(Left + Unit * pos + BeatWidth / 2, 0.01f, BeatHeight / 2),
            new Vector3(Left + Unit * pos - BeatWidth / 2, 0.01f, BeatHeight / 2)
        };
        mesh.uv = new Vector2[] {
            new Vector2 (0, 0),
            new Vector2 (0, 1),
            new Vector2(1, 1),
            new Vector2 (1, 0)
        };
        mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
        mesh.RecalculateNormals();

        r.material.shader = Shader.Find("KT/Mobile/DiffuseTint");
        r.material.color = new Color(0, 0, 0);

        currentQuads.Add(o);
    }

    public void ApplyState()
    {
        foreach (GameObject o in currentQuads)
        {
            o.SetActive(false);
            Destroy(o);
        }
        currentQuads.Clear();

        int start = -1;
        bool active = false;
        for (int a = 0; a < Length; a++)
        {
            if (stateList[a])
            {
                if (!active) start = a;
                active = true;
            }
            else
            {
                if (active)
                {
                    GenQuad(start, a);
                }
                active = false;
            }
            if (beatList[a]) GenBeat(a);
        }
        if (beatList[Length]) GenBeat(Length);
        if (active) GenQuad(start, Length);
    }
}