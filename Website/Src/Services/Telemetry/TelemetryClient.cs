// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Website.Services.Telemetry;

public class TelemetryClient : ITelemetryClient
{
    internal static readonly Meter MeterInstance = new("ClickerHeroesTracker.Website");

    private readonly ILogger<TelemetryClient> _logger;

    private readonly ConcurrentDictionary<string, Counter<long>> _counters = new(StringComparer.OrdinalIgnoreCase);

    public TelemetryClient(ILogger<TelemetryClient> logger)
    {
        _logger = logger;
    }

    public void IncrementCounter(string counterName, ReadOnlySpan<KeyValuePair<string, string>> tags)
    {
        Counter<long> counter = _counters.GetOrAdd(counterName, static counterName => MeterInstance.CreateCounter<long>(counterName));

        TagList tagList = default;
        for (int i = 0; i < tags.Length; i++)
        {
            KeyValuePair<string, string> kvp = tags[i];
            tagList.Add(kvp.Key, kvp.Value);
        }

        counter.Add(1, tagList);
    }

    public void TrackCustomEvent(string eventName, ReadOnlySpan<KeyValuePair<string, string>> attributes)
    {
        // This is a bit awkward, but is how custom events are logged when using OpenTelemetry.
        // See: https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-add-modify?tabs=aspnetcore#send-custom-events
        object[] args = new object[attributes.Length + 1];
        StringBuilder sb = new(args.Length * 10);

        int argIdx = 0;
        sb.Append("{microsoft.custom_event.name}");
        args[argIdx++] = eventName;

        for (int i = 0; i < attributes.Length; i++)
        {
            KeyValuePair<string, string> kvp = attributes[i];
            sb.Append(" {");
            sb.Append(kvp.Key);
            sb.Append('}');
            args[i + 1] = kvp.Value;
        }

        _logger.LogInformation(sb.ToString(), args);
    }
}
