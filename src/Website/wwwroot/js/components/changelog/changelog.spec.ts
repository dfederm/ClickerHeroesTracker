import { ComponentFixture, TestBed, async } from "@angular/core/testing";
import { By } from "@angular/platform-browser";

import { ChangelogComponent } from "./changelog";
import { NewsService } from "../../services/newsService/newsService";

describe("ChangelogComponent", () =>
{
    let component: ChangelogComponent;
    let fixture: ComponentFixture<ChangelogComponent>;

    beforeEach(async(() =>
    {
        let newsService = { getNews: (): void => void 0 };

        TestBed.configureTestingModule(
        {
            declarations: [ ChangelogComponent ],
            providers: [ {provide: NewsService, useValue: newsService } ],
        })
        .compileComponents()
        .then(() =>
        {
            fixture = TestBed.createComponent(ChangelogComponent);
            component = fixture.componentInstance;
        });
    }));

    it("should display all sections of news entries when isFull=true", async(() =>
    {
        let siteNewsEntryListResponse: ISiteNewsEntryListResponse =
        {
            entries:
            {
                "2017-01-01": [ "2017-01-01.0", "2017-01-01.1" ],
                "2017-01-02": [ "2017-01-02.0", "2017-01-02.1" ],
                "2017-01-03": [ "2017-01-03.0", "2017-01-03.1" ],
            },
        };
        let newsService = TestBed.get(NewsService);
        spyOn(newsService, "getNews").and.returnValue(Promise.resolve(siteNewsEntryListResponse));

        component.isFull = true;
        fixture.detectChanges();

        fixture.whenStable().then(() =>
        {
            fixture.detectChanges();

            let sections = fixture.debugElement.queryAll(By.css("div"));
            expect(sections.length).toEqual(3);

            for (let i = 0; i < sections.length; i++)
            {
                // The sections are rendered in reverse order
                let expectedDate = sections.length - i;

                let dateElement = sections[i].query(By.css("h3"));
                expect(dateElement.nativeElement.textContent.trim()).toEqual(`1/${expectedDate}/2017`);

                let list = sections[i].query(By.css("ul"));
                let listItems = list.queryAll(By.css("li"));
                expect(listItems.length).toEqual(2);
                for (let j = 0; j < listItems.length; j++)
                {
                    let listItem: HTMLElement = listItems[j].nativeElement;
                    expect(listItem.textContent).toEqual(`2017-01-0${expectedDate}.${j}`);
                }
            }
        });
    }));

    it("should display one section of 3 news entries when isFull=false", async(() =>
    {
        let siteNewsEntryListResponse: ISiteNewsEntryListResponse =
        {
            entries:
            {
                "2017-01-01": [ "2017-01-01.0", "2017-01-01.1" ],
                "2017-01-02": [ "2017-01-02.0", "2017-01-02.1" ],
                "2017-01-03": [ "2017-01-03.0", "2017-01-03.1" ],
            },
        };
        let newsService = TestBed.get(NewsService);
        spyOn(newsService, "getNews").and.returnValue(Promise.resolve(siteNewsEntryListResponse));

        component.isFull = false;
        fixture.detectChanges();

        fixture.whenStable().then(() =>
        {
            fixture.detectChanges();

            let sections = fixture.debugElement.queryAll(By.css("div"));
            expect(sections.length).toEqual(1);

            let section = sections[0];

            let dateElement = section.query(By.css("h3"));
            expect(dateElement).toBeNull();

            let list = section.query(By.css("ul"));
            let listItems = list.queryAll(By.css("li"));
            expect(listItems.length).toEqual(3);
            expect(listItems[0].nativeElement.textContent).toEqual("2017-01-03.0");
            expect(listItems[1].nativeElement.textContent).toEqual("2017-01-03.1");
            expect(listItems[2].nativeElement.textContent).toEqual("2017-01-02.0");
        });
    }));

    it("should display an error when the news service errors", async(() =>
    {
        let newsService = TestBed.get(NewsService);
        spyOn(newsService, "getNews").and.returnValue(Promise.reject("someReason"));

        fixture.detectChanges();

        fixture.whenStable().then(() =>
        {
            fixture.detectChanges();

            let error = fixture.debugElement.query(By.css("p"));
            expect(error.nativeElement.textContent.trim()).toEqual("There was a problem getting the site news");
        });
    }));
});
