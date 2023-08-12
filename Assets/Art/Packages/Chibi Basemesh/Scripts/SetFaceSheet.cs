using UnityEngine;
public class SetFaceSheet : StateMachineBehaviour
{
    public FaceSheet faceSheet;
    private FaceController m_FaceController;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       m_FaceController = animator.GetComponent<FaceController>();
       m_FaceController.sheet = faceSheet;
    }
    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       m_FaceController.UpdateSheet();
    }
}