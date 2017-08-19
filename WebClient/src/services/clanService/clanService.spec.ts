import { ReflectiveInjector } from "@angular/core";
import { fakeAsync, tick } from "@angular/core/testing";
import { BaseRequestOptions, ConnectionBackend, Http, RequestOptions } from "@angular/http";
import { Response, ResponseOptions, RequestMethod } from "@angular/http";
import { MockBackend, MockConnection } from "@angular/http/testing";

import { ClanService, ISendMessageResponse, ILeaderboardClan, IClanData, ILeaderboardSummaryListResponse } from "./clanService";
import { AuthenticationService } from "../authenticationService/authenticationService";

declare global
{
    // tslint:disable-next-line:interface-name - We don't own this interface name, just extending it
    interface Window
    {
        appInsights: Microsoft.ApplicationInsights.IAppInsights;
    }
}

describe("ClanService", () =>
{
    let clanService: ClanService;
    let authenticationService: AuthenticationService;
    let backend: MockBackend;
    let lastConnection: MockConnection;

    beforeEach(() =>
    {
        authenticationService = jasmine.createSpyObj("authenticationService", ["getAuthHeaders"]);
        (authenticationService.getAuthHeaders as jasmine.Spy).and.returnValue(Promise.resolve(new Headers()));

        let injector = ReflectiveInjector.resolveAndCreate(
            [
                ClanService,
                { provide: ConnectionBackend, useClass: MockBackend },
                { provide: RequestOptions, useClass: BaseRequestOptions },
                Http,
                { provide: AuthenticationService, useValue: authenticationService },
            ]);

        clanService = injector.get(ClanService) as ClanService;
        backend = injector.get(ConnectionBackend) as MockBackend;
        backend.connections.subscribe((connection: MockConnection) => lastConnection = connection);

        // Mock the global variable. We should figure out a better way to both inject this in the product and mock this in tests.
        window.appInsights = jasmine.createSpyObj("appInsights", ["trackEvent"]);
    });

    afterEach(() =>
    {
        lastConnection = null;
        backend.verifyNoPendingRequests();
    });

    describe("getClan", () =>
    {
        it("should make an api call", fakeAsync(() =>
        {
            clanService.getClan();
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Get, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/clans", "url invalid");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should return clan information", fakeAsync(() =>
        {
            let response: IClanData;
            clanService.getClan()
                .then((r: IClanData) => response = r);
            tick();

            let expectedResponse: IClanData = { clanName: "", currentRaidLevel: 0, guildMembers: [], messages: [] };
            lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(expectedResponse) })));
            tick();

            expect(response).toEqual(expectedResponse, "should return the expected response");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should return null when the user isn't in a clan", fakeAsync(() =>
        {
            let response: IClanData;
            clanService.getClan()
                .then((r: IClanData) => response = r);
            tick();

            lastConnection.mockRespond(new Response(new ResponseOptions({ status: 204 })));
            tick();

            expect(response).toBeNull("should return null");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle errors from authenticationService.getAuthHeaders", fakeAsync(() =>
        {
            (authenticationService.getAuthHeaders as jasmine.Spy).and.callFake(() => Promise.reject("someError"));

            tick();

            let response: IClanData;
            let error: string;
            clanService.getClan()
                .then((r: IClanData) => response = r)
                .catch((e: string) => error = e);
            tick();

            expect(response).toBeUndefined();
            expect(error).toEqual("someError");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() =>
        {
            let response: IClanData;
            let error: string;
            clanService.getClan()
                .then((r: IClanData) => response = r)
                .catch((e: string) => error = e);
            tick();

            lastConnection.mockError(new Error("someError"));
            tick();

            expect(response).toBeUndefined();
            expect(error).toEqual("someError");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));
    });

    describe("getUserClan", () =>
    {
        it("should make an api call", fakeAsync(() =>
        {
            clanService.getUserClan();
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Get, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/clans/userClan", "url invalid");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should return the user's clan leaderboard data", fakeAsync(() =>
        {
            let response: ILeaderboardClan;
            clanService.getUserClan()
                .then((r: ILeaderboardClan) => response = r);
            tick();

            let expectedResponse: ILeaderboardClan = { name: "", currentRaidLevel: 0, memberCount: 0, rank: 0, isUserClan: false };
            lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(expectedResponse) })));
            tick();

            expect(response).toEqual(expectedResponse, "should return the expected response");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should return null when the user isn't in a clan", fakeAsync(() =>
        {
            let response: ILeaderboardClan;
            clanService.getUserClan()
                .then((r: ILeaderboardClan) => response = r);
            tick();

            lastConnection.mockRespond(new Response(new ResponseOptions({ status: 204 })));
            tick();

            expect(response).toBeNull("should return null");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle errors from authenticationService.getAuthHeaders", fakeAsync(() =>
        {
            (authenticationService.getAuthHeaders as jasmine.Spy).and.callFake(() => Promise.reject("someError"));

            tick();

            let response: ILeaderboardClan;
            let error: string;
            clanService.getUserClan()
                .then((r: ILeaderboardClan) => response = r)
                .catch((e: string) => error = e);
            tick();

            expect(response).toBeUndefined();
            expect(error).toEqual("someError");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() =>
        {
            let response: ILeaderboardClan;
            let error: string;
            clanService.getUserClan()
                .then((r: ILeaderboardClan) => response = r)
                .catch((e: string) => error = e);
            tick();

            lastConnection.mockError(new Error("someError"));
            tick();

            expect(response).toBeUndefined();
            expect(error).toEqual("someError");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));
    });

    describe("getLeaderboard", () =>
    {
        it("should make an api call", fakeAsync(() =>
        {
            clanService.getLeaderboard(1, 2);
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Get, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/clans/leaderboard?page=1&count=2", "url invalid");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should return the leaderboard data", fakeAsync(() =>
        {
            let response: ILeaderboardSummaryListResponse;
            clanService.getLeaderboard(1, 2)
                .then((r: ILeaderboardSummaryListResponse) => response = r);
            tick();

            let expectedResponse: ILeaderboardSummaryListResponse = { leaderboardClans: [], pagination: null };
            lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(expectedResponse) })));
            tick();

            expect(response).toEqual(expectedResponse, "should return the expected response");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle errors from authenticationService.getAuthHeaders", fakeAsync(() =>
        {
            (authenticationService.getAuthHeaders as jasmine.Spy).and.callFake(() => Promise.reject("someError"));

            tick();

            let response: ILeaderboardSummaryListResponse;
            let error: string;
            clanService.getLeaderboard(1, 2)
                .then((r: ILeaderboardSummaryListResponse) => response = r)
                .catch((e: string) => error = e);
            tick();

            expect(response).toBeUndefined();
            expect(error).toEqual("someError");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() =>
        {
            let response: ILeaderboardSummaryListResponse;
            let error: string;
            clanService.getLeaderboard(1, 2)
                .then((r: ILeaderboardSummaryListResponse) => response = r)
                .catch((e: string) => error = e);
            tick();

            lastConnection.mockError(new Error("someError"));
            tick();

            expect(response).toBeUndefined();
            expect(error).toEqual("someError");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));
    });

    describe("sendMessage", () =>
    {
        it("should make an api call", fakeAsync(() =>
        {
            clanService.sendMessage("someMessage", "someClanName");
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Post, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/clans/messages", "url invalid");
            expect(lastConnection.request.text()).toEqual("message=someMessage&clanName=someClanName", "request body invalid");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle when the api returns a success response", fakeAsync(() =>
        {
            let succeeded = false;
            clanService.sendMessage("someMessage", "someClanName")
                .then(() => succeeded = true);
            tick();

            let expectedResponse: ISendMessageResponse = { success: true };
            lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(expectedResponse) })));
            tick();

            expect(succeeded).toEqual(true);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle when the api returns a failed response", fakeAsync(() =>
        {
            let succeeded = false;
            let error: string;
            clanService.sendMessage("someMessage", "someClanName")
                .then(() => succeeded = true)
                .catch((e: string) => error = e);
            tick();

            let expectedResponse: ISendMessageResponse = { success: false, reason: "someReason" };
            lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(expectedResponse) })));
            tick();

            expect(succeeded).toEqual(false);
            expect(error).toEqual("someReason");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle errors from authenticationService.getAuthHeaders", fakeAsync(() =>
        {
            (authenticationService.getAuthHeaders as jasmine.Spy).and.callFake(() => Promise.reject("someError"));

            tick();

            let succeeded = false;
            let error: string;
            clanService.sendMessage("someMessage", "someClanName")
                .then(() => succeeded = true)
                .catch((e: string) => error = e);
            tick();

            expect(succeeded).toEqual(false);
            expect(error).toEqual("someError");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() =>
        {
            let succeeded = false;
            let error: string;
            clanService.sendMessage("someMessage", "someClanName")
                .then(() => succeeded = true)
                .catch((e: string) => error = e);
            tick();

            lastConnection.mockError(new Error("someError"));
            tick();

            expect(succeeded).toEqual(false);
            expect(error).toEqual("someError");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));
    });
});
