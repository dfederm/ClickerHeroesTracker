import { ComponentFixture, TestBed, async } from "@angular/core/testing";
import { By } from "@angular/platform-browser";

import { NotFoundComponent } from "./notFound";

describe("NotFoundComponent", () => {
    let fixture: ComponentFixture<NotFoundComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule(
            {
                declarations: [NotFoundComponent],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(NotFoundComponent);
            });
    }));

    it("should display an error message", () => {
        fixture.detectChanges();

        let error = fixture.debugElement.query(By.css(".alert-danger"));
        expect(error).not.toBeNull();
    });
});
