import { NO_ERRORS_SCHEMA } from "@angular/core";
import { ComponentFixture, TestBed, async } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { ActivatedRoute, Params } from "@angular/router";
import { BehaviorSubject } from "rxjs";

import { UserUploadsComponent } from "./userUploads";

describe("UserUploadsComponent", () => {
    let component: UserUploadsComponent;
    let fixture: ComponentFixture<UserUploadsComponent>;
    let routeParams: BehaviorSubject<Params>;

    const userName = "someUserName";

    beforeEach(async(() => {
        routeParams = new BehaviorSubject({ userName });
        let route = { params: routeParams };

        TestBed.configureTestingModule(
            {
                declarations: [UserUploadsComponent],
                providers: [
                    { provide: ActivatedRoute, useValue: route },
                ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(UserUploadsComponent);
                component = fixture.componentInstance;
            });
    }));

    it("should display a paginated upload table", () => {
        fixture.detectChanges();

        let uploadsTable = fixture.debugElement.query(By.css("uploadsTable"));
        expect(uploadsTable).not.toBeNull();
        expect(uploadsTable.properties.userName).toEqual(userName);
        expect(uploadsTable.properties.count).toEqual(20);
        expect(uploadsTable.properties.paginate).toEqual(true);
    });
});
