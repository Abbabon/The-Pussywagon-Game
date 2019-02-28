using UnityEngine;
using System.Collections;

public static class SpriteRendererExt {

	public static void ForEachChild(this SpriteRenderer sprite, System.Func<SpriteRenderer, bool> iteration) {
		int childCount = sprite.transform.childCount;

		for (int i = 0; i < childCount; i++) {
			Transform      child       = sprite.transform.GetChild (i);
			SpriteRenderer childSprite = child.GetComponent<SpriteRenderer> ();

			if (childSprite && !iteration (childSprite))
				return;

			// NOTE: This method only calls 'iteration' on first-level children of 'sprite' and not on children of its children.
		}
	}

	public static void GetActiveBounds(this SpriteRenderer sprite, ref float minX, ref float minY, ref float maxX, ref float maxY, bool includeChildren = false, System.Func<SpriteRenderer, bool> shouldIgnoreSprite = null) {
		if (shouldIgnoreSprite == null || !shouldIgnoreSprite (sprite)) {
			if (sprite.bounds.min.x < minX) minX = sprite.bounds.min.x;
			if (sprite.bounds.min.y < minY) minY = sprite.bounds.min.y;
			if (sprite.bounds.max.x > maxX) maxX = sprite.bounds.max.x;
			if (sprite.bounds.max.y > maxY) maxY = sprite.bounds.max.y;
		}

		if (!includeChildren)
			return;

		int childCount = sprite.transform.childCount;

		for (int i = 0; i < childCount; i++) {
			Transform      child       = sprite.transform.GetChild (i);
			SpriteRenderer childSprite = child.GetComponent<SpriteRenderer> ();

			if (childSprite) {
				GetActiveBounds (childSprite, ref minX, ref minY, ref maxX, ref maxY, includeChildren, shouldIgnoreSprite);
			}
		}
	}

}
