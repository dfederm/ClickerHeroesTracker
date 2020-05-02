import { NO_ERRORS_SCHEMA } from "@angular/core";
import { By } from "@angular/platform-browser";
import { ComponentFixture, TestBed } from "@angular/core/testing";

import { ClansComponent } from "./clans";
import { ClanService, ILeaderboardClan, IClanData, ILeaderboardSummaryListResponse } from "../../services/clanService/clanService";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { BehaviorSubject } from "rxjs";
import { UserService } from "../../services/userService/userService";
import { IUser } from "../../models";

describe("ClansComponent", () => {
    let component: ClansComponent;
    let fixture: ComponentFixture<ClansComponent>;

    let clans: ILeaderboardClan[] =
        [
            { name: "clan0", memberCount: 10, rank: 1, currentRaidLevel: 0, isUserClan: false },
            { name: "clan1", memberCount: 11, rank: 2, currentRaidLevel: 10, isUserClan: false },
            { name: "clan2", memberCount: 12, rank: 3, currentRaidLevel: 20, isUserClan: false },
            { name: "clan3", memberCount: 13, rank: 4, currentRaidLevel: 30, isUserClan: false },
            { name: "clan4", memberCount: 14, rank: 5, currentRaidLevel: 40, isUserClan: true },
            { name: "clan5", memberCount: 15, rank: 6, currentRaidLevel: 50, isUserClan: false },
            { name: "clan6", memberCount: 16, rank: 7, currentRaidLevel: 60, isUserClan: false },
            { name: "clan7", memberCount: 17, rank: 8, currentRaidLevel: 70, isUserClan: false },
            { name: "clan8", memberCount: 18, rank: 9, currentRaidLevel: 80, isUserClan: false },
        ];

    let userClanIndex = clans.findIndex(_ => _.isUserClan);
    let userClan = clans[userClanIndex];

    let clanMembers = [];
    for (let i = 0; i < userClan.memberCount; i++) {
        clanMembers.push({ uid: "userId" + i, nickname: "nickname" + i, highestZone: i, userName: i % 2 ? "userName" + i : null });
    }

    let clanData: IClanData = {
        rank: userClan.rank,
        clanName: userClan.name,
        currentRaidLevel: userClan.currentRaidLevel,
        guildMembers: clanMembers,
        isBlocked: false,
    };

    const userInfo: IUserInfo = {
        isLoggedIn: true,
        id: "someId",
        username: "someUsername",
        email: "someEmail",
    };

    const user: IUser = {
        name: userInfo.username,
        clanName: clanData.clanName,
    };

    beforeEach(done => {
        let clanService = {
            getClan(): Promise<IClanData> {
                return Promise.resolve(clanData);
            },
            getLeaderboard(page: number, count: number): Promise<ILeaderboardSummaryListResponse> {
                let leaderboardSummaryListResponse: ILeaderboardSummaryListResponse = {
                    pagination:
                    {
                        count: clans.length,
                        next: "someNext",
                        previous: "somePrevious",
                    },
                    leaderboardClans: clans.slice((page - 1) * count, page * count),
                };
                return Promise.resolve(leaderboardSummaryListResponse);
            },
        };

        let authenticationService = {
            userInfo: () => new BehaviorSubject(userInfo),
        };

        let userService = {
            getUser: (): Promise<IUser> => Promise.resolve(user),
        };

        TestBed.configureTestingModule(
            {
                declarations: [ClansComponent],
                providers: [
                    { provide: ClanService, useValue: clanService },
                    { provide: AuthenticationService, useValue: authenticationService },
                    { provide: UserService, useValue: userService },
                ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(ClansComponent);
                component = fixture.componentInstance;
            })
            .then(done)
            .catch(done.fail);
    });

    it("should display clan leaderboard when the user's clan is ranked lower", done => {
        component.count = 3;
        verifyLeaderboard(1)
            .then(done)
            .catch(done.fail);
    });

    it("should update the leaderboard when the user's clan is within the current page", done => {
        component.count = 3;
        verifyLeaderboard(2)
            .then(done)
            .catch(done.fail);
    });

    it("should display clan leaderboard when the user's clan is ranked higher", done => {
        component.count = 3;
        verifyLeaderboard(3)
            .then(done)
            .catch(done.fail);
    });

    it("should update the leaderboard when the page changes", done => {
        component.count = 3;
        verifyLeaderboard(1)
            .then(() => verifyLeaderboard(2))
            .then(done)
            .catch(done.fail);
    });

    it("should display error when clanService.getLeaderboard errors", done => {
        let clanService = TestBed.get(ClanService);
        spyOn(clanService, "getLeaderboard").and.returnValue(Promise.reject("someReason"));

        fixture.detectChanges();
        fixture.whenStable()
            .then(() => {
                fixture.detectChanges();

                let error = fixture.debugElement.query(By.css(".alert-danger"));
                expect(error.nativeElement.textContent.trim()).toEqual("There was a problem getting leaderboard data");

                let leaderboard = fixture.debugElement.query(By.css("table"));
                expect(leaderboard).toBeNull();
            })
            .then(done)
            .catch(done.fail);
    });

    function verifyLeaderboard(page: number): Promise<void> {
        component.page = page;

        fixture.detectChanges();
        return fixture.whenStable().then(() => {
            fixture.detectChanges();

            let error = fixture.debugElement.query(By.css(".alert-danger"));
            expect(error).toBeNull();

            let leaderboard = fixture.debugElement.query(By.css("table"));
            expect(leaderboard).not.toBeNull();

            let clanRows = leaderboard.query(By.css("tbody")).children;

            let expectedClanStart = (page - 1) * component.count;
            let expectedClanEnd = page * component.count;
            let expectedClans = clans.slice(expectedClanStart, expectedClanEnd);

            if (expectedClanStart > userClanIndex) {
                expectedClans.unshift(userClan);
            }

            if (expectedClanEnd <= userClanIndex) {
                expectedClans.push(userClan);
            }

            expect(clanRows.length).toEqual(expectedClans.length);
            for (let i = 0; i < clanRows.length; i++) {
                let clanRow = clanRows[i];
                let clan = expectedClans[i];

                expect(clanRow.nativeElement.classList.contains("table-success")).toBe(clan === userClan);

                let cells = clanRow.children;
                expect(cells.length).toEqual(4);
                expect(cells[0].nativeElement.textContent.trim()).toEqual(clan.rank.toString());
                expect(cells[1].nativeElement.textContent.trim()).toEqual(clan.name);
                expect(cells[2].nativeElement.textContent.trim()).toEqual(clan.memberCount.toString());
                expect(cells[3].nativeElement.textContent.trim()).toEqual(clan.currentRaidLevel.toString());
            }

            let pagination = fixture.debugElement.query(By.css("ngb-pagination"));
            expect(pagination).not.toBeNull();
            expect(pagination.properties.collectionSize).toEqual(clans.length);
            expect(pagination.properties.page).toEqual(page);
            expect(pagination.properties.pageSize).toEqual(component.count);
            expect(pagination.properties.maxSize).toEqual(5);
            expect(pagination.properties.rotate).toEqual(true);
            expect(pagination.properties.ellipses).toEqual(false);
            expect(pagination.properties.boundaryLinks).toEqual(true);
        });
    }
});
