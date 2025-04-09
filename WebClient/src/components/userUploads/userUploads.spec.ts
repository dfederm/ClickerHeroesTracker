import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { ActivatedRoute, Params } from "@angular/router";
import { BehaviorSubject } from "rxjs";

import { UserUploadsComponent } from "./userUploads";
import { Component, Input } from "@angular/core";
import { UploadsTableComponent } from "../uploadsTable/uploadsTable";

describe("UserUploadsComponent", () => {
    let fixture: ComponentFixture<UserUploadsComponent>;
    let routeParams: BehaviorSubject<Params>;

    const userName = "someUserName";

    @Component({ selector: "uploadsTable", template: "" })
    class MockUploadsTableComponent {
        @Input()
        public userName: string;

        @Input()
        public count: number;

        @Input()
        public paginate: boolean;
    }

    beforeEach(async () => {
        routeParams = new BehaviorSubject({ userName });
        let route = { params: routeParams };

        await TestBed.configureTestingModule(
            {
                imports: [UserUploadsComponent],
                providers: [
                    { provide: ActivatedRoute, useValue: route },
                ],
            })
            .compileComponents();
        TestBed.overrideComponent(UserUploadsComponent, {
            remove: { imports: [ UploadsTableComponent ]},
            add: { imports: [ MockUploadsTableComponent ] },
        });

        fixture = TestBed.createComponent(UserUploadsComponent);
    });

    it("should display a paginated upload table", () => {
        fixture.detectChanges();

        let uploadsTable = fixture.debugElement.query(By.css("uploadsTable"))?.componentInstance as UploadsTableComponent;
        expect(uploadsTable).not.toBeNull();
        expect(uploadsTable.userName).toEqual(userName);
        expect(uploadsTable.count).toEqual(20);
        expect(uploadsTable.paginate).toEqual(true);
    });
});
