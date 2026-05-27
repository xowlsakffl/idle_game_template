using System.Collections;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

namespace IdleGame.UI.Common
{
    [RequireComponent(typeof(Button))]
    public sealed class HoldRepeatButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, ICancelHandler
    {
        private const float DefaultInitialDelaySeconds = 0.34f;
        private const float DefaultRepeatIntervalSeconds = 0.075f;

        private Button button;
        private Action action;
        private Func<bool> canRepeat;
        private Coroutine repeatCoroutine;
        private bool pointerHeld;
        private float initialDelaySeconds = DefaultInitialDelaySeconds;
        private float repeatIntervalSeconds = DefaultRepeatIntervalSeconds;

        public void Configure(Action repeatAction, Func<bool> repeatCondition = null, float initialDelay = DefaultInitialDelaySeconds, float repeatInterval = DefaultRepeatIntervalSeconds)
        {
            action = repeatAction;
            canRepeat = repeatCondition;
            initialDelaySeconds = Mathf.Max(0.05f, initialDelay);
            repeatIntervalSeconds = Mathf.Max(0.03f, repeatInterval);
            EnsureButton();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            EnsureButton();
            if (!CanInvoke())
            {
                return;
            }

            pointerHeld = true;
            InvokeAction();
            StopRepeat();
            repeatCoroutine = StartCoroutine(RepeatWhileHeld());
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pointerHeld = false;
            StopRepeat();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointerHeld = false;
            StopRepeat();
        }

        public void OnCancel(BaseEventData eventData)
        {
            pointerHeld = false;
            StopRepeat();
        }

        private void OnDisable()
        {
            pointerHeld = false;
            StopRepeat();
        }

        private IEnumerator RepeatWhileHeld()
        {
            yield return new WaitForSecondsRealtime(initialDelaySeconds);

            while (pointerHeld && CanInvoke() && (canRepeat == null || canRepeat()))
            {
                InvokeAction();
                yield return new WaitForSecondsRealtime(repeatIntervalSeconds);
            }

            repeatCoroutine = null;
        }

        private void InvokeAction()
        {
            if (CanInvoke())
            {
                action.Invoke();
            }
        }

        private bool CanInvoke()
        {
            EnsureButton();
            return action != null && button != null && button.IsInteractable();
        }

        private void StopRepeat()
        {
            if (repeatCoroutine != null)
            {
                StopCoroutine(repeatCoroutine);
                repeatCoroutine = null;
            }
        }

        private void EnsureButton()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }
        }
    }
}
