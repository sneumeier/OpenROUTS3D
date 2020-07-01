using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.InterfaceScene
{

    public class RadioButtonContainer : MonoBehaviour
    {
        public InterfaceSceneInteraction interaction;
        public GameObject radioPrefab;
        public GameObject multiPrefab;
        public float radioLength;
        public float radioHeight;
        public ToggleGroup group;

        public List<GameObject> radioObjects = new List<GameObject>();

        public void PrepareRadioButtons(RadioQuestion rq)
        {
            foreach(GameObject ro in radioObjects)
            {
                ro.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                group.UnregisterToggle(ro.GetComponent<Toggle>());       

                GameObject.Destroy(ro);
            }
            radioObjects.Clear();

            int radiocount = 0;
            float currentPositionX = -radioLength;
            float currentPositionY = 0;

            foreach (var option in rq.radioOptions)
            {
                GameObject newRadio;
                if (rq.multiselect)
                {
                    newRadio = GameObject.Instantiate(multiPrefab);
                }
                else
                {
                    newRadio = GameObject.Instantiate(radioPrefab);
                }
                newRadio.GetComponentInChildren<Text>().text = option;

                currentPositionX += radioLength;

                if (currentPositionX > this.GetComponent<RectTransform>().rect.width)
                {
                    currentPositionX = 0;
                    currentPositionY += radioHeight;
                }

                Toggle tog = newRadio.GetComponent<Toggle>();

                newRadio.transform.SetParent(transform);
                newRadio.transform.position = radioPrefab.transform.position+new Vector3(currentPositionX,-currentPositionY,0);

                newRadio.SetActive(true);

                radioObjects.Add(newRadio);
                if (rq.multiselect)
                {
                    interaction.SetNextAvailable(true);
                }
                else
                { 
                    group.RegisterToggle(tog);
                    tog.group = group;
                    tog.onValueChanged.AddListener(delegate
                    {
                        ToggleValueChanged(tog);
                    });

                    
                }
                radiocount++;
            }
            if (!rq.multiselect)
            { 
                group.SetAllTogglesOff();
            }
            
        }

        public void ToggleValueChanged(Toggle tog)
        {
            if (tog.isOn)
            {
                interaction.SetNextAvailable(true);
            }
        }



    }
}