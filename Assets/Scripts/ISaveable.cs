﻿namespace Birch.SavingLoading
{
	public interface ISaveable
	{
		object CaptureState();
		void RestoreState(object state);
	}
}