using UnityEngine;
using System.Collections;

// Parent class for in-editor configurable data classes
public class UpdatableData : ScriptableObject
{
	public event System.Action OnValuesUpdated;
	public bool autoUpdate;

	#if UNITY_EDITOR
	protected virtual void OnValidate()
    {
		if (autoUpdate)
        {
            // Subscribe NotifyOfUpdatedValues() to the update delegate
            UnityEditor.EditorApplication.update += NotifyOfUpdatedValues;
		}
	}

	public void NotifyOfUpdatedValues()
    {
        // Unsubscribe from the update delegate when values change - we will be resubscribed when OnValidate is called after data changes
		UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;
        // If the OnValuesUpdated invocation list is not null, broadcast the delegate
		if (OnValuesUpdated != null)
        {
			OnValuesUpdated();
		}
	}
	#endif
}
