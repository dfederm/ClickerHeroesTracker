import { ReflectiveInjector } from "@angular/core";
import { fakeAsync, tick, discardPeriodicTasks } from "@angular/core/testing";
import { BaseRequestOptions, ConnectionBackend, Http, RequestOptions, Response, ResponseOptions, Headers, RequestMethod } from "@angular/http";
import { MockBackend, MockConnection } from "@angular/http/testing";
import { BehaviorSubject } from "rxjs";

import { SettingsService, IUserSettings, PlayStyle, Theme } from "./settingsService";
import { AuthenticationService, IUserInfo } from "../authenticationService/authenticationService";

describe("SettingsService", () => {
    let injector: ReflectiveInjector;
    let backend: MockBackend;
    let lastConnection: MockConnection = null;
    let userInfo: BehaviorSubject<IUserInfo>;

    const loggedInUser: IUserInfo = {
        isLoggedIn: true,
        id: "someId",
        username: "someUsername",
        email: "someEmail",
    };

    const notLoggedInUser: IUserInfo = {
        isLoggedIn: false,
    };

    beforeEach(() => {
        userInfo = new BehaviorSubject(notLoggedInUser);
        let authenticationService: AuthenticationService = jasmine.createSpyObj("authenticationService", ["userInfo", "getAuthHeaders"]);
        (authenticationService.userInfo as jasmine.Spy).and.returnValue(userInfo);
        (authenticationService.getAuthHeaders as jasmine.Spy).and.returnValue(Promise.resolve(new Headers()));

        injector = ReflectiveInjector.resolveAndCreate(
            [
                { provide: AuthenticationService, useValue: authenticationService },
                { provide: ConnectionBackend, useClass: MockBackend },
                { provide: RequestOptions, useClass: BaseRequestOptions },
                Http,
                SettingsService,
            ]);

        backend = injector.get(ConnectionBackend) as MockBackend;
        backend.connections.subscribe((connection: MockConnection) => {
            if (lastConnection != null) {
                fail("Previous connection not handled");
            }

            lastConnection = connection;
        });

        spyOn(localStorage, "getItem");
        spyOn(localStorage, "setItem");
        spyOn(localStorage, "removeItem");
    });

    afterEach(() => {
        lastConnection = null;
        backend.verifyNoPendingRequests();
    });

    describe("settings", () => {
        it("should return default settings initially when there is no cached data", fakeAsync(() => {
            let settingsLog = createService();
            expect(settingsLog.length).toEqual(1);
            expect(settingsLog[0]).toEqual(SettingsService.defaultSettings);
        }));

        it("should return default settings initially when there is cached data but the user is not logged in", fakeAsync(() => {
            let settings: IUserSettings = {
                areUploadsPublic: false,
                playStyle: "active",
                useScientificNotation: false,
                scientificNotationThreshold: 123,
                useEffectiveLevelForSuggestions: true,
                useLogarithmicGraphScale: false,
                logarithmicGraphScaleThreshold: 456,
                hybridRatio: 2,
                theme: "dark",
            };
            (localStorage.getItem as jasmine.Spy).and.returnValue(JSON.stringify(settings));

            let settingsLog = createService();

            expect(settingsLog.length).toEqual(1);
            expect(settingsLog[0]).toEqual(SettingsService.defaultSettings);
            expect(localStorage.getItem).toHaveBeenCalledWith(SettingsService.settingsKey);

            // The cache is cleared
            expect(localStorage.removeItem).toHaveBeenCalledWith(SettingsService.settingsKey);
        }));

        it("should return cached settings initially when there is cached data and the user is logged in", fakeAsync(() => {
            let expectedSettings: IUserSettings = {
                areUploadsPublic: false,
                playStyle: "active",
                useScientificNotation: false,
                scientificNotationThreshold: 123,
                useEffectiveLevelForSuggestions: true,
                useLogarithmicGraphScale: false,
                logarithmicGraphScaleThreshold: 456,
                hybridRatio: 2,
                theme: "dark",
            };
            (localStorage.getItem as jasmine.Spy).and.returnValue(JSON.stringify(expectedSettings));

            userInfo.next(loggedInUser);
            let settingsLog = createService();

            expect(settingsLog.length).toEqual(1);
            expect(settingsLog[0]).toEqual(expectedSettings);
            expect(localStorage.getItem).toHaveBeenCalledWith(SettingsService.settingsKey);
            expect(localStorage.removeItem).not.toHaveBeenCalled();
        }));

        it("should return settings when logged in", fakeAsync(() => {
            userInfo.next(loggedInUser);
            let settingsLog = createService();

            // Tick the getAuthHeaders call, but don't tick longer than the refresh interval
            tick(1);

            let expectedSettings = respondToGetSettingsRequest(0);

            // 2 because the settings fetch is async so we initially started with the defaults
            expect(settingsLog.length).toEqual(2);
            expect(settingsLog[0]).toEqual(SettingsService.defaultSettings);
            expect(settingsLog[1]).toEqual(expectedSettings);
            expect(localStorage.setItem).toHaveBeenCalledWith(SettingsService.settingsKey, JSON.stringify(expectedSettings));

            // An interval was started, so abandon it.
            discardPeriodicTasks();
        }));

        it("should return default settings after logging out", fakeAsync(() => {
            userInfo.next(loggedInUser);
            let settingsLog = createService();

            // Tick the getAuthHeaders call, but don't tick longer than the refresh interval
            tick(1);

            let expectedSettings0 = respondToGetSettingsRequest(0);

            userInfo.next(notLoggedInUser);

            expect(settingsLog.length).toEqual(3);
            expect(settingsLog[0]).toEqual(SettingsService.defaultSettings);
            expect(settingsLog[1]).toEqual(expectedSettings0);
            expect(settingsLog[2]).toEqual(SettingsService.defaultSettings);
        }));

        it("should update the settings when it changes", fakeAsync(() => {
            userInfo.next(loggedInUser);
            let settingsLog = createService();

            // Tick the getAuthHeaders call, but don't tick longer than the refresh interval
            tick(1);

            let expectedSettings0 = respondToGetSettingsRequest(0);

            // Let the sync interval tick
            tick(SettingsService.syncInterval);

            let expectedSettings1 = respondToGetSettingsRequest(1);

            expect(settingsLog.length).toEqual(3);
            expect(settingsLog[0]).toEqual(SettingsService.defaultSettings);
            expect(settingsLog[1]).toEqual(expectedSettings0);
            expect(settingsLog[2]).toEqual(expectedSettings1);

            // An interval was started, so abandon it.
            discardPeriodicTasks();
        }));

        it("should not update the settings when it doesn't change", fakeAsync(() => {
            userInfo.next(loggedInUser);
            let settingsLog = createService();

            // Tick the getAuthHeaders call, but don't tick longer than the refresh interval
            tick(1);

            let expectedSettings0 = respondToGetSettingsRequest(0);

            // Let the sync interval tick
            tick(SettingsService.syncInterval);

            let expectedSettings1 = respondToGetSettingsRequest(1);

            // Let the sync interval lapse a whole bunch more times with the same settings
            for (let i = 0; i < 100; i++) {
                tick(SettingsService.syncInterval);
                respondToGetSettingsRequest(1);
            }

            // Let the sync interval tick again
            tick(SettingsService.syncInterval);

            let expectedSettings2 = respondToGetSettingsRequest(2);

            expect(settingsLog.length).toEqual(4);
            expect(settingsLog[0]).toEqual(SettingsService.defaultSettings);
            expect(settingsLog[1]).toEqual(expectedSettings0);
            expect(settingsLog[2]).toEqual(expectedSettings1);
            expect(settingsLog[3]).toEqual(expectedSettings2);

            // An interval was started, so abandon it.
            discardPeriodicTasks();
        }));

        it("should not update the settings when it errors", fakeAsync(() => {
            userInfo.next(loggedInUser);
            let settingsLog = createService();

            // Tick the getAuthHeaders call, but don't tick longer than the refresh interval
            tick(1);

            let expectedSettings0 = respondToGetSettingsRequest(0);

            // Let the sync interval tick
            tick(SettingsService.syncInterval);

            let expectedSettings1 = respondToGetSettingsRequest(1);

            // Let the sync interval lapse a whole bunch more times with an error
            for (let i = 0; i < 100; i++) {
                tick(SettingsService.syncInterval);
                errorToGetSettingsRequest();
            }

            // Let the sync interval lapse a whole bunch more times with an empty settings
            for (let i = 0; i < 100; i++) {
                tick(SettingsService.syncInterval);
                respondEmptyToGetSettingsRequest();
            }

            // Let the sync interval tick again
            tick(SettingsService.syncInterval);

            let expectedSettings2 = respondToGetSettingsRequest(2);

            expect(settingsLog.length).toEqual(4);
            expect(settingsLog[0]).toEqual(SettingsService.defaultSettings);
            expect(settingsLog[1]).toEqual(expectedSettings0);
            expect(settingsLog[2]).toEqual(expectedSettings1);
            expect(settingsLog[3]).toEqual(expectedSettings2);

            // An interval was started, so abandon it.
            discardPeriodicTasks();
        }));

        it("should retry the initial fetch", fakeAsync(() => {
            userInfo.next(loggedInUser);
            let settingsLog = createService();
            let retryDelay = SettingsService.retryDelay;

            // Tick the getAuthHeaders call, but don't tick longer than the refresh interval
            tick(1);

            // Retry and fail a bunch of times
            const numRetries = 100;
            for (let i = 0; i < numRetries; i++) {
                errorToGetSettingsRequest();
                tick(retryDelay);

                // Exponential backoff. This is tested since the last connection throws if the one before it isn't handled.
                retryDelay = Math.min(2 * retryDelay, SettingsService.syncInterval);
            }

            // Eventually succeed
            let expectedSettings = respondToGetSettingsRequest(0);

            expect(settingsLog.length).toEqual(2);
            expect(settingsLog[0]).toEqual(SettingsService.defaultSettings);
            expect(settingsLog[1]).toEqual(expectedSettings);

            // An interval was started, so abandon it.
            discardPeriodicTasks();
        }));

        function createService(): IUserSettings[] {
            let settingsLog: IUserSettings[] = [];
            let settingsService = injector.get(SettingsService) as SettingsService;

            settingsService.settings().subscribe(settings => {
                settingsLog.push(settings);
            });

            return settingsLog;
        }
    });

    describe("setSetting", () => {
        let settingsService: SettingsService;

        it("should make the correct api call", fakeAsync(() => {
            let settingsLog = createService();

            let resolved = false;
            let rejected = false;
            settingsService.setSetting("playStyle", "somePlayStyle")
                .then(() => resolved = true)
                .catch(() => rejected = true);

            // Tick the getAuthHeaders call
            tick();

            verifySetSettingsRequest();
            expect(settingsLog.length).toEqual(0);

            expect(resolved).toEqual(false);
            expect(rejected).toEqual(false);
        }));

        it("should only refresh the settings once all pending patches are complete", fakeAsync(() => {
            let settingsLog = createService();

            let resolved1 = false;
            let rejected1 = false;
            settingsService.setSetting("playStyle", "somePlayStyle")
                .then(() => resolved1 = true)
                .catch(() => rejected1 = true);

            // Tick the getAuthHeaders call
            tick();

            let connection1 = lastConnection;
            expect(connection1).not.toBeNull();
            lastConnection = null;

            // Let the sync interval tick
            tick(SettingsService.syncInterval);
            expect(settingsLog.length).toEqual(0);

            let resolved2 = false;
            let rejected2 = false;
            settingsService.setSetting("playStyle", "somePlayStyle")
                .then(() => resolved2 = true)
                .catch(() => rejected2 = true);

            // Tick the getAuthHeaders call
            tick();

            let connection2 = lastConnection;
            expect(connection2).not.toBeNull();
            lastConnection = null;

            respondToSetSettingsRequest(connection1);
            expect(resolved1).toEqual(true);
            expect(rejected1).toEqual(false);

            // Let the sync interval tick again
            tick(SettingsService.syncInterval);
            expect(settingsLog.length).toEqual(0);

            respondToSetSettingsRequest(connection2);
            expect(resolved1).toEqual(true);
            expect(rejected1).toEqual(false);

            // Let the sync interval tick again
            tick(SettingsService.syncInterval);
            expect(settingsLog.length).toEqual(0);

            let expectedSettings1 = respondToGetSettingsRequest(1);

            // Let the sync interval tick one last time, where it should re-fetch
            tick(SettingsService.syncInterval);
            let expectedSettings2 = respondToGetSettingsRequest(2);

            expect(settingsLog.length).toEqual(2);
            expect(settingsLog[0]).toEqual(expectedSettings1);
            expect(settingsLog[1]).toEqual(expectedSettings2);

            // An interval was started, so abandon it.
            discardPeriodicTasks();
        }));

        it("should not refresh the settings if a patch happens during a refresh", fakeAsync(() => {
            let settingsLog = createService();

            // Let the sync interval tick, meaning the new request is pending but not returned yet
            tick(SettingsService.syncInterval);
            let getSettingsConnection = lastConnection;
            expect(getSettingsConnection).not.toBeNull();
            lastConnection = null;

            let resolved = false;
            let rejected = false;
            settingsService.setSetting("playStyle", "somePlayStyle")
                .then(() => resolved = true)
                .catch(() => rejected = true);

            // Tick the getAuthHeaders call
            tick();

            // The refresh returned after the patch started
            respondToGetSettingsRequest(1, getSettingsConnection);

            verifySetSettingsRequest();
            expect(settingsLog.length).toEqual(0);

            expect(resolved).toEqual(false);
            expect(rejected).toEqual(false);
        }));

        it("should start refreshing the settings again after patches fail", fakeAsync(() => {
            let settingsLog = createService();

            let resolved = false;
            let rejected = false;
            settingsService.setSetting("playStyle", "somePlayStyle")
                .then(() => resolved = true)
                .catch(() => rejected = true);

            // Let the sync interval tick
            tick(SettingsService.syncInterval);
            expect(settingsLog.length).toEqual(0);

            errorToSetSettingsRequest();
            expect(resolved).toEqual(false);
            expect(rejected).toEqual(true);

            // Let the sync interval tick again
            tick(SettingsService.syncInterval);
            expect(settingsLog.length).toEqual(0);

            let expectedSettings1 = respondToGetSettingsRequest(1);

            // Let the sync interval tick one last time, where it should re-fetch
            tick(SettingsService.syncInterval);
            let expectedSettings2 = respondToGetSettingsRequest(2);

            expect(settingsLog.length).toEqual(2);
            expect(settingsLog[0]).toEqual(expectedSettings1);
            expect(settingsLog[1]).toEqual(expectedSettings2);

            // An interval was started, so abandon it.
            discardPeriodicTasks();
        }));

        function createService(): IUserSettings[] {
            userInfo.next(loggedInUser);

            let settingsLog: IUserSettings[];
            settingsService = injector.get(SettingsService) as SettingsService;

            settingsService.settings().subscribe(settings => {
                if (settingsLog) {
                    settingsLog.push(settings);
                }
            });

            // Tick the getAuthHeaders call
            tick();

            respondToGetSettingsRequest(0);

            // Start the log now so we skip the initial state and initial fetch.
            settingsLog = [];

            return settingsLog;
        }
    });

    function verifyGetSettingsRequest(connection?: MockConnection): void {
        if (!connection) {
            connection = lastConnection;
        }

        expect(connection).not.toBeNull("no http service connection made");
        expect(connection.request.method).toEqual(RequestMethod.Get, "method invalid");
        expect(connection.request.url).toEqual("/api/users/someUsername/settings", "url invalid");
    }

    function respondToGetSettingsRequest(index: number, connection?: MockConnection): IUserSettings {
        if (!connection) {
            connection = lastConnection;
            lastConnection = null;
        }

        verifyGetSettingsRequest(connection);

        let settings: IUserSettings = {
            areUploadsPublic: index % 2 === 0,
            playStyle: (["idle", "hybrid", "active"] as PlayStyle[])[index % 3],
            useScientificNotation: index % 2 === 0,
            scientificNotationThreshold: index,
            useEffectiveLevelForSuggestions: index % 2 === 0,
            useLogarithmicGraphScale: index % 2 === 0,
            logarithmicGraphScaleThreshold: index,
            hybridRatio: index,
            theme: (["light", "dark"] as Theme[])[index % 2],
        };

        connection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(settings) })));

        // Don't tick longer than the refresh interval
        tick(1);

        return settings;
    }

    function respondEmptyToGetSettingsRequest(): void {
        verifyGetSettingsRequest();

        lastConnection.mockRespond(new Response(new ResponseOptions({ body: "" })));
        lastConnection = null;

        // Don't tick longer than the refresh interval
        tick(1);
    }

    function errorToGetSettingsRequest(): void {
        verifyGetSettingsRequest();

        lastConnection.mockError();
        lastConnection = null;

        // Don't tick longer than the refresh interval
        tick(1);
    }

    function verifySetSettingsRequest(connection?: MockConnection): void {
        if (!connection) {
            connection = lastConnection;
        }

        expect(connection).not.toBeNull("no http service connection made");
        expect(connection.request.method).toEqual(RequestMethod.Patch, "method invalid");
        expect(connection.request.url).toEqual("/api/users/someUsername/settings", "url invalid");
        expect(connection.request.json()).toEqual({ playStyle: "somePlayStyle" }, "request body invalid");
    }

    function respondToSetSettingsRequest(connection?: MockConnection): void {
        if (!connection) {
            connection = lastConnection;
            lastConnection = null;
        }

        verifySetSettingsRequest(connection);
        connection.mockRespond(new Response(new ResponseOptions()));

        // Don't tick longer than the refresh interval
        tick(1);
    }

    function errorToSetSettingsRequest(): void {
        verifySetSettingsRequest();

        lastConnection.mockError();
        lastConnection = null;

        // Don't tick longer than the refresh interval
        tick(1);
    }
});
