//#define ENABLE_LOG
/*
 * Copyright (c) 2014 - 2022 t_saki@serenegiant.com 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Serenegiant.UVC
{

	public class UVCDrawer : MonoBehaviour, IUVCDrawer
	{
		public bool UACEnabled = false;
		/**
		 * 接続時及び描画時のフィルタ用
		 */
		public UVCFilter[] UVCFilters;
		/**
		 * UVC機器からの映像の描画先Materialを保持しているGameObject
		 * 設定していない場合はこのスクリプトを割当てたのと同じGameObjecを使う。
		 */
		public List<GameObject> RenderTargets;
		/**
		 * UVC機器のUAC機能で取得した音声を再生するために使用するAudioSourceを保持するGameObject
		 * 設定していない場合はこのスクリプトを割当てたのと同じGameObjecを使う。
		 */
		public GameObject AudioTarget;
	
		//--------------------------------------------------------------------------------
		private const string TAG = "UVCDrawer#";

		/**
		* The Material used to render the video feed from the UVC device.
		* Retrieved from the TargetGameObject.
		* Priority order:
		*	 Skybox on TargetGameObject
		*	 > Renderer on TargetGameObject
		*	 > RawImage on TargetGameObject
		*	 > Material on TargetGameObject
		* If it cannot be retrieved via any of these methods, a UnityException is thrown in Start().
		*/
		private UnityEngine.Object[] TargetMaterials;
		/**
		 * オリジナルのテクスチャ
		 * UVCカメラ映像受け取り用テクスチャをセットする前に
		 * GetComponent<Renderer>().material.mainTextureに設定されていた値
		 */
		private Texture[] SavedTextures;

		private Quaternion[] quaternions;

		//================================================================================

		// Start is called before the first frame update
		void Start()
		{
			UpdateRenderTarget();
		}

		/**
		 * UVC機器が接続された
		 * IOnUVCAttachHandlerの実装
		 * @param manager 呼び出し元のUVCManager
		 * @param device 対象となるUVC機器の情報
		 * @return true: UVC機器を使用する, false: UVC機器を使用しない
		 */
		public bool OnUVCAttachEvent(UVCManager manager, UVCDevice device)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnUVCAttachEvent:{device}");
#endif
			// XXX 今の実装では基本的に全てのUVC機器を受け入れる
			// ただしTHETA SとTHETA VとTHETA Z1は映像を取得できないインターフェースがあるのでオミットする
			// IsUVCEnabledと同様にUVC機器フィルターをインスペクタで設定できるようにする
			var result = !device.IsRicoh || device.IsTHETA;

			result &= UVCFilter.Match(device, UVCFilters);

			return result;
		}

		/**
		 * UVC機器が取り外された
		 * IOnUVCDetachEventHandlerの実装
		 * @param manager 呼び出し元のUVCManager
		 * @param device 対象となるUVC機器の情報
		 */
		public void OnUVCDetachEvent(UVCManager manager, UVCDevice device)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnUVCDetachEvent:{device}");
#endif
		}

		/**
		 * IUVCDrawerが指定したUVC機器の映像を描画できるかどうかを取得
		 * IUVCDrawerの実装
		 * @param manager 呼び出し元のUVCManager
		 * @param device 対象となるUVC機器の情報
		 */
		public bool IsUVCEnabled(UVCManager manager, UVCDevice device)
		{
			return UVCFilter.Match(device, UVCFilters);
		}

		/**
		* Video capture has started
		* IUVCDrawer implementation
		* @param manager The calling UVCManager
		* @param device Information about the target UVC device
		* @param tex Texture instance to receive video from the UVC device
		*/
		public void OnUVCStartEvent(UVCManager manager, UVCDevice device, Texture tex)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnUVCStartEvent:{device}");
#endif
			HandleOnStartPreview(tex);
		}

		/**
		 * 映像取得を終了した
		 * IUVCDrawerの実装
		 * @param manager 呼び出し元のUVCManager
		 * @param device 対象となるUVC機器の情報
		 */
		public void OnUVCStopEvent(UVCManager manager, UVCDevice device)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnUVCStopEvent:{device}");
#endif
			HandleOnStopPreview();
		}

		/**
		 * IUVCDrawerが指定したUAC機器kからの音声を取得を有効にするかどうか取得
		 * XXX とりあえずUACに対応した機器であればtrueを返す, 必要に応じて書き換えること
		 * IUVCDrawerの実装
		 * @param manager 呼び出し元のUVCManager
		 * @param device 対象となるUAC機器の情報
		 */
		public bool IsUACEnabled(UVCManager manager, UVCDevice device)
		{
			return UACEnabled && device.isUAC;
		}

		/**
		 * UAC機器からの音声取得を開始した
		 * @param manager 呼び出し元のUVCManager
		 * @param device 接続されたUVC機器情報
		 * @param audioClip UAC機器からの音声を受け取るAudioClipオブジェクト
		 */
		public void OnUACStartEvent(UVCManager manager, UVCDevice device, AudioClip audioClip)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnUACStartEvent:{device}");
#endif
			HandleOnStartAudio(audioClip);
		}

		/**
		 * UAC機器からの音声取得を終了した
		 * @param manager 呼び出し元のUVCManager
		 * @param device 接続されたUVC機器情報
		 */
		public void OnUACStopEvent(UVCManager manager, UVCDevice device)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnUACStopEvent:{device}");
#endif
			HandleOnStopAudio();
		}

		//================================================================================
		/**
		* Update the rendering target
		*/
		private void UpdateRenderTarget()
		{
			bool found = false;
			if ((RenderTargets != null) && (RenderTargets.Count > 0))
			{
				TargetMaterials = new UnityEngine.Object[RenderTargets.Count];
				SavedTextures = new Texture[RenderTargets.Count];
				quaternions = new Quaternion[RenderTargets.Count];
				int i = 0;
				foreach (var target in RenderTargets)
				{
					if (target != null)
					{
						var material = TargetMaterials[i] = GetTargetMaterial(target);
						if (material != null)
						{
							found = true;
						}
#if (!NDEBUG && DEBUG && ENABLE_LOG)
						Console.WriteLine($"{TAG}UpdateRenderTarget:material={material}");
#endif
					}
					i++;
				}
			}
			if (!found)
			{   // 描画先が1つも見つからなかったときはこのスクリプトが
				// AddComponentされているGameObjectからの取得を試みる
				// XXX RenderTargetsにgameObjectをセットする？
				TargetMaterials = new UnityEngine.Object[1];
				SavedTextures = new Texture[1];
				quaternions = new Quaternion[1];
				TargetMaterials[0] = GetTargetMaterial(gameObject);
				found = TargetMaterials[0] != null;
			}

			if (!found)
			{
				throw new UnityException("no target material found.");
			}
		}

		/**
		* Gets a Material that renders the video as a texture.
		* Retrieves the Material from the specified GameObject if it has a Skybox, Renderer, RawImage, or Material component.
		* If multiple instances of a component type are assigned, returns the first usable one found.
		* Priority: Skybox > Renderer > RawImage > Material
		* @param target
		* @return Returns null if not found.
		*/
		private UnityEngine.Object GetTargetMaterial(GameObject target/*NonNull*/)
		{
			// Skyboxの取得を試みる
			var skyboxs = target.GetComponents<Skybox>();
			if (skyboxs != null)
			{
				foreach (var skybox in skyboxs)
				{
					if (skybox.isActiveAndEnabled && (skybox.material != null))
					{
						RenderSettings.skybox = skybox.material;
						return skybox.material;
					}
				}
			}
			// Skyboxが取得できなければRendererの取得を試みる
			var renderers = target.GetComponents<Renderer>();
			if (renderers != null)
			{
				foreach (var renderer in renderers)
				{
					if (renderer.enabled && (renderer.material != null))
					{
						return renderer.material;
					}

				}
			}
			// SkyboxもRendererも取得できなければRawImageの取得を試みる
			var rawImages = target.GetComponents<RawImage>();
			if (rawImages != null)
			{
				foreach (var rawImage in rawImages)
				{
					if (rawImage.enabled && (rawImage.material != null))
					{
						return rawImage;
					}

				}
			}
			// SkyboxもRendererもRawImageも取得できなければMaterialの取得を試みる
			var material = target.GetComponent<Material>();
			if (material != null)
			{
				return material;
			}
			return null;
		}

		private void RestoreTexture()
		{
			for (int i = 0; i < TargetMaterials.Length; i++)
			{
				var target = TargetMaterials[i];
				try
				{
					if (target is Material)
					{
						(target as Material).mainTexture = SavedTextures[i];
					}
					else if (target is RawImage)
					{
						(target as RawImage).texture = SavedTextures[i];
					}
				}
				catch
				{
					Console.WriteLine($"{TAG}RestoreTexture:Exception cought");
				}
				SavedTextures[i] = null;
				quaternions[i] = Quaternion.identity;
			}
		}

		private void ClearTextures()
		{
			for (int i = 0; i < SavedTextures.Length; i++)
			{
				SavedTextures[i] = null;
			}
		}

		/**
		* Processing when video capture starts
		* @param tex The texture that receives the video
		*/
		private void HandleOnStartPreview(Texture tex)
		{
			Debug.Log($"UVC texture: {tex.width} x {tex.height}");
			int i = 0;
			foreach (var target in TargetMaterials)
			{
				if (target is Material)
				{

					Debug.Log($"Assigning texture {tex} to Material {target}");

					SavedTextures[i++] = (target as Material).mainTexture;
					(target as Material).mainTexture = tex;
				}
				else if (target is RawImage)
				{
					Debug.Log($"Assigning texture {tex} to RawImage {target}");
					SavedTextures[i++] = (target as RawImage).texture;
					(target as RawImage).texture = tex;
				}
			}
		}

		/**
		* Processing on the Unity side when video capture finishes
		*/
		private void HandleOnStopPreview()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}HandleOnStopPreview:");
#endif
			// 描画先のテクスチャをもとに戻す
			RestoreTexture();
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}HandleOnStopPreview:finished");
#endif
		}

		/**
		 * UACの音声再生を行うAudioSourceを取得する
		 */
		private AudioSource GetAudioSource()
		{
			AudioSource result = null;
			if (AudioTarget != null)
			{
				result = AudioTarget.GetComponent<AudioSource>();
			}
			if (result == null)
			{
				result = GetComponent<AudioSource>();
			}

#if (!NDEBUG && DEBUG && ENABLE_LOG)
			if (result == null)
			{
				Console.WriteLine($"{TAG}GetAudioSource:audio source not found");
			}
#endif
			return result;
		}

		/**
		 * 音声取得開始した時のUnity側の処理
		 * @param audioClip
		 */
		private void HandleOnStartAudio(AudioClip audioClip)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}HandleOnStartAudio:");
#endif
			var audioSource = GetAudioSource();
			if (audioSource != null)
			{
				audioSource.Stop();
				audioSource.clip = audioClip;
				audioSource.Play();
			}
		}

		/**
		 * 音声取得終了した時のUnity側の処理
		 */
		private void HandleOnStopAudio()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}HandleOnStopAudio:");
#endif
			var audioSource = GetAudioSource();
			if (audioSource != null)
			{
				audioSource.Stop();
				audioSource.clip = null;
			}
		}

	} // class UVCDrawer

} // namespace Serenegiant.UVC
