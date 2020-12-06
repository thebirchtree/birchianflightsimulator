/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Birch.SavingLoading
{
	public class LevelSystem : MonoBehaviour, ISaveable {
		[SerializeField] private int level = 1;
		[SerializeField] private int xp = 100;
	// Use this for initialization
		public object CaptureState()
		{
			return new SaveData
			{
				level = level,
				xp = xp
			};
		}
	
	// Update is called once per frame
		public void RestoreState (object state) {
			var saveData = (SavaData)state;

			level = saveData.level;
			xp = saveData.xp;
		}
	
	private struct SavaData
	{
			public int level;
			public int xp;
	}
}
}*/