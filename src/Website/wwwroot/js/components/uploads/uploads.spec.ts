import { NO_ERRORS_SCHEMA } from "@angular/core";
import { ComponentFixture, TestBed, async } from "@angular/core/testing";
import { By } from "@angular/platform-browser";

import { UploadsComponent } from "./uploads";

describe("UploadsComponent", () =>
{
    let component: UploadsComponent;
    let fixture: ComponentFixture<UploadsComponent>;

    beforeEach(async(() =>
    {
        TestBed.configureTestingModule(
        {
            declarations: [ UploadsComponent ],
            schemas: [ NO_ERRORS_SCHEMA ],
        })
        .compileComponents()
        .then(() =>
        {
            fixture = TestBed.createComponent(UploadsComponent);
            component = fixture.componentInstance;
        });
    }));

    it("should display a paginated upload table", () =>
    {
        fixture.detectChanges();

        let uploadsTable = fixture.debugElement.query(By.css("uploadsTable"));
        expect(uploadsTable).not.toBeNull();
        expect(uploadsTable.properties["count"]).toEqual(20);
        expect(uploadsTable.properties["paginate"]).toEqual(true);
    });
});
