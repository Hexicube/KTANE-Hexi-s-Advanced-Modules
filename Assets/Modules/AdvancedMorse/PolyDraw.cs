using UnityEngine;
using System.Collections.Generic;

public class PolyDraw : MonoBehaviour
{
    public static GameObject SQUARE;

    public float Width, Height;
    public float BeatWidth, BeatHeight;
    public int Length;

    private bool[] stateList;
    private bool[] beatList;

    private float Left, Unit;

    public void Awake()
    {
        if (SQUARE == null)
        {
            SQUARE = new GameObject();

            Mesh mesh = new Mesh();
            MeshRenderer r = SQUARE.AddComponent<MeshRenderer>();
            r.material.shader = Shader.Find("KT/Mobile/DiffuseTint");
            r.material.color = new Color(0, 0, 0);

            mesh.vertices = new Vector3[] {
                new Vector3(0, 0.01f, -0.5f),
                new Vector3(1, 0.01f, -0.5f),
                new Vector3(1, 0.01f, 0.5f),
                new Vector3(0, 0.01f, 0.5f)
            };
            mesh.uv = new Vector2[] {
                new Vector2 (0, 0),
                new Vector2 (0, 1),
                new Vector2(1, 1),
                new Vector2 (1, 0)
            };
            mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateNormals();

            MeshFilter f = SQUARE.AddComponent<MeshFilter>();
            f.mesh = mesh;

            SQUARE.SetActive(false);
        }

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
        GameObject o = (GameObject)Instantiate(SQUARE, gameObject.transform);
        o.transform.localPosition = new Vector3(Left + Unit * start, 0, 0);
        o.transform.localRotation = new Quaternion();
        o.transform.localScale = new Vector3(Unit * (end - start), 1, Height);
        o.SetActive(true);
        currentQuads.Add(o);
    }

    private void GenBeat(int pos)
    {
        GameObject o = (GameObject)Instantiate(SQUARE, gameObject.transform);
        o.transform.localPosition = new Vector3(Left + Unit * pos - BeatWidth, 0, 0);
        o.transform.localRotation = new Quaternion();
        o.transform.localScale = new Vector3(BeatWidth, 1, BeatHeight);
        o.SetActive(true);
        currentQuads.Add(o);
    }

    public void ApplyState()
    {
        foreach (GameObject o in currentQuads)
        {
            o.SetActive(false);
            DestroyImmediate(o);
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