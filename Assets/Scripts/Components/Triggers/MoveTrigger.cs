using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using AngryDash.Game.API;
using UnityEngine;

namespace AngryDash.Game.Events
{
    public class MoveTrigger : MonoBehaviour
    {
        public string[] Blocks;

        public int AffectationType;
        public int Group;
        public Vector2 Translation = new Vector2(0, 0);
        public bool[] TranslationFromPlayer = new bool[2];
        public int Type;
        public float Speed = 1;
        public bool MultiUsage;
        public Vector3 Rotation;
        public bool[] Reset = new bool[2];
        public bool GlobalRotation;

        private bool Used;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!Used) StartCoroutine(Move());
        }

        private IEnumerator Move()
        {
            Used = true;
            GameObject[] go = null;
            if (AffectationType == 0)
            {
                Blocks = BlockUtilities.GetBlocks(Group).Select(x => x.ToString()).ToArray();
                go = new GameObject[Blocks.Length];
                for (var b = 0; b < Blocks.Length; b++)
                    go[b] = GameObject.Find("Objet n° " + Blocks[b]);
            }
            else if (AffectationType == 1)
            {
                go = new[] { GameObject.Find("Main Camera") };
                go[0].GetComponent<MainCam>().OnPlayer = false;
            }

            var affectedPos = new Vector3();
            var affectedRot = new Vector3();
            if (go.Length > 0 & Blocks.Length > 0)
            {
                if (AffectationType == 0)
                {
                    var lvlPlayer = GameObject.Find("Main Camera").GetComponent<LevelPlayer>();
                    try { affectedPos = lvlPlayer.GetObjectPos(int.Parse(Blocks[0])); } catch { }
                    try { affectedRot = Editor_MoveTrigger.getVector3(lvlPlayer.GetBlocStatus("Rotation", int.Parse(Blocks[0]))); } catch { }
                }
                else if (AffectationType == 1)
                {
                    affectedPos = Player.userPlayer.transform.position;
                    affectedPos.z = -10;
                    affectedRot = new Vector3();
                }
                if (Reset[0])
                    Translation = (affectedPos - go[0].transform.position) / 50F;
                if (Reset[1])
                    Rotation = affectedRot - go[0].transform.rotation.eulerAngles;
            }

            var Objects = go;
            if (GlobalRotation)
            {
                var parent = new GameObject();
                parent.transform.parent = GameObject.Find("Items").transform;
                var centerPoints = new Vector3[go.Length];
                for (var i = 0; i < go.Length; i++)
                    centerPoints[i] = go[i].transform.position;
                var centroid = new Vector3(0, 0, 0);
                var numPoints = centerPoints.Length;
                foreach (var point in centerPoints)
                    centroid += point;
                centroid /= numPoints;

                parent.transform.position = centroid;
                for (var i = 0; i < go.Length; i++)
                    go[i].transform.parent = parent.transform;

                Objects = new[] { parent };
            }
            var InitialPos = new Vector2[Objects.Length];
            var InitialRot = new Vector3[Objects.Length];
            for (var i = 0; i < Objects.Length; i++)
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
            Vector2 InitialPlayerPos = Player.userPlayer.transform.position;


            var stopwatch = new Stopwatch();
            var l = 0;

            var MoveTime_F = Speed;
            if ((int)(MoveTime_F / 2) != MoveTime_F / 2 & Type == 1) MoveTime_F = MoveTime_F + 1; //Support Duration impair si le type est Fluid
            var MoveTime = TimeSpan.FromSeconds(MoveTime_F);
            stopwatch.Start();
            var lastFrameNotRender = true;
            while (stopwatch.Elapsed < MoveTime | lastFrameNotRender)
            {
                var Time = stopwatch.ElapsedMilliseconds;
                if (stopwatch.Elapsed >= MoveTime)
                {
                    lastFrameNotRender = false;
                    Time = (long)MoveTime.TotalMilliseconds;
                }
                for (var b = 0; b < Objects.Length; b++)
                {
                    var moveVector = new Vector2();
                    if (Type == 0)
                    {
                        var millisecondMove = Translation / (float)MoveTime.TotalMilliseconds;
                        var movedRange = ((Vector2)Objects[b].transform.position - InitialPos[b]) / 50F;
                        moveVector = (millisecondMove * Time) - movedRange;
                    }
                    else if (Type == 1)
                    {
                        var maxDistance = ((float)MoveTime.TotalMilliseconds / 2) + 1;
                        var moveFrame = new Vector2();
                        for (var i = 0; i < Time + 1; i++)
                        {
                            long vi = i + 1;
                            if (i > MoveTime.TotalMilliseconds / 2F)
                                vi = (long)MoveTime.TotalMilliseconds - i;
                            moveFrame = moveFrame + ((Translation / ((maxDistance / vi) / maxDistance)) / (long)MoveTime.TotalMilliseconds / (maxDistance / 2F));
                        }

                        var movedRange = ((Vector2)Objects[b].transform.position - InitialPos[b]) / 50F;
                        moveVector = (moveFrame - movedRange);
                    }

                    if (Objects != null)
                    {
                        if (Objects[b] != null)
                        {
                            var pos = Objects[b].transform.position;
                            for (var m = 0; m < 2; m++)
                            {
                                if (TranslationFromPlayer[m])
                                {
                                    var distanceObjectPlayer = InitialPos[b][m] - InitialPlayerPos[m];
                                    var playerPos = Player.userPlayer.transform.position[m];
                                    pos[m] = distanceObjectPlayer + playerPos + (Translation[m] * 50);
                                }
                                else pos[m] = pos[m] + moveVector[m] * 50;
                            }
                            Objects[b].transform.position = pos;
                            if (Objects[b].GetComponent<Mur>() != null) Objects[b].GetComponent<Mur>().Move = moveVector;

                            var rotateVector = new Vector3();
                            if (Type == 0)
                            {
                                var millisecondMove = Rotation / (float)MoveTime.TotalMilliseconds;
                                var movedRange = (Objects[b].transform.rotation.eulerAngles - InitialRot[b]) / 50F;
                                rotateVector = (millisecondMove * Time) - movedRange;
                            }
                            else if (Type == 1)
                            {
                                var maxDistance = ((float)MoveTime.TotalMilliseconds / 2) + 1;
                                var moveFrame = new Vector3();
                                for (var i = 0; i < Time + 1; i++)
                                {
                                    long vi = i + 1;
                                    if (i > MoveTime.TotalMilliseconds / 2F)
                                        vi = (long)MoveTime.TotalMilliseconds - i;
                                    moveFrame = moveFrame + ((Rotation / ((maxDistance / vi) / maxDistance)) / (long)MoveTime.TotalMilliseconds / (maxDistance / 2F));
                                }

                                var movedRange = (Objects[b].transform.rotation.eulerAngles - InitialRot[b]);
                                rotateVector = (moveFrame - movedRange);
                            }
                            var quaternion = new Quaternion();
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
                var tempParent = go[0].transform.parent.gameObject;
                var parent = GameObject.Find("Items");
                for (var i = 0; i < go.Length; i++)
                    go[i].transform.parent = parent.transform;
                Destroy(tempParent);
            }

            if (MultiUsage) Used = false;
            if (Reset[0] & AffectationType == 1) GameObject.Find("Main Camera").GetComponent<MainCam>().OnPlayer = true;
            if (Reset[1] & AffectationType == 1) GameObject.Find("Main Camera").transform.rotation = new Quaternion();
        }
    }
}
