﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(TrackNavigator))]
public class TrackNavigatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("StartingPosition"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("NoseOffset"));

        serializedObject.ApplyModifiedProperties();

        var navigator = target as Navigator;

        EditorGUILayout.LabelField("Distance", navigator.Distance.ToString());
        EditorGUILayout.LabelField("Lap", navigator.Lap.ToString());
    }
}

[CustomEditor(typeof(PathNavigator))]
public class PathNavigatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var paths = new Dictionary<string, TrackPath>();
        foreach (var item in (target as Navigator).GetComponentsInParent<TrackPath>())
        {
            paths.Add(item.UniqueName(), item);
        }
        var pathNames = paths.Keys.ToList();
        var selected = serializedObject.FindProperty("waypoints").objectReferenceValue as TrackPath;
        var selectedIndex = (selected == null) ? -1 : pathNames.IndexOf(selected.UniqueName());
        var newSelectedIndex = EditorGUILayout.Popup(selectedIndex, pathNames.ToArray());
        if(selectedIndex != newSelectedIndex)
        {
            serializedObject.FindProperty("waypoints").objectReferenceValue = paths[pathNames[newSelectedIndex]];
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("StartingPosition"));

        serializedObject.ApplyModifiedProperties();

        var navigator = target as Navigator;

        EditorGUILayout.LabelField("Distance", navigator.Distance.ToString());
        EditorGUILayout.LabelField("Lap", navigator.Lap.ToString());
    }
}
