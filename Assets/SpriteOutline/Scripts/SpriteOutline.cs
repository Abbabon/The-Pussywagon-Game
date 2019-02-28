using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOutline : MonoBehaviour {

	public enum SortMethod {
		SORTING_ORDER,
		Z_AXIS,
	}

	public const string RESOURCE_DIR = "Assets/SpriteOutline/Resources/";
	public const string RESOURCE_EXT = ".outline";
	public const string IMAGE_EXT    = RESOURCE_EXT + ".png";

	[UnityEngine.Serialization.FormerlySerializedAs("outlineSize")]
	[Tooltip("Adjusts the total thickness of the outline, in pixels.")]
	[Range(1, 20)]
	public int size = 1;

	[UnityEngine.Serialization.FormerlySerializedAs("outlineBlur")]
	[Tooltip("Blurs the outline by gradually fading the number of outer edges equivalent to the specified value.")]
	[Range(0, 19)]
	public int blurSize;

	[UnityEngine.Serialization.FormerlySerializedAs("outlineColor")]
	[Tooltip("Defines the color (and overall opacity) of the outline.")]
	public Color color = Color.white;

	[Tooltip("Adjusts the opacity of *only* the blurred edges.")]
	[Range(0, 1)]
	public float blurAlphaMultiplier = 0.7f;

	[Tooltip("Adjusts how quickly the blurred edges fade away.")]
	[Range(0, 1)]
	public float blurAlphaChoke = 1;

	[Tooltip("Reverses the fade direction of the blurring (from the inside out to the outside in).")]
	public bool invertBlur;

	[Tooltip("Defines the minimum amount of opacity a sprite pixel must have for an outline to be placed around it.")]
	[Range(0.01f, 1)]
	public float alphaThreshold = 0.05f;

	[Tooltip("Include child sprites in the outline.")]
	public bool includeChildren;

	[Tooltip("Filter child sprites on a per-layer basis (only those that belong to one of the checked layers will be included).")]
	public LayerMask childLayers = 1 << 0; // Use the 'Default' layer by default.

	[Tooltip("Exclude child sprites by their game object name.")]
	public string[] ignoreChildNames = new string[0];

	[Tooltip("Change how the outline is sorted (either the lowest sorting order - 1; or the highest z-axis value + 1).")] // TODO
	public SortMethod sortMethod = SortMethod.Z_AXIS;

	[Tooltip("Auto-regenerate the outline when the main sprite frame changes (does not track child sprites).")]
	public bool isAnimated;

	[Tooltip("Use the pre-rendered image of the outline instead of rendering in real time (you must \"Export\" the outline first).")]
	public bool useExportedFrame;

	[Tooltip("Override the file name of the exported outline (use to allow multiple game objects sharing the same name to export unique outlines).")]
	public string customFrameName = string.Empty;

	[Tooltip("Auto-regenerate the outline on game start.")]
	public bool generatesOnStart;

	#if UNITY_EDITOR
	[Tooltip("Auto-regenerate the outline when the component is loaded in the editor or when any value is changed via the Inspector.")]
	public bool generatesOnValidate = true;
	#endif

	SpriteRenderer sprite;
	GameObject     outline;
	SpriteRenderer outlineSprite;
	Material       material;
	Texture2D      texture;

	float   _boundsMinX;
	float   _boundsMinY;
	float   _boundsMaxX;
	float   _boundsMaxY;
	Vector3 _scale;
	Vector2 _anchor;
	Rect    _textureRect = Rect.zero;
	bool    _cachedUseExportedFrame;
	bool    _shouldRegenerateMaterial;

	SpriteRenderer          _sortingSprite;
	Dictionary<int, Sprite> _cachedOutlineSprites = new Dictionary<int, Sprite> ();
	int                     _lastSpriteFrameId;

	void Start() {
		#if UNITY_EDITOR
		if (!Application.isPlaying) {
			if (generatesOnValidate) {
				Regenerate ();
			}

			return;
		}
		#endif

		if (!generatesOnStart)
			return;

		Regenerate ();
	}

	void TryGetOutline() {
		Transform outlineTransform = transform.Find ("Outline");

		if (outlineTransform) {
			outline       = outlineTransform.gameObject;
			outlineSprite = outline.GetComponent<SpriteRenderer> ();
		}

		sprite = gameObject.GetComponent<SpriteRenderer> (); // Get a reference to the game object's sprite renderer.
	}

	void LateUpdate() {
		SortOutline ();

		if (!Application.isPlaying || !isAnimated)
			return;

		if (useExportedFrame) {
			LogError ("Cannot use the exported frame when \"Is Animated\" is enabled (disable \"Is Animated\" or \"Use Exported Frame\" to fix)");
			return;
		}

		if (!outline) {
			TryGetOutline ();

			if (!outline)
				return;
		}

		int spriteFrameId = sprite.sprite.GetInstanceID ();

		if (spriteFrameId == _lastSpriteFrameId)
			return;

		if (_cachedOutlineSprites.ContainsKey (spriteFrameId)) {
			outlineSprite.sprite = _cachedOutlineSprites [spriteFrameId];
		} else {
			Regenerate ();
		}

		_lastSpriteFrameId = spriteFrameId;
	}

	public void Regenerate() {
		_sortingSprite = null;

		if (useExportedFrame != _cachedUseExportedFrame) {
			_shouldRegenerateMaterial = true;
		}

		if (!material || _shouldRegenerateMaterial) {
			string shaderName = useExportedFrame ? "Particles/Alpha Blended Premultiply" : "Sprites/Outline";
			Shader shader     = Shader.Find (shaderName);

			if (!shader) {
				LogError ("Material could not be created (\"{0}\" shader is missing)", shaderName); // NOTE: This should never happen.
				return;
			}

			material = new Material (shader);
			texture  = null; // Force the texture to be recreated.

			_cachedUseExportedFrame   = useExportedFrame;
			_shouldRegenerateMaterial = false;
		}

		if (!outline) {
			TryGetOutline ();

			if (!outline) {
				outline       = new GameObject ("Outline");
				outlineSprite = outline.AddComponent<SpriteRenderer> ();
			}
		}

		Vector3    cachedPosition = transform.position;
		Quaternion cachedRotation = transform.rotation;
		Vector3    cachedScale    = transform.localScale;

		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;

		_scale.x = transform.localScale.x / transform.lossyScale.x;
		_scale.y = transform.localScale.y / transform.lossyScale.y;
		_scale.z = transform.localScale.z / transform.lossyScale.z;

		transform.localScale = _scale;

		outline.transform.parent = transform;

		outlineSprite.material = material;

		_boundsMinX = float.MaxValue;
		_boundsMinY = float.MaxValue;
		_boundsMaxX = float.MinValue;
		_boundsMaxY = float.MinValue;

		SpriteRendererExt.GetActiveBounds (sprite, ref _boundsMinX, ref _boundsMinY, ref _boundsMaxX, ref _boundsMaxY, includeChildren, ShouldIgnoreSprite);

		if (_boundsMinX == float.MaxValue) {
			SetTransformValues (cachedPosition, cachedRotation, cachedScale);

			LogError ("Outline cannot be created (there are no active sprites)");
			return;
		}

		if (useExportedFrame) {
			string sanitizedName = GetSanitizedName ((customFrameName != string.Empty) ? customFrameName : name);
			string resourcePath  = sanitizedName + RESOURCE_EXT;

			texture = Resources.Load<Texture2D> (resourcePath);

			if (!texture) {
				SetTransformValues (cachedPosition, cachedRotation, cachedScale);

				string texturePath = RESOURCE_DIR + sanitizedName + IMAGE_EXT;

				LogError ("Exported frame \"{0}\" not found (disable \"Use Exported Frame\" and press \"Export\" to fix)", texturePath);
				return;
			}
		} else {
			SetupTexture ();
			ClearTexture ();

			try {
				FillTexture (sprite);
			} catch (UnityException e) {
				SetTransformValues (cachedPosition, cachedRotation, cachedScale);

				int startIndex = e.Message.IndexOf ("'");
				int endIndex   = e.Message.IndexOf ("'", startIndex + 1) + 1;

				string textureName = e.Message.Substring (startIndex, endIndex - startIndex);

				LogError ("Texture {0} is not readable (enable \"Read/Write Enabled\" in the Texture Import Settings to fix)", textureName);
				return;
			}

			texture.Apply ();

			outlineSprite.sharedMaterial.SetInt   ("_Size",                size);
			outlineSprite.sharedMaterial.SetInt   ("_BlurSize",            blurSize);
			outlineSprite.sharedMaterial.SetColor ("_Color",               color);
			outlineSprite.sharedMaterial.SetFloat ("_BlurAlphaMultiplier", blurAlphaMultiplier);
			outlineSprite.sharedMaterial.SetFloat ("_BlurAlphaChoke",      blurAlphaChoke);
			outlineSprite.sharedMaterial.SetInt   ("_InvertBlur",          invertBlur ? 1 : 0);
			outlineSprite.sharedMaterial.SetFloat ("_AlphaThreshold",      alphaThreshold);
		}

		_textureRect.width  = texture.width;
		_textureRect.height = texture.height;

		_anchor.x = (sprite.sprite.pivot.x + GetOffsetX (sprite)) / texture.width;
		_anchor.y = (sprite.sprite.pivot.y + GetOffsetY (sprite)) / texture.height;

		outlineSprite.sprite = Sprite.Create (texture, _textureRect, _anchor, sprite.sprite.pixelsPerUnit, 0, SpriteMeshType.FullRect);

		SortOutline ();

		SetTransformValues (cachedPosition, cachedRotation, cachedScale);

		if (Application.isPlaying && isAnimated) {
			if (useExportedFrame) // Do not cache the exported frame as animations are not currently supported.
				return;

			Texture2D textureClone       = Texture2D.Instantiate (texture);
			Sprite    outlineSpriteClone = Sprite   .Create      (textureClone, _textureRect, _anchor, sprite.sprite.pixelsPerUnit, 0, SpriteMeshType.FullRect);

			int spriteFrameId = sprite.sprite.GetInstanceID ();

			_cachedOutlineSprites [spriteFrameId] = outlineSpriteClone;
		}
	}

	void SetupTexture() {
		int padding = size * 2;
		int width   = Mathf.CeilToInt ((_boundsMaxX - _boundsMinX) * sprite.sprite.pixelsPerUnit) + padding;
		int height  = Mathf.CeilToInt ((_boundsMaxY - _boundsMinY) * sprite.sprite.pixelsPerUnit) + padding;

		if (!texture) {
			texture            = new Texture2D (width, height, TextureFormat.RGBA32, false);
			texture.filterMode = FilterMode.Point;
			texture.wrapMode   = TextureWrapMode.Clamp;
		} else {
			texture.Resize (width, height);
		}
	}

	void ClearTexture() {
		Color32[] pixels      = texture.GetPixels32 ();
		int       pixelsCount = pixels.Length;

		for (int i = 0; i < pixelsCount; i++) {
			pixels [i].a = 0;
		}

		texture.SetPixels32 (pixels);
	}

	void FillTexture(SpriteRenderer sprite) {
		if (!ShouldIgnoreSprite (sprite)) {
			int width  = (int)sprite.sprite.rect.width;
			int height = (int)sprite.sprite.rect.height;

			Color[] pixels = sprite.sprite.texture.GetPixels ((int)sprite.sprite.rect.x, (int)sprite.sprite.rect.y, width, height);

			int offsetX = GetOffsetX (sprite);
			int offsetY = GetOffsetY (sprite);

			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					int index = width * y + x;

					if (pixels [index].a > 0) {
						texture.SetPixel (x + offsetX, y + offsetY, pixels [index]);
					}
				}
			}

			switch (sortMethod) {

			case SortMethod.SORTING_ORDER: if (!_sortingSprite || sprite.sortingOrder         < _sortingSprite.sortingOrder)         _sortingSprite = sprite; break;
			case SortMethod.Z_AXIS:        if (!_sortingSprite || sprite.transform.position.z > _sortingSprite.transform.position.z) _sortingSprite = sprite; break;

			}
		}

		if (!includeChildren)
			return;

		SpriteRendererExt.ForEachChild (sprite, childSprite => {
			FillTexture (childSprite);
			return true;
		});
	}

	int GetOffsetX(SpriteRenderer sprite) {
		return size + Mathf.RoundToInt ((sprite.bounds.min.x - _boundsMinX) * sprite.sprite.pixelsPerUnit);
	}

	int GetOffsetY(SpriteRenderer sprite) {
		return size + Mathf.RoundToInt ((sprite.bounds.min.y - _boundsMinY) * sprite.sprite.pixelsPerUnit);
	}

	void SetTransformValues(Vector3 position, Quaternion rotation, Vector3 scale) {
		transform.position   = position;
		transform.rotation   = rotation;
		transform.localScale = scale;
	}

	public virtual bool ShouldIgnoreSprite(SpriteRenderer sprite) {
		return !sprite.gameObject.activeInHierarchy || !sprite.enabled || !sprite.sprite || sprite == outlineSprite ||
			(sprite.gameObject != gameObject && (!LayerMaskExt.ContainsLayer (childLayers, sprite.gameObject.layer) || System.Array.IndexOf (ignoreChildNames, sprite.name) > -1));
	}

	public void SortOutline(float zOffset = 1, int? sortingOrder = null, int? sortingLayerId = null) {
		if (!outline || !_sortingSprite)
			return;

		outlineSprite.flipX = sprite.flipX;
		outlineSprite.flipY = sprite.flipY;

		outlineSprite.sortingLayerID = sortingLayerId.HasValue ? sortingLayerId.Value : _sortingSprite.sortingLayerID;
		outlineSprite.sortingOrder   = sortingOrder  .HasValue ? sortingOrder  .Value : _sortingSprite.sortingOrder - ((sortMethod == SortMethod.SORTING_ORDER) ? 1 : 0);

		if (sortMethod == SortMethod.Z_AXIS) {
			outlineSprite.transform.localPosition = Vector3.forward * (_sortingSprite.transform.localPosition.z + zOffset);
		} else {
			outlineSprite.transform.localPosition = Vector3.zero;
		}
	}

	public void Show() {
		if (outline) {
			outline.SetActive (true);
		}
	}

	public void Hide() {
		if (outline) {
			outline.SetActive (false);
		}
	}

	public void Clear() {
		if (!outline) {
			TryGetOutline ();

			if (!outline)
				return;
		}

		if (Application.isPlaying) {
			Destroy (outline);
		} else {
			DestroyImmediate (outline);
		}
	}

	public void Export() {
		if (!outline) {
			LogError ("Nothing to export (the outline does not exist)");
			return;
		}

		if (isAnimated || useExportedFrame) {
			LogError ("Cannot export when \"Is Animated\" or \"Use Exported Frame\" is enabled");
			return;
		}

		RenderTexture cachedRenderTexture = RenderTexture.active;
		RenderTexture renderTexture       = RenderTexture.GetTemporary (texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

		RenderTexture.active = renderTexture;

		Graphics.Blit (texture, renderTexture, outlineSprite.sharedMaterial);

		Texture2D screenshot = new Texture2D (texture.width, texture.height, TextureFormat.RGBA32, false);
		screenshot.ReadPixels (_textureRect, 0, 0, false);
		screenshot.Apply ();

		RenderTexture.active = cachedRenderTexture;

		RenderTexture.ReleaseTemporary (renderTexture);

		string texturePath = RESOURCE_DIR + GetSanitizedName ((customFrameName != string.Empty) ? customFrameName : name) + IMAGE_EXT;

		System.IO.FileStream   image       = System.IO.File.Open (texturePath, System.IO.FileMode.Create);
		System.IO.BinaryWriter imageWriter = new System.IO.BinaryWriter (image);

		imageWriter.Write (screenshot.EncodeToPNG ());

		image.Close ();

		Log ("Outline exported to \"{0}\"", texturePath);
	}

	string GetSanitizedName(string name) {
		return string.Concat (name.Split (System.IO.Path.GetInvalidFileNameChars ())).ToLower ();
	}

	void Log(string message, params object[] args) {
		Debug.LogFormat ("{0}: {1}", this, string.Format (message, args));
	}

	void LogError(string error, params object[] args) {
		Debug.LogErrorFormat ("{0}: {1}", this, string.Format (error, args));
	}

	#if UNITY_EDITOR
	bool _shouldRegenerate;

	void OnValidate() {
		if (!gameObject.activeInHierarchy || !generatesOnValidate)
			return;

		_shouldRegenerate = true;
	}

	void Update() {
		if (_shouldRegenerate) {
			Regenerate ();
			_shouldRegenerate = false;
		}
	}
	#endif

}
