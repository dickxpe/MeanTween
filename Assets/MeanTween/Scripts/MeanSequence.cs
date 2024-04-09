// Author: Peter Dickx https://github.com/dickxpe
// MIT License - Copyright (c) 2024 Peter Dickx

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace com.zebugames.meantween.unity
{
    [Serializable]
    public class StringEvent : UnityEvent<string> { };

    [System.Serializable]
    public struct SequenceTween
    {
        public GameObject targetGameObject;

        public bool playSimultaneously;

        public List<MeanBehaviour> tweens;
    }

    public class MeanSequence : MonoBehaviour
    {
        [SerializeField]
        public bool playOnAwake = false;

        [SerializeField]
        public List<SequenceTween> sequence = new List<SequenceTween>();

        [SerializeField]
        public StringEvent onPlayNext;
        [SerializeField]
        public UnityEvent onCompleted;

        public bool showEvents = false;

        List<MeanBehaviour> tweens = new List<MeanBehaviour>();

        void Awake()
        {
            for (int i = 0; i < sequence.Count; i++)
            {
                for (int j = 0; j < sequence[i].tweens.Count; j++)
                {
                    tweens.Add(sequence[i].tweens[j]);

                }
            }
        }

        void Start()
        {
            if (playOnAwake)
            {
                Play();
            }

        }

        public void Play()
        {
            StartCoroutine(PlaySequence());
        }

        public void Cancel()
        {
            foreach (SequenceTween sequenceTween in sequence)
            {
                LeanTween.cancel(sequenceTween.targetGameObject);
            }
        }

        private IEnumerator PlaySequence()
        {
            foreach (SequenceTween sequenceTween in sequence)
            {
                if (sequenceTween.playSimultaneously)
                {
                    MeanBehaviour longestTween = sequenceTween.tweens.OrderByDescending(x => x.totalDuration).First();
                    foreach (MeanBehaviour tween in sequenceTween.tweens)
                    {
                        if (tween != longestTween)
                        {
                            if (tween.infiniteLoop)
                            {
                                tween.AnimateOnce();
                            }
                            else
                            {
                                tween.Animate();
                            }
                        }
                    }
                    yield return WaitUntilEvent(longestTween, longestTween.onLoopsComplete);
                }
                else
                {
                    foreach (MeanBehaviour tween in sequenceTween.tweens.ToList())
                    {
                        yield return WaitUntilEvent(tween, tween.onLoopsComplete);
                    }
                }
            }
            onCompleted.Invoke();
        }

        private IEnumerator WaitUntilEvent(MeanBehaviour playNext, UnityEvent unityEvent)
        {
            var trigger = false;
            Action action = () => trigger = true;
            unityEvent.AddListener(action.Invoke);
            if (playNext.infiniteLoop)
            {
                playNext.AnimateOnce();
            }
            else
            {
                playNext.Animate();
            }
            onPlayNext.Invoke(playNext.objectToTween.name + " → " + playNext.tweenName);
            yield return new WaitUntil(() => trigger);
            unityEvent.RemoveListener(action.Invoke);
        }
    }
}