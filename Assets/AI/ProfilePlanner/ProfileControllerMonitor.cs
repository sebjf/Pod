﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProfileControllerMonitor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        var activateGameObject = UnityEditor.Selection.activeGameObject;
        if (activateGameObject)
        {
            if (activateGameObject.activeInHierarchy)
            {
                var agent = UnityEditor.Selection.activeGameObject.GetComponentInChildren<ProfileController>();
                if (agent)
                {
                    GraphOverlay.Plot("Curvature", agent.curvature);
                    GraphOverlay.Plot("Camber", agent.camber);
                    GraphOverlay.Plot("Inclination", agent.inclination);
                }
            }
        }
#endif
    }
}
