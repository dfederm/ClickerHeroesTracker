import { NO_ERRORS_SCHEMA } from "@angular/core";
import { ComponentFixture, TestBed, async } from "@angular/core/testing";
import { By } from "@angular/platform-browser";

import { DashboardComponent } from "./dashboard";

describe("DashboardComponent", () =>
{
    let component: DashboardComponent;
    let fixture: ComponentFixture<DashboardComponent>;

    beforeEach(async(() =>
    {
        TestBed.configureTestingModule(
        {
            declarations: [ DashboardComponent ],
            schemas: [ NO_ERRORS_SCHEMA ],
        })
        .compileComponents()
        .then(() =>
        {
            fixture = TestBed.createComponent(DashboardComponent);
            component = fixture.componentInstance;
        });
    }));

    it("should display an upload table without pagination", () =>
    {
        fixture.detectChanges();

        let uploadsTable = fixture.debugElement.query(By.css("uploadsTable"));
        expect(uploadsTable).not.toBeNull();
        expect(uploadsTable.properties["count"]).toEqual(10);
        expect(uploadsTable.properties["paginate"]).toBeFalsy();
    });
});
