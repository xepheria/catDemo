using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (ThirdPersonCharacter))]
    public class ThirdPersonUserControl : MonoBehaviour
    {
        private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
        private Transform m_Cam;                  // A reference to the main camera in the scenes transform
        private Vector3 m_CamForward;             // The current forward direction of the camera
        private Vector3 m_Move;
        private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.

		//Alex's crappy quick code
		//shoots a ball from forward vector of pistol
		public GameObject shot;
		public GameObject pistol;
		private bool shoot;
		//AIM ARM
		/*private float aimGun = 0.5f;
		public Transform aimBone;*/
        
        private void Start()
        {
            // get the transform of the main camera
            if (Camera.main != null)
            {
                m_Cam = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.");
                // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            }

            // get the third person character ( this should never be null due to require component )
            m_Character = GetComponent<ThirdPersonCharacter>();
        }
		
		void OnGUI(){
			GUI.Box(new Rect(0, 0, 200, 300), "Controls");
			GUI.Label(new Rect(10, 20, 180, 280), "Arrow Keys: Movement\nC: Crouch\nSpacebar: Jump\nB: Shoot\n\nBullet will fire in an arc in direction of the ray being cast from barrel of gun.\n\nClicking on the screen will simulate firing a bullet from the viewer's perspective. If the mouse is on top of the character when you click, he will fall down.");
			
			GUI.Box(new Rect(Screen.width - 200, 0, 200, 200), "Debug");
			GUI.Label(new Rect(Screen.width - 150, 25, 100, 50), "Weapon vector: " + -pistol.transform.right);
			GUI.Label(new Rect(Screen.width - 150, 75, 100, 50), "Weapon pos: " + pistol.transform.position);
		}

        private void Update()
        {
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }
        }


        // Fixed update is called in sync with physics
        private void FixedUpdate()
        {
            // read inputs
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");
            bool crouch = Input.GetKey(KeyCode.C);
			shoot = Input.GetKeyDown(KeyCode.B);

            // calculate move direction to pass to character
            if (m_Cam != null)
            {
                // calculate camera relative direction to move:
                m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
                m_Move = v*m_CamForward + h*m_Cam.right;
            }
            else
            {
                // we use world-relative directions in the case of no main camera
                m_Move = v*Vector3.forward + h*Vector3.right;
            }
#if !MOBILE_INPUT
			// walk speed multiplier
	        if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 0.5f;
#endif

            // pass all parameters to the character control script
            m_Character.Move(m_Move, crouch, m_Jump);
            m_Jump = false;
			
			//if click, shoot a ray and if it hits character, knock him down
			if(Input.GetMouseButtonDown(0)){
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if(Physics.Raycast(ray, out hit)){
					print("shoot at: " + Input.mousePosition);
					if(hit.collider.gameObject.tag == "character"){
						print("hit char");
						GetComponent<Animator>().SetTrigger("hit");
					}
				}
			}
			
			if(shoot){
				GameObject shotInstance = (GameObject)Instantiate(shot, pistol.transform.position, pistol.transform.rotation);
				shotInstance.GetComponent<Rigidbody>().AddForce(pistol.transform.forward * 40);
				Destroy(shotInstance, 3f);
			}
			
			Debug.DrawRay(pistol.transform.position, pistol.transform.forward * 1.0f, Color.red);
        }
    }
}
