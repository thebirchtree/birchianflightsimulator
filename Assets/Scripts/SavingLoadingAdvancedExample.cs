/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Birch.SavingLoading;
namespace Birch.SavingLoading
{
public class SavingLoadingAdvancedExample : MonoBehaviour 
	{

		private string SavePath => $"{Application.persistentDataPath}/save.txt";

		[ContextMenu("Save")]
			private void Save () 
		{
			var state = LoadFile();
			CaptureState(state);
			SaveFile(state);
		}
			
		[ContextMenu("Load")]
		private void Load () 
		{
			var state = LoadFile ();
			RestoreState ();
			
		}
		private Dictionary<string, object> LoadFile()
		{
			if (!File.Exists (SavePath)) 
			{
				return new Dictionary<string, object> ();

			}
			using (FileStream stream = File.Open (SavePath, FileMode.Open))
			{
				var formatter = new BinaryFormatter ();
				return  (Dictionary<string, object>)formatter.Deserialize(stream);
			}
		}
		private void SaveFile(object state)
		{
			using (var stream = File.Open (SavePath, FileMode.Create))
			{
				var formatter = new BinaryFormatter ();
				formatter.Serialize (stream, state);
			}
		}

	}

}*/