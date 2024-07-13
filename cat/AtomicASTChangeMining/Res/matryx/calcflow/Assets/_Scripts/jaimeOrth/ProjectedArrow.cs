﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace orthProj
{
    public class ProjectedArrow : MonoBehaviour
    {

        public Transform origin; //center
        private Vector3 projPt1, projPt2;
        private Vector3 projectedRez;
        public PresentLine presentline;

        void Update()
        {
            //TODO: good place to make the conditional between projecting onto a line vs a plane
            var mesh = GetComponentInChildren<MeshRenderer>().enabled;

            //grab projection calculation
            projectedRez = presentline.projectedResult;
            //scaled to project
            Vector3 scaledRes = presentline.ScaledPoint(projectedRez);

            //show them the projected value

            //funky scaling?
            //projline.localScale = new Vector3(1, 1, scaledRes.magnitude); 

            //show them the unscaled numbers
            GetComponentInChildren<TextMesh>().text = "(" + projectedRez.y + ", " + projectedRez.x + ", " + projectedRez.z + ")";
            transform.localPosition = scaledRes;
            Vector3 position = transform.position;

            if (origin.position - position != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(origin.position - position);
            }

            LineRenderer line = GetComponent<LineRenderer>();
            line.SetWidth(.05f, .05f);
            line.SetPosition(0, transform.position);
            line.SetPosition(1, origin.position);

            var sharedMaterial = transform.GetComponentInChildren<MeshRenderer>().sharedMaterial;
            sharedMaterial.SetInt("_planeClippingEnabled", 1);

            for (int i = 0; i < 6; i++)
            {
                sharedMaterial.SetVector("_planePos" + i, presentline.walls[i].transform.position);
                //plane normal vector is the rotated 'up' vector.
                sharedMaterial.SetVector("_planeNorm" + i, presentline.walls[i].transform.rotation * Vector3.up);
            }
        }
    }
}