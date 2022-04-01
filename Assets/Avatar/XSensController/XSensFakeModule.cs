﻿using System;
using System.Timers;
using UnityEngine;

public class XSensFakeModule : XSensModule
{
    Timer TimerHolder;

    public XSensFakeModule(int _expectedNbSensors) : base(_expectedNbSensors)
    {
    }
    public override bool SetupMaterial()
    {
        IsStationInitialized = true;
        return true;
    }
    public override int NbSensorsConnected()
    {
        return NbSensorsExpected;
    }

    public override bool SetupSensors()
    {
        IsSensorsConnected = NbSensorsConnected() == NbSensorsExpected;
        return IsSensorsConnected;
    }

    public override bool FinalizeSetup()
    {
        CurrentData = null;

        TimerHolder = new Timer();
        TimerHolder.Interval = 500; // In milliseconds
        TimerHolder.AutoReset = true; // Stops it from repeating
        TimerHolder.Elapsed += new ElapsedEventHandler(HandlerDataAvailableCallback);
        TimerHolder.Start();

        return true;
    }

    public override void Disconnect()
    {
        TimerHolder.Stop();
    }

    void HandlerDataAvailableCallback(object sender, ElapsedEventArgs e)
    {
        PreviousPacketCounter++;

        CurrentData = new AvatarData(PreviousPacketCounter, NbSensorsConnected());
        for (uint i = 0; i < NbSensorsConnected(); i++)
        {
            double[] _angles = { Math.PI/12, -Math.PI / 12, Math.PI / 6 };
            CurrentData.AddData(
                i, AvatarMatrixRotation.FromEulerXYZ(_angles)
            );
        }
    }
    
}
