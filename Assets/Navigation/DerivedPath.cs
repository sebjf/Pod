﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

[Serializable]
public class DerivedWaypoint : Waypoint
{
    /// <summary>
    /// Position along the Track
    /// </summary>
    public float x;

    /// <summary>
    /// Distance (coefficient) from the Centerline
    /// </summary>
    public float w;
}

public class DerivedPath : Waypoints<DerivedWaypoint>
{
    public float Resolution = 5;
    public float Barrier = 3f;

    [SerializeField]
    protected WaypointsBroadphase1D broadphase;

    [SerializeField]
    protected TrackGeometry _track;

    [SerializeField]
    protected PathCache cache;

    public override TrackGeometry track
    {
        get { return _track; } 
    }

    private void Awake()
    {
        if(_track == null)
        {
            _track = GetComponentInParent<TrackGeometry>();
        }
        cache = new PathCache(this, 0.5f);
    }

    public override void Recompute()
    {
        base.Recompute();
        Awake();
    }

    public virtual void Initialise()
    {
        _track = GetComponentInParent<TrackGeometry>();

        var numWaypoints = Mathf.CeilToInt(track.totalLength / Resolution);
        var trueResolution = track.totalLength / numWaypoints;

        waypoints.Clear();
        for (int i = 0; i < numWaypoints; i++)
        {
            waypoints.Add(new DerivedWaypoint()
            {
                x = i * trueResolution,
                w = 0f
            });
        }

        Recompute();
    }

    public IList<TrackSection> GetSections()
    {
        if(waypoints.Count <= 0)
        {
            Initialise();
        }
        return waypoints.Select(wp => track.Section(wp.x)).ToList();
    }

    public void Load(float[] weights)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            waypoints[i].w = (weights[i] - 0.5f) * 2;
        }

        Recompute();
    }

    private void Update()
    {
        // for the enable checkbox in editor
    }

    public Vector3 Position(float x, float w)
    {
        var T = track.Section(x);
        return  (1 - ((w * 0.5f) + 0.5f)) * T.left + ((w * 0.5f) + 0.5f) * T.right; // convert w from domain -0.5..0.5 to domain 0..1 then interpolate
    }

    public override Vector3 Position(DerivedWaypoint wp)
    {
        return Position(wp.x, wp.w);
    }

    public Vector3 Position(WaypointQueryResult wq)
    {
        return Vector3.Lerp(Position(wq.waypoint), Position(wq.next), wq.t);
    }

    public Vector3 Position(float distance)
    {
        return Position(WaypointQuery(distance));
    }

    public override PathQuery Query(float distance)
    {
        if (cache != null)
        {
            return cache.Query(distance);
        }

        Profiler.BeginSample("DerivedPath Query");

        var wq = WaypointQuery(distance);

        PathQuery query;
        query.Midpoint = Position(wq);

        var A = Position(wq.waypoint);
        var B = Position(wq.next);
        var C = Position(Next(wq.next));
        var a = B - A;
        var b = C - B;
        query.Forward = Vector3.Lerp(a.normalized, b.normalized, wq.t);

        var trackdistance = Mathf.Lerp(wq.waypoint.x, wq.next.x, wq.t);
        var T = track.Query(trackdistance);
        query.Tangent = T.Tangent;
        query.Camber = T.Camber;

        var samplingDistance = Resolution * 1.5f;

        var X = Position(distance + samplingDistance);  // always use the true sample distance, rather than rely on the next/prev waypoints, because they could end up quite close together
        var Y = query.Midpoint;                         // (position(wp) and position(wpq) are not the same!)
        var Z = Position(distance - samplingDistance);
        query.Curvature = Curvature(X, Y, Z);

        query.Inclination = Inclination(query.Forward);

        Profiler.EndSample();

        return query;
    }

    public override float TrackDistance(float pathdistance)
    {
        var wq = WaypointQuery(pathdistance);
        return Mathf.Lerp(wq.waypoint.x, wq.next.x, wq.t);
    }

    public override TrackFlags Flags(float pathdistance)
    {
        return track.Flags(TrackDistance(pathdistance));
    }

    private void OnDrawGizmos()
    {
        if(!enabled)
        {
            return;
        }
        if (waypoints != null)
        {
            if (track == null)
            {
                _track = GetComponentInParent<TrackGeometry>();
            }

            Gizmos.color = Color.red;
            for (int i = 0; i < waypoints.Count; i++)
            {
                var wp1 = Position(Waypoint(i));
                var wp2 = Position(Next(Waypoint(i)));

                Gizmos.DrawLine(
                    wp1,
                    wp2);

                Gizmos.DrawWireSphere(
                    wp1,
                    0.5f);
            }
        }
    }
}
