using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ↓こういうBGMいい！！！！！！
// Yu*Dachi - Running In The Snow

// カメラにプレイヤーとステージ写す（XY軸の２D）
// -->結構複雑なので後回し。暫定措置として、プレイヤーオブジェクトの子として追従させる。

// 接地したらvelocity=0
// --> 実装したが、水平以下の角度を指定した際、
// 	   少しだけ横に移動できてしまうのでそのうち修正する

// 壁にぶつかると反射
// --> 壁の向きを垂直固定値で実装(190811)
//   ・今後の予定
//     今作では、ぶつかる衝突するオブジェクトにそれとわかるレイヤーを指定し、
//     そのオブジェクトを壁とかに貼り付ける。そして、そのオブジェクトの向き（forward）
//      から法線ベクトルを取得し、反射させる。

// ☆ジャンプの予測線を表示
// --> 実装(190812)
//     反射の予測線表示は未実装

// キャラクター　チビユニティちゃん
// ドラッグ中はジャンプ待機モーション　腕をゆっくり上げ下げ　カービィのぴょーんってやつみたいな
// 壁にぶつかったら壁蹴りモーション？（壁にぶつかって反射する）
// 着地したら着地モーション？
// 移動速度一定もありかも


public class Player : MonoBehaviour {

	private DrawArc drawArc;

	private Vector3 startPos;
	private Vector3 endPos;
	private Vector3 currentPos;
	private Vector3 predictionVel;
	private float dist;
	private const float MIN_TAP_DISTANCE = 3f;
	private Vector3 pullVector;
	private float jumpPower = 0.15f;
	public float maxVel;
	public float minVel;
	public bool isGrounded = false;
	public bool reflectTrigger = false;
	public bool isOnWall = false;
	public bool isOnCeiling = false;
	RaycastHit hit;
	Rigidbody rigidbody;

	// Use this for initialization
	void Start () {
		drawArc = GetComponent<DrawArc> ();
		rigidbody = GetComponent<Rigidbody>();
		maxVel = 50f;
		minVel = 10f;
	}
	
	// Update is called once per frame
	void Update () {
		updateIsGrounded ();
		if (isGrounded == true) {
			manageInputs ();
		}

		if (isGrounded == false) {
			checkColWall ();
			if (isOnWall == false) {
				checkColCeiling ();
			}
		}

		// 壁と衝突したら反射するようにベクトルを変更
		if (reflectTrigger == true) {

			Vector3 baseCol, minusVi, n;//vf
			minusVi = rigidbody.velocity;

			if (isOnWall) {
				baseCol = new Vector3(1,0);
				n = new Vector3 (-baseCol.y, baseCol.x).normalized;    //baseColに対する正規化された法線
			} else { //(isOnCeiling)
				baseCol = new Vector3(0,1);
				n = new Vector3 (-baseCol.y, baseCol.x).normalized;    //baseColに対する正規化された法線
			}
			rigidbody.velocity = Vector3.Reflect(-minusVi,n);    //unityの標準関数。第二引数は正規化の必要がある

			reflectTrigger = false;
		}

	}

	// 接地判定
	void updateIsGrounded () {
		int layerMask = LayerMask.GetMask(new string[] {"ground"});//1 << 8;
		var radius = transform.lossyScale.x * 0.5f;
		if (Physics.CheckSphere (transform.position - transform.up * 0.01f, radius * 99 / 100, layerMask)) {		 
			isGrounded = true;
			rigidbody.velocity = Vector3.zero;
		} else {
			isGrounded = false;
		}
	}

	// 接壁判定（接地判定とあとでまとめて接触判定関数に入れる）
	void checkColWall () {
		int layerMask = LayerMask.GetMask(new string[] {"wall"});//1 << 8;
		var radius = transform.lossyScale.x * 0.5f;

		if (Physics.CheckSphere (transform.position + transform.up * 0.1f, radius * 101 / 100, layerMask)) {		 
			if (isOnWall == false) {
				reflectTrigger = true;
				isOnWall = true;
			}
		} else {
			isOnWall = false;
			// reflectTrigger = false;
		}
	}

	// 接天井判定（接地判定とあとでまとめて接触判定関数に入れる）
	void checkColCeiling () {
		int layerMask = LayerMask.GetMask(new string[] {"ceiling"});//1 << 8;
		var radius = transform.lossyScale.x * 0.5f;

		if (Physics.CheckSphere (transform.position + transform.up * 0.1f, radius * 101 / 100, layerMask)) {		 
			if (isOnCeiling == false) {
				reflectTrigger = true;
				isOnCeiling = true;
			}
		} else {
			isOnCeiling = false;
			// reflectTrigger = false;
		}
	}

	// 入力制御
	void manageInputs () {
		if (Input.GetKey(KeyCode.Z)) {
			rigidbody.velocity = new Vector3(0,1,0) * jumpPower * 10;
		}
			
		if (Input.GetMouseButtonDown (0)) {
			startPos = Input.mousePosition;
//			Debug.Log ("startPos(x,y,z)：(" + startPos.x + "," + startPos.y + "," + startPos.z + ")");
		}


		// 予測線表示用の予測速度を取得
		if (Input.GetMouseButton (0)) {
			currentPos = Input.mousePosition;
			dist = Vector2.Distance (new Vector2 (startPos.x, startPos.y), new Vector2 (currentPos.x, currentPos.y));
			// ドラッグ距離が一定以上なら予測線の描画を行う
			if (dist > MIN_TAP_DISTANCE) {
				drawArc.activate ();
				pullVector = (startPos - currentPos).normalized;
				predictionVel = pullVector * jumpPower * dist;

				// 最低速度・最大速度を制限
				if (predictionVel.magnitude > maxVel) {
					predictionVel = predictionVel * (maxVel / predictionVel.magnitude);
				} else if (predictionVel.magnitude > 0 && predictionVel.magnitude < minVel) {
					predictionVel = predictionVel * (minVel / predictionVel.magnitude);
				}
			} else {
				// ドラッグ距離が小さいなら予測線の描画をオフにする
				drawArc.deactivate();
			}
		}

		// アンタップでドラッグ距離を取得
		if (Input.GetMouseButtonUp (0)) {
			endPos = Input.mousePosition;
			dist = Vector2.Distance (new Vector2 (startPos.x, startPos.y), new Vector2 (endPos.x, endPos.y));
			Debug.Log ("startPos(x,y,z)：(" + startPos.x + "," + startPos.y + "," + startPos.z + ")");
			Debug.Log ("endPos(x,y,z)：(" + endPos.x + "," + endPos.y + "," + endPos.z + ")");
			Debug.Log ("dist : " + dist);
			// ドラッグ距離が一定以上なら移動を行う
			if (dist > MIN_TAP_DISTANCE) {
				drawArc.deactivate ();
				//			Debug.Log ("endPos(x,y,z)：(" + endPos.x + "," + endPos.y + "," + endPos.z + ")");

				pullVector = (startPos - endPos).normalized;

				rigidbody.velocity = pullVector * jumpPower * dist;

				// 最低速度・最大速度を制限
				if (rigidbody.velocity.magnitude > maxVel) {
					rigidbody.velocity = rigidbody.velocity * (maxVel / rigidbody.velocity.magnitude);
				} else if (rigidbody.velocity.magnitude < minVel) {
					rigidbody.velocity = rigidbody.velocity * (minVel / rigidbody.velocity.magnitude);
				}

				Debug.Log(rigidbody.velocity.magnitude);
				Debug.Log ("vel : " + rigidbody.velocity);
			}
		}
	}

	// タップ中の予測速度取得
	public Vector3 getPredictionVel() {
		// TODO:タップ中以外は０にするとかの制御入れる？
		return predictionVel;
	}

	// to be deleted
	public Vector3 getVelocity() {
		return rigidbody.velocity;
	}

}
