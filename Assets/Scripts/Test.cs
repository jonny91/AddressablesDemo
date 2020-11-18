using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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

	[SerializeField]
	private Text UpdatePercent;

	[SerializeField]
	private Text DownloadPercent;

	private List<object> _updateKeys = new List<object>();

	public async void UpdateCatalog()
	{
		var updateCatalogHandle = Addressables.CheckForCatalogUpdates(false);
		await updateCatalogHandle.Task;
		if (updateCatalogHandle.Status == AsyncOperationStatus.Succeeded)
		{
			List<string> catalogs = updateCatalogHandle.Result;
			if (catalogs != null && catalogs.Count > 0)
			{
				foreach (var catalog in catalogs)
				{
					Debug.Log("catalog  " + catalog);
				}

				Debug.Log("download catalog start ");
				var updateHandle = Addressables.UpdateCatalogs(catalogs, false);
				await updateHandle.Task;
				foreach (var item in updateHandle.Result)
				{
					Debug.Log("catalog result " + item.LocatorId);
					foreach (var key in item.Keys)
					{
						Debug.Log("catalog key " + key);
					}

					_updateKeys.AddRange(item.Keys);
				}

				UpdatePercent.text = "download catalog finish " + updateHandle.Status;
				Debug.Log("download catalog finish " + updateHandle.Status);
			}
			else
			{
				UpdatePercent.text = "dont need update catalogs";
				Debug.Log("dont need update catalogs");
			}
		}

		Addressables.Release(updateCatalogHandle);
	}

	private AsyncOperationHandle _downloadHandle;

	private bool _isDownloading = false;

	public IEnumerator DownAssetImpl()
	{
		var downloadsize = Addressables.GetDownloadSizeAsync((IEnumerable) _updateKeys);
		yield return downloadsize;
		Debug.Log("start download size :" + downloadsize.Result);
		DownloadPercent.text = "start download size :" + downloadsize.Result;

		if (downloadsize.Result > 0)
		{
			_isDownloading = true;
			_downloadHandle =
				Addressables.DownloadDependenciesAsync((IEnumerable) _updateKeys, Addressables.MergeMode.Union);
			yield return _downloadHandle;
			//await download.Task;
			Debug.Log("download result type " + _downloadHandle.Result.GetType());
			foreach (var item in _downloadHandle.Result as
				List<UnityEngine.ResourceManagement.ResourceProviders.IAssetBundleResource>)
			{
				var ab = item.GetAssetBundle();
				foreach (var name in ab.GetAllAssetNames())
				{
					Debug.Log("asset name " + name);
				}
			}

			Addressables.Release(_downloadHandle);
		}

		_isDownloading = false;
		Addressables.Release(downloadsize);
	}

	public void DownLoad()
	{
		StartCoroutine(DownAssetImpl());
	}

	private void Update()
	{
		if (_isDownloading)
		{
			DownloadPercent.text = _downloadHandle.PercentComplete.ToString();
		}
	}

	public void CreateChicken()
	{
		if (!SyncAddressables.Ready)
		{
			return;
		}

//		var prefab = SyncAddressables.LoadAsset<GameObject>("Chicken");
//		_chicken = Instantiate(prefab,
//			new Vector3(Random.Range(-4.5f, 4.5f), 0, Random.Range(-4.5f, 4.5f)),
//			Quaternion.Euler(0, Random.Range(0, 360), 0));


		_chickenHandle = Addressables.LoadAssetAsync<GameObject>("Chicken");
		_chickenHandle.Completed += operationHandle =>
		{
			if (operationHandle.Status == AsyncOperationStatus.Succeeded)
			{
				var prefab = operationHandle.Result;
				_chicken = Instantiate(prefab,
					new Vector3(Random.Range(-4.5f, 4.5f), 0, Random.Range(-4.5f, 4.5f)),
					Quaternion.Euler(0, Random.Range(0, 360), 0));
			}
		};
	}

	public void Release()
	{
		Addressables.ReleaseInstance(_chicken);
		Destroy(_chicken);
//		Addressables.Release(_chickenHandle);
	}

	public void CreateCube()
	{
		var handle = Addressables.LoadAssetAsync<GameObject>("Cubes/Cube.prefab");
		handle.Completed += operationHandle =>
		{
			if (operationHandle.Status == AsyncOperationStatus.Succeeded)
			{
				var prefab = operationHandle.Result;
				_cube = Instantiate(prefab,
					new Vector3(Random.Range(-4.5f, 4.5f), 0, Random.Range(-4.5f, 4.5f)),
					Quaternion.identity);
			}
		};

//		var prefab = SyncAddressables.LoadAsset<GameObject>("Cube");
//		_cube = Instantiate(prefab,
//			new Vector3(Random.Range(-4.5f, 4.5f), 0, Random.Range(-4.5f, 4.5f)),
//			Quaternion.identity);
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