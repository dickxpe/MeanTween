// Author: Peter Dickx https://github.com/dickxpe
// MIT License - Copyright (c) 2024 Peter Dickx

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace com.zebugames.meantween.ult
{
    public class MeanTween : MeanBehaviour
    {


        public override void Start()
        {
            base.Start();
        }
        public override void Awake()
        {
            base.Awake();
        }


        public override void Animate(bool ignoreLoops = false)
        {

            base.Animate(ignoreLoops);
            List<Vector3> bezierPoints = new List<Vector3>();

            if (path)
            {

                bezierPoints.Add(startPoint);
                if (pathPoints.Count > 0)
                {
                    int i = 0;
                    foreach (BezierPoint bp in pathPoints)
                    {
                        if (i == 0)
                        {
                            bezierPoints.Add(bp.control1);
                            bezierPoints.Add(startControlPoint);
                            bezierPoints.Add(bp.point);
                        }
                        else
                        {
                            bezierPoints.Add(pathPoints[i - 1].point);
                            bezierPoints.Add(bp.control1);
                            bezierPoints.Add(pathPoints[i - 1].control2);
                            bezierPoints.Add(bp.point);
                        }


                        if (i != 0 && i == pathPoints.Count - 1)
                        {
                            bezierPoints.Add(pathPoints[i].point);
                            bezierPoints.Add(endControlPoint);
                            bezierPoints.Add(pathPoints[i].control2);
                        }
                        if (i == 0 && i == pathPoints.Count - 1)
                        {
                            bezierPoints.Add(bp.point);
                            bezierPoints.Add(endControlPoint);
                            bezierPoints.Add(bp.control2);
                        }
                        i++;
                    }
                }
                else
                {
                    bezierPoints.Add(endControlPoint);
                    bezierPoints.Add(startControlPoint);
                }

                bezierPoints.Add(endPoint);
            }


            if (tweenType == TWEENTYPE.SpriteColor)
            {
                tween.to = new Vector3(1.0f, color.a, 0.0f);
                SpriteRenderer ren = objectToTween.GetComponent<SpriteRenderer>();
                if (ren == null)
                {
                    Debug.LogError("No SpriteRenderer on Gameobject " + objectToTween.name);
                }
                else
                {
                    tween.spriteRen = ren;
                    tween.setColor().setPoint(new Vector3(color.r, color.g, color.b));
                }
            }
            else if (tweenType == TWEENTYPE.SpriteAlpha)
            {
                tween.to = new Vector3(alpha, 0, 0);
                SpriteRenderer ren = objectToTween.GetComponent<SpriteRenderer>();
                if (ren == null)
                {
                    Debug.LogError("No SpriteRenderer on Gameobject " + objectToTween.name);
                }
                else
                {
                    tween.spriteRen = ren;
                    tween.from = new Vector3(ren.color.a, 0, 0);
                    tween.setAlpha();
                }
            }
            else if (tweenType == TWEENTYPE.ComponentFieldValue)
            {
                tween.setValue3();
            }
            else
            {
                if (space == SPACE.Global)
                {
                    if (tweenType == TWEENTYPE.Move)
                    {
                        if (path)
                        {
                            LTBezierPath path = new LTBezierPath(bezierPoints.ToArray());
                            tween.setSpeed(speed);

                            if (orientToPath)
                            {
                                path.orientToPath = true;
                            }
                            else
                            {
                                path.orientToPath = false;
                            }

                            tween.setTo(new Vector3(1.0f, 0.0f, 0.0f));
                            tween.optional.path = path;
                            tween.setMoveCurved();

                        }
                        else
                        {
                            if (additive)
                            {
                                tween.to = objectToTween.transform.position + tween.to;
                            }
                            tween.setMove();
                        }

                    }
                    else if (tweenType == TWEENTYPE.Rotate)
                    {
                        if (rotateAroundAxis)
                        {
                            if (axis == AROUND.x)
                            {
                                tween.setAxis(Vector3.right);
                            }
                            else if (axis == AROUND.y)
                            {
                                tween.setAxis(Vector3.up);
                            }
                            else if (axis == AROUND.z)
                            {
                                tween.setAxis(Vector3.forward);
                            }

                            tween.to = new Vector3(degrees, 0, 0);
                            tween.setRotateAround();
                        }
                        else
                        {
                            if (additive)
                            {
                                tween.to = objectToTween.transform.rotation.eulerAngles + tween.to;
                            }
                            tween.setRotate();
                        }

                    }
                    else if (tweenType == TWEENTYPE.Scale)
                    {
                        if (additive)
                        {
                            tween.to = objectToTween.transform.localScale + tween.to;
                        }
                        tween.setScale();

                    }

                }
                else if (space == SPACE.Local)
                {
                    if (tweenType == TWEENTYPE.Move)
                    {
                        if (path)
                        {
                            tween.setSpeed(speed);
                            LTBezierPath path = new LTBezierPath(bezierPoints.ToArray());

                            if (orientToPath)
                            {
                                path.orientToPath = true;
                            }
                            else
                            {
                                path.orientToPath = false;
                            }

                            tween.setTo(new Vector3(1.0f, 0.0f, 0.0f));
                            tween.optional.path = path;
                            if (transform.parent != null)
                            {
                                tween.setMoveCurvedLocal();
                            }
                            else
                            {
                                tween.setMoveCurved();
                            }
                        }
                        else
                        {
                            if (additive)
                            {
                                tween.to = objectToTween.transform.localPosition + tween.to;
                            }
                            tween.setMoveLocal();
                        }
                    }
                    else if (tweenType == TWEENTYPE.Rotate)
                    {
                        if (rotateAroundAxis)
                        {
                            if (axis == AROUND.x)
                            {
                                tween.setAxis(Vector3.right);
                            }
                            else if (axis == AROUND.y)
                            {
                                tween.setAxis(Vector3.up);
                            }
                            else if (axis == AROUND.z)
                            {
                                tween.setAxis(Vector3.forward);
                            }
                            tween.to = new Vector3(degrees, 0, 0);
                            tween.setRotateAroundLocal();
                        }
                        else
                        {
                            if (additive)
                            {
                                tween.to = objectToTween.transform.localRotation.eulerAngles + tween.to;
                            }
                            tween.setRotateLocal();
                        }
                    }
                    else if (tweenType == TWEENTYPE.Scale)
                    {
                        if (additive)
                        {
                            tween.to = objectToTween.transform.localScale + tween.to;
                        }
                        tween.setScale();

                    }

                }

            }

        }
        protected override void Complete()
        {
            base.Complete();
        }

        public override void CancelAll()
        {
            base.CancelAll();
        }

        public override void PauseAll()
        {
            base.PauseAll();
        }

        public override void ResumeAll()
        {
            base.ResumeAll();
        }

        public override void Cancel()
        {
            base.Cancel();
        }

        public override void Pause()
        {
            base.Pause();
        }

        public override void Resume()
        {
            base.Resume();
        }

        void OnDrawGizmosSelected()
        {

            if (path)
            {
                List<Vector3> bezierPoints = new List<Vector3>();

                bezierPoints.Add(startPoint);

                if (pathPoints.Count > 0)
                {
                    int i = 0;
                    foreach (BezierPoint bp in pathPoints)
                    {
                        if (i == 0)
                        {
                            bezierPoints.Add(bp.control1);
                            bezierPoints.Add(startControlPoint);
                            bezierPoints.Add(bp.point);
                        }
                        else
                        {
                            bezierPoints.Add(pathPoints[i - 1].point);
                            bezierPoints.Add(bp.control1);
                            bezierPoints.Add(pathPoints[i - 1].control2);
                            bezierPoints.Add(bp.point);
                        }


                        if (i != 0 && i == pathPoints.Count - 1)
                        {
                            bezierPoints.Add(pathPoints[i].point);
                            bezierPoints.Add(endControlPoint);
                            bezierPoints.Add(pathPoints[i].control2);
                        }
                        if (i == 0 && i == pathPoints.Count - 1)
                        {
                            bezierPoints.Add(bp.point);
                            bezierPoints.Add(endControlPoint);
                            bezierPoints.Add(bp.control2);
                        }
                        i++;
                    }
                }
                else
                {
                    bezierPoints.Add(endControlPoint);
                    bezierPoints.Add(startControlPoint);
                }

                bezierPoints.Add(endPoint);


                var lookRotation = Quaternion.LookRotation(Camera.current.transform.forward);
                LTBezierPath bezier = new LTBezierPath();
                Gizmos.color = new Color(1, 0, 1, 0.5f);

                List<Vector3> localPositions = new List<Vector3>();
                foreach (Vector3 pos in bezierPoints)
                {

                    if (space == SPACE.Local && transform.parent != null)
                    {
                        Vector3 localPos = transform.parent.TransformPoint(pos);
                        localPositions.Add(localPos);
                    }
                }
                if (space == SPACE.Local && transform.parent != null)
                {
                    bezier.setPoints(localPositions.ToArray());
                }
                else
                {
                    bezier.setPoints(bezierPoints.ToArray());
                }
                bezier.gizmoDraw();
            }

        }
    }
}


