using System.Collections;
using UnityEngine;
using System.Linq;

public class MoveTrigger : MonoBehaviour
{

    public int AffectationType = 0;
    public string[] Blocks;
    public Vector2 Translation = new Vector2(0, 0);
    public bool[] TranslationFromPlayer = new bool[2];
    public int Type = 0;
    public float Speed = 1;
    public bool MultiUsage = false;
    public Vector3 Rotation;
    public bool[] Reset = new bool[2];
    public bool GlobalRotation = false;

    bool Used = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!Used | MultiUsage)
            StartCoroutine(Move());
    }

    IEnumerator Move()
    {
        GameObject[] go = null;
        if (AffectationType == 0)
        {
            go = new GameObject[Blocks.Length];
            for (int b = 0; b < Blocks.Length; b++)
                go[b] = GameObject.Find("Objet n° " + Blocks[b]);
        }
        else if (AffectationType == 1)
        {
            go = new GameObject[] { GameObject.Find("Main Camera") };
            go[0].GetComponent<MainCam>().OnPlayer = false;
        }

        Vector3 affectedPos = new Vector3();
        Vector3 affectedRot = new Vector3();
        if (go.Length > 0 & Blocks.Length > 0)
        {
            if (AffectationType == 0)
            {
                try { affectedPos = GameObject.Find("Main Camera").GetComponent<LevelPlayer>().GetObjectPos(int.Parse(Blocks[0])); } catch { }
                try { affectedRot = Editor_MoveTrigger.getVector3(GameObject.Find("Main Camera").GetComponent<LevelPlayer>().GetBlocStatus("Rotation", int.Parse(Blocks[0]))); } catch { }
            }
            else if (AffectationType == 1)
            {
                affectedPos = GameObject.Find("Main Camera").GetComponent<MainCam>().Player.transform.position;
                affectedPos.z = -10;
                affectedRot = new Vector3();
            }
            if (Reset[0])
                Translation = (affectedPos - go[0].transform.position) / 50F;
            if (Reset[1])
                Rotation = affectedRot - go[0].transform.rotation.eulerAngles;
        }

        GameObject[] Objects = go;
        if (GlobalRotation)
        {
            GameObject parent = new GameObject();
            parent.transform.parent = GameObject.Find("Items").transform;
            Vector3[] centerPoints = new Vector3[go.Length];
            for (int i = 0; i < go.Length; i++)
                centerPoints[i] = go[i].transform.position;
            Vector3 centroid = new Vector3(0, 0, 0);
            var numPoints = centerPoints.Length;
            foreach (Vector3 point in centerPoints)
                centroid += point;
            centroid /= numPoints;

            parent.transform.position = centroid;
            for (int i = 0; i < go.Length; i++)
                go[i].transform.parent = parent.transform;

            Objects = new GameObject[] { parent };
        }
        Vector2[] InitialPos = new Vector2[Objects.Length];
        Vector3[] InitialRot = new Vector3[Objects.Length];
        for (int i = 0; i < Objects.Length; i++)
        {
            if (Objects[i] != null)
            {
                InitialPos[i] = Objects[i].transform.position;
                InitialRot[i] = Objects[i].transform.eulerAngles;
            }
            else
            {
                var list = Objects.ToList();
                list.RemoveAt(i);
                Objects = list.ToArray();
                i--;
            }
        }
        Vector2 InitialPlayerPos = GameObject.Find("Main Camera").GetComponent<MainCam>().Player.transform.position;


        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        int l = 0;

        float MoveTime_F = Speed;
        if ((int)(MoveTime_F / 2) != MoveTime_F / 2 & Type == 1) MoveTime_F = MoveTime_F + 1; //Support Duration impair si le type est Fluid
        System.TimeSpan MoveTime = System.TimeSpan.FromSeconds(MoveTime_F);
        stopwatch.Start();
        bool lastFrameNotRender = true;
        while (stopwatch.Elapsed < MoveTime | lastFrameNotRender)
        {
            long Time = stopwatch.ElapsedMilliseconds;
            if (stopwatch.Elapsed >= MoveTime)
            {
                lastFrameNotRender = false;
                Time = (long)MoveTime.TotalMilliseconds;
            }
            for (int b = 0; b < Objects.Length; b++)
            {
                Vector2 moveVector = new Vector2();
                if (Type == 0)
                {
                    Vector2 millisecondMove = Translation / (float)MoveTime.TotalMilliseconds;
                    Vector2 movedRange = ((Vector2)Objects[b].transform.position - InitialPos[b]) / 50F;
                    moveVector = (millisecondMove * Time) - movedRange;
                }
                else if (Type == 1)
                {
                    float maxDistance = ((float)MoveTime.TotalMilliseconds / 2) + 1;
                    Vector2 moveFrame = new Vector2();
                    for (int i = 0; i < Time + 1; i++)
                    {
                        long vi = i + 1;
                        if (i > MoveTime.TotalMilliseconds / 2F)
                            vi = (long)MoveTime.TotalMilliseconds - i;
                        moveFrame = moveFrame + ((Translation / ((maxDistance / vi) / maxDistance)) / (long)MoveTime.TotalMilliseconds / (maxDistance / 2F));
                    }

                    Vector2 movedRange = ((Vector2)Objects[b].transform.position - InitialPos[b]) / 50F;
                    moveVector = (moveFrame - movedRange);
                }

                if (Objects != null)
                {
                    if (Objects[b] != null)
                    {
                        Vector3 pos = Objects[b].transform.position;
                        for (int m = 0; m < 2; m++)
                        {
                            if (TranslationFromPlayer[m])
                            {
                                float distanceObjectPlayer = InitialPos[b][m] - InitialPlayerPos[m];
                                float playerPos = GameObject.Find("Main Camera").GetComponent<MainCam>().Player.transform.position[m];
                                pos[m] = distanceObjectPlayer + playerPos + (Translation[m] * 50);
                            }
                            else pos[m] = pos[m] + moveVector[m] * 50;
                        }
                        Objects[b].transform.position = pos;
                        if (Objects[b].GetComponent<Mur>() != null) Objects[b].GetComponent<Mur>().Move = moveVector;

                        Vector3 rotateVector = new Vector3();
                        if (Type == 0)
                        {
                            Vector3 millisecondMove = Rotation / (float)MoveTime.TotalMilliseconds;
                            Vector3 movedRange = (Objects[b].transform.rotation.eulerAngles - InitialRot[b]) / 50F;
                            rotateVector = (millisecondMove * Time) - movedRange;
                        }
                        else if (Type == 1)
                        {
                            float maxDistance = ((float)MoveTime.TotalMilliseconds / 2) + 1;
                            Vector3 moveFrame = new Vector3();
                            for (int i = 0; i < Time + 1; i++)
                            {
                                long vi = i + 1;
                                if (i > MoveTime.TotalMilliseconds / 2F)
                                    vi = (long)MoveTime.TotalMilliseconds - i;
                                moveFrame = moveFrame + ((Rotation / ((maxDistance / vi) / maxDistance)) / (long)MoveTime.TotalMilliseconds / (maxDistance / 2F));
                            }

                            Vector3 movedRange = (Objects[b].transform.rotation.eulerAngles - InitialRot[b]);
                            rotateVector = (moveFrame - movedRange);
                        }
                        Quaternion quaternion = new Quaternion();
                        quaternion.eulerAngles = Objects[b].transform.rotation.eulerAngles + rotateVector;
                        Objects[b].transform.rotation = quaternion;
                    }
                }
            }

            yield return new WaitForEndOfFrame();
            l++;
        }
        stopwatch.Stop();


        if (GlobalRotation)
        {
            GameObject tempParent = go[0].transform.parent.gameObject;
            GameObject parent = GameObject.Find("Items");
            for (int i = 0; i < go.Length; i++)
                go[i].transform.parent = parent.transform;
            Destroy(tempParent);
        }

        Used = true;
        if (Reset[0] & AffectationType == 1) GameObject.Find("Main Camera").GetComponent<MainCam>().OnPlayer = true;
        if (Reset[1] & AffectationType == 1) GameObject.Find("Main Camera").transform.rotation = new Quaternion();
    }
}
