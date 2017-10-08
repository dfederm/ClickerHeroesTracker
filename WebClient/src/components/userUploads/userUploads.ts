import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

@Component({
  selector: "userUploads",
  templateUrl: "./userUploads.html",
})
export class UserUploadsComponent implements OnInit {
  public userName: string;

  constructor(
    private route: ActivatedRoute,
  ) { }

  public ngOnInit(): void {
    this.route
      .params
      .subscribe(params => this.userName = params.userName);
  }
}
