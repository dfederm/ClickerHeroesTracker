import { TestBed } from "@angular/core/testing";
import { AppInsightsService } from "@markpieszak/ng-application-insights";
import { HttpErrorResponse } from "@angular/common/http";

import { HttpErrorHandlerService, IValidationErrorResponse } from "./httpErrorHandlerService";

describe("HttpErrorHandlerService", () => {
    let service: HttpErrorHandlerService;
    let appInsights: AppInsightsService;

    beforeEach(() => {
        appInsights = jasmine.createSpyObj("appInsights", ["trackEvent"]);

        TestBed.configureTestingModule(
            {
                providers:
                    [
                        HttpErrorHandlerService,
                        { provide: AppInsightsService, useValue: appInsights },
                    ],
            });

        service = TestBed.inject(HttpErrorHandlerService);
    });

    describe("logError", () => {
        it("should log a client error", () => {
            let appInsightsService = TestBed.inject(AppInsightsService);

            let error = new ErrorEvent("someType", { message: "someMessage" });
            let err = new HttpErrorResponse({ error });

            service.logError("someEventName", err);
            expect(appInsightsService.trackEvent).toHaveBeenCalledWith("someEventName", { status: "0", message: "someMessage" });
        });

        it("should log a server error", () => {
            let appInsightsService = TestBed.inject(AppInsightsService);

            let error = JSON.stringify({ someField: "someValue" });
            let err = new HttpErrorResponse({ status: 123, error });

            service.logError("someEventName", err);
            expect(appInsightsService.trackEvent).toHaveBeenCalledWith("someEventName", { status: "123", message: error });
        });
    });

    describe("getValidationErrors", () => {
        it("should get errors from a client error", () => {
            let error = new ErrorEvent("someType", { message: "someMessage" });
            let err = new HttpErrorResponse({ error });

            let errors = service.getValidationErrors(err);
            expect(errors).toEqual(["someMessage"]);
        });

        it("should get errors from a validation error response", () => {
            let error: IValidationErrorResponse = {
                field0: ["error0_0", "error0_1", "error0_2"],
                field1: ["error1_0", "error1_1", "error1_2"],
                field2: ["error2_0", "error2_1", "error2_2"],
            };
            let err = new HttpErrorResponse({ error: JSON.stringify(error) });

            let errors = service.getValidationErrors(err);
            expect(errors).toEqual(["error0_0", "error0_1", "error0_2", "error1_0", "error1_1", "error1_2", "error2_0", "error2_1", "error2_2"]);
        });
    });
});
