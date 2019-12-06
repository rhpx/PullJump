using UnityEngine;
using System.Collections;

public class RefVector3 : MonoBehaviour {

	Vector3 baseCol,minusVi,vf,n;

	void Start () {
		baseCol=new Vector3(5,2);
		minusVi = new Vector3(-5,3);

		n = new Vector3(-baseCol.y,baseCol.x).normalized;    //baseColに対する正規化された法線
		vf = Vector3.Reflect(-minusVi,n);            //unityの標準関数。第二引数は正規化の必要がある
	}

	void Update () {
		Debug.DrawLine(Vector3.zero,baseCol);
		Debug.DrawLine(Vector3.zero,minusVi,Color.green);
		Debug.DrawLine(Vector3.zero,n,Color.blue);
		Debug.DrawLine(Vector3.zero,vf,Color.red);
	}
}