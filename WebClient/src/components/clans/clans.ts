import { Component, OnInit } from "@angular/core";
import { ClanService, IGuildMember, IMessage, ILeaderboardClan, ILeaderboardSummaryListResponse } from "../../services/clanService/clanService";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { UserService } from "../../services/userService/userService";

@Component({
    selector: "clans",
    templateUrl: "./clans.html",
    styleUrls: ["./clans.css"],
})
export class ClansComponent implements OnInit {
    public isClanInformationError = false;

    public isClanInformationLoading: boolean;

    public messagesError = "";

    public isMessagesLoading: boolean;

    public isLeaderboardError = false;

    public isLeaderboardLoading: boolean;

    public clanName: string;

    public guildMembers: IGuildMember[];

    public messages: IMessage[];

    public newMessage = "";

    public clans: ILeaderboardClan[];

    public totalClans: number;

    public leaderboardCount = 10;

    private userClan: ILeaderboardClan;

    private leaderboardResponse: ILeaderboardSummaryListResponse;

    private _leaderboardPage = 1;

    constructor(
        private readonly clanService: ClanService,
        private readonly authenticationService: AuthenticationService,
        private readonly userService: UserService,
    ) { }

    public get leaderboardPage(): number {
        return this._leaderboardPage;
    }

    public set leaderboardPage(leaderboardPage: number) {
        this._leaderboardPage = leaderboardPage;
        this.getLeaderboard();
    }

    public ngOnInit(): void {
        this.getMessages();
        this.getLeaderboard();

        this.authenticationService
            .userInfo()
            .subscribe(userInfo => {
                this.getClanInformation(userInfo.username);
            });
    }

    public sendMessage(): void {
        this.messagesError = "";
        this.isMessagesLoading = true;
        this.clanService.sendMessage(this.newMessage, this.clanName)
            .then(() => {
                this.newMessage = "";
                return this.getMessages();
            })
            .catch(() => {
                this.messagesError = "There was a problem sending your message. Please try again.";
            });
    }

    private getClanInformation(username: string): Promise<void> {
        this.isClanInformationError = false;
        this.isClanInformationLoading = true;
        this.clanName = null;
        this.guildMembers = null;
        this.userClan = null;

        return this.userService.getUser(username)
            .then(user => {
                if (!user || !user.clanName) {
                    return null;
                }

                this.clanName = user.clanName;
                return this.clanService.getClan(this.clanName);
            })
            .then(response => {
                this.isClanInformationLoading = false;
                if (!response) {
                    return;
                }

                this.guildMembers = response.guildMembers;

                this.userClan = {
                    name: response.clanName,
                    currentRaidLevel: response.currentRaidLevel,
                    memberCount: response.guildMembers.length,
                    rank: response.rank,
                    isUserClan: true,
                };

                // The leaderboard depends on the user clan, so update the leaderboard when the leaderboard changes.
                this.updateLeaderboard();
            })
            .catch(() => {
                this.isClanInformationError = true;
                this.clanName = null;
            });
    }

    private getMessages(): Promise<void> {
        this.messagesError = "";
        this.isMessagesLoading = true;
        return this.clanService.getMessages()
            .then(messages => {
                this.isMessagesLoading = false;
                this.messages = messages;
            })
            .catch(() => {
                this.messagesError = "There was a problem getting your clan's messages. Please try again.";
            });
    }

    private getLeaderboard(): Promise<void> {
        this.isLeaderboardLoading = true;
        this.isLeaderboardError = false;
        return this.clanService.getLeaderboard(this.leaderboardPage, this.leaderboardCount)
            .then(response => {
                this.isLeaderboardLoading = false;
                this.leaderboardResponse = response;
                this.updateLeaderboard();
            })
            .catch(() => {
                this.isLeaderboardError = true;
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
