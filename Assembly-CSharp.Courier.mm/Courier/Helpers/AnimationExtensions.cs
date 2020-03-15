using System;
using System.Collections;
using UnityEngine;

namespace Mod.Courier.Helpers {
    public static class AnimationExtensions {
        public static void Play(this Animation animation, MonoBehaviour script, string clipName, bool useTimeScale, Action onComplete = null) {
            script.StartCoroutine(animation.PlayRoutine(clipName, useTimeScale, onComplete));
        }

        // https://answers.unity.com/questions/217351/animations-ignore-timescale.html
        public static IEnumerator PlayRoutine(this Animation animation, string clipName, bool useTimeScale, Action onComplete = null) {
            //We Don't want to use timeScale, so we have to animate by frame..
            if (!useTimeScale) {
                AnimationState _currState = animation[clipName];
                bool isPlaying = true;
                float _progressTime = 0F;
                float _timeAtLastFrame = 0F;
                float _timeAtCurrentFrame = 0F;
                float deltaTime = 0F;

                animation.Play(clipName);

                _timeAtLastFrame = Time.realtimeSinceStartup;
                while (isPlaying) {
                    _timeAtCurrentFrame = Time.realtimeSinceStartup;
                    deltaTime = _timeAtCurrentFrame - _timeAtLastFrame;
                    _timeAtLastFrame = _timeAtCurrentFrame;

                    _progressTime += deltaTime;
                    _currState.normalizedTime = _progressTime / _currState.length;
                    animation.Sample();

                    //Debug.Log(_progressTime);

                    if (_progressTime >= _currState.length) {
                        //Debug.Log(&quot;Bam! Done animating&quot;);
                        if (_currState.wrapMode != WrapMode.Loop) {
                            //Debug.Log(&quot;Animation is not a loop anim, kill it.&quot;);
                            //_currState.enabled = false;
                            isPlaying = false;
                        } else {
                            //Debug.Log(&quot;Loop anim, continue.&quot;);
                            _progressTime = 0.0f;
                        }
                    }

                    yield return new WaitForEndOfFrame();
                }
                yield return null;
                onComplete?.Invoke();
            } else {
                animation.Play(clipName);
            }
        }
    }
}
