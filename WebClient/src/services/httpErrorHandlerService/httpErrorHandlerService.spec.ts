import { TestBed } from "@angular/core/testing";
import { LoggingService } from "../../services/loggingService/loggingService";
import { HttpErrorResponse } from "@angular/common/http";

import { HttpErrorHandlerService, IValidationErrorResponse } from "./httpErrorHandlerService";

describe("HttpErrorHandlerService", () => {
    let service: HttpErrorHandlerService;
    let loggingService: LoggingService;

    beforeEach(() => {
        loggingService = jasmine.createSpyObj("loggingService", ["logEvent"]);

        TestBed.configureTestingModule(
            {
                providers:
                    [
                        HttpErrorHandlerService,
                        { provide: LoggingService, useValue: loggingService },
                    ],
            });

        service = TestBed.inject(HttpErrorHandlerService);
    });

    describe("logError", () => {
        it("should log a client error", () => {
            let loggingService = TestBed.inject(LoggingService);

            let error = new ErrorEvent("someType", { message: "someMessage" });
            let err = new HttpErrorResponse({ error });

            service.logError("someEventName", err);
            expect(loggingService.logEvent).toHaveBeenCalledWith("someEventName", { status: "0", message: "someMessage" });
        });

        it("should log a server error", () => {
            let loggingService = TestBed.inject(LoggingService);

            let error = JSON.stringify({ someField: "someValue" });
            let err = new HttpErrorResponse({ status: 123, error });

            service.logError("someEventName", err);
            expect(loggingService.logEvent).toHaveBeenCalledWith("someEventName", { status: "123", message: error });
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
