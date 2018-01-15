import { NO_ERRORS_SCHEMA, ChangeDetectorRef, DebugElement } from "@angular/core";
import { By } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { TimeAgoPipe } from "time-ago-pipe";
import { ClanComponent } from "./clan";
import { ClanService, IClanData, IMessage } from "../../services/clanService/clanService";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { BehaviorSubject } from "rxjs";
import { UserService } from "../../services/userService/userService";
import { IUser } from "../../models";
import { Params, ActivatedRoute } from "@angular/router";

describe("ClanComponent", () => {
    let fixture: ComponentFixture<ClanComponent>;
    let routeParams: BehaviorSubject<Params>;

    let clanMembers = [];
    for (let i = 0; i < 5; i++) {
        clanMembers.push({ uid: "userId" + i, nickname: "nickname" + i, highestZone: i, userName: i % 2 ? "userName" + i : null });
    }

    let clan: IClanData = {
        rank: 123,
        clanName: "someClanName",
        currentRaidLevel: 456,
        guildMembers: clanMembers,
    };

    let clanMessages = [
        { username: "user0", date: "2017-01-01T00:00:00", content: "message0" },
        { username: null, date: "2017-01-02T00:00:00", content: "message1" },
        { username: "user2", date: "2017-01-03T00:00:00", content: "message2" },
    ];

    const userInfo: IUserInfo = {
        isLoggedIn: true,
        id: "someId",
        username: "someUsername",
        email: "someEmail",
    };

    const user: IUser = {
        name: userInfo.username,
        clanName: clan.clanName,
    };

    beforeEach(done => {
        let clanService = {
            getClan(): Promise<IClanData> {
                return Promise.resolve(clan);
            },
            getMessages(): Promise<IMessage[]> {
                return Promise.resolve(clanMessages);
            },
            sendMessage(): Promise<void> {
                return Promise.resolve();
            },
        };

        let authenticationService = {
            userInfo: () => new BehaviorSubject(userInfo),
        };

        let userService = {
            getUser: (): Promise<IUser> => Promise.resolve(user),
        };

        routeParams = new BehaviorSubject({ clanName: clan.clanName });
        let route = { params: routeParams };

        TestBed.configureTestingModule(
            {
                imports: [FormsModule],
                declarations:
                    [
                        ClanComponent,
                        TimeAgoPipe,
                    ],
                providers:
                    [
                        { provide: ClanService, useValue: clanService },
                        { provide: AuthenticationService, useValue: authenticationService },
                        { provide: UserService, useValue: userService },
                        { provide: ActivatedRoute, useValue: route },
                        TimeAgoPipe,
                        ChangeDetectorRef, // Needed for TimeAgoPipe
                    ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(ClanComponent);
            })
            .then(done)
            .catch(done.fail);
    });

    it("should display clan information", done => {
        fixture.detectChanges();
        fixture.whenStable()
            .then(() => {
                fixture.detectChanges();
                expectNoErrors();

                let clanTitle = fixture.debugElement.query(By.css("h2"));
                expect(clanTitle.nativeElement.textContent.trim()).toEqual(clan.clanName);

                let container = fixture.debugElement.query(By.css(".col-md-6"));
                expect(container).not.toBeNull();

                let clanInformation = container.queryAll(By.css("li span"));
                expect(clanInformation.length).toEqual(2);
                expect(clanInformation[0].nativeElement.textContent.trim()).toEqual(clan.rank.toString());
                expect(clanInformation[1].nativeElement.textContent.trim()).toEqual(clan.currentRaidLevel.toString());
            })
            .then(done)
            .catch(done.fail);
    });

    it("should display clan members", done => {
        fixture.detectChanges();
        fixture.whenStable()
            .then(() => {
                fixture.detectChanges();
                expectNoErrors();

                let containers = fixture.debugElement.queryAll(By.css(".col-lg-6"));
                expect(containers.length).toEqual(2);

                let userRows = containers[0].query(By.css("table tbody")).children;
                expect(userRows.length).toEqual(clan.guildMembers.length);
                for (let i = 0; i < userRows.length; i++) {
                    let userRow = userRows[i];
                    let guildMember = clan.guildMembers[i];

                    let expectedName = guildMember.nickname;
                    if (guildMember.userName) {
                        expectedName += " ( " + guildMember.userName + " )";
                    }

                    let cells = userRow.children;
                    expect(cells.length).toEqual(2);
                    expect(cells[0].nativeElement.textContent.trim().replace(/\s+/g, " ")).toEqual(expectedName);
                    expect(cells[1].nativeElement.textContent.trim()).toEqual(guildMember.highestZone.toString());
                }
            })
            .then(done)
            .catch(done.fail);
    });

    it("should display an error when fetching the clan fails", done => {
        let clanService = TestBed.get(ClanService);
        spyOn(clanService, "getClan").and.returnValue(Promise.reject("someReason"));

        fixture.detectChanges();
        fixture.whenStable()
            .then(() => {
                fixture.detectChanges();
                expectError("There was a problem getting the clan's data");
            })
            .then(done)
            .catch(done.fail);
    });

    it("should display an error when the clan does not exist", done => {
        let clanService = TestBed.get(ClanService);
        spyOn(clanService, "getClan").and.returnValue(Promise.reject({ status: 404 }));

        fixture.detectChanges();
        fixture.whenStable()
            .then(() => {
                fixture.detectChanges();
                expectError("This clan does not exist");
            })
            .then(done)
            .catch(done.fail);
    });

    it("should not display messages if it's not the user's clan", done => {
        let userService = TestBed.get(UserService);
        spyOn(userService, "getUser").and.returnValue(Promise.resolve({ clanName: "someOtherClan" }));

        fixture.detectChanges();
        fixture.whenStable()
            .then(() => {
                fixture.detectChanges();
                expectNoErrors();

                let containers = fixture.debugElement.queryAll(By.css(".col-lg-6"));
                expect(containers.length).toEqual(1);
            })
            .then(done)
            .catch(done.fail);
    });

    it("should display messages if it's the user's clan", done => {
        let timeAgoPipe = TestBed.get(TimeAgoPipe) as TimeAgoPipe;

        fixture.detectChanges();
        fixture.whenStable()
            .then(() => {
                fixture.detectChanges();
                expectNoErrors();

                let containers = fixture.debugElement.queryAll(By.css(".col-lg-6"));
                expect(containers.length).toEqual(2);

                let messages = containers[1].queryAll(By.css(".clan-message"));
                expect(messages.length).toEqual(clanMessages.length);
                for (let i = 0; i < messages.length; i++) {
                    let messageElement = messages[i];
                    let expectedMessage = clanMessages[i];

                    let messageElementsPieces = messageElement.children;
                    expect(messageElementsPieces.length).toEqual(2);

                    let metadataElement = messageElementsPieces[0].children;
                    expect(metadataElement[0].nativeElement.textContent.trim()).toEqual("(" + timeAgoPipe.transform(expectedMessage.date) + ")");
                    if (expectedMessage.username) {
                        expect(metadataElement[1].nativeElement.classList.contains("text-muted")).toBe(false);
                        expect(metadataElement[1].nativeElement.textContent.trim()).toEqual(expectedMessage.username);
                    } else {
                        expect(metadataElement[1].nativeElement.classList.contains("text-muted")).toBe(true);
                        expect(metadataElement[1].nativeElement.textContent.trim()).toEqual("(Unknown)");
                    }

                    expect(messageElementsPieces[1].nativeElement.textContent.trim()).toEqual(expectedMessage.content);
                }
            })
            .then(done)
            .catch(done.fail);
    });

    it("should display an error when fetcing messages fails", done => {
        let clanService = TestBed.get(ClanService);
        spyOn(clanService, "getMessages").and.returnValue(Promise.reject("someReason"));

        fixture.detectChanges();
        fixture.whenStable()
            .then(() => {
                fixture.detectChanges();
                expectError("There was a problem getting your clan's messages. Please try again.");
            })
            .then(done)
            .catch(done.fail);
    });

    it("should send messages", done => {
        let clanService = TestBed.get(ClanService) as ClanService;

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
                expectNoErrors();

                let containers = fixture.debugElement.queryAll(By.css(".col-lg-6"));
                expect(containers.length).toEqual(2);

                let form = containers[1].query(By.css("form"));
                expect(form).not.toBeNull();

                let input = form.query(By.css("input"));
                expect(input).not.toBeNull();
                setInputValue(input, message);

                let button = form.query(By.css("button"));
                expect(button).not.toBeNull();

                spyOn(clanService, "sendMessage").and.callThrough();
                button.nativeElement.click();
                expect(clanService.sendMessage).toHaveBeenCalledWith(message, clan.clanName);

                // Ensure we refresh the UI
                spyOn(clanService, "getMessages");
                return fixture.whenStable();
            })
            .then(() => {
                expect(clanService.getMessages).toHaveBeenCalled();
            })
            .then(done)
            .catch(done.fail);
    });

    it("should show an error message when sending a message fails", done => {
        let clanService = TestBed.get(ClanService) as ClanService;

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
                expectNoErrors();

                let containers = fixture.debugElement.queryAll(By.css(".col-lg-6"));
                expect(containers.length).toEqual(2);

                let form = containers[1].query(By.css("form"));
                expect(form).not.toBeNull();

                let input = form.query(By.css("input"));
                expect(input).not.toBeNull();
                setInputValue(input, message);

                let button = form.query(By.css("button"));
                expect(button).not.toBeNull();

                spyOn(clanService, "sendMessage").and.returnValue(Promise.reject("someReason"));
                button.nativeElement.click();
                expect(clanService.sendMessage).toHaveBeenCalledWith(message, clan.clanName);

                return fixture.whenStable();
            })
            .then(() => {
                fixture.detectChanges();
                expectError("There was a problem sending your message. Please try again.");
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

    function expectNoErrors(): void {
        let error = fixture.debugElement.query(By.css(".alert-danger"));
        expect(error).toBeNull();
    }

    function expectError(expectedError: string): void {
        let error = fixture.debugElement.queryAll(By.css(".alert-danger"));
        expect(error.length).toEqual(1);
        expect(error[0].nativeElement.textContent.trim()).toEqual(expectedError);
    }
});
