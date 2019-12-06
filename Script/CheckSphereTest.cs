using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckSphereTest : MonoBehaviour {

	RaycastHit hit;

	[SerializeField]
	bool isEnable = false;

	void OnDrawGizmos()
	{
		if (isEnable == false)
			return;

		var radius = transform.lossyScale.x * 0.5f;
		Gizmos.DrawWireSphere (transform.position + transform.up * 0.1f, radius * 101 / 100);
//		if (Physics.CheckSphere(transform.position, radius))
//		{
//			Gizmos.DrawWireSphere (transform.position,  radius );
//		}
//
//		var isHit = Physics.SphereCast (transform.position, radius, -transform.up, out hit);
//		if (isHit) {
//			Gizmos.DrawRay (transform.position, -transform.up * hit.distance);
//			Gizmos.DrawWireSphere ( transform.position + -transform.up  *( hit.distance ) ,  radius );
//		} else {
//			Gizmos.DrawRay (transform.position, -transform.up * 5);
//		}
	}
}
