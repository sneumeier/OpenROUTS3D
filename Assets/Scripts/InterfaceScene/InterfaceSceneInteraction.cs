using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
namespace Assets.Scripts.InterfaceScene
{
    public class InterfaceSceneInteraction : MonoBehaviour
    {

        public GameObject radioObject;
        public GameObject scrollObject;
        public GameObject imageObject;
        public GameObject inputObject;
        public GameObject inputObjectNr;

        public Text questionText;
        public Text largeText;

        private Image image;
        private RadioButtonContainer radios;
        private ScrollBarContainer scrollCont;
        private InputField input;
        private InputField inputNr;

        public GameObject buttonNext;
        public GameObject buttonPrev;

        private InterfaceQuestion[] questions;
        private int index = 0;


        // Use this for initialization
        void Start()
        {
            Cursor.visible = true;
            Time.timeScale = 1;
            radioObject.SetActive(false);
            inputObject.SetActive(false);
            inputObjectNr.SetActive(false);
            imageObject.SetActive(false);
            scrollObject.SetActive(false);

            radios = radioObject.GetComponent<RadioButtonContainer>();
            image = imageObject.GetComponent<Image>();
            scrollCont = scrollObject.GetComponent<ScrollBarContainer>();
            input = inputObject.GetComponent<InputField>();
            inputNr = inputObjectNr.GetComponent<InputField>();

            PrepareQuestions();

            index = 0;
            LoadQuestion(questions[0]);
        }

        public void PrepareQuestions()
        {
            AutomatedScene autSc = AutorunDeserializer.GetScene();
            InterfaceScene ic = (InterfaceScene)autSc;
            questions = ic.questions;
        }

        public void Reset()
        {
            radioObject.SetActive(false);
            inputObject.SetActive(false);
            imageObject.SetActive(false);
            scrollObject.SetActive(false);
            inputObjectNr.SetActive(false);
            largeText.gameObject.SetActive(false);
            questionText.gameObject.SetActive(false);
            inputNr.text = "";
            input.text = "";
        }

        public void LoadQuestion(InterfaceQuestion iq)
        {
            Reset();
            buttonPrev.SetActive(index>0);

            SetNextAvailable(true);

            questionText.gameObject.SetActive(true);
            questionText.text = iq.text;

            if (iq is BarQuestion)
            {
                BarQuestion bq = iq as BarQuestion;
                SetNextAvailable(false);
                scrollCont.SetRange(bq.min,bq.max);
                scrollObject.SetActive(true);
                scrollCont.SetHandleInvisible();
                if(bq.customDescription)
                {
                    scrollCont.SetCustomText(bq.minDescription,bq.maxDescription);
                }
                scrollCont.SetSubdivisions(bq.subdivisions);
                
            }
            else if (iq is FreeTextQuestion)
            {
                FreeTextQuestion ftq = iq as FreeTextQuestion;
                if (ftq.isNumber)
                {
                    inputObjectNr.SetActive(true);
                    UpdateNumberField();

                    SetNextAvailable(false);
                }
                else
                { 
                    inputObject.SetActive(true);
                    UpdateTextField();
                    
                }

            }
            else if (iq is RadioQuestion)
            {
                RadioQuestion rq = iq as RadioQuestion;
                radioObject.SetActive(true);
                SetNextAvailable(false);
                radios.PrepareRadioButtons(rq);
            }
            else if (iq is TutorialQuestion)
            {
                TutorialQuestion tq = iq as TutorialQuestion;
                if (!tq.hasImage)
                {
                    questionText.gameObject.SetActive(false);
                    largeText.gameObject.SetActive(true);
                    largeText.text = tq.text;
                }
                else
                {
                    byte[] bytes = File.ReadAllBytes(tq.imagePath);
                    Texture2D dynamicTex = new Texture2D(4,4,TextureFormat.DXT1, false);
                    dynamicTex.LoadImage(bytes);
                    image.sprite = Sprite.Create(dynamicTex, new Rect(0,0,dynamicTex.width, dynamicTex.height), new Vector2(0,0));
                    imageObject.SetActive(true);
                    
                }
            }
        }

        public void SaveQuestion()
        {
            InterfaceQuestion iq = questions[index];

            if (iq is RadioQuestion)
            {
                RadioQuestion rq = iq as RadioQuestion;
                if (rq.multiselect)
                {
                    foreach (GameObject go in radios.radioObjects)
                    {
                        Toggle tg = go.GetComponent<Toggle>();
                        if (tg.isOn)
                        {
                            iq.multipleAnswers.Add(go.GetComponentInChildren<Text>().text);
                        }
                    }
                }
                else {
                    foreach (GameObject go in radios.radioObjects)
                    {
                        Toggle tg = go.GetComponent<Toggle>();
                        if (tg.isOn)
                        {
                            iq.answer = go.GetComponentInChildren<Text>().text;
                        }
                    }
                }
            }
            else if (iq is FreeTextQuestion)
            {
                FreeTextQuestion ftq = iq as FreeTextQuestion;
                if (ftq.isNumber)
                {
                    iq.answer = inputNr.text;
                }
                else {
                    iq.answer = input.text;
                }
            }
            else if (iq is BarQuestion)
            {
                iq.answer = ( ( scrollCont.bar.value* (scrollCont.max-scrollCont.min) ) + scrollCont.min).ToString();
            }
        }

        public void SetNextAvailable(bool available)
        {
            buttonNext.GetComponent<Button>().interactable = available;
        }

        public void UpdateNumberField()
        {
            float result;
            var style = NumberStyles.AllowDecimalPoint;
            var culture = CultureInfo.InvariantCulture;
            if (float.TryParse(inputNr.text, style,culture, out result))
            {
                SetNextAvailable(true);
            }
            else
            {
                SetNextAvailable(false);
                
            }
        }

        public void UpdateTextField()
        {
            if (((FreeTextQuestion)questions[index]).required && input.text == "")
            {
                SetNextAvailable(false);
            }
        }

        public void LoadNextScene()
        {
            foreach (InterfaceQuestion qst in questions)
            {
                qst.SaveAnswers();
            }
            AutomatedScene autSc = AutorunDeserializer.NextScene();
            AutorunDeserializer.LoadScene();
        }

        public void Next()
        {
            SaveQuestion();
            index++;
            if (index >= questions.Length)
            {
                LoadNextScene();
            }
            else
            {
                LoadQuestion(questions[index]);
            }
        }

        public void Prev()
        {
            index--;
            if (index < 0)
            {
                index = 0;
            }
            else
            {
                LoadQuestion(questions[index]);
            }
        }

        void Update()
        {
            if (Input.GetButtonDown("Submit") && buttonNext.GetComponent<Button>().interactable)
            {
                Next();
            }
            { 
                
            }
        }

    }
}