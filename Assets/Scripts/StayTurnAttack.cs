using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayTurnAttack : MonoBehaviour
{
    private Vector3 initRenderPosition;
    private Quaternion startRotation;
    private bool stayInPlace;
    private int turnCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
      stayInPlace = false;
      turnCounter = 0;
    }

    private Vector3 getRenderPosition() {
      return GetComponentInChildren<Renderer>().bounds.center;
    }


    // Update is called once per frame
    /*
    void Update()
    {
      if(!stayInPlace) {
        Animator animator = GetComponent<Animator>();
        if (animator.IsInTransition(0) && animator.GetAnimatorTransitionInfo(0).userNameHash == Animator.StringToHash("InPlaceIdle")) {
          initRenderPosition = getRenderPosition();
          Debug.Log("set init Render Positon:" + initRenderPosition);
          startRotation = animator.transform.rotation;
          stayInPlace = true;
          turnCounter += 1;
        }
      }
    }*/

    void LateUpdate() {
      Animator animator = GetComponent<Animator>();
      if (animator.IsInTransition(0)) {

        if(animator.GetAnimatorTransitionInfo(0).userNameHash == Animator.StringToHash("InPlaceIdle")) {
          if (!stayInPlace) {
            initRenderPosition = getRenderPosition();
            //Debug.Log("set init Render Positon:" + initRenderPosition);
            startRotation = animator.transform.rotation;
            stayInPlace = true;
            turnCounter += 1;
          } else {
            //Debug.Log("Transition State:" + animator.GetCurrentAnimatorStateInfo(0).shortNameHash);
            //Debug.Log("Next idle2: " + (animator.GetNextAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("idle2")));
            //Debug.Log("Duration:" + animator.GetNextAnimatorStateInfo(0).length);
            //
            //if (animator.GetNextAnimatorStateInfo(0).normalizedTime < 0.1f) {
            /*
            Debug.Log("init Positon:" + initRenderPosition);
            Debug.Log("renderPosition:" + getRenderPosition());
            Debug.Log("DeltaTime:" + Time.deltaTime);
            */
            Vector3 deltaPosition = Vector3.ClampMagnitude(getRenderPosition() - initRenderPosition, 0.025f);
            // clamp down the up and down movement
            deltaPosition.y = 0;
            //Debug.Log("DeltaPositon:" + deltaPosition + " move amount:" + deltaPosition.magnitude);
            transform.position -= deltaPosition;
            //}
          }
        }
        // do this only in Transisition
        //
        /*
        if(stayInPlace) {
          AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        }*/
      } else if ((animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("idle2")) && (turnCounter % 4 == 0)) {
          Quaternion r = startRotation * Quaternion.AngleAxis(180, Vector3.up);
          animator.transform.rotation = Quaternion.Lerp(startRotation, r, (animator.GetCurrentAnimatorStateInfo(0).normalizedTime - 0.5f) / 0.25f);
      } else if (stayInPlace) {
        stayInPlace = false;
        //Debug.Log("Moving along..." + getRenderPosition());
      }
    }

}
