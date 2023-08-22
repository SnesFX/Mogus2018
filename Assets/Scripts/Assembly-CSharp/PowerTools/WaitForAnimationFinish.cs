using System.Collections;
using UnityEngine;

namespace PowerTools
{
	public class WaitForAnimationFinish : IEnumerator
	{
		private SpriteAnim animator;

		private AnimationClip clip;

		private bool first = true;

		public object Current
		{
			get
			{
				return null;
			}
		}

		public WaitForAnimationFinish(SpriteAnim animator, AnimationClip clip)
		{
			this.animator = animator;
			this.clip = clip;
			this.animator.Play(this.clip);
		}

		public bool MoveNext()
		{
			if (first)
			{
				first = false;
				return true;
			}
			return animator.IsPlaying(clip);
		}

		public void Reset()
		{
			first = true;
			animator.Play(clip);
		}
	}
}
