import { TestBed, fakeAsync, tick } from "@angular/core/testing";
import { HttpTestingController, provideHttpClientTesting } from "@angular/common/http/testing";
import { HttpHeaders, HttpErrorResponse, provideHttpClient } from "@angular/common/http";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";

import { ClanService, ISendMessageResponse, IClanData, ILeaderboardSummaryListResponse, IMessage } from "./clanService";
import { AuthenticationService } from "../authenticationService/authenticationService";

describe("ClanService", () => {
    let clanService: ClanService;
    let authenticationService: AuthenticationService;
    let httpErrorHandlerService: HttpErrorHandlerService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        authenticationService = jasmine.createSpyObj("authenticationService", ["getAuthHeaders"]);
        (authenticationService.getAuthHeaders as jasmine.Spy).and.returnValue(Promise.resolve(new HttpHeaders()));

        httpErrorHandlerService = jasmine.createSpyObj("httpErrorHandlerService", ["logError"]);

        TestBed.configureTestingModule({
            providers: [
                    ClanService,
                    provideHttpClient(),
                    provideHttpClientTesting(),
                    { provide: AuthenticationService, useValue: authenticationService },
                    { provide: HttpErrorHandlerService, useValue: httpErrorHandlerService },
                ],
        });

        clanService = TestBed.inject(ClanService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    describe("getClan", () => {
        const apiRequest = { method: "get", url: "/api/clans/someClan" };

        it("should make an api call", fakeAsync(() => {
            clanService.getClan("someClan");

            // Tick the getAuthHeaders call
            tick();

            httpMock.expectOne(apiRequest);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should return clan information", fakeAsync(() => {
            let response: IClanData;
            clanService.getClan("someClan")
                .then((r: IClanData) => response = r);

            // Tick the getAuthHeaders call
            tick();

            let expectedResponse: IClanData = { clanName: "", currentRaidLevel: 0, guildMembers: [], rank: 0, isBlocked: false };
            let request = httpMock.expectOne(apiRequest);
            request.flush(expectedResponse);
            tick();

            expect(response).toEqual(expectedResponse, "should return the expected response");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should return null when the user isn't in a clan", fakeAsync(() => {
            let response: IClanData;
            clanService.getClan("someClan")
                .then((r: IClanData) => response = r);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 204, statusText: "someStatus" });
            tick();

            expect(response).toBeNull("should return null");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let response: IClanData;
            let error: HttpErrorResponse;
            clanService.getClan("someClan")
                .then((r: IClanData) => response = r)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(response).toBeUndefined();
            expect(error).toBeDefined();
            expect(error.status).toEqual(500);
            expect(error.statusText).toEqual("someStatus");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("ClanService.getClan.error", error);
        }));
    });

    describe("getLeaderboard", () => {
        const apiRequest = { method: "get", url: "/api/clans?filter=foo&page=1&count=2" };

        it("should make an api call", fakeAsync(() => {
            clanService.getLeaderboard("foo", 1, 2);

            // Tick the getAuthHeaders call
            tick();

            httpMock.expectOne(apiRequest);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should return the leaderboard data", fakeAsync(() => {
            let response: ILeaderboardSummaryListResponse;
            clanService.getLeaderboard("foo", 1, 2)
                .then((r: ILeaderboardSummaryListResponse) => response = r);

            // Tick the getAuthHeaders call
            tick();

            let expectedResponse: ILeaderboardSummaryListResponse = { leaderboardClans: [], pagination: null };
            let request = httpMock.expectOne(apiRequest);
            request.flush(expectedResponse);
            tick();

            expect(response).toEqual(expectedResponse, "should return the expected response");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let response: ILeaderboardSummaryListResponse;
            let error: HttpErrorResponse;
            clanService.getLeaderboard("foo", 1, 2)
                .then((r: ILeaderboardSummaryListResponse) => response = r)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(response).toBeUndefined();
            expect(error).toBeDefined();
            expect(error.status).toEqual(500);
            expect(error.statusText).toEqual("someStatus");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("ClanService.getLeaderboard.error", error);
        }));
    });

    describe("getMessages", () => {
        const apiRequest = { method: "get", url: "/api/clans/messages" };

        it("should make an api call", fakeAsync(() => {
            clanService.getMessages();

            // Tick the getAuthHeaders call
            tick();

            httpMock.expectOne(apiRequest);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle when the api returns a success response", fakeAsync(() => {
            let response: IMessage[];
            let error: HttpErrorResponse;
            clanService.getMessages()
                .then((r: IMessage[]) => response = r)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let expectedResponse: IMessage[] = [{ date: "someDate", username: "someUsername", content: "someContent" }];
            let request = httpMock.expectOne(apiRequest);
            request.flush(expectedResponse);
            tick();

            expect(response).toEqual(expectedResponse);
            expect(error).toBeUndefined();
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(httpErrorHandlerService.logError).not.toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let response: IMessage[];
            let error: HttpErrorResponse;
            clanService.getMessages()
                .then((r: IMessage[]) => response = r)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(response).toBeUndefined();
            expect(error).toBeDefined();
            expect(error.status).toEqual(500);
            expect(error.statusText).toEqual("someStatus");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("ClanService.getMessages.error", error);
        }));
    });

    describe("sendMessage", () => {
        const apiRequest = { method: "post", url: "/api/clans/messages" };

        it("should make an api call", fakeAsync(() => {
            clanService.sendMessage("someMessage");

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            expect(request.request.body).toEqual("message=someMessage");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle when the api returns a success response", fakeAsync(() => {
            let succeeded = false;
            clanService.sendMessage("someMessage")
                .then(() => succeeded = true);

            // Tick the getAuthHeaders call
            tick();

            let expectedResponse: ISendMessageResponse = { success: true };
            let request = httpMock.expectOne(apiRequest);
            request.flush(expectedResponse);
            tick();

            expect(succeeded).toEqual(true);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle when the api returns a failed response", fakeAsync(() => {
            let succeeded = false;
            let error: string;
            clanService.sendMessage("someMessage")
                .then(() => succeeded = true)
                .catch((e: string) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let expectedResponse: ISendMessageResponse = { success: false, reason: "someReason" };
            let request = httpMock.expectOne(apiRequest);
            request.flush(expectedResponse);
            tick();

            expect(succeeded).toEqual(false);
            expect(error).toEqual("someReason");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let succeeded = false;
            let error: HttpErrorResponse;
            clanService.sendMessage("someMessage")
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
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("ClanService.sendMessage.error", error);
        }));
    });
});
