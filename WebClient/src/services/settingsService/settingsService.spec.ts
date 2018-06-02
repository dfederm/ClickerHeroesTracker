import { TestBed, fakeAsync, tick, discardPeriodicTasks } from "@angular/core/testing";
import { HttpClientTestingModule, HttpTestingController, TestRequest } from "@angular/common/http/testing";
import { HttpHeaders } from "@angular/common/http";
import { BehaviorSubject } from "rxjs";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";

import { SettingsService, IUserSettings, PlayStyle, Theme, GraphSpacingType } from "./settingsService";
import { AuthenticationService, IUserInfo } from "../authenticationService/authenticationService";

describe("SettingsService", () => {
    let httpMock: HttpTestingController;
    let userInfo: BehaviorSubject<IUserInfo>;

    const getSettingsRequest = { method: "get", url: "/api/users/someUsername/settings" };
    const setSettingsRequest = { method: "patch", url: "/api/users/someUsername/settings" };

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
        (authenticationService.getAuthHeaders as jasmine.Spy).and.returnValue(Promise.resolve(new HttpHeaders()));

        let httpErrorHandlerService = {
            logError: (): void => void 0,
        };

        TestBed.configureTestingModule(
            {
                imports: [
                    HttpClientTestingModule,
                ],
                providers:
                    [
                        { provide: AuthenticationService, useValue: authenticationService },
                        SettingsService,
                        { provide: HttpErrorHandlerService, useValue: httpErrorHandlerService },
                    ],
            });

        httpMock = TestBed.get(HttpTestingController) as HttpTestingController;

        spyOn(localStorage, "getItem");
        spyOn(localStorage, "setItem");
        spyOn(localStorage, "removeItem");
    });

    afterEach(() => {
        httpMock.verify();
    });

    describe("settings", () => {
        it("should return default settings initially when there is no cached data", fakeAsync(() => {
            let settingsLog = createService();
            expect(settingsLog.length).toEqual(1);
            expect(settingsLog[0]).toEqual(SettingsService.defaultSettings);
        }));

        it("should return cached settings initially when there is cached data and the user is not logged in", fakeAsync(() => {
            let expectedSettings: IUserSettings = {
                playStyle: "active",
                useScientificNotation: false,
                scientificNotationThreshold: 123,
                useLogarithmicGraphScale: false,
                logarithmicGraphScaleThreshold: 456,
                hybridRatio: 2,
                theme: "dark",
                shouldLevelSkillAncients: false,
                skillAncientBaseAncient: 1,
                skillAncientLevelDiff: 2,
                graphSpacingType: "ascension",
            };
            (localStorage.getItem as jasmine.Spy).and.returnValue(JSON.stringify(expectedSettings));

            let settingsLog = createService();

            expect(settingsLog.length).toEqual(1);
            expect(settingsLog[0]).toEqual(expectedSettings);
            expect(localStorage.getItem).toHaveBeenCalledWith(SettingsService.settingsKey);
            expect(localStorage.removeItem).not.toHaveBeenCalled();
        }));

        it("should return cached settings initially when there is cached data and the user is logged in", fakeAsync(() => {
            let expectedSettings: IUserSettings = {
                playStyle: "active",
                useScientificNotation: false,
                scientificNotationThreshold: 123,
                useLogarithmicGraphScale: false,
                logarithmicGraphScaleThreshold: 456,
                hybridRatio: 2,
                theme: "dark",
                shouldLevelSkillAncients: false,
                skillAncientBaseAncient: 1,
                skillAncientLevelDiff: 2,
                graphSpacingType: "time",
            };
            (localStorage.getItem as jasmine.Spy).and.returnValue(JSON.stringify(expectedSettings));

            userInfo.next(loggedInUser);
            let settingsLog = createService();

            // Tick the getAuthHeaders call
            tick();

            // Initial fetch
            expectGetSettingsRequest();

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

            // The cache is cleared
            expect(localStorage.removeItem).toHaveBeenCalledWith(SettingsService.settingsKey);
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
            let settingsService = TestBed.get(SettingsService) as SettingsService;

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

            expectSetSettingsRequest();
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

            let request1 = expectSetSettingsRequest();

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

            let request2 = expectSetSettingsRequest();

            respondToSetSettingsRequest(request1);
            expect(resolved1).toEqual(true);
            expect(rejected1).toEqual(false);

            // Let the sync interval tick again
            tick(SettingsService.syncInterval);
            expect(settingsLog.length).toEqual(0);

            respondToSetSettingsRequest(request2);
            expect(resolved2).toEqual(true);
            expect(rejected2).toEqual(false);

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
            let request = expectGetSettingsRequest();

            let resolved = false;
            let rejected = false;
            settingsService.setSetting("playStyle", "somePlayStyle")
                .then(() => resolved = true)
                .catch(() => rejected = true);

            // Tick the getAuthHeaders call
            tick();

            // The refresh returned after the patch started
            respondToGetSettingsRequest(1, request);

            expectSetSettingsRequest();
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

        it("should update settings when the user is not logged in", done => {
            userInfo.next(notLoggedInUser);

            let settingsLog: IUserSettings[];
            settingsService = TestBed.get(SettingsService) as SettingsService;

            settingsService.settings().subscribe(settings => {
                if (settingsLog) {
                    settingsLog.push(settings);
                }
            });

            // Start the log now so we skip the initial state.
            settingsLog = [];

            settingsService.setSetting("playStyle", "somePlayStyle")
                .then(() => {
                    expect(settingsLog.length).toEqual(1);
                    expect(settingsLog[0]).toEqual(Object.assign({}, SettingsService.defaultSettings, { playStyle: "somePlayStyle" }));
                })
                .then(done)
                .catch(done.fail);
        });

        function createService(): IUserSettings[] {
            userInfo.next(loggedInUser);

            let settingsLog: IUserSettings[];
            settingsService = TestBed.get(SettingsService) as SettingsService;

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

    function expectGetSettingsRequest(): TestRequest {
        return httpMock.expectOne(getSettingsRequest);
    }

    function respondToGetSettingsRequest(index: number, request?: TestRequest): IUserSettings {
        if (!request) {
            request = expectGetSettingsRequest();
        }

        let settings: IUserSettings = {
            playStyle: (["idle", "hybrid", "active"] as PlayStyle[])[index % 3],
            useScientificNotation: index % 2 === 0,
            scientificNotationThreshold: index,
            useLogarithmicGraphScale: index % 2 === 0,
            logarithmicGraphScaleThreshold: index,
            hybridRatio: index,
            theme: (["light", "dark"] as Theme[])[index % 2],
            shouldLevelSkillAncients: index % 2 === 0,
            skillAncientBaseAncient: index,
            skillAncientLevelDiff: index,
            graphSpacingType: (["time", "ascension"] as GraphSpacingType[])[index % 2],
        };

        request.flush(settings);

        // Don't tick longer than the refresh interval
        tick(1);

        return settings;
    }

    function errorToGetSettingsRequest(): void {
        let request = expectGetSettingsRequest();
        request.flush(null, { status: 500, statusText: "someStatus" });

        // Don't tick longer than the refresh interval
        tick(1);
    }

    function expectSetSettingsRequest(): TestRequest {
        let request = httpMock.expectOne(setSettingsRequest);
        expect(request.request.body).toEqual({ playStyle: "somePlayStyle" });
        return request;
    }

    function respondToSetSettingsRequest(request?: TestRequest): void {
        if (!request) {
            request = expectSetSettingsRequest();
        }

        request.flush(null);

        // Don't tick longer than the refresh interval
        tick(1);
    }

    function errorToSetSettingsRequest(): void {
        let request = expectSetSettingsRequest();
        request.flush(null, { status: 500, statusText: "someStatus" });

        // Don't tick longer than the refresh interval
        tick(1);
    }
});
