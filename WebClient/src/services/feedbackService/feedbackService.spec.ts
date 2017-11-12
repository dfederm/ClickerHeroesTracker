import { TestBed, fakeAsync, tick } from "@angular/core/testing";
import { HttpClientTestingModule, HttpTestingController } from "@angular/common/http/testing";
import { HttpHeaders, HttpErrorResponse } from "@angular/common/http";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";

import { FeedbackService } from "./feedbackService";
import { AuthenticationService } from "../authenticationService/authenticationService";

describe("FeedbackService", () => {
    let feedbackService: FeedbackService;
    let authenticationService: AuthenticationService;
    let httpErrorHandlerService: HttpErrorHandlerService;
    let httpMock: HttpTestingController;

    const comments = "someComments";
    const email = "someEmail";

    beforeEach(() => {
        authenticationService = jasmine.createSpyObj("authenticationService", ["getAuthHeaders"]);
        (authenticationService.getAuthHeaders as jasmine.Spy).and.returnValue(Promise.resolve(new HttpHeaders()));

        httpErrorHandlerService = jasmine.createSpyObj("httpErrorHandlerService", ["logError"]);

        TestBed.configureTestingModule(
            {
                imports: [
                    HttpClientTestingModule,
                ],
                providers:
                    [
                        FeedbackService,
                        { provide: AuthenticationService, useValue: authenticationService },
                        { provide: HttpErrorHandlerService, useValue: httpErrorHandlerService },
                    ],
            });

        feedbackService = TestBed.get(FeedbackService) as FeedbackService;
        httpMock = TestBed.get(HttpTestingController) as HttpTestingController;
    });

    afterEach(() => {
        httpMock.verify();
    });

    describe("send", () => {
        const apiRequest = { method: "post", url: "/api/feedback" };

        it("should make the correct api call", fakeAsync(() => {
            feedbackService.send(comments, email);

            // Tick the getAuthHeaders call
            tick();

            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();

            let request = httpMock.expectOne(apiRequest);
            expect(request.request.body).toEqual("comments=someComments&email=someEmail");
        }));

        it("should handle when the api returns a success response", fakeAsync(() => {
            let succeeded = false;
            let error: HttpErrorResponse;
            feedbackService.send(comments, email)
                .then(() => succeeded = true)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null);
            tick();

            expect(succeeded).toEqual(true);
            expect(error).toBeUndefined();
            expect(httpErrorHandlerService.logError).not.toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let succeeded = false;
            let error: HttpErrorResponse;
            feedbackService.send(comments, email)
                .then(() => succeeded = true)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(succeeded).toEqual(false);
            expect(error).toBeDefined();
            expect(error.status).toEqual(500);
            expect(error.statusText).toEqual("someStatus");
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("FeedbackService.send.error", error);
        }));
    });
});
