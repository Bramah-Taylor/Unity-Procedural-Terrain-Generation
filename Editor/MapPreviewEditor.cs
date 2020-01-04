using UnityEngine;
using System.Collections;
using UnityEditor;

// Editor class to handle in-editor generation of preview maps.
[CustomEditor (typeof (MapPreview))]
public class MapPreviewEditor : Editor
{
	public override void OnInspectorGUI()
    {
		MapPreview mapPreview = (MapPreview)target;

		if (DrawDefaultInspector())
        {
			if (mapPreview.autoUpdate)
            {
				mapPreview.DrawMapInEditor();
			}
		}

		if (GUILayout.Button("Generate"))
        {
			mapPreview.DrawMapInEditor();
		}
	}
}
