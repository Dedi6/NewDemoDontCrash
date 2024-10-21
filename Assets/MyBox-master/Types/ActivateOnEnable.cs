using UnityEngine;

namespace MyBox
{
	public class ActivateOnEnable : MonoBehaviour
	{
		public bool Active;
		[MustBeAssigned] public GameObject Target;

		private void OnEnable()
		{
			Target.gameObject.SetActive(Active);
		}
	
	}
}