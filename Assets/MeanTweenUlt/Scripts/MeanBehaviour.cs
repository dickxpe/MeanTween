// Author: Peter Dickx https://github.com/dickxpe
// MIT License - Copyright (c) 2024 Peter Dickx
using UnityEngine;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Reflection;
using UltEvents;

namespace com.zebugames.meantween.ult
{
    public abstract class MeanBehaviour : MonoBehaviour
    {

        public enum LOOPTYPE
        {
            Once,
            Restart,
            PingPong
        }
        [SerializeField]
        public string tweenName = "Tween1";
        [SerializeField]
        public GameObject objectToTween;

        [Serializable]
        public class UpdateEventVector : UltEvent<Vector3> { };

        public enum SPACE
        {
            Local,
            Global
        }
        public enum TWEENTYPE { Move, Rotate, Scale, SpriteAlpha, SpriteColor, ComponentFieldValue };
        public enum AROUND { x, y, z };
        public enum VALUETYPE { FloatValue, Vector3Value };

        [HideInInspector]
        public int selectedComponent = 0;

        [HideInInspector]
        public int selectedField = 0;
        [HideInInspector]
        public FieldInfo fieldInfo;
        [HideInInspector]
        public PropertyInfo propertyInfo;



        [SerializeField]
        public TWEENTYPE tweenType = TWEENTYPE.Move;

        [SerializeField]
        public bool spline = false;

        [SerializeField]
        public bool rotateAroundAxis = false;
        [SerializeField]
        public AROUND axis = AROUND.x;

        [SerializeField]
        public float degrees = 360;

        [SerializeField]
        public SPACE space = SPACE.Local;
        [SerializeField]
        public LeanTweenType easeType = LeanTweenType.easeInOutCubic;

        [SerializeField]
        public bool additive = false;
        [SerializeField]
        public Vector3 target;

        [SerializeField]
        public Color color;

        [SerializeField]
        public float alpha;

        [SerializeField]
        public float value;

        [SerializeField]
        public Vector2 vector2Value;

        [SerializeField]
        public Component component;

        [SerializeField]
        public List<Vector3> splinePositions = new List<Vector3>();
        [SerializeField]
        public float duration = 2;

        public float totalDuration = 2;

        [SerializeField]
        public bool playOnAwake = false;



        [SerializeField]
        public bool ignoreTimeScale;
        [SerializeField]
        public UltEvent onStart;
        [SerializeField]
        public UpdateEventVector onUpdate;

        protected MethodInfo pushNewTween;

        protected LTDescr tween;



        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

        [SerializeField]
        public bool infiniteLoop = true;
        [SerializeField]
        public int loops = 2;
        [SerializeField]
        public UltEvent onComplete;

        [SerializeField]
        public UltEvent onLoopsComplete;

        [SerializeField]
        public LOOPTYPE loopType = LOOPTYPE.Once;

        public int tweenId;

        int loopsPlayed = 0;

        public bool showEvents = false;

        public virtual void Animate(bool once = false)
        {
            tween = LeanTween.options();
            tweenId = tween.id;

            if (pushNewTween == null)
            {
                pushNewTween = typeof(LeanTween).GetMethod("pushNewTween", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            }
            pushNewTween.Invoke(this, new object[] { objectToTween, target, duration, tween });

            tween.setTo(target)
              .setTime(duration)
              .setEase(easeType)
              .setOnStart(() => { onStart.Invoke(); })
              .setOnUpdate((Vector3 vector) => { onUpdate.Invoke(vector); UpdateVector(vector); })
              .setOnCompleteOnRepeat(true)
              .setOnComplete(() => { onComplete.Invoke(); })
              .setIgnoreTimeScale(ignoreTimeScale);

            if (infiniteLoop)
            {
                loops = -1;
            }

            if (loopType == LOOPTYPE.Once || once)
            {
                tween.setLoopOnce();
            }
            else if (loopType == LOOPTYPE.Restart)
            {
                tween.setLoopClamp(loops);
                tween.setLoopClamp();
            }
            else if (loopType == LOOPTYPE.PingPong)
            {
                tween.setLoopPingPong(loops);
                tween.setLoopPingPong();
            }
        }


        public virtual void Awake()
        {
            pushNewTween = typeof(LeanTween).GetMethod("pushNewTween", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            onComplete.AddPersistentCall((Action)Complete);
            totalDuration = duration;
            if (loops > 0)
            {
                totalDuration = duration * loops;
            }
        }

        public void AnimateOnce()
        {
            Animate(true);
        }


        public virtual void Start()
        {
            if (playOnAwake)
            {
                Animate();
            }
        }

        protected void Reset()
        {
            objectToTween = gameObject;
            tweenName = "Tween " + (Array.IndexOf(GetComponents<MeanTween>(), this) + 1);
        }

        protected void UpdateVector(Vector3 vector)
        {

            if (tweenType == TWEENTYPE.ComponentFieldValue)
            {
                if (fieldInfo != null)
                {
                    if (fieldInfo.FieldType == typeof(float))
                    {
                        fieldInfo.SetValue(component, vector.x);
                    }
                    else if (fieldInfo.FieldType == typeof(Vector3))
                    {
                        fieldInfo.SetValue(component, vector);
                    }
                    else if (fieldInfo.FieldType == typeof(Vector2))
                    {
                        fieldInfo.SetValue(component, new Vector2(vector.x, vector.y));
                    }
                }
                else if (propertyInfo != null)
                {
                    if (propertyInfo.PropertyType == typeof(float))
                    {
                        propertyInfo.SetValue(component, vector.x);
                    }
                    else if (propertyInfo.PropertyType == typeof(Vector3))
                    {
                        propertyInfo.SetValue(component, vector);
                    }
                    else if (propertyInfo.PropertyType == typeof(Vector2))
                    {
                        propertyInfo.SetValue(component, new Vector2(vector.x, vector.y));
                    }
                }
            }
        }

        public void CancelAll()
        {
            LeanTween.cancel(gameObject);
        }

        public void PauseAll()
        {
            LeanTween.pause(gameObject);
        }

        public void ResumeAll()
        {
            LeanTween.resume(gameObject);
        }

        public void Cancel()
        {
            LeanTween.cancel(tweenId);
        }

        public void Pause()
        {
            LeanTween.pause(tweenId);
        }

        public void Resume()
        {
            LeanTween.resume(tweenId);
        }

        public void Complete()
        {
            loopsPlayed++;
            if (loopType == LOOPTYPE.Restart)
            {
                if (loops == loopsPlayed || infiniteLoop)
                {
                    onLoopsComplete.Invoke();
                }

            }
            else if (loopType == LOOPTYPE.PingPong)
            {
                if (loops * 2 == loopsPlayed || infiniteLoop)
                {
                    onLoopsComplete.Invoke();
                }
            }
            else
            {
                onLoopsComplete.Invoke();
            }


        }
    }
}