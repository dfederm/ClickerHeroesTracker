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
                // Make sure this matches the API settings as well. Is there a better way to do this?
                instrumentationKey: "99fba640-790d-484f-83c4-3c97450d8698",

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