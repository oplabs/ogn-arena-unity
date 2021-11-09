using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayTurnAttack : MonoBehaviour
{
    private Vector3 standPosition;
    private Vector3 initPosition;
    private Quaternion startRotation;
    private bool stayInPlace;
    private int turnCounter = 0;

    private Transform lFoot;
    private Transform rFoot;

    // Start is called before the first frame update
    void Start()
    {
      stayInPlace = false;
      turnCounter = 0;
      // find the feet of the person
      Transform pelvis = transform.Find("root").Find("CC_Base_Hip").Find("CC_Base_Pelvis");
      lFoot = pelvis.Find("CC_Base_L_Thigh").Find("CC_Base_L_Calf").Find("CC_Base_L_Foot");
      rFoot = pelvis.Find("CC_Base_R_Thigh").Find("CC_Base_R_Calf").Find("CC_Base_R_Foot");
    }

    private Vector3 getRenderPosition() {
      return GetComponentInChildren<Renderer>().bounds.center;
    }


    // Update is called once per frame
    void LateUpdate() {
      Animator animator = GetComponent<Animator>();
      if (animator.IsInTransition(0)) {
        if(animator.GetAnimatorTransitionInfo(0).userNameHash == Animator.StringToHash("InPlaceIdle")) {
          AnimatorTransitionInfo trans = animator.GetAnimatorTransitionInfo(0);
          if (!stayInPlace) {
            initPosition = transform.position;
            standPosition = (lFoot.position + rFoot.position)/2;
            //don't go up or down
            standPosition.y = 0;
            startRotation = animator.transform.rotation;
            stayInPlace = true;
            turnCounter += 1;
          } else {
            transform.position = Vector3.Lerp(initPosition, standPosition, trans.normalizedTime);
          }
        }
      } else if (stayInPlace) {
        transform.position = standPosition;
        stayInPlace = false;
      } else if ((animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("idle2")) && (turnCounter % 2 == 0)) {
          Quaternion r = startRotation * Quaternion.AngleAxis(180, Vector3.up);
          animator.transform.rotation = Quaternion.Lerp(startRotation, r, (animator.GetCurrentAnimatorStateInfo(0).normalizedTime - 0.6f) / 0.25f);
      }
    }
}
