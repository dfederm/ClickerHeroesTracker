// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Collections.Generic;
using Website.Services.Telemetry;

namespace UnitTests.Mocks;

internal sealed class MockTelemetryClient : ITelemetryClient
{
    public void IncrementCounter(string counterName, ReadOnlySpan<KeyValuePair<string, string>> tags)
    {
    }

    public void TrackCustomEvent(string eventName, ReadOnlySpan<KeyValuePair<string, string>> attributes)
    {
    }
}
