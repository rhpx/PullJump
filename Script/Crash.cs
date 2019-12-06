using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 190827 今後の予定
// プレイヤーにぶつかったらクラッシュパーティクルを生成してこのオブジェクトを削除する

public class Crash : MonoBehaviour {

	public GameObject particle;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (isColPlayer()) {
			crash ();
		}
	}

	// 破壊フェクトを再生してこのオブジェクトを削除する
	public void crash() {
		Instantiate (particle,transform.position,Quaternion.identity);
		Destroy (gameObject);
	}

	// プレイヤーとの接触判定
	bool isColPlayer () {
		int layerMask = LayerMask.GetMask(new string[] {"player"});//1 << 8;
		var halfExtents = transform.lossyScale.x * 0.5f;
		if (Physics.CheckSphere (transform.position, halfExtents * 1.2f , layerMask)) {
			return true;
		}
		return false;
	}
}
