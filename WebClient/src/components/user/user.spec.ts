import { NO_ERRORS_SCHEMA } from "@angular/core";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import Decimal from "decimal.js";
import { ChartDataSets, ChartOptions, ChartPoint } from "chart.js";
import { BehaviorSubject } from "rxjs";

import { UserComponent } from "./user";
import { UserService, IProgressData, IFollowsData } from "../../services/userService/userService";
import { SettingsService } from "../../services/settingsService/settingsService";
import { ActivatedRoute } from "@angular/router";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { IUser } from "../../models";

describe("UserComponent", () => {
    let fixture: ComponentFixture<UserComponent>;

    const userName = "someUserName";

    let routeParams = new BehaviorSubject({ userName });

    const settings = SettingsService.defaultSettings;

    let settingsSubject = new BehaviorSubject(settings);

    let progress: IProgressData = {
        soulsSpentData: {
            "2017-01-01T00:00:00Z": "0",
            "2017-01-02T00:00:00Z": "1",
            "2017-01-03T00:00:00Z": "2",
            "2017-01-04T00:00:00Z": "3",
            "2017-01-05T00:00:00Z": "4",
        },
        titanDamageData: undefined,
        heroSoulsSacrificedData: undefined,
        totalAncientSoulsData: undefined,
        transcendentPowerData: undefined,
        rubiesData: undefined,
        highestZoneThisTranscensionData: undefined,
        highestZoneLifetimeData: undefined,
        ascensionsThisTranscensionData: undefined,
        ascensionsLifetimeData: undefined,
        ancientLevelData: undefined,
        outsiderLevelData: undefined,
    };

    let followsData: IFollowsData = {
        follows: [
            "followedUser1",
            "followedUser2",
            "followedUser3",
        ],
    };

    let userInfo: BehaviorSubject<IUserInfo>;

    const loggedInUser: IUserInfo = {
        isLoggedIn: true,
        id: "currentUserId",
        username: "currentUserName",
        email: "currentUserEmail",
    };

    const notLoggedInUser: IUserInfo = {
        isLoggedIn: false,
    };

    const user: IUser = {
        name: loggedInUser.username,
        clanName: "someClanName",
    };

    beforeEach(done => {
        let route = { params: routeParams };
        let userService = {
            getProgress: () => Promise.resolve(progress),
            getFollows: () => Promise.resolve(followsData),
            getUser: () => Promise.resolve(user),
            addFollow: (): void => void 0,
            removeFollow: (): void => void 0,
        };
        let settingsService = { settings: () => settingsSubject };

        userInfo = new BehaviorSubject(loggedInUser);

        let authenticationService = {
            logInWithPassword: (): void => void 0,
            logInWithAssertion: (): void => void 0,
            userInfo: () => userInfo,
        };

        TestBed.configureTestingModule(
            {
                declarations: [UserComponent],
                providers: [
                    { provide: ActivatedRoute, useValue: route },
                    { provide: UserService, useValue: userService },
                    { provide: SettingsService, useValue: settingsService },
                    { provide: AuthenticationService, useValue: authenticationService },
                ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(UserComponent);
            })
            .then(done)
            .catch(done.fail);
    });

    describe("Actions", () => {
        it("should not display when the current user is not logged in", done => {
            userInfo.next(notLoggedInUser);

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));

                    // Actions container is missing
                    expect(containers.length).toEqual(3);
                })
                .then(done)
                .catch(done.fail);
        });

        it("should not display when the current user is the user being viewed", done => {
            userInfo.next({
                isLoggedIn: true,
                id: "someId",
                username: userName,
                email: "someEmail",
            });

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));

                    // Actions container is missing
                    expect(containers.length).toEqual(3);
                })
                .then(done)
                .catch(done.fail);
        });

        it("should be able to follow the user", done => {
            let userService = TestBed.get(UserService);
            spyOn(userService, "getFollows").and.returnValue(Promise.resolve({ follows: ["someOtherUser"] }));
            spyOn(userService, "addFollow").and.returnValue(Promise.resolve());

            userInfo.next(loggedInUser);

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(4);

                    let actionsContainer = containers[0];

                    let buttons = actionsContainer.queryAll(By.css("button"));
                    expect(buttons.length).toEqual(1);

                    let followButton = buttons[0];
                    expect(followButton).not.toBeNull();
                    expect(followButton.nativeElement.textContent.trim()).toEqual("Follow");
                    followButton.nativeElement.click();

                    expect(userService.addFollow).toHaveBeenCalledWith(loggedInUser.username, userName);

                    return fixture.whenStable();
                })
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(4);

                    let actionsContainer = containers[0];

                    let buttons = actionsContainer.queryAll(By.css("button"));
                    expect(buttons.length).toEqual(1);

                    let followButton = buttons[0];
                    expect(followButton).not.toBeNull();
                    expect(followButton.nativeElement.textContent.trim()).toEqual("Unfollow");
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when following the user fails", done => {
            let userService = TestBed.get(UserService);
            spyOn(userService, "getFollows").and.returnValue(Promise.resolve({ follows: ["someOtherUser"] }));
            spyOn(userService, "addFollow").and.returnValue(Promise.reject("someReason"));

            userInfo.next(loggedInUser);

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(4);

                    let actionsContainer = containers[0];

                    let buttons = actionsContainer.queryAll(By.css("button"));
                    expect(buttons.length).toEqual(1);

                    let followButton = buttons[0];
                    expect(followButton).not.toBeNull();
                    expect(followButton.nativeElement.textContent.trim()).toEqual("Follow");
                    followButton.nativeElement.click();

                    expect(userService.addFollow).toHaveBeenCalledWith(loggedInUser.username, userName);

                    return fixture.whenStable();
                })
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(4);

                    let actionsContainer = containers[0];

                    let buttons = actionsContainer.queryAll(By.css("button"));
                    expect(buttons.length).toEqual(1);

                    let followButton = buttons[0];
                    expect(followButton).not.toBeNull();
                    expect(followButton.nativeElement.textContent.trim()).toEqual("Follow");

                    let error = actionsContainer.query(By.css(".alert-danger"));
                    expect(error).not.toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should be able to unfollow the user", done => {
            let userService = TestBed.get(UserService);
            spyOn(userService, "getFollows").and.returnValue(Promise.resolve({ follows: [userName] }));
            spyOn(userService, "removeFollow").and.returnValue(Promise.resolve());

            userInfo.next(loggedInUser);

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(4);

                    let actionsContainer = containers[0];

                    let buttons = actionsContainer.queryAll(By.css("button"));
                    expect(buttons.length).toEqual(1);

                    let followButton = buttons[0];
                    expect(followButton).not.toBeNull();
                    expect(followButton.nativeElement.textContent.trim()).toEqual("Unfollow");
                    followButton.nativeElement.click();

                    expect(userService.removeFollow).toHaveBeenCalledWith(loggedInUser.username, userName);

                    return fixture.whenStable();
                })
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(4);

                    let actionsContainer = containers[0];

                    let buttons = actionsContainer.queryAll(By.css("button"));
                    expect(buttons.length).toEqual(1);

                    let followButton = buttons[0];
                    expect(followButton).not.toBeNull();
                    expect(followButton.nativeElement.textContent.trim()).toEqual("Follow");
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when unfollowing the user fails", done => {
            let userService = TestBed.get(UserService);
            spyOn(userService, "getFollows").and.returnValue(Promise.resolve({ follows: [userName] }));
            spyOn(userService, "removeFollow").and.returnValue(Promise.reject("someReason"));

            userInfo.next(loggedInUser);

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(4);

                    let actionsContainer = containers[0];

                    let buttons = actionsContainer.queryAll(By.css("button"));
                    expect(buttons.length).toEqual(1);

                    let followButton = buttons[0];
                    expect(followButton).not.toBeNull();
                    expect(followButton.nativeElement.textContent.trim()).toEqual("Unfollow");
                    followButton.nativeElement.click();

                    expect(userService.removeFollow).toHaveBeenCalledWith(loggedInUser.username, userName);

                    return fixture.whenStable();
                })
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(4);

                    let actionsContainer = containers[0];

                    let buttons = actionsContainer.queryAll(By.css("button"));
                    expect(buttons.length).toEqual(1);

                    let followButton = buttons[0];
                    expect(followButton).not.toBeNull();
                    expect(followButton.nativeElement.textContent.trim()).toEqual("Unfollow");

                    let error = actionsContainer.query(By.css(".alert-danger"));
                    expect(error).not.toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when the current users' follows fail", done => {
            let userService = TestBed.get(UserService);
            spyOn(userService, "getFollows").and.returnValue(Promise.reject("someReason"));

            userInfo.next(loggedInUser);

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(4);

                    let actionsContainer = containers[0];

                    let buttons = actionsContainer.queryAll(By.css("button"));
                    expect(buttons.length).toEqual(1);

                    // Show follow button by defailt
                    let followButton = buttons[0];
                    expect(followButton).not.toBeNull();
                    expect(followButton.nativeElement.textContent.trim()).toEqual("Follow");

                    let error = actionsContainer.query(By.css(".alert-danger"));
                    expect(error).not.toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });
    });

    describe("Upload Table", () => {
        it("should display without pagination", () => {
            fixture.detectChanges();

            let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
            expect(containers.length).toEqual(4);

            let uploadsContainer = containers[1];

            let uploadsTable = uploadsContainer.query(By.css("uploadsTable"));
            expect(uploadsTable).not.toBeNull();
            expect(uploadsTable.properties.userName).toEqual(userName);
            expect(uploadsTable.properties.count).toEqual(10);
            expect(uploadsTable.properties.paginate).toBeFalsy();
        });
    });

    describe("Progress Summary", () => {
        it("should display a chart with linear scale", done => {
            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(4);

                    let progressContainer = containers[2];

                    let error = progressContainer.query(By.css(".alert-danger"));
                    expect(error).toBeNull();

                    let warning = progressContainer.query(By.css(".alert-warning"));
                    expect(warning).toBeNull();

                    let chart = progressContainer.query(By.css("canvas"));
                    expect(chart).not.toBeNull();
                    expect(chart.attributes.baseChart).toBeDefined();
                    expect(chart.attributes.height).toEqual("235");
                    expect(chart.properties.chartType).toEqual("line");

                    let datasets: ChartDataSets[] = chart.properties.datasets;
                    expect(datasets).toBeTruthy();
                    expect(datasets.length).toEqual(1);

                    let data = datasets[0].data as ChartPoint[];
                    let dataKeys = Object.keys(progress.soulsSpentData);
                    expect(data.length).toEqual(dataKeys.length);
                    for (let i = 0; i < data.length; i++) {
                        expect(data[i].x).toEqual(new Date(dataKeys[i]).getTime());
                        expect(data[i].y).toEqual(Number(progress.soulsSpentData[dataKeys[i]]));
                    }

                    let colors = chart.properties.colors;
                    expect(colors).toBeTruthy();

                    let options: ChartOptions = chart.properties.options;
                    expect(options).toBeTruthy();
                    expect(options.title.text).toEqual("Souls Spent");
                    expect(options.scales.yAxes[0].type).toEqual("linear");
                })
                .then(done)
                .catch(done.fail);
        });

        it("should display a chart with logarithmic scale", done => {
            // Using values greater than normal numbers can handle
            let soulsSpentData: { [date: string]: string } = {
                "2017-01-01T00:00:00Z": "1e1000",
                "2017-01-02T00:00:00Z": "1e1001",
                "2017-01-03T00:00:00Z": "1e1002",
                "2017-01-04T00:00:00Z": "1e1003",
                "2017-01-05T00:00:00Z": "1e1004",
            };
            let userService = TestBed.get(UserService);
            spyOn(userService, "getProgress").and.returnValue(Promise.resolve({ soulsSpentData }));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(4);

                    let progressContainer = containers[2];

                    let error = progressContainer.query(By.css(".alert-danger"));
                    expect(error).toBeNull();

                    let warning = progressContainer.query(By.css(".alert-warning"));
                    expect(warning).toBeNull();

                    let chart = progressContainer.query(By.css("canvas"));
                    expect(chart).not.toBeNull();
                    expect(chart.attributes.baseChart).toBeDefined();
                    expect(chart.attributes.height).toEqual("235");
                    expect(chart.properties.chartType).toEqual("line");

                    let datasets: ChartDataSets[] = chart.properties.datasets;
                    expect(datasets).toBeTruthy();
                    expect(datasets.length).toEqual(1);

                    let data = datasets[0].data as ChartPoint[];
                    let dataKeys = Object.keys(soulsSpentData);
                    expect(data.length).toEqual(dataKeys.length);
                    for (let i = 0; i < data.length; i++) {
                        expect(data[i].x).toEqual(new Date(dataKeys[i]).getTime());

                        // The value we plot is actually the log of the value to fake log scale
                        expect(data[i].y).toEqual(new Decimal(soulsSpentData[dataKeys[i]]).log().toNumber());
                    }

                    let colors = chart.properties.colors;
                    expect(colors).toBeTruthy();

                    let options: ChartOptions = chart.properties.options;
                    expect(options).toBeTruthy();
                    expect(options.title.text).toEqual("Souls Spent");

                    // Linear since we're manually managing log scale
                    expect(options.scales.yAxes[0].type).toEqual("linear");
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when userService.getProgress fails", done => {
            let userService = TestBed.get(UserService);
            spyOn(userService, "getProgress").and.returnValue(Promise.reject("someReason"));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(4);

                    let progressContainer = containers[2];

                    let chart = progressContainer.query(By.css("canvas"));
                    expect(chart).toBeNull();

                    let error = progressContainer.query(By.css(".alert-danger"));
                    expect(error).not.toBeNull();

                    let warning = progressContainer.query(By.css(".alert-warning"));
                    expect(warning).toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show a warning when there is no data", done => {
            let userService = TestBed.get(UserService);
            spyOn(userService, "getProgress").and.returnValue(Promise.resolve({ soulsSpentData: {} }));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(4);

                    let progressContainer = containers[2];

                    let chart = progressContainer.query(By.css("canvas"));
                    expect(chart).toBeNull();

                    let error = progressContainer.query(By.css(".alert-danger"));
                    expect(error).toBeNull();

                    let warning = progressContainer.query(By.css(".alert-warning"));
                    expect(warning).not.toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });
    });

    describe("Follows", () => {
        it("should display the table", done => {
            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(4);

                    let followsContainer = containers[3];

                    let error = followsContainer.query(By.css(".alert-danger"));
                    expect(error).toBeNull();

                    let noData = followsContainer.query(By.css("p:not(.alert-danger)"));
                    expect(noData).toBeNull();

                    let table = followsContainer.query(By.css("table"));
                    expect(table).not.toBeNull();

                    let rows = table.query(By.css("tbody")).children;
                    expect(rows.length).toEqual(followsData.follows.length);

                    for (let i = 0; i < rows.length; i++) {
                        let expectedFollow = followsData.follows[i];

                        let cells = rows[i].children;
                        expect(cells.length).toEqual(2);

                        let followCell = cells[0];
                        expect(followCell.nativeElement.textContent.trim()).toEqual(expectedFollow);

                        let compareCell = cells[1];
                        let link = compareCell.query(By.css("a"));
                        expect(link.properties.routerLink).toEqual(`/users/${userName}/compare/${expectedFollow}`);
                        expect(link.nativeElement.textContent.trim()).toEqual("Compare");
                    }
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when userService.getFollows fails", done => {
            let userService = TestBed.get(UserService);
            spyOn(userService, "getFollows").and.returnValue(Promise.reject("someReason"));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(4);

                    let followsContainer = containers[3];

                    let table = followsContainer.query(By.css("table"));
                    expect(table).toBeNull();

                    let noData = followsContainer.query(By.css("p:not(.alert-danger)"));
                    expect(noData).toBeNull();

                    let error = followsContainer.query(By.css(".alert-danger"));
                    expect(error).not.toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show a message when there is no data", done => {
            let userService = TestBed.get(UserService);
            spyOn(userService, "getFollows").and.returnValue(Promise.resolve({}));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(4);

                    let followsContainer = containers[3];

                    let table = followsContainer.query(By.css("table"));
                    expect(table).toBeNull();

                    let error = followsContainer.query(By.css(".alert-danger"));
                    expect(error).toBeNull();

                    let noData = followsContainer.query(By.css("p:not(.alert-danger)"));
                    expect(noData).not.toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });
    });
});
