import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { DatePipe } from "@angular/common";
import { Component, DebugElement, Input } from "@angular/core";
import { BehaviorSubject } from "rxjs";

import { ChangelogComponent } from "./changelog";
import { NewsService, ISiteNewsEntryListResponse, ISiteNewsEntry } from "../../services/newsService/newsService";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { NgxSpinnerModule } from "ngx-spinner";

describe("ChangelogComponent", () => {
    let component: ChangelogComponent;
    let fixture: ComponentFixture<ChangelogComponent>;

    @Component({ selector: "ngx-spinner", template: "", standalone: true })
    class MockNgxSpinnerComponent {
        @Input()
        public fullScreen: boolean;
    }

    let userInfo: BehaviorSubject<IUserInfo>;

    const siteNewsEntryListResponse: ISiteNewsEntryListResponse = {
        entries:
            {
                "2017-01-01": ["1.0", "1.1"],
                "2017-01-02": ["2.0", "2.1"],
                "2017-01-03": ["3.0", "3.1"],
            },
    };

    const adminUser: IUserInfo = {
        isLoggedIn: true,
        id: "someId",
        username: "someUsername",
        email: "someEmail",
        isAdmin: true,
    };

    const notLoggedInUser: IUserInfo = {
        isLoggedIn: false,
    };

    beforeEach(async () => {
        let newsService = {
            getNews: (): void => void 0,
            addNews: (): void => void 0,
            deleteNews: (): void => void 0,
        };

        userInfo = new BehaviorSubject(notLoggedInUser);
        let authenticationService = { userInfo: () => userInfo };

        await TestBed.configureTestingModule(
            {
                imports: [
                    ChangelogComponent,
                ],
                providers: [
                    { provide: NewsService, useValue: newsService },
                    DatePipe,
                    { provide: AuthenticationService, useValue: authenticationService },
                ],
            })
            .compileComponents();
        TestBed.overrideComponent(ChangelogComponent, {
            remove: { imports: [ NgxSpinnerModule ]},
            add: { imports: [ MockNgxSpinnerComponent ] },
        });

        fixture = TestBed.createComponent(ChangelogComponent);
        component = fixture.componentInstance;
    });

    it("should display all news entries grouped by date", async () => {
        let newsService = TestBed.inject(NewsService);
        spyOn(newsService, "getNews").and.returnValue(Promise.resolve(siteNewsEntryListResponse));

        let datePipe = TestBed.inject(DatePipe);

        component.showDates = true;
        fixture.detectChanges();

        await fixture.whenStable();
        fixture.detectChanges();

        expect(newsService.getNews).toHaveBeenCalled();

        let sections = fixture.debugElement.queryAll(By.css("div"));
        expect(sections.length).toEqual(Object.keys(siteNewsEntryListResponse.entries).length);

        for (let i = 0; i < sections.length; i++) {
            // The sections are rendered in reverse order
            let expectedDate = sections.length - i;

            let dateElement = sections[i].query(By.css("h3"));
            expect(dateElement.nativeElement.textContent.trim()).toEqual(datePipe.transform(`1/${expectedDate}/2017`, "shortDate"));

            let list = sections[i].query(By.css("ul"));
            let listItems = list.queryAll(By.css("li"));
            expect(listItems.length).toEqual(2);
            for (let j = 0; j < listItems.length; j++) {
                let listItem: HTMLElement = listItems[j].nativeElement;
                expect(listItem.textContent.trim()).toEqual(`${expectedDate}.${j}`);
            }
        }
    });

    it("should display limited news entries in a single group", async () => {
        let newsService = TestBed.inject(NewsService);
        spyOn(newsService, "getNews").and.returnValue(Promise.resolve(siteNewsEntryListResponse));

        component.showDates = false;
        component.maxEntries = 3;

        fixture.detectChanges();
        await fixture.whenStable()
        fixture.detectChanges();

        expect(newsService.getNews).toHaveBeenCalled();

        let sections = fixture.debugElement.queryAll(By.css("div"));
        expect(sections.length).toEqual(1);

        let section = sections[0];

        let dateElement = section.query(By.css("h3"));
        expect(dateElement).toBeNull();

        let list = section.query(By.css("ul"));
        let listItems = list.queryAll(By.css("li"));
        expect(listItems.length).toEqual(3);
        expect(listItems[0].nativeElement.textContent.trim()).toEqual("3.0");
        expect(listItems[1].nativeElement.textContent.trim()).toEqual("3.1");
        expect(listItems[2].nativeElement.textContent.trim()).toEqual("2.0");
    });

    it("should display an error when the news service errors", async () => {
        let newsService = TestBed.inject(NewsService);
        spyOn(newsService, "getNews").and.returnValue(Promise.reject("someReason"));

        fixture.detectChanges();

        await fixture.whenStable()
        fixture.detectChanges();
        expect(newsService.getNews).toHaveBeenCalled();

        let error = fixture.debugElement.query(By.css("p"));
        expect(error.nativeElement.textContent.trim()).toEqual("There was a problem getting the site news");
    });

    describe("Admin users", () => {
        let newsService: NewsService;

        beforeEach(() => {
            userInfo.next(adminUser);

            newsService = TestBed.inject(NewsService);
            spyOn(newsService, "getNews").and.returnValue(Promise.resolve(siteNewsEntryListResponse));
        });

        it("should not show edit and delete buttons when isFull=false", async () => {
            component.showDates = false;

            fixture.detectChanges();
            await fixture.whenStable()
            fixture.detectChanges();

            expect(newsService.getNews).toHaveBeenCalled();

            let sections = fixture.debugElement.queryAll(By.css("div"));
            expect(sections.length).toEqual(1);

            let section = sections[0];

            let dateElement = section.query(By.css("h3"));
            expect(dateElement).toBeNull();

            let buttons = section.query(By.css("button"));
            expect(buttons).toBeNull();
        });

        it("should show edit and delete buttons when isFull=true", async () => {
            component.showDates = true;

            fixture.detectChanges();
            await fixture.whenStable()
            fixture.detectChanges();

            expect(newsService.getNews).toHaveBeenCalled();

            let sections = fixture.debugElement.queryAll(By.css("div"));
            expect(sections.length).toEqual(3);

            for (let i = 0; i < sections.length; i++) {
                let dateElement = sections[i].query(By.css("h3"));
                expect(dateElement).not.toBeNull();

                let buttons = dateElement.queryAll(By.css("button"));
                expect(buttons.length).toEqual(2);
                expect(buttons[0].nativeElement.textContent.trim()).toEqual("Edit");
                expect(buttons[1].nativeElement.textContent.trim()).toEqual("Delete");
            }
        });

        it("should add a new section", async () => {
            spyOn(newsService, "addNews").and.returnValue(Promise.resolve());

            component.showDates = true;

            fixture.detectChanges();
            await fixture.whenStable()
            fixture.detectChanges();

            expect(newsService.getNews).toHaveBeenCalled();

            let sections = fixture.debugElement.queryAll(By.css("div"));
            expect(sections.length).toEqual(3);

            let addButton = fixture.debugElement.query(By.css("button"));
            expect(addButton).not.toBeNull();
            addButton.nativeElement.click();

            fixture.detectChanges();

            // Wait since ngModel is async
            await fixture.whenStable();

            sections = fixture.debugElement.queryAll(By.css("div"));
            expect(sections.length).toEqual(4);

            let section = sections[0];

            let dateElement = section.query(By.css("h3"));
            expect(dateElement).not.toBeNull();

            let dateInput = dateElement.query(By.css("input"));
            expect(dateInput).not.toBeNull();
            let dateStr = dateInput.nativeElement.value;

            let buttons = section.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);
            expect(buttons[0].nativeElement.textContent.trim()).toEqual("Add");
            expect(buttons[1].nativeElement.textContent.trim()).toEqual("Save");
            expect(buttons[2].nativeElement.textContent.trim()).toEqual("Cancel");

            let addMessageButton = buttons[0];
            let saveButton = buttons[1];

            addMessageButton.nativeElement.click();
            fixture.detectChanges();

            let messages = section.queryAll(By.css("textarea"));
            expect(messages.length).toEqual(2);

            setInputValue(messages[0], "someMessage0");
            setInputValue(messages[1], "someMessage1");

            addMessageButton.nativeElement.click();
            fixture.detectChanges();

            messages = section.queryAll(By.css("textarea"));
            expect(messages.length).toEqual(3);

            setInputValue(messages[2], "someMessage2");

            saveButton.nativeElement.click();
            await fixture.whenStable();

            let expectedEntry: ISiteNewsEntry = {
                date: new Date(dateStr).toISOString().substring(0, 10),
                messages: ["someMessage0", "someMessage1", "someMessage2"],
            };

            expect(newsService.addNews).toHaveBeenCalledWith(expectedEntry);
        });

        it("should edit an existing section", async () => {
            spyOn(newsService, "addNews").and.returnValue(Promise.resolve());

            component.showDates = true;

            let section: DebugElement;
            let saveButton: DebugElement;

            fixture.detectChanges();
            await fixture.whenStable();
            fixture.detectChanges();

            expect(newsService.getNews).toHaveBeenCalled();

            let sections = fixture.debugElement.queryAll(By.css("div"));
            expect(sections.length).toEqual(3);
            section = sections[1];

            let dateElement = section.query(By.css("h3"));
            expect(dateElement).not.toBeNull();

            let buttons = dateElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(2);
            expect(buttons[0].nativeElement.textContent.trim()).toEqual("Edit");
            expect(buttons[1].nativeElement.textContent.trim()).toEqual("Delete");

            let editButton = buttons[0];
            editButton.nativeElement.click();

            // Need to wait since ngModel is async
            fixture.detectChanges();
            await fixture.whenStable();

            dateElement = section.query(By.css("h3"));
            expect(dateElement).not.toBeNull();
            
            let dateInput = dateElement.query(By.css("input"));
            expect(dateInput).toBeNull();
            
            buttons = section.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);
            expect(buttons[0].nativeElement.textContent.trim()).toEqual("Add");
            expect(buttons[1].nativeElement.textContent.trim()).toEqual("Save");
            expect(buttons[2].nativeElement.textContent.trim()).toEqual("Cancel");

            let messages = section.queryAll(By.css("textarea"));
            expect(messages.length).toEqual(2);

            let addMessageButton = buttons[0];
            saveButton = buttons[1];
            addMessageButton.nativeElement.click();
            fixture.detectChanges();

            messages = section.queryAll(By.css("textarea"));
            expect(messages.length).toEqual(3);
            setInputValue(messages[1], "someMessage1");
            setInputValue(messages[2], "someMessage2");
            saveButton.nativeElement.click();
            await fixture.whenStable();

            let expectedEntry: ISiteNewsEntry = {
                date: "2017-01-02",
                messages: ["2.0", "someMessage1", "someMessage2"],
            };
            expect(newsService.addNews).toHaveBeenCalledWith(expectedEntry);
        });

        it("should delete an existing section", async () => {
            spyOn(newsService, "deleteNews").and.returnValue(Promise.resolve());

            component.showDates = true;

            fixture.detectChanges();
            await fixture.whenStable();
            fixture.detectChanges();

            expect(newsService.getNews).toHaveBeenCalled();
            (newsService.getNews as jasmine.Spy).calls.reset();

            let sections = fixture.debugElement.queryAll(By.css("div"));
            expect(sections.length).toEqual(3);

            let section = sections[1];
            let dateElement = section.query(By.css("h3"));
            expect(dateElement).not.toBeNull();

            let buttons = dateElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(2);
            expect(buttons[0].nativeElement.textContent.trim()).toEqual("Edit");
            expect(buttons[1].nativeElement.textContent.trim()).toEqual("Delete");

            let deleteButton = buttons[1];
            deleteButton.nativeElement.click();

            // Need to wait since ngModel is async
            fixture.detectChanges();
            await fixture.whenStable();

            expect(newsService.deleteNews).toHaveBeenCalledWith("2017-01-02");
            expect(newsService.getNews).toHaveBeenCalled();
        });

        function setInputValue(element: DebugElement, value: string): void {
            element.nativeElement.value = value;

            // Tell Angular
            let evt = document.createEvent("CustomEvent");
            evt.initCustomEvent("input", false, false, null);
            element.nativeElement.dispatchEvent(evt);
        }
    });
});
