// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Collections.Generic;

namespace Website.Services.Telemetry;

public interface ITelemetryClient
{
    void IncrementCounter(string counterName, ReadOnlySpan<KeyValuePair<string, string>> tags);

    void TrackCustomEvent(string eventName, ReadOnlySpan<KeyValuePair<string, string>> attributes);
}
