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
        [Serializable]
        public struct BezierPoint
        {
            public Vector3 point;

            public Vector3 control1;

            public Vector3 control2;

        }
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

        [HideInInspector]
        public int selectedComponentId = 0;

        [HideInInspector]
        public int selectedFieldId = 0;

        [HideInInspector]
        [SerializeField]
        public Component selectedComponent;

        [HideInInspector]
        public string selectedFieldName;

        FieldInfo fieldInfo;
        PropertyInfo propertyInfo;

        public Vector3 from;

        public bool fromCheck = false;

        [SerializeField]
        public TWEENTYPE tweenType = TWEENTYPE.Move;

        [SerializeField]
        public bool path = false;

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
        public Vector2 vector2Value;

        [SerializeField]
        public float value;

        [SerializeField]
        public Color color;
        [Range(0, 255)]
        [SerializeField]
        public float alpha;


        [SerializeField]
        public bool orientToPath;
        [SerializeField]
        public Vector3 startPoint;
        [SerializeField]
        public Vector3 startControlPoint;

        [SerializeField]
        public Vector3 endPoint;
        [SerializeField]
        public Vector3 endControlPoint;

        [SerializeField]
        public List<BezierPoint> pathPoints = new List<BezierPoint>();

        [Min(0)]
        [SerializeField]
        public float speed = 2;

        [Min(0)]
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

        public int loopsPlayed = 0;

        public bool showEvents = false;

        public virtual void Animate(bool ignoreLoops = false)
        {
            tween = LeanTween.options();
            tweenId = tween.id;
            pushNewTween = typeof(LeanTween).GetMethod("pushNewTween", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            pushNewTween.Invoke(this, new object[] { objectToTween, target, duration, tween });

            if (fromCheck)
            {
                tween.from = from;
            }

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

            if (loopType == LOOPTYPE.Once || ignoreLoops)
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

            propertyInfo = selectedComponent.GetType().GetProperty(selectedFieldName);
            if (propertyInfo != null)
            {
                fromCheck = true;
                if (propertyInfo.PropertyType == typeof(float))
                {
                    from = new Vector3((float)propertyInfo.GetValue(selectedComponent), 0, 0);
                    target = new Vector3(value, 0, 0);
                }
                else if (propertyInfo.PropertyType == typeof(Vector3))
                {
                    from = (Vector3)propertyInfo.GetValue(selectedComponent);
                }
                else if (propertyInfo.PropertyType == typeof(Vector2))
                {
                    from = (Vector2)propertyInfo.GetValue(selectedComponent);
                    target = new Vector3(vector2Value.x, vector2Value.y, 0);
                }
            }
            else
            {
                fieldInfo = selectedComponent.GetType().GetField(selectedFieldName);
                if (fieldInfo != null)
                {
                    fromCheck = true;
                    if (fieldInfo.FieldType == typeof(float))
                    {
                        from = new Vector3((float)fieldInfo.GetValue(selectedComponent), 0, 0);
                        target = new Vector3(value, 0, 0);
                    }
                    else if (fieldInfo.FieldType == typeof(Vector3))
                    {
                        from = (Vector3)fieldInfo.GetValue(selectedComponent);
                    }
                    else if (fieldInfo.FieldType == typeof(Vector2))
                    {
                        from = (Vector2)fieldInfo.GetValue(selectedComponent);
                        target = new Vector3(vector2Value.x, vector2Value.y, 0);
                    }
                }
            }

            onComplete.AddPersistentCall((Action)Complete);
            totalDuration = duration;
            if (loops > 0)
            {
                totalDuration = duration * loops;
            }
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
                        fieldInfo.SetValue(selectedComponent, vector.x);
                    }
                    else if (fieldInfo.FieldType == typeof(Vector3))
                    {
                        fieldInfo.SetValue(selectedComponent, vector);
                    }
                    else if (fieldInfo.FieldType == typeof(Vector2))
                    {
                        fieldInfo.SetValue(selectedComponent, new Vector2(vector.x, vector.y));
                    }
                }
                else if (propertyInfo != null)
                {
                    if (propertyInfo.PropertyType == typeof(float))
                    {
                        propertyInfo.SetValue(selectedComponent, vector.x);
                    }
                    else if (propertyInfo.PropertyType == typeof(Vector3))
                    {
                        propertyInfo.SetValue(selectedComponent, vector);
                    }
                    else if (propertyInfo.PropertyType == typeof(Vector2))
                    {
                        propertyInfo.SetValue(selectedComponent, new Vector2(vector.x, vector.y));
                    }
                }
            }
        }

        public virtual void CancelAll()
        {
            LeanTween.cancel(gameObject);
        }

        public virtual void PauseAll()
        {
            LeanTween.pause(gameObject);
        }

        public virtual void ResumeAll()
        {
            LeanTween.resume(gameObject);
        }

        public virtual void Cancel()
        {
            LeanTween.cancel(tweenId);
        }

        public virtual void Pause()
        {
            LeanTween.pause(tweenId);
        }

        public virtual void Resume()
        {
            LeanTween.resume(tweenId);
        }

        protected virtual void Complete()
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