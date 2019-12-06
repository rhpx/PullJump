using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawArc : MonoBehaviour
{
	/// <summary>
	/// 放物線の描画ON/OFF
	/// </summary>
	//[SerializeField]
	private bool drawArc = false;

	/// <summary>
	/// 放物線を構成する線分の数
	/// </summary>
	//[SerializeField]
	private int segmentCount = 300;

	/// <summary>
	/// 放物線を何秒分計算するか
	/// </summary>
	private float predictionTime = 3.0F;

	/// <summary>
	/// 放物線のMaterial
	/// </summary>
	[SerializeField, Tooltip("放物線のマテリアル")]
	private Material arcMaterial;

	/// <summary>
	/// 放物線の幅
	/// </summary>
	[SerializeField, Tooltip("放物線の幅")]
	private float arcWidth = 1F;

	/// <summary>
	/// 放物線を構成するLineRenderer
	/// </summary>
	private LineRenderer[] lineRenderers;

	/// <summary>
	/// 弾の初速度や生成座標を持つコンポーネント
	/// </summary>
	private Player player;

	/// <summary>
	/// 弾の初速度
	/// </summary>
	private Vector3 initialVelocity;

	/// <summary>
	/// 放物線の開始座標
	/// </summary>
	private Vector3 arcStartPosition;

	/// <summary>
	/// 着弾マーカーオブジェクトのPrefab
	/// </summary>
	[SerializeField, Tooltip("着弾地点に表示するマーカーのPrefab")]
	private GameObject pointerPrefab;

	/// <summary>
	/// 着弾点のマーカーのオブジェクト
	/// </summary>
//	private GameObject pointerObject;

	void Start()
	{
		// 放物線のLineRendererオブジェクトを用意
		CreateLineRendererObjects();

		// マーカーのオブジェクトを用意
//		pointerObject = Instantiate(pointerPrefab, Vector3.zero, Quaternion.identity);
//		pointerObject.SetActive(false);

		// 弾の初速度や生成座標を持つコンポーネント
		player = gameObject.GetComponent<Player>();
	}

	void Update()
	{
		// 初速度と放物線の開始座標を更新
		initialVelocity = player.getPredictionVel();
		arcStartPosition = player.transform.position;

		if (drawArc)
		{
			// 放物線を表示
			float timeStep = predictionTime / segmentCount;
			bool draw = false;
			float hitTime = float.MaxValue;
			int reflectIndex = 0;
			for (int i = 0; i < segmentCount; i++)
			{
				// 線の座標を更新
//				 float startTime = timeStep * i;
				float startTime = timeStep * (i - reflectIndex); // 反射したときのIndexを差し引く
				float endTime = startTime + timeStep;
				SetLineRendererPosition(i, startTime, endTime, !draw);
//				SetLineRendererPosition(i, startTime, endTime, !draw);  // 反射したときのIndexを差し引く

				// 地面と衝突していれば以降の描画をしない
				if (isGrounded (startTime, endTime)) {
					draw = true;
				}

				// 壁との衝突判定
				if (!draw)
				{
					hitTime = GetArcHitTime(startTime, endTime);
					if (hitTime != float.MaxValue)
					{
						// 初期座標を衝突座標に更新
						arcStartPosition = GetArcPositionAtTime(startTime);

						//draw = true; // 衝突したらその先の放物線は表示しない

						// 衝突時の速度
						float colVelX;
						float colVelY;

						// 予測線に反射を反映（仮）
						Vector3 baseCol, minusVi, n;//vf
						baseCol = new Vector3(1,0);
						minusVi = initialVelocity;
						n = new Vector3(-baseCol.y,baseCol.x).normalized;    //baseColに対する正規化された法線
						// https://www.urablog.xyz/entry/2017/05/16/235548 衝突後の初速　vx vy に分解して考える(190815)
						// 衝突後の初速を計算(190820)
						colVelX = initialVelocity.x;
						colVelY = initialVelocity.y + Physics.gravity.y * startTime;
						initialVelocity = new Vector3 (-colVelX, colVelY, 0);

						//initialVelocity = Vector3.Reflect(-minusVi,n);    //colVelを採用するためコメントアウト(190820)
						reflectIndex = i;
					}
				}
			}

			// マーカーの表示
//			if (hitTime != float.MaxValue)
//			{
//				Vector3 hitPosition = GetArcPositionAtTime(hitTime);
//				ShowPointer(hitPosition);
//			}
		}
		else
		{
			// 放物線とマーカーを表示しない
			for (int i = 0; i < lineRenderers.Length; i++)
			{
				lineRenderers[i].enabled = false;
			}
//			pointerObject.SetActive(false);
		}
	}

	/// <summary>
	/// 指定時間に対するアーチの放物線上の座標を返す
	/// </summary>
	/// <param name="time">経過時間</param>
	/// <returns>座標</returns>
	private Vector3 GetArcPositionAtTime(float time)
	{
		return (arcStartPosition + ((initialVelocity * time) + (0.5f * time * time) * Physics.gravity));
	}

	/// <summary>
	/// LineRendererの座標を更新
	/// </summary>
	/// <param name="index"></param>
	/// <param name="startTime"></param>
	/// <param name="endTime"></param>
	private void SetLineRendererPosition(int index, float startTime, float endTime, bool draw = true)
	{
		lineRenderers[index].SetPosition(0, GetArcPositionAtTime(startTime));
		lineRenderers[index].SetPosition(1, GetArcPositionAtTime(endTime));
		lineRenderers[index].enabled = draw;
	}

	/// <summary>
	/// LineRendererオブジェクトを作成
	/// </summary>
	private void CreateLineRendererObjects()
	{
		// 親オブジェクトを作り、LineRendererを持つ子オブジェクトを作る
		GameObject arcObjectsParent = new GameObject("ArcObject");

		lineRenderers = new LineRenderer[segmentCount];
		for (int i = 0; i < segmentCount; i++)
		{
			GameObject newObject = new GameObject("LineRenderer_" + i);
			newObject.transform.SetParent(arcObjectsParent.transform);
			lineRenderers[i] = newObject.AddComponent<LineRenderer>();

			// 光源関連を使用しない
			lineRenderers[i].receiveShadows = false;
			lineRenderers[i].reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
			lineRenderers[i].lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
			lineRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

			// 線の幅とマテリアル
			lineRenderers[i].material = arcMaterial;
			lineRenderers[i].startWidth = arcWidth;
			lineRenderers[i].endWidth = arcWidth;
			lineRenderers[i].numCapVertices = 5;
			lineRenderers[i].enabled = false;
		}
	}    

	/// <summary>
	/// 指定座標にマーカーを表示
	/// </summary>
	/// <param name="position"></param>
//	private void ShowPointer(Vector3 position)
//	{
//		pointerObject.transform.position = position;
//		pointerObject.SetActive(true);
//	}

	/// <summary>
	/// 2点間の線分で衝突判定し、衝突する時間を返す
	/// </summary>
	/// <returns>衝突した時間(してない場合はfloat.MaxValue)</returns>
	private float GetArcHitTime(float startTime, float endTime)
	{
		// Linecastする線分の始終点の座標
		Vector3 startPosition = GetArcPositionAtTime(startTime);
		Vector3 endPosition = GetArcPositionAtTime(endTime);

		// 衝突判定
		RaycastHit hitInfo;

		int layerMaskWall = LayerMask.GetMask(new string[] {"wall"});//1 << 8;
		if (Physics.Linecast(startPosition, endPosition, out hitInfo, layerMaskWall))
		{
			// 壁との衝突（反射する）
			// 衝突したColliderまでの距離から実際の衝突時間を算出
			float distance = Vector3.Distance(startPosition, endPosition);
			return startTime + (endTime - startTime) * (hitInfo.distance / distance);
		}
		return float.MaxValue;
	}

	// 地面との衝突判定
	private bool isGrounded(float startTime, float endTime) {
		// Linecastする線分の始終点の座標
		Vector3 startPosition = GetArcPositionAtTime(startTime);
		Vector3 endPosition = GetArcPositionAtTime(endTime);

		// 衝突判定
		RaycastHit hitInfo;
		int layerMaskGround = LayerMask.GetMask(new string[] {"ground"});
		if (Physics.Linecast(startPosition, endPosition, out hitInfo, layerMaskGround))
		{
			return true;
		}
		return false;
	}

	// 予測線表示オン
	public void activate() {
		drawArc = true;
	}

	// 予測線表示オフ
	public void deactivate() {
		drawArc = false;
	}

}