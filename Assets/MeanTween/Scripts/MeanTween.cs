// Author: Peter Dickx https://github.com/dickxpe
// MIT License - Copyright (c) 2024 Peter Dickx

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace com.zebugames.meantween.unity
{
    public class MeanTween : MeanBehaviour
    {

        public override void Awake()
        {
            base.Awake();


        }

        public override void Animate(bool once = false)
        {

            base.Animate(once);

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
                        if (spline)
                        {
                            splinePositions.Insert(0, objectToTween.transform.position);
                            tween.optional.spline = new LTSpline(splinePositions.ToArray());
                            tween.setMoveSpline();
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
                        if (spline)
                        {
                            splinePositions.Insert(0, objectToTween.transform.localPosition);
                            tween.optional.spline = new LTSpline(splinePositions.ToArray());
                            tween.setMoveSplineLocal();
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
    }
}