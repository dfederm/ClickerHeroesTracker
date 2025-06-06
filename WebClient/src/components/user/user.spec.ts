import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import Decimal from "decimal.js";
import { ChartDataset, ChartOptions, ScatterDataPoint } from "chart.js";
import { BehaviorSubject } from "rxjs";

import { UserComponent } from "./user";
import { UserService, IProgressData, IFollowsData } from "../../services/userService/userService";
import { SettingsService } from "../../services/settingsService/settingsService";
import { ActivatedRoute, RouterLink } from "@angular/router";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { IUser } from "../../models";
import { NgxSpinnerModule, NgxSpinnerService } from "ngx-spinner";
import { Component, Directive, Input } from "@angular/core";
import { UploadsTableComponent } from "../uploadsTable/uploadsTable";
import { BaseChartDirective } from "ng2-charts";

describe("UserComponent", () => {
    let fixture: ComponentFixture<UserComponent>;

    @Component({ selector: "ngx-spinner", template: "" })
    class MockNgxSpinnerComponent {
        @Input()
        public fullScreen: boolean;
    }

    @Component({ selector: "uploadsTable", template: "" })
    class MockUploadsTableComponent {
        @Input()
        public userName: string;

        @Input()
        public count: number;
    }

    @Directive({
        selector: "canvas[baseChart]",
    })
    class MockBaseChartDirective {
        @Input()
        public type: string;

        @Input()
        public datasets: ChartDataset<"line">[];

        @Input()
        public options: ChartOptions<"line">;
    }

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

    beforeEach(async () => {
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

        let spinnerService = {
            show: (): void => void 0,
            hide: (): void => void 0,
        };

        await TestBed.configureTestingModule(
            {
                imports: [UserComponent],
                providers: [
                    { provide: ActivatedRoute, useValue: route },
                    { provide: UserService, useValue: userService },
                    { provide: SettingsService, useValue: settingsService },
                    { provide: AuthenticationService, useValue: authenticationService },
                    { provide: NgxSpinnerService, useValue: spinnerService },
                ],
            })
            .compileComponents();
        TestBed.overrideComponent(UserComponent, {
            remove: { imports: [ NgxSpinnerModule, UploadsTableComponent, BaseChartDirective ]},
            add: { imports: [ MockNgxSpinnerComponent, MockUploadsTableComponent, MockBaseChartDirective ] },
        });

        fixture = TestBed.createComponent(UserComponent);
    });

    describe("Actions", () => {
        it("should not display when the current user is not logged in", async () => {
            userInfo.next(notLoggedInUser);

            fixture.detectChanges();
            await fixture.whenStable();
            fixture.detectChanges();

            let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));

            // Actions container is missing
            expect(containers.length).toEqual(3);
        });

        it("should not display when the current user is the user being viewed", async () => {
            userInfo.next({
                isLoggedIn: true,
                id: "someId",
                username: userName,
                email: "someEmail",
            });

            fixture.detectChanges();
            await fixture.whenStable();
            fixture.detectChanges();

            let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));

            // Actions container is missing
            expect(containers.length).toEqual(3);
        });

        it("should be able to follow the user", async () => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getFollows").and.returnValue(Promise.resolve({ follows: ["someOtherUser"] }));
            spyOn(userService, "addFollow").and.returnValue(Promise.resolve());

            userInfo.next(loggedInUser);

            fixture.detectChanges();
            await fixture.whenStable();
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

            await fixture.whenStable();
            fixture.detectChanges();

            buttons = actionsContainer.queryAll(By.css("button"));
            expect(buttons.length).toEqual(1);

            let unfollowButton = buttons[0];
            expect(unfollowButton).not.toBeNull();
            expect(unfollowButton.nativeElement.textContent.trim()).toEqual("Unfollow");
        });

        it("should show an error when following the user fails", async () => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getFollows").and.returnValue(Promise.resolve({ follows: ["someOtherUser"] }));
            spyOn(userService, "addFollow").and.returnValue(Promise.reject("someReason"));

            userInfo.next(loggedInUser);

            fixture.detectChanges();
            await fixture.whenStable();
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

            await fixture.whenStable();
            fixture.detectChanges();

            buttons = actionsContainer.queryAll(By.css("button"));
            expect(buttons.length).toEqual(1);

            followButton = buttons[0];
            expect(followButton).not.toBeNull();
            expect(followButton.nativeElement.textContent.trim()).toEqual("Follow");

            let error = actionsContainer.query(By.css(".alert-danger"));
            expect(error).not.toBeNull();
        });

        it("should be able to unfollow the user", async () => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getFollows").and.returnValue(Promise.resolve({ follows: [userName] }));
            spyOn(userService, "removeFollow").and.returnValue(Promise.resolve());

            userInfo.next(loggedInUser);

            fixture.detectChanges();
            await fixture.whenStable();
            fixture.detectChanges();

            let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
            expect(containers.length).toEqual(4);

            let actionsContainer = containers[0];

            let buttons = actionsContainer.queryAll(By.css("button"));
            expect(buttons.length).toEqual(1);

            let unfollowButton = buttons[0];
            expect(unfollowButton).not.toBeNull();
            expect(unfollowButton.nativeElement.textContent.trim()).toEqual("Unfollow");
            unfollowButton.nativeElement.click();

            expect(userService.removeFollow).toHaveBeenCalledWith(loggedInUser.username, userName);

            await fixture.whenStable();
            fixture.detectChanges();

            buttons = actionsContainer.queryAll(By.css("button"));
            expect(buttons.length).toEqual(1);

            let followButton = buttons[0];
            expect(followButton).not.toBeNull();
            expect(followButton.nativeElement.textContent.trim()).toEqual("Follow");
        });

        it("should show an error when unfollowing the user fails", async () => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getFollows").and.returnValue(Promise.resolve({ follows: [userName] }));
            spyOn(userService, "removeFollow").and.returnValue(Promise.reject("someReason"));

            userInfo.next(loggedInUser);

            fixture.detectChanges();
            await fixture.whenStable();
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

            await fixture.whenStable();
            fixture.detectChanges();

            buttons = actionsContainer.queryAll(By.css("button"));
            expect(buttons.length).toEqual(1);

            let unfollowButton = buttons[0];
            expect(unfollowButton).not.toBeNull();
            expect(unfollowButton.nativeElement.textContent.trim()).toEqual("Unfollow");

            let error = actionsContainer.query(By.css(".alert-danger"));
            expect(error).not.toBeNull();
        });

        it("should show an error when the current users' follows fail", async () => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getFollows").and.returnValue(Promise.reject("someReason"));

            userInfo.next(loggedInUser);

            fixture.detectChanges();
            await fixture.whenStable();
            fixture.detectChanges();

            let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
            expect(containers.length).toEqual(4);

            let actionsContainer = containers[0];

            let buttons = actionsContainer.queryAll(By.css("button"));
            expect(buttons.length).toEqual(1);

            // Show follow button by default
            let followButton = buttons[0];
            expect(followButton).not.toBeNull();
            expect(followButton.nativeElement.textContent.trim()).toEqual("Follow");

            let error = actionsContainer.query(By.css(".alert-danger"));
            expect(error).not.toBeNull();
        });
    });

    describe("Upload Table", () => {
        it("should display without pagination", () => {
            fixture.detectChanges();

            let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
            expect(containers.length).toEqual(4);

            let uploadsContainer = containers[1];

            let uploadsTable = uploadsContainer.query(By.css("uploadsTable"))?.componentInstance as UploadsTableComponent;
            expect(uploadsTable).not.toBeNull();
            expect(uploadsTable.userName).toEqual(userName);
            expect(uploadsTable.count).toEqual(10);
            expect(uploadsTable.paginate).toBeFalsy();
        });
    });

    describe("Progress Summary", () => {
        it("should display a chart with linear scale", async () => {
            fixture.detectChanges();
            await fixture.whenStable();
            fixture.detectChanges();

            let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
            expect(containers.length).toEqual(4);

            let progressContainer = containers[2];

            let error = progressContainer.query(By.css(".alert-danger"));
            expect(error).toBeNull();

            let warning = progressContainer.query(By.css(".alert-warning"));
            expect(warning).toBeNull();

            let chart = progressContainer.query(By.directive(MockBaseChartDirective));
            expect(chart).not.toBeNull();
            expect(chart.properties.height).toEqual(235);

            let chartDirective = chart.injector.get(MockBaseChartDirective) as MockBaseChartDirective;
            expect(chartDirective).not.toBeNull();
            expect(chartDirective.type).toEqual("line");

            expect(chartDirective.datasets).toBeTruthy();
            expect(chartDirective.datasets.length).toEqual(1);
            let data = chartDirective.datasets[0].data as ScatterDataPoint[];
            let dataKeys = Object.keys(progress.soulsSpentData);
            expect(data.length).toEqual(dataKeys.length);
            for (let i = 0; i < data.length; i++) {
                expect(data[i].x).toEqual(new Date(dataKeys[i]).getTime());
                expect(data[i].y).toEqual(Number(progress.soulsSpentData[dataKeys[i]]));
            }

            expect(chartDirective.options).toBeTruthy();
            expect(chartDirective.options.plugins.title.text).toEqual("Souls Spent");
            expect(chartDirective.options.scales.yAxis.type).toEqual("linear");
        });

        it("should display a chart with logarithmic scale", async () => {
            // Using values greater than normal numbers can handle
            let soulsSpentData: { [date: string]: string } = {
                "2017-01-01T00:00:00Z": "1e1000",
                "2017-01-02T00:00:00Z": "1e1001",
                "2017-01-03T00:00:00Z": "1e1002",
                "2017-01-04T00:00:00Z": "1e1003",
                "2017-01-05T00:00:00Z": "1e1004",
            };
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getProgress").and.returnValue(Promise.resolve({ soulsSpentData } as any));

            fixture.detectChanges();
            await fixture.whenStable();
            fixture.detectChanges();

            let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
            expect(containers.length).toEqual(4);

            let progressContainer = containers[2];

            let error = progressContainer.query(By.css(".alert-danger"));
            expect(error).toBeNull();

            let warning = progressContainer.query(By.css(".alert-warning"));
            expect(warning).toBeNull();

            let chart = progressContainer.query(By.directive(MockBaseChartDirective));
            expect(chart).not.toBeNull();
            expect(chart.properties.height).toEqual(235);

            let chartDirective = chart.injector.get(MockBaseChartDirective) as MockBaseChartDirective;
            expect(chartDirective).not.toBeNull();
            expect(chartDirective.type).toEqual("line");

            expect(chartDirective.datasets).toBeTruthy();
            expect(chartDirective.datasets.length).toEqual(1);
            let data = chartDirective.datasets[0].data as ScatterDataPoint[];
            let dataKeys = Object.keys(soulsSpentData);
            expect(data.length).toEqual(dataKeys.length);
            for (let i = 0; i < data.length; i++) {
                expect(data[i].x).toEqual(new Date(dataKeys[i]).getTime());

                // The value we plot is actually the log of the value to fake log scale
                expect(data[i].y).toEqual(new Decimal(soulsSpentData[dataKeys[i]]).log().toNumber());
            }

            expect(chartDirective.options).toBeTruthy();
            expect(chartDirective.options.plugins.title.text).toEqual("Souls Spent");

            // Linear since we're manually managing log scale
            expect(chartDirective.options.scales.yAxis.type).toEqual("linear");
        });

        it("should show an error when userService.getProgress fails", async () => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getProgress").and.returnValue(Promise.reject("someReason"));

            fixture.detectChanges();
            await fixture.whenStable();
            fixture.detectChanges();

            let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
            expect(containers.length).toEqual(4);

            let progressContainer = containers[2];

            let chart = progressContainer.query(By.directive(MockBaseChartDirective));
            expect(chart).toBeNull();

            let error = progressContainer.query(By.css(".alert-danger"));
            expect(error).not.toBeNull();

            let warning = progressContainer.query(By.css(".alert-warning"));
            expect(warning).toBeNull();
        });

        it("should show a warning when there is no data", async () => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getProgress").and.returnValue(Promise.resolve({ soulsSpentData: {} } as any));

            fixture.detectChanges();
            await fixture.whenStable();
            fixture.detectChanges();

            let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
            expect(containers.length).toEqual(4);

            let progressContainer = containers[2];

            let chart = progressContainer.query(By.directive(MockBaseChartDirective));
            expect(chart).toBeNull();

            let error = progressContainer.query(By.css(".alert-danger"));
            expect(error).toBeNull();

            let warning = progressContainer.query(By.css(".alert-warning"));
            expect(warning).not.toBeNull();
        });
    });

    describe("Follows", () => {
        it("should display the table", async () => {
            fixture.detectChanges();
            await fixture.whenStable();
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
                let routerLink = link.injector.get(RouterLink) as RouterLink;
                expect(routerLink.href).toEqual(`/users/${userName}/compare/${expectedFollow}`);
                expect(link.nativeElement.textContent.trim()).toEqual("Compare");
            }
        });

        it("should show an error when userService.getFollows fails", async () => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getFollows").and.returnValue(Promise.reject("someReason"));

            fixture.detectChanges();
            await fixture.whenStable();
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
        });

        it("should show a message when there is no data", async () => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getFollows").and.returnValue(Promise.resolve({} as any));

            fixture.detectChanges();
            await fixture.whenStable();
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
        });
    });
});
