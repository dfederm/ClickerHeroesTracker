declare interface IClanData {
    clanName: string;

    currentRaidLevel: number;

    guildMembers: Array<IGuildMember>;

    messages: Array<IMessage>;
}
