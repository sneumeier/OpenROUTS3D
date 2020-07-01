using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.InterfaceScene
{
    public class ScrollBarContainer : MonoBehaviour
    {

        public InterfaceSceneInteraction interaction;
        public Scrollbar bar;
        public Text maxText;
        public Text minText;
        public Text customMaxText;
        public Text customMinText;
        public Text requestText;
        public Text currentText;

        public Image handle;

        public GameObject subdivisionContainer;
        public List<GameObject> subdivisionStrokes;
        public GameObject subdivisionStrokePrefab;

        public RectTransform slidingArea;

        public float min;
        public float max;

        private bool showLabel;

        // Use this for initialization
        void Start()
        {

        }

        public void SetPos(float fraction)
        {
            SetHandleVisible();
            bar.value = fraction;
        }

        public void SetRange(float min, float max)
        {
            this.min = min;
            this.max = max;
            minText.text = ((int)min).ToString();
            maxText.text = ((int)max).ToString();
            customMaxText.gameObject.SetActive(false);
            customMinText.gameObject.SetActive(false);
            showLabel = true;
        }

        public void SetCustomText(string mintext, string maxtext)
        {
            customMaxText.gameObject.SetActive(true);
            customMinText.gameObject.SetActive(true);
            minText.gameObject.SetActive(false);
            maxText.gameObject.SetActive(false);

            customMaxText.text = maxtext;
            customMinText.text = mintext;

            showLabel = false;
        }

        public void SetSubdivisions(int subdivisions)
        {
            bar.numberOfSteps = subdivisions;
            foreach (GameObject stroke in this.subdivisionStrokes)
            {
                GameObject.Destroy(stroke);
            }
            subdivisionStrokes.Clear();
            if (subdivisions > 1)
            {
                for (int i = 0; i < subdivisions; i++)
                {
                    GameObject newstroke = GameObject.Instantiate(subdivisionStrokePrefab);
                    newstroke.transform.SetParent(subdivisionContainer.transform);
                    newstroke.SetActive(true);
                    float width = slidingArea.rect.width;
                    Vector2 strokepos = new Vector2((width / (subdivisions-1)) * i, 0);
                    newstroke.GetComponent<RectTransform>().anchoredPosition = strokepos;
                    Debug.Log("Setting subdivision stroke to " + strokepos);
                    subdivisionStrokes.Add(newstroke);
                }
            }
        }

        public void SetHandleInvisible()
        {
            requestText.gameObject.SetActive(true);
            handle.color = new Color(handle.color.r, handle.color.g, handle.color.b, 0);
            currentText.gameObject.SetActive(false);
            handle.gameObject.SetActive(false);
        }

        public void SetHandleVisible()
        {
            requestText.gameObject.SetActive(false);
            handle.color = new Color(handle.color.r, handle.color.g, handle.color.b, 1);
            currentText.gameObject.SetActive(true);
            handle.gameObject.SetActive(true);
            interaction.SetNextAvailable(true);
        }

        public void Update()
        {
            if (showLabel)
            {
                currentText.text = ((int)(bar.value * (max - min) + min)).ToString();
            }
            else
            {
                currentText.text = "";
            }
        }

    }
}