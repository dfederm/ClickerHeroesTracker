import { Component, OnInit } from "@angular/core";
import { ClanService, ILeaderboardClan, ILeaderboardSummaryListResponse } from "../../services/clanService/clanService";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { UserService } from "../../services/userService/userService";

@Component({
    selector: "clans",
    templateUrl: "./clans.html",
})
export class ClansComponent implements OnInit {
    public isError = false;

    public isLoading: boolean;

    public clans: ILeaderboardClan[];

    public totalClans: number;

    public count = 10;

    private userClan: ILeaderboardClan;

    private leaderboardResponse: ILeaderboardSummaryListResponse;

    private _page = 1;

    constructor(
        private readonly clanService: ClanService,
        private readonly authenticationService: AuthenticationService,
        private readonly userService: UserService,
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
                this.getUserClan(userInfo.username);
            });
    }

    private getUserClan(username: string): Promise<void> {
        this.userClan = null;

        return this.userService.getUser(username)
            .then(user => {
                if (!user || !user.clanName) {
                    return null;
                }

                return this.clanService.getClan(user.clanName);
            })
            .then(response => {
                if (!response) {
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
    }

    private getLeaderboard(): Promise<void> {
        this.isLoading = true;
        this.isError = false;
        return this.clanService.getLeaderboard(this.page, this.count)
            .then(response => {
                this.isLoading = false;
                this.leaderboardResponse = response;
                this.updateLeaderboard();
            })
            .catch(() => {
                this.isError = true;
            });
    }

    private updateLeaderboard(): void {
        if (!this.leaderboardResponse || !this.leaderboardResponse.leaderboardClans) {
            return;
        }

        this.clans = this.leaderboardResponse.leaderboardClans;

        // Only add the user clan if it's not in the results
        if (this.userClan && !this.clans.find(clan => clan.isUserClan)) {
            this.clans.push(this.userClan);
        }

        this.clans = this.clans.sort((a, b) => a.rank - b.rank);

        if (this.leaderboardResponse.pagination) {
            this.totalClans = this.leaderboardResponse.pagination.count;
        }
    }
}
