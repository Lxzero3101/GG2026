    using System.Collections;
    using UnityEngine;

    public class PointerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 250f;
        [SerializeField] private float leftLimit = -195f;
        [SerializeField] private float rightLimit = 195f;

        [Header("Input")]
        [SerializeField] private KeyCode stopKey = KeyCode.Space;

        private RectTransform rectTransform;

        private bool movingRight = true;
        private bool isStopped = false;

        public bool IsStopped => isStopped;

        public float CurrentX
        {
            get
            {
                return rectTransform.anchoredPosition.x;
            }
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (!isStopped && Input.GetKeyDown(stopKey))
            {
                isStopped = true;
                StartCoroutine(ResetPointer());
            }

            if (isStopped)
                return;

            Vector2 pos = rectTransform.anchoredPosition;

            if (movingRight)
            {
                pos.x += moveSpeed * Time.deltaTime;

                if (pos.x >= rightLimit)
                {
                    pos.x = rightLimit;
                    movingRight = false;
                }
            }
            else
            {
                pos.x -= moveSpeed * Time.deltaTime;

                if (pos.x <= leftLimit)
                {
                    pos.x = leftLimit;
                    movingRight = true;
                }
            }

            rectTransform.anchoredPosition = pos;
        }

        IEnumerator ResetPointer()
        {
            yield return new WaitForSeconds(1f);

            rectTransform.anchoredPosition =
                new Vector2(0f, rectTransform.anchoredPosition.y);

            isStopped = false;
        }
    }