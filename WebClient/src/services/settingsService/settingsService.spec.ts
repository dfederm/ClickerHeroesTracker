import { ReflectiveInjector } from "@angular/core";
import { fakeAsync, tick, discardPeriodicTasks } from "@angular/core/testing";
import { BaseRequestOptions, ConnectionBackend, Http, RequestOptions } from "@angular/http";
import { Response, ResponseOptions, RequestMethod } from "@angular/http";
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
        (authenticationService.getAuthHeaders as jasmine.Spy).and.returnValue(new Headers());

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

    it("should return default settings initially when there is no cached data", () => {
        let settingsLog = createService();
        expect(settingsLog.length).toEqual(1);
        expect(settingsLog[0]).toEqual(SettingsService.defaultSettings);
    });

    it("should return default settings initially when there is cached data but the user is not logged in", () => {
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
    });

    it("should return cached settings initially when there is cached data and the user is logged in", () => {
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
    });

    it("should return settings when logged in", fakeAsync(() => {
        userInfo.next(loggedInUser);
        let settingsLog = createService();
        let expectedSettings = respondToLastConnection(0);

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
        let expectedSettings0 = respondToLastConnection(0);

        userInfo.next(notLoggedInUser);

        expect(settingsLog.length).toEqual(3);
        expect(settingsLog[0]).toEqual(SettingsService.defaultSettings);
        expect(settingsLog[1]).toEqual(expectedSettings0);
        expect(settingsLog[2]).toEqual(SettingsService.defaultSettings);
    }));

    it("should update the settings when it changes", fakeAsync(() => {
        userInfo.next(loggedInUser);
        let settingsLog = createService();
        let expectedSettings0 = respondToLastConnection(0);

        // Let the sync interval tick
        tick(SettingsService.syncInterval);

        let expectedSettings1 = respondToLastConnection(1);

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
        let expectedSettings0 = respondToLastConnection(0);

        // Let the sync interval tick
        tick(SettingsService.syncInterval);

        let expectedSettings1 = respondToLastConnection(1);

        // Let the sync interval lapse a whole bunch more times with the same settings
        for (let i = 0; i < 100; i++) {
            tick(SettingsService.syncInterval);
            respondToLastConnection(1);
        }

        // Let the sync interval tick again
        tick(SettingsService.syncInterval);

        let expectedSettings2 = respondToLastConnection(2);

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
        let expectedSettings0 = respondToLastConnection(0);

        // Let the sync interval tick
        tick(SettingsService.syncInterval);

        let expectedSettings1 = respondToLastConnection(1);

        // Let the sync interval lapse a whole bunch more times with an error
        for (let i = 0; i < 100; i++) {
            tick(SettingsService.syncInterval);
            errorToLastConnection();
        }

        // Let the sync interval lapse a whole bunch more times with an empty settings
        for (let i = 0; i < 100; i++) {
            tick(SettingsService.syncInterval);
            respondEmptyToLastConnection();
        }

        // Let the sync interval tick again
        tick(SettingsService.syncInterval);

        let expectedSettings2 = respondToLastConnection(2);

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

        // Retry and fail a bunch of times
        const numRetries = 100;
        for (let i = 0; i < numRetries; i++) {
            errorToLastConnection();
            tick(retryDelay);

            // Exponential backoff. This is tested since the last connection throws if the one before it isn't handled.
            retryDelay = Math.min(2 * retryDelay, SettingsService.syncInterval);
        }

        // Eventually succeed
        let expectedSettings = respondToLastConnection(0);

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

    function verifyLastConnection(): void {
        expect(lastConnection).not.toBeNull("no http service connection made");
        expect(lastConnection.request.method).toEqual(RequestMethod.Get, "method invalid");
        expect(lastConnection.request.url).toEqual("/api/users/someUsername/settings", "url invalid");
    }

    function respondToLastConnection(index: number): IUserSettings {
        verifyLastConnection();

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

        lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(settings) })));
        lastConnection = null;

        // Don't tick longer than the refresh interval
        tick(1);

        return settings;
    }

    function respondEmptyToLastConnection(): void {
        verifyLastConnection();

        lastConnection.mockRespond(new Response(new ResponseOptions({ body: "" })));
        lastConnection = null;

        // Don't tick longer than the refresh interval
        tick(1);
    }

    function errorToLastConnection(): void {
        verifyLastConnection();

        lastConnection.mockError();
        lastConnection = null;

        // Don't tick longer than the refresh interval
        tick(1);
    }
});
