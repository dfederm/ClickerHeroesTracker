import { Component, OnInit } from "@angular/core";
import { ClanService, IGuildMember, IMessage, ILeaderboardClan, ILeaderboardSummaryListResponse } from "../../services/clanService/clanService";

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

    public isInClan: boolean;

    public clanName: string;

    public guildMembers: IGuildMember[];

    public messages: IMessage[];

    public newMessage = "";

    public clans: ILeaderboardClan[];

    public totalClans: number;

    public leaderboardCount = 10;

    private userClan: ILeaderboardClan;

    private _leaderboardPage = 1;

    constructor(private clanService: ClanService) { }

    public get leaderboardPage(): number {
        return this._leaderboardPage;
    }

    public set leaderboardPage(leaderboardPage: number) {
        this._leaderboardPage = leaderboardPage;
        this.getLeaderboard()
            .then(response => this.updateLeaderboard(response))
            .catch(() => {
                this.isLeaderboardError = true;
            });
    }

    public ngOnInit(): void {
        this.getMessages();

        // The leaderboard depends on the user clan, so kick them off in parallel but don't process until they're both done.
        Promise.all([
            this.getClanInformation(),
            this.getLeaderboard(),
        ])
            .then(results => {
                this.updateLeaderboard(results[1]);
            })
            .catch(() => {
                this.isLeaderboardError = true;
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

    private getClanInformation(): Promise<void> {
        this.isClanInformationLoading = true;
        return this.clanService.getClan()
            .then(response => {
                this.isClanInformationLoading = false;
                if (response == null) {
                    return;
                }

                this.isInClan = true;
                this.clanName = response.clanName;
                this.guildMembers = response.guildMembers;

                this.userClan = {
                    name: response.clanName,
                    currentRaidLevel: response.currentRaidLevel,
                    memberCount: response.guildMembers.length,
                    rank: response.rank,
                    isUserClan: true,
                };
            })
            .catch(() => {
                this.isClanInformationError = true;
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

    private getLeaderboard(): Promise<ILeaderboardSummaryListResponse> {
        this.isLeaderboardLoading = true;
        return this.clanService.getLeaderboard(this.leaderboardPage, this.leaderboardCount);
    }

    private updateLeaderboard(response: ILeaderboardSummaryListResponse): void {
        this.isLeaderboardLoading = false;
        if (!response || !response.leaderboardClans) {
            this.isLeaderboardError = true;
            return;
        }

        this.clans = response.leaderboardClans;

        // Only add the user clan if it's not in the results
        if (this.userClan && !this.clans.find(clan => clan.isUserClan)) {
            this.clans.push(this.userClan);
        }

        this.clans = this.clans.sort((a, b) => a.rank - b.rank);

        if (response.pagination) {
            this.totalClans = response.pagination.count;
        }
    }
}
