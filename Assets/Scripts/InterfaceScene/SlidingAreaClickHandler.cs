using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Assets.Scripts.InterfaceScene
{
    public class SlidingAreaClickHandler : MonoBehaviour, IPointerClickHandler 
    {

        public ScrollBarContainer container;
        private RectTransform rt;

        // Update is called once per frame
        void Start()
        {
            rt = container.GetComponent<RectTransform>();
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            OnMouseDown();
        }

        public void OnMouseDown()
        {
            Debug.Log("Clicked on scrollbar");
            float fraction = (Input.mousePosition.x - rt.position.x) / rt.rect.width + 0.5f;
            float relpos = (Input.mousePosition.x - rt.position.x);
            Debug.Log(fraction + " = Fraction");
            Debug.Log(relpos + " = Relpos");
            container.SetPos(fraction);
        }

    }
}