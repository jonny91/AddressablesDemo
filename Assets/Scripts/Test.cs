using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Test : MonoBehaviour
{
	private AsyncOperationHandle<GameObject> _chickenHandle = default;
	private GameObject _chicken;
	private Material _matRed;
	private Material _matBlue;
	private GameObject _cube;

	[SerializeField]
	private Image MDImg;

	[SerializeField]
	private Image LDImg;

	public void CreateChicken()
	{
		if (!SyncAddressables.Ready)
		{
			return;
		}

		var prefab = SyncAddressables.LoadAsset<GameObject>("Chicken");
		_chicken = Instantiate(prefab,
			new Vector3(Random.Range(-4.5f, 4.5f), 0, Random.Range(-4.5f, 4.5f)),
			Quaternion.Euler(0, Random.Range(0, 360), 0));


//		_chickenHandle = Addressables.LoadAssetAsync<GameObject>("Toon Chicken");
//		_chickenHandle.Completed += operationHandle =>
//		{
//			if (operationHandle.Status == AsyncOperationStatus.Succeeded)
//			{
//				var prefab = operationHandle.Result;
//				_chicken = Instantiate(prefab,
//					new Vector3(Random.Range(-4.5f, 4.5f), 0, Random.Range(-4.5f, 4.5f)),
//					Quaternion.Euler(0, Random.Range(0, 360), 0));
//			}
//		};
	}

	public void Release()
	{
		Addressables.ReleaseInstance(_chicken);
		Destroy(_chicken);
//		Addressables.Release(_chickenHandle);
	}

	public void CreateCube()
	{
		var prefab1 = SyncAddressables.LoadAsset<GameObject>("Cube");
		_cube = Instantiate(prefab1);
		return;
		_cube = SyncAddressables.Instantiate("Assets/CubeExample/Cube.prefab");
		return;
		var handle = Addressables.LoadAssetAsync<GameObject>("Assets/CubeExample/Cube.prefab");
		handle.Completed += operationHandle =>
		{
			if (operationHandle.Status == AsyncOperationStatus.Succeeded)
			{
				var prefab = operationHandle.Result;
				_cube = Instantiate(prefab);
			}
		};
	}

	public void ChangeBlue()
	{
		var mat = _cube.GetComponent<MeshRenderer>();
		if (_matBlue == null)
		{
			var handle = Addressables.LoadAssetAsync<Material>("Assets/CubeExample/Mats/Blue.mat");
			handle.Completed += operationHandle =>
			{
				if (operationHandle.Status == AsyncOperationStatus.Succeeded)
				{
					_matBlue = operationHandle.Result;

					mat.material = _matBlue;
				}
			};
		}
		else
		{
			mat.material = _matBlue;
		}
	}

	public void ChangeRed()
	{
		var mat = _cube.GetComponent<MeshRenderer>();
		if (_matRed == null)
		{
			var handle = Addressables.LoadAssetAsync<Material>("Assets/CubeExample/Mats/Red.mat");
			handle.Completed += operationHandle =>
			{
				if (operationHandle.Status == AsyncOperationStatus.Succeeded)
				{
					_matRed = operationHandle.Result;

					mat.material = _matRed;
				}
			};
		}
		else
		{
			mat.material = _matRed;
		}
	}

	public void LoadMD()
	{
		Load<Sprite>("Bust_10017", "MD", t => { MDImg.sprite = t; });
	}

	public void LoadLD()
	{
		Load<Sprite>("Bust_10017", "LD", t => { LDImg.sprite = t; });
	}

	private void Load<T>(string address, string label, Action<T> cb)
	{
		Addressables.LoadAssetsAsync<T>((IEnumerable) new List<object> {address, label}, null,
				Addressables.MergeMode.Intersection)
			.Completed += handle =>
		{
			if (handle.Status == AsyncOperationStatus.Succeeded)
			{
				cb.Invoke(handle.Result[0]);
			}
		};
	}
}