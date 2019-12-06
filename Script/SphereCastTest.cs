using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCastTest : MonoBehaviour {

	RaycastHit hit;

	[SerializeField]
	bool isEnable = false;

	void OnDrawGizmos()
	{
		if (isEnable == false)
			return;

		var radius = transform.lossyScale.x * 0.5f;

		var isHit = Physics.SphereCast (transform.position, radius, -transform.up, out hit);
		if (isHit) {
			Gizmos.DrawRay (transform.position, -transform.up * hit.distance);
			if (hit.distance <= transform.lossyScale.y * 0.5f) {
				Gizmos.DrawWireSphere (transform.position + -transform.up * (hit.distance), radius);
			}
		} else {
			Gizmos.DrawRay (transform.position, -transform.up * 5);
		}
	}
}
