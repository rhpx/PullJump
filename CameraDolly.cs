using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDolly : MonoBehaviour
{
	// マウスホイールの回転値を格納する変数
	private float scroll;
	// カメラ移動の速度
	public float speed = 1f;

	// ゲーム実行中の繰り返し処理
	void Update()
	{
		// マウスホイールの回転値を変数 scroll に渡す
		scroll = Input.GetAxis("Mouse ScrollWheel");

		// カメラの前後移動処理
		//（カメラが向いている方向 forward に変数 scroll と speed を乗算して加算する）
		Camera.main.transform.position += transform.forward * scroll * speed;
	}
}