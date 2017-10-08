import { ReflectiveInjector } from "@angular/core";
import { fakeAsync, tick } from "@angular/core/testing";
import { BaseRequestOptions, ConnectionBackend, Http, RequestOptions } from "@angular/http";
import { Response, ResponseOptions, RequestMethod } from "@angular/http";
import { MockBackend, MockConnection } from "@angular/http/testing";
import { AppInsightsService } from "@markpieszak/ng-application-insights";

import { FeedbackService } from "./feedbackService";
import { AuthenticationService } from "../authenticationService/authenticationService";

describe("FeedbackService", () => {
    let feedbackService: FeedbackService;
    let authenticationService: AuthenticationService;
    let appInsights: AppInsightsService;
    let backend: MockBackend;
    let lastConnection: MockConnection;

    const comments = "someComments";
    const email = "someEmail";

    beforeEach(() => {
        authenticationService = jasmine.createSpyObj("authenticationService", ["getAuthHeaders"]);
        (authenticationService.getAuthHeaders as jasmine.Spy).and.returnValue(new Headers());

        appInsights = jasmine.createSpyObj("appInsights", ["trackEvent"]);

        let injector = ReflectiveInjector.resolveAndCreate(
            [
                FeedbackService,
                { provide: ConnectionBackend, useClass: MockBackend },
                { provide: RequestOptions, useClass: BaseRequestOptions },
                Http,
                { provide: AuthenticationService, useValue: authenticationService },
                { provide: AppInsightsService, useValue: appInsights },
            ]);

        feedbackService = injector.get(FeedbackService) as FeedbackService;
        backend = injector.get(ConnectionBackend) as MockBackend;
        backend.connections.subscribe((connection: MockConnection) => lastConnection = connection);
    });

    afterEach(() => {
        lastConnection = null;
        backend.verifyNoPendingRequests();
    });

    describe("send", () => {
        it("should make the correct api call when the use is not logged in", fakeAsync(() => {
            feedbackService.send(comments, email);

            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Post, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/feedback", "url invalid");
            expect(lastConnection.request.text()).toEqual("comments=someComments&email=someEmail", "request body invalid");
        }));

        it("should handle when the api returns a success response", fakeAsync(() => {
            let succeeded = false;
            let error: string;
            feedbackService.send(comments, email)
                .then(() => succeeded = true)
                .catch((e: string) => error = e);

            lastConnection.mockRespond(new Response(new ResponseOptions()));
            tick();

            expect(succeeded).toEqual(true);
            expect(error).toBeUndefined();
            expect(appInsights.trackEvent).not.toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let succeeded = false;
            let error: Error;
            feedbackService.send(comments, email)
                .then(() => succeeded = true)
                .catch((e: Error) => error = e);

            let expectedError = new Error("someError");
            lastConnection.mockError(expectedError);
            tick();

            expect(succeeded).toEqual(false);
            expect(error).toEqual(expectedError);
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));
    });
});
