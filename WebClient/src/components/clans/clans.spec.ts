import { NO_ERRORS_SCHEMA, ChangeDetectorRef, DebugElement } from "@angular/core";
import { By } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { TimeAgoPipe } from "time-ago-pipe";

import { ClansComponent } from "./clans";
import { ClanService, ILeaderboardClan, IClanData, ILeaderboardSummaryListResponse } from "../../services/clanService/clanService";

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

    let clanData: IClanData = {
        clanName: userClan.name,
        currentRaidLevel: userClan.currentRaidLevel,
        guildMembers:
        [
            { uid: "userId0", nickname: "user0", highestZone: 0 },
            { uid: "userId1", nickname: "user1", highestZone: 1 },
            { uid: "userId2", nickname: "user2", highestZone: 2 },
        ],
        messages:
        [
            { username: "user0", date: "2017-01-01T00:00:00", content: "message0" },
            { username: null, date: "2017-01-02T00:00:00", content: "message1" },
            { username: "user2", date: "2017-01-03T00:00:00", content: "message2" },
        ],
    };

    beforeEach(done => {
        let clanService = {
            getClan(): Promise<IClanData> {
                return Promise.resolve(clanData);
            },
            getUserClan(): Promise<ILeaderboardClan> {
                return Promise.resolve(userClan);
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
            sendMessage(): Promise<void> {
                return Promise.resolve();
            },
        };

        TestBed.configureTestingModule(
            {
                imports: [FormsModule],
                declarations:
                [
                    ClansComponent,
                    TimeAgoPipe,
                ],
                providers:
                [
                    { provide: ClanService, useValue: clanService },
                    TimeAgoPipe,
                    ChangeDetectorRef, // Needed for TimeAgoPipe
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

    it("should display clan information", done => {
        let timeAgoPipe = TestBed.get(TimeAgoPipe) as TimeAgoPipe;

        fixture.detectChanges();
        fixture.whenStable()
            .then(() => {
                fixture.detectChanges();

                let containers = fixture.debugElement.queryAll(By.css(".col-lg-6"));
                expect(containers.length).toEqual(2);

                let clanInformation = containers[0];

                let clanTitle = clanInformation.query(By.css("h3"));
                expect(clanTitle.nativeElement.textContent.trim()).toEqual(userClan.name);

                let userRows = clanInformation.query(By.css("table tbody")).children;
                expect(userRows.length).toEqual(clanData.guildMembers.length);
                for (let i = 0; i < userRows.length; i++) {
                    let userRow = userRows[i];
                    let guildMember = clanData.guildMembers[i];

                    let cells = userRow.children;
                    expect(cells.length).toEqual(2);
                    expect(cells[0].nativeElement.textContent.trim()).toEqual(guildMember.nickname);
                    expect(cells[1].nativeElement.textContent.trim()).toEqual(guildMember.highestZone.toString());
                }

                let messages = clanInformation.queryAll(By.css(".clan-message"));
                expect(messages.length).toEqual(clanData.messages.length);
                for (let i = 0; i < messages.length; i++) {
                    let messageElement = messages[i];
                    let message = clanData.messages[i];

                    let messageElementsPieces = messageElement.children;
                    expect(messageElementsPieces.length).toEqual(2);

                    let metadataElement = messageElementsPieces[0].children;
                    expect(metadataElement[0].nativeElement.textContent.trim()).toEqual("(" + timeAgoPipe.transform(message.date) + ")");
                    if (message.username) {
                        expect(metadataElement[1].nativeElement.classList.contains("text-muted")).toBe(false);
                        expect(metadataElement[1].nativeElement.textContent.trim()).toEqual(message.username);
                    } else {
                        expect(metadataElement[1].nativeElement.classList.contains("text-muted")).toBe(true);
                        expect(metadataElement[1].nativeElement.textContent.trim()).toEqual("(Unknown)");
                    }

                    expect(messageElementsPieces[1].nativeElement.textContent.trim()).toEqual(message.content);
                }
            })
            .then(done)
            .catch(done.fail);
    });

    it("should display an error when clanService.getClan errors", done => {
        let clanService = TestBed.get(ClanService);
        spyOn(clanService, "getClan").and.returnValue(Promise.reject("someReason"));

        fixture.detectChanges();
        fixture.whenStable()
            .then(() => {
                fixture.detectChanges();

                let error = fixture.debugElement.query(By.css("p"));
                expect(error.nativeElement.textContent.trim()).toEqual("There was a problem getting your clan's data");
            })
            .then(done)
            .catch(done.fail);
    });

    it("should display an error when the user is not in a clan", done => {
        let clanService = TestBed.get(ClanService);
        spyOn(clanService, "getClan").and.returnValue(Promise.resolve(null));

        fixture.detectChanges();
        fixture.whenStable()
            .then(() => {
                fixture.detectChanges();

                let error = fixture.debugElement.query(By.css("p"));
                expect(error.nativeElement.textContent.trim()).toEqual("Please join a clan to view the clan's data");
            })
            .then(done)
            .catch(done.fail);
    });

    it("should send messages", done => {
        let clanService = TestBed.get(ClanService) as ClanService;

        let form: DebugElement;
        const message = "Test Message";

        // The first stabilization is for the initial population of clan data
        fixture.detectChanges();
        fixture.whenStable()
            .then(() => {
                // We need a second stabilization round for binding ngModel since it's async and we added it to the DOM *after* the initial stabilization.
                fixture.detectChanges();
                return fixture.whenStable();
            })
            .then(() => {
                let containers = fixture.debugElement.queryAll(By.css(".col-lg-6"));
                expect(containers.length).toEqual(2);

                let clanInformation = containers[0];
                form = clanInformation.query(By.css("form"));
                expect(form).not.toBeNull();

                let input = form.query(By.css("input"));
                expect(input).not.toBeNull();
                setInputValue(input, message);

                let button = form.query(By.css("button"));
                expect(button).not.toBeNull();

                spyOn(clanService, "sendMessage").and.callThrough();
                button.nativeElement.click();
                expect(clanService.sendMessage).toHaveBeenCalledWith(message, userClan.name);

                // Ensure we refresh the UI
                spyOn(clanService, "getClan");
                return fixture.whenStable();
            })
            .then(() => {
                expect(clanService.getClan).toHaveBeenCalled();
            })
            .then(done)
            .catch(done.fail);
    });

    it("should show an error message when sending a message fails", done => {
        let clanService = TestBed.get(ClanService) as ClanService;

        let form: DebugElement;
        const message = "Test Message";

        // The first stabilization is for the initial population of clan data
        fixture.detectChanges();
        fixture.whenStable()
            .then(() => {
                // We need a second stabilization round for binding ngModel since it's async and we added it to the DOM *after* the initial stabilization.
                fixture.detectChanges();
                return fixture.whenStable();
            })
            .then(() => {
                let containers = fixture.debugElement.queryAll(By.css(".col-lg-6"));
                expect(containers.length).toEqual(2);

                let clanInformation = containers[0];
                form = clanInformation.query(By.css("form"));
                expect(form).not.toBeNull();

                let input = form.query(By.css("input"));
                expect(input).not.toBeNull();
                setInputValue(input, message);

                let button = form.query(By.css("button"));
                expect(button).not.toBeNull();

                spyOn(clanService, "sendMessage").and.returnValue(Promise.reject("someReason"));
                button.nativeElement.click();
                expect(clanService.sendMessage).toHaveBeenCalledWith(message, userClan.name);

                return fixture.whenStable();
            })
            .then(() => {
                fixture.detectChanges();
                let error = fixture.debugElement.query(By.css("p"));
                expect(error.nativeElement.textContent.trim()).toEqual("There was a problem sending your message. Please try again.");
            })
            .then(done)
            .catch(done.fail);
    });

    it("should display clan leaderboard when the user's clan is ranked lower", done => {
        component.leaderboardCount = 3;
        verifyLeaderboard(1)
            .then(done)
            .catch(done.fail);
    });

    it("should update the leaderboard when the user's clan is within the current page", done => {
        component.leaderboardCount = 3;
        verifyLeaderboard(2)
            .then(done)
            .catch(done.fail);
    });

    it("should display clan leaderboard when the user's clan is ranked higher", done => {
        component.leaderboardCount = 3;
        verifyLeaderboard(3)
            .then(done)
            .catch(done.fail);
    });

    it("should update the leaderboard when the page changes", done => {
        component.leaderboardCount = 3;
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

                let containers = fixture.debugElement.queryAll(By.css(".col-lg-6"));
                expect(containers.length).toEqual(2);

                let leaderboard = containers[1];

                let error = leaderboard.query(By.css("p"));
                expect(error.nativeElement.textContent.trim()).toEqual("There was a problem getting leaderboard data");
            })
            .then(done)
            .catch(done.fail);
    });

    function setInputValue(element: DebugElement, value: string): void {
        element.nativeElement.value = value;

        // Tell Angular
        let evt = document.createEvent("CustomEvent");
        evt.initCustomEvent("input", false, false, null);
        element.nativeElement.dispatchEvent(evt);
    }

    function verifyLeaderboard(page: number): Promise<void> {
        component.leaderboardPage = page;

        fixture.detectChanges();
        return fixture.whenStable().then(() => {
            fixture.detectChanges();

            let containers = fixture.debugElement.queryAll(By.css(".col-lg-6"));
            expect(containers.length).toEqual(2);

            let leaderboard = containers[1];

            let clanRows = leaderboard.query(By.css("table tbody")).children;

            let expectedClanStart = (page - 1) * component.leaderboardCount;
            let expectedClanEnd = page * component.leaderboardCount;
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
            expect(pagination.properties.pageSize).toEqual(component.leaderboardCount);
            expect(pagination.properties.maxSize).toEqual(5);
            expect(pagination.properties.rotate).toEqual(true);
            expect(pagination.properties.ellipses).toEqual(false);
            expect(pagination.properties.boundaryLinks).toEqual(true);
        });
    }
});
