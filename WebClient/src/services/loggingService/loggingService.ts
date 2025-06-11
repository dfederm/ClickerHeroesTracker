import { Injectable } from "@angular/core";
import { ApplicationInsights } from "@microsoft/applicationinsights-web";

@Injectable({
    providedIn: "root",
})
export class LoggingService {
    private appInsights: ApplicationInsights;

    constructor() {
        this.appInsights = new ApplicationInsights({
            config: {
                connectionString: "InstrumentationKey=388783a8-a141-49d5-9828-d5be7635625d;IngestionEndpoint=https://westus2-2.in.applicationinsights.azure.com/;LiveEndpoint=https://westus2.livediagnostics.monitor.azure.com/;ApplicationId=60c3b37a-d70a-4b52-b3e3-194f2824ea23",

                // log all route changes
                enableAutoRouteTracking: true,
            }
        });
        this.appInsights.loadAppInsights();
    }

    logEvent(name: string, properties?: { [key: string]: any }) {
        this.appInsights.trackEvent({ name: name }, properties);
    }

    logMetric(name: string, average: number, properties?: { [key: string]: any }) {
        this.appInsights.trackMetric({ name: name, average: average }, properties);
    }

    logException(exception: Error, severityLevel?: number) {
        this.appInsights.trackException({ exception: exception, severityLevel: severityLevel });
    }

    logTrace(message: string, properties?: { [key: string]: any }) {
        this.appInsights.trackTrace({ message: message }, properties);
    }

    setAuthenticatedUserContext(authenticatedUserId: string) {
        this.appInsights.setAuthenticatedUserContext(authenticatedUserId);
    }

    clearAuthenticatedUserContext() {
        this.appInsights.clearAuthenticatedUserContext();
    }
}