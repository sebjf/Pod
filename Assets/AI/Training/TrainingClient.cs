﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(TrainingManager))]
[RequireComponent(typeof(TrainingWorker))]
public class TrainingClient : MonoBehaviour
{
    public TrainingProcessSettings settings;

    private TrainingManagerEndpoint client;
    private TrainingManager manager;
    private TrainingWorker worker;

    private TrainingState state;

    private void Awake()
    {
        manager = GetComponent<TrainingManager>();
        worker = GetComponent<TrainingWorker>();
        state = new TrainingState();
        worker.OnTrainingFrame += OnTrainingFrame;
        worker.OnTrainingRequestComplete += OnTrainingRequestComplete;
    }

    void Start()
    {
        client = new TrainingManagerEndpoint(new TcpClient(settings.host, settings.port));
        client.OnMessage += OnMessage;
        client.PostComplete();
        if (Camera.main)
        {
            Camera.main.enabled = false;
        }
    }
    private void OnTrainingRequestComplete(IAgentManager manager)
    {
        client.PostComplete(manager.Filename, manager.ExportJson());
    }

    private void OnTrainingFrame(TrainingRequest request, IAgentManager manager)
    {
        lock (state)
        {
            state.scenekey = request.circuit;
            state.carkey = request.car;
            state.agents.Clear();
            state.agents.AddRange(manager.Agents.Select(t => new TrainingAgentTransform() { position = t.position, rotation = t.rotation }));
        }
    }

    private void OnMessage(TrainingManagerEndpoint instance, TrainingMessage message)
    {
        if (message.type == "trainingrequest")
        {
            var request = JsonUtility.FromJson<TrainingRequest>(message.payload);
            manager.requests.Enqueue(request);
        }

        if (message.type == "staterequest")
        {
            lock (state)
            {
                instance.SendStateUpdate(state);
            }
        }
    }

    // Update is called once per frame
    private void OnApplicationQuit()
    {
        client.Close();
    }
}
