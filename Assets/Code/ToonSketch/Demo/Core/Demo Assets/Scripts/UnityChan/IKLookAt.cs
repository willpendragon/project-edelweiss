//
// Mecanim.IkLookAt
// 使用時には、AnimatorのBase LayerのIK PassをONにすること.
//
using UnityEngine;
using System.Collections;


namespace ToonSketch.Demo.UnityChan
{

//	[RequireComponent(typeof(Animator))]  

	public class IKLookAt : MonoBehaviour {

        private Animator avator;
		
		public bool ikActive = false;
		public Transform lookAtObj = null;
		
		public float lookAtWeight = 1.0f;
		public float bodyWeight = 0.3f;
		public float headWeight = 0.8f;
		public float eyesWeight = 1.0f;
		public float clampWeight = 0.5f;
        public bool isGUI = true;





		// Use this for initialization
		void Start () {
            avator = GetComponent<Animator>();
		}

		void OnGUI()
		{
            if (isGUI)
            {
                Rect rect1 = new Rect(Screen.width - 120, Screen.height - 40, 100, 30);
                ikActive = GUI.Toggle(rect1, ikActive, "Look at Target");
            }
		}


		void OnAnimatorIK(int layorIndex)
		{		
			if(avator)
			{
				if(ikActive)
				{
                    avator.SetLookAtWeight(lookAtWeight,bodyWeight,headWeight,eyesWeight,clampWeight);
                    if (lookAtObj != null)
                    {
                        avator.SetLookAtPosition(lookAtObj.position);
                    }
                    else
                    {
                        avator.SetLookAtWeight(0.0f);
                    }
				}
				else
				{
					avator.SetLookAtWeight(0.0f);
				}
			}
		}   		  
	}
}
