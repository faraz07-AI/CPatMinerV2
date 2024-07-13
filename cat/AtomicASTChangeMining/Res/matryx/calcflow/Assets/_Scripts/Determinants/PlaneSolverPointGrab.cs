﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Determinants
{
    public class PlaneSolverPointGrab : MonoBehaviour
    {

        public Transform point1, point2, point3;

        public PresentPlane presentPlane;
        public PtManager ptManager;
        public PtManager2D ptManager2D;

        public ConstraintGrabbable pt1Grabber, pt2Grabber, pt3Grabber;
        public bool FixedPlane = false;

        public FlexButtonLockColumns lockButton;


        void Update()
        {
                if (!pt1Grabber.IsGrabbed) pt1Grabber.lastLocalPos = point1.localPosition;
                else grabbingPoint(point1, pt1Grabber);
                if (!pt2Grabber.IsGrabbed) pt2Grabber.lastLocalPos = point2.localPosition;
                else grabbingPoint(point2, pt2Grabber);
                if (point3 != null){
                    if (!pt3Grabber.IsGrabbed) pt3Grabber.lastLocalPos = point3.localPosition;
                    else grabbingPoint(point3, pt3Grabber);
                }
        } 

        private void grabbingPoint(Transform point, ConstraintGrabbable grabber)
        {
            Vector3 newLoc = Vector3.zero;
            if (!(FixedPlane))//&& presentPlane.forwardPlane.GetComponent<MeshRenderer>().enabled))
            {
                FixedPlane = false;
                lockButton.LockOff();
                newLoc = grabber.lastLocalPos;
                point.localPosition = newLoc;
                          
                if (ptManager!=null){
                    ptManager.updatePoint(point.name, presentPlane.UnscaledPoint(newLoc), FixedPlane);
                }
                else {
                    ptManager2D.updatePoint(point.name, presentPlane.UnscaledPoint(newLoc), FixedPlane);
                }
            }
        }
    }
}
