import { Component, OnInit } from "@angular/core";
import { ClanService, ILeaderboardClan, ILeaderboardSummaryListResponse } from "../../services/clanService/clanService";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { UserService } from "../../services/userService/userService";
import { NgxSpinnerService } from "ngx-spinner";
import { debounceTime, map, Subject } from "rxjs";

@Component({
    selector: "clans",
    templateUrl: "./clans.html",
})
export class ClansComponent implements OnInit {
    public isError = false;

    public clans: ILeaderboardClan[];

    public totalClans: number;

    public count = 20;

    public filterSubject = new Subject<string>();

    private filter = "";

    private userClan: ILeaderboardClan;

    private leaderboardResponse: ILeaderboardSummaryListResponse;

    private _page = 1;

    constructor(
        private readonly clanService: ClanService,
        private readonly authenticationService: AuthenticationService,
        private readonly userService: UserService,
        private readonly spinnerService: NgxSpinnerService,
    ) { }

    public get page(): number {
        return this._page;
    }

    public set page(value: number) {
        this._page = value;
        this.getLeaderboard();
    }

    public ngOnInit(): void {
        this.getLeaderboard();

        this.authenticationService
            .userInfo()
            .subscribe(userInfo => {
                this.handleUser(userInfo);
            });

        this.filterSubject.pipe(
            debounceTime(250),
            map(newFilter => {
                this.filter = newFilter;
                this.getLeaderboard();
            })
        ).subscribe();
    }

    public setFilter(filterValue: string): void {
        this.filterSubject.next(filterValue);
    }

    private handleUser(userInfo: IUserInfo): void {
        this.userClan = null;

        if (userInfo.isLoggedIn) {
            this.userService.getUser(userInfo.username)
                .then(user => {
                    if (!user || !user.clanName) {
                        return null;
                    }

                    return this.clanService.getClan(user.clanName);
                })
                .then(response => {
                    if (!response || response.isBlocked) {
                        return;
                    }

                    this.userClan = {
                        name: response.clanName,
                        currentRaidLevel: response.currentRaidLevel,
                        memberCount: response.guildMembers.length,
                        rank: response.rank,
                        isUserClan: true,
                    };

                    this.updateLeaderboard();
                });
        } else {
            this.updateLeaderboard();
        }
    }

    private getLeaderboard(): Promise<void> {
        this.isError = false;
        this.spinnerService.show("clans");
        return this.clanService.getLeaderboard(this.filter, this.page, this.count)
            .then(response => {
                this.leaderboardResponse = response;
                this.updateLeaderboard();
            })
            .catch(() => {
                this.isError = true;
            })
            .finally(() => {
                this.spinnerService.hide("clans");
            });
    }

    private updateLeaderboard(): void {
        if (!this.leaderboardResponse || !this.leaderboardResponse.leaderboardClans) {
            return;
        }

        // Clone the list since we may mutate it.
        this.clans = this.leaderboardResponse.leaderboardClans.slice();

        // Only add the user clan if it's not in the results and it matches the filter
        if (this.userClan
            && !this.clans.find(clan => clan.isUserClan)
            && (!this.filter || this.userClan.name.toLowerCase().includes(this.filter.toLowerCase()))) {
            this.clans.push(this.userClan);
        }

        this.clans = this.clans.sort((a, b) => a.rank - b.rank);

        if (this.leaderboardResponse.pagination) {
            this.totalClans = this.leaderboardResponse.pagination.count;
        }
    }
}
