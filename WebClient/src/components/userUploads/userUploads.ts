import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, RouterLink } from "@angular/router";
import { UploadsTableComponent } from "../uploadsTable/uploadsTable";

@Component({
  selector: "userUploads",
  templateUrl: "./userUploads.html",
  imports: [
    RouterLink,
    UploadsTableComponent,
  ],
  standalone: true,
})
export class UserUploadsComponent implements OnInit {
  public userName: string;

  constructor(
    private readonly route: ActivatedRoute,
  ) { }

  public ngOnInit(): void {
    this.route
      .params
      .subscribe(params => this.userName = params.userName);
  }
}
