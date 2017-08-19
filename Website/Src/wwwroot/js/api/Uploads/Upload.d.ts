declare interface IUpload
{
    id: number;

    timeSubmitted: string;

    playStyle: string;

    user?: IUser;

    uploadContent?: string;

    stats?: IMap<string>;
}
