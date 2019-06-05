import { Injectable } from "@angular/core";
import { AppInsightsService } from "@markpieszak/ng-application-insights";
import { HttpErrorResponse } from "@angular/common/http";

export interface IValidationErrorResponse {
    [field: string]: string[];
}

@Injectable({
    providedIn: "root",
})
export class HttpErrorHandlerService {
    constructor(
        private readonly appInsights: AppInsightsService,
    ) { }

    public logError(eventName: string, err: HttpErrorResponse): void {
        let status = (err.status || 0).toString();
        let message = err.error instanceof ErrorEvent
            // A client-side or network error occurred.
            ? err.error.message
            // The backend returned an unsuccessful response code.
            : err.error;

        this.appInsights.trackEvent(eventName, { status, message });
    }

    public getValidationErrors(err: HttpErrorResponse): string[] {
        if (err.error instanceof ErrorEvent) {
            return [err.error.message];
        }

        let errors: string[] = [];
        let validationErrorResponse: IValidationErrorResponse = JSON.parse(err.error);
        for (let field in validationErrorResponse) {
            errors.push(...validationErrorResponse[field]);
        }

        return errors;
    }
}
